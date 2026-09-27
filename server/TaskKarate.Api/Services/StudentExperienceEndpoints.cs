using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TaskKarate.Api.Services;

public static class StudentAuth
{
    public const string Scheme = "TaskKarateStudent";
    public const string DisclaimerClaim = "task_karate_disclaimer";
    public const string DisclaimerVersion = "student-profile-v2";

    public static async Task<int?> GetStudentIdAsync(HttpContext context)
    {
        var result = await context.AuthenticateAsync(Scheme);
        return result.Succeeded && int.TryParse(result.Principal?.FindFirstValue("student_id"), out var id) ? id : null;
    }
}

internal static class StudentTextSafety
{
    private static readonly Regex BlockedWords = new(
        "(?<![A-Za-z0-9])(?:" + string.Join('|', new[] { "fuck", "shit", "bitch", "asshole", "bastard", "dick", "piss", "cunt", "nigger", "faggot" }.Select(Regex.Escape)) + ")(?![A-Za-z0-9])",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string Filter(string value) => BlockedWords.Replace(value, match => new string('•', Math.Max(3, match.Value.Length)));
}

public static class StudentExperienceEndpoints
{
    public static void MapStudentExperience(this WebApplication app)
    {
        var publicApi = app.MapGroup("/api/student/public");
        publicApi.MapGet("/schedule", async (StudentExperienceService service, DateTime? from, DateTime? to, CancellationToken ct) => Results.Ok(await service.GetScheduleAsync((from ?? DateTime.UtcNow.Date).Date, (to ?? DateTime.UtcNow.Date.AddDays(30)).Date.AddDays(1), ct)));
        publicApi.MapGet("/news", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetNewsAsync(ct)));
        publicApi.MapGet("/students", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetStudentDirectoryAsync(ct)));
        publicApi.MapGet("/schedule/{sessionId:int}/roster", async (int sessionId, StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPublicRosterAsync(sessionId, ct)));
        publicApi.MapPost("/schedule/{sessionId:int}/check-in", async (int sessionId, PublicCheckInRequest request, StudentExperienceService service, CancellationToken ct) =>
        {
            if (!request.Confirmed) return Results.BadRequest(new { title = "Confirmation required", detail = "Confirm that the selected student is present before recording attendance." });
            if (string.IsNullOrWhiteSpace(request.Pin) || !await service.VerifyStudentPinAsync(request.StudentId, request.Pin, ct)) return Results.BadRequest(new { title = "Check-in PIN required", detail = "Enter the check-in PIN assigned to this student before recording attendance." });
            var result = await service.CheckInAsync(request.StudentId, sessionId, request.Helper, ct);
            if (result.Duplicate) return Results.Conflict(new { title = "Already checked in", detail = result.Error ?? "This student is already checked in for this class." });
            if (!result.Success) return Results.BadRequest(new { title = "Check-in not available", detail = result.Error ?? "Only an active class happening today can accept a public check-in." });
            return Results.Ok(new { checkedIn = true, helper = result.HelperRecorded });
        });
        publicApi.MapPost("/schedule/{sessionId:int}/pre-enroll", async (int sessionId, PublicCheckInRequest request, StudentExperienceService service, CancellationToken ct) =>
        {
            if (!request.Confirmed) return Results.BadRequest(new { title = "Confirmation required", detail = "Confirm that the selected student plans to attend before reserving the next class." });
            if (string.IsNullOrWhiteSpace(request.Pin) || !await service.VerifyStudentPinAsync(request.StudentId, request.Pin, ct)) return Results.BadRequest(new { title = "Student PIN required", detail = "Enter the student PIN assigned to this account before reserving the next class." });
            var result = await service.PreEnrollAsync(request.StudentId, sessionId, ct);
            if (result.Duplicate) return Results.Conflict(new { title = "Already on the list", detail = result.Error ?? "This student is already reserved for this class." });
            if (!result.Success) return Results.BadRequest(new { title = "Pre-enrollment unavailable", detail = result.Error ?? "Only the next upcoming class can accept a reservation." });
            return Results.Ok(new { reserved = true });
        });

        var auth = app.MapGroup("/api/student/auth");
        auth.MapPost("/login", async (StudentLoginRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            if (((request.StudentId is null || request.StudentId <= 0) && string.IsNullOrWhiteSpace(request.Username)) || string.IsNullOrWhiteSpace(request.Pin)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["credentials"] = ["Choose a student and enter the 4–6 digit student PIN."] });
            var account = request.StudentId is > 0 ? await service.AuthenticateByStudentIdAsync(request.StudentId.Value, request.Pin, ct) : await service.AuthenticateAsync(request.Username!, request.Pin, ct);
            if (account is null) return Results.Problem("Invalid student credentials.", statusCode: StatusCodes.Status401Unauthorized);
            await service.RefreshAutomaticMilestonesAsync(account.StudentId, ct);
            // A roster login can follow a prior student session on the same device. Clear it first
            // so the new identity never inherits stale disclaimer/session state.
            await context.SignOutAsync(StudentAuth.Scheme);
            var loginId = Guid.NewGuid().ToString("N");
            var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("student_id", account.StudentId.ToString()), new Claim(ClaimTypes.Name, account.DisplayName), new Claim("student_login_id", loginId)], StudentAuth.Scheme));
            await context.SignInAsync(StudentAuth.Scheme, principal, new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30), AllowRefresh = false });
            return Results.Ok(new { authenticated = true, studentId = account.StudentId, displayName = account.DisplayName, rankName = account.RankName, disclaimerRequired = true, birthdayWeek = account.BirthdayWeek, loginId });
        });
        auth.MapGet("/me", async (StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var authResult = await context.AuthenticateAsync(StudentAuth.Scheme);
            if (!authResult.Succeeded || !int.TryParse(authResult.Principal?.FindFirstValue("student_id"), out var id)) return Results.Unauthorized();
            var state = await service.GetStudentAccountStateAsync(id, ct);
            return state is null ? Results.Unauthorized() : Results.Ok(new { authenticated = true, studentId = id, displayName = authResult.Principal?.FindFirstValue(ClaimTypes.Name), disclaimerRequired = authResult.Principal?.HasClaim(StudentAuth.DisclaimerClaim, StudentAuth.DisclaimerVersion) != true, birthdayWeek = state.BirthdayWeek, loginId = authResult.Principal?.FindFirstValue("student_login_id") });
        });
        auth.MapPost("/logout", async (HttpContext context) => { await context.SignOutAsync(StudentAuth.Scheme); return Results.NoContent(); });
        auth.MapPost("/pin", async (StudentPinChangeRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            var errors = StudentPinPolicy.Validate(request.NewPin, request.ConfirmPin);
            if (errors.Count > 0) return Results.ValidationProblem(errors.ToDictionary(item => item.Key, item => item.Value));
            if (!await service.ChangeStudentPinAsync(gate.Id.Value, request.CurrentPin, request.NewPin, ct)) return Results.BadRequest(new { title = "Current PIN not accepted", detail = "Enter the current check-in PIN for this student account and try again." });
            await audit.RecordAsync(context, "ChangePin", "StudentAccount", Guid.Empty, new { legacyStudentId = gate.Id.Value, via = "student" }); return Results.NoContent();
        });

        var student = app.MapGroup("/api/student");
        student.MapPost("/disclaimer", async (DisclaimerRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var id = await StudentAuth.GetStudentIdAsync(context); if (id is null) return Results.Unauthorized();
            if (!request.Accepted || request.DisclaimerVersion != StudentAuth.DisclaimerVersion) return Results.BadRequest(new { title = "Current terms required", detail = "Accept the current profile access terms before opening a student profile." });
            await service.AcceptDisclaimerAsync(id.Value, ct);
            var authResult = await context.AuthenticateAsync(StudentAuth.Scheme);
            if (!authResult.Succeeded || authResult.Principal is null) return Results.Unauthorized();
            var identity = new ClaimsIdentity(authResult.Principal.Claims, StudentAuth.Scheme);
            identity.AddClaim(new Claim(StudentAuth.DisclaimerClaim, StudentAuth.DisclaimerVersion));
            await context.SignInAsync(StudentAuth.Scheme, new ClaimsPrincipal(identity), authResult.Properties ?? new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30), AllowRefresh = false });
            return Results.Ok(new { accepted = true });
        });

        student.MapGet("/profile", async (StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            var profile = await service.GetProfileAsync(gate.Id.Value, ct); return profile is null ? Results.NotFound() : Results.Ok(profile);
        });
        student.MapGet("/profile/emergency-contacts", async (StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            if (!await service.IsAdultStudentAsync(gate.Id.Value, ct)) return Results.Forbid();
            return Results.Ok(await service.GetEmergencyContactsAsync(gate.Id.Value, ct));
        });
        student.MapPost("/profile/emergency-contacts", async (EmergencyContactRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            if (!await service.IsAdultStudentAsync(gate.Id.Value, ct)) return Results.Forbid();
            var errors = ValidateEmergencyContact(request); if (errors.Count > 0) return Results.ValidationProblem(errors);
            return Results.Ok(await service.CreateEmergencyContactAsync(gate.Id.Value, request, ct));
        });
        student.MapPut("/profile/emergency-contacts/{contactId:long}", async (long contactId, EmergencyContactRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            if (!await service.IsAdultStudentAsync(gate.Id.Value, ct)) return Results.Forbid();
            var errors = ValidateEmergencyContact(request); if (errors.Count > 0) return Results.ValidationProblem(errors);
            var contact = await service.UpdateEmergencyContactAsync(gate.Id.Value, contactId, request, ct); return contact is null ? Results.NotFound() : Results.Ok(contact);
        });
        student.MapDelete("/profile/emergency-contacts/{contactId:long}", async (long contactId, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            if (!await service.IsAdultStudentAsync(gate.Id.Value, ct)) return Results.Forbid();
            return await service.DeleteEmergencyContactAsync(gate.Id.Value, contactId, ct) ? Results.NoContent() : Results.NotFound();
        });
        student.MapGet("/dojo-check-in", async (StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            return Results.Ok(await service.GetDojoCheckInSummaryAsync(gate.Id.Value, ct));
        });
        student.MapPost("/dojo-check-in", async (StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            var result = await service.CheckInAtDojoAsync(gate.Id.Value, ct); if (result.Duplicate) return Results.Conflict(new { title = "Already checked in", detail = result.Error }); return Results.Ok(result.Summary);
        });
        student.MapGet("/dojo-check-in/leaderboard", async (string? period, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            return Results.Ok(await service.GetDojoLeaderboardAsync(gate.Id.Value, period, ct));
        });
        student.MapPut("/profile", async (StudentProfileUpdateRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            var errors = new Dictionary<string, string[]>();
            if (request.DisplayName?.Length > 100) errors["displayName"] = ["Display name must be 100 characters or fewer."];
            if (request.Nickname?.Length > 100) errors["nickname"] = ["Nicknames must be 100 characters or fewer."];
            if (request.Pronouns?.Length > 50) errors["pronouns"] = ["Pronouns must be 50 characters or fewer."];
            if (request.Bio?.Length > 500) errors["bio"] = ["Bio must be 500 characters or fewer."];
            if (request.FavoriteTechnique?.Length > 100) errors["favoriteTechnique"] = ["Favorite technique must be 100 characters or fewer."];
            if (request.Email?.Length > 254) errors["email"] = ["Email must be 254 characters or fewer."];
            if (request.Phone?.Length > 50) errors["phone"] = ["Phone must be 50 characters or fewer."];
            if (request.UniformSize?.Length > 50) errors["uniformSize"] = ["Uniform size must be 50 characters or fewer."];
            if (request.BeltSize?.Length > 50) errors["beltSize"] = ["Belt size must be 50 characters or fewer."];
            if (errors.Count > 0) return Results.ValidationProblem(errors);
            await service.UpdateProfileAsync(gate.Id.Value, request, ct);
            var profile = await service.GetProfileAsync(gate.Id.Value, ct);
            return Results.Ok(profile);
        });
        student.MapPut("/profile/showcase", async (StudentProfileShowcaseRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            await service.UpdateProfileShowcaseAsync(gate.Id.Value, request, ct);
            return Results.Ok(new { saved = true });
        });
        student.MapGet("/profiles/{studentId:int}", async (int studentId, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            var profile = await service.GetPublicProfileAsync(gate.Id.Value, studentId, ct);
            return profile is null ? Results.NotFound() : Results.Ok(profile with { Bio = profile.Bio is null ? null : StudentTextSafety.Filter(profile.Bio), FavoriteTechnique = profile.FavoriteTechnique is null ? null : StudentTextSafety.Filter(profile.FavoriteTechnique), Achievements = profile.Achievements.Select(achievement => achievement with { Name = StudentTextSafety.Filter(achievement.Name), Description = achievement.Description is null ? null : StudentTextSafety.Filter(achievement.Description) }).ToArray(), RecentPosts = profile.RecentPosts.Select(post => post with { Text = StudentTextSafety.Filter(post.Text) }).ToArray(), FeaturedTiles = profile.FeaturedTiles.Select(tile => tile with { Label = StudentTextSafety.Filter(tile.Label), Value = StudentTextSafety.Filter(tile.Value), Description = StudentTextSafety.Filter(tile.Description) }).ToArray() });
        });
        student.MapPost("/schedule/{sessionId:int}/check-in", async (int sessionId, StudentCheckInRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            var result = await service.CheckInAsync(gate.Id.Value, sessionId, request.Helper, ct); if (result.Duplicate) return Results.Conflict(new { title = "Already checked in", detail = result.Error ?? "This student already has attendance recorded for this class session." }); if (!result.Success) return Results.BadRequest(new { title = "Check-in not available", detail = result.Error ?? "This class session cannot accept a check-in." }); return Results.Ok(new { checkedIn = true, helper = result.HelperRecorded });
        });
        student.MapGet("/check-ins", async (DateTime? from, DateTime? to, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            var start = (from ?? DateTime.Now.Date).Date; var end = (to ?? start.AddDays(1)).Date; return Results.Ok(await service.GetAttendanceSessionIdsAsync(gate.Id.Value, start, end, ct));
        });
        student.MapGet("/social/search", async (string? q, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2) return Results.Ok(Array.Empty<StudentSummary>()); return Results.Ok(await service.SearchStudentsAsync(gate.Id.Value, q.Trim(), ct));
        });
        student.MapGet("/gifs/search", async (string? q, int? page, IHttpClientFactory httpClientFactory, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            return await FetchGifSnapAsync(httpClientFactory, string.IsNullOrWhiteSpace(q) ? null : q.Trim(), page ?? 1, ct);
        });
        student.MapGet("/gifs/trending", async (int? page, IHttpClientFactory httpClientFactory, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            return await FetchGifSnapAsync(httpClientFactory, null, page ?? 1, ct);
        });
        student.MapGet("/friends", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetFriendsAsync(gate.Id.Value, ct)); });
        student.MapPost("/friends/{friendId:int}/request", async (int friendId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var status = await service.RequestFriendAsync(gate.Id.Value, friendId, ct); return status == "invalid" ? Results.BadRequest() : status is "pending" ? Results.Accepted($"/api/student/friends/{friendId}", new { status = "pending" }) : Results.Conflict(new { title = "Friend request already exists", status }); });
        student.MapPost("/friends/{friendId:int}/respond", async (int friendId, FriendResponseRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return await service.RespondToFriendAsync(gate.Id.Value, friendId, request.Accept, ct) ? Results.NoContent() : Results.NotFound(); });
        student.MapGet("/messages/{friendId:int}", async (int friendId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (!await service.AreFriendsAsync(gate.Id.Value, friendId, ct)) return Results.Forbid(); var messages = await service.GetMessagesAsync(gate.Id.Value, friendId, ct); return Results.Ok(messages.Select(message => message with { MessageText = StudentTextSafety.Filter(message.MessageText) })); });
        student.MapPost("/messages", async (MessageRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (request.RecipientId <= 0 || string.IsNullOrWhiteSpace(request.Message) || request.Message.Trim().Length > 2000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["message"] = ["A message from 1 to 2,000 characters is required."] }); if (!await service.AreFriendsAsync(gate.Id.Value, request.RecipientId, ct)) return Results.Forbid(); var messageId = await service.SendMessageAsync(gate.Id.Value, request.RecipientId, StudentTextSafety.Filter(request.Message), ct); return Results.Created($"/api/student/messages/{messageId}", new { messageId }); });
        student.MapPost("/messages/{messageId:long}/report", async (long messageId, MessageReportRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return await service.ReportMessageAsync(gate.Id.Value, messageId, request.Reason ?? "safety", ct) ? Results.NoContent() : Results.NotFound(); });
        student.MapPost("/messages/{messageId:long}/pin", async (long messageId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var pinned = await service.ToggleMessagePinAsync(gate.Id.Value, messageId, ct); return pinned is false && !await service.MessageBelongsToStudentAsync(gate.Id.Value, messageId, ct) ? Results.NotFound() : Results.Ok(new { pinned }); });
        student.MapPost("/messages/{messageId:long}/reaction", async (long messageId, ReactionRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return await service.ToggleMessageReactionAsync(gate.Id.Value, messageId, request.Code, ct) ? Results.Ok(new { updated = true }) : Results.BadRequest(new { title = "Message reaction unavailable" }); });
        student.MapPatch("/feed/{postId:long}", async (long postId, PostEditRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var filteredText = StudentTextSafety.Filter(request.Text); if (string.IsNullOrWhiteSpace(filteredText) || filteredText.Trim().Length > 2000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["text"] = ["A post from 1 to 2,000 characters is required."] }); return await service.UpdatePostAsync(gate.Id.Value, postId, filteredText, ct) ? Results.NoContent() : Results.NotFound(); });
        student.MapDelete("/feed/{postId:long}", async (long postId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return await service.HidePostAsync(gate.Id.Value, postId, ct) ? Results.NoContent() : Results.NotFound(); });
        student.MapPost("/feed/{postId:long}/report", async (long postId, ContentReportRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return await service.ReportPostAsync(gate.Id.Value, postId, request.Reason ?? "safety", ct) ? Results.NoContent() : Results.NotFound(); });
        student.MapGet("/feed", async (int? limit, int? offset, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var pageSize = Math.Clamp(limit ?? 25, 1, 50); var start = Math.Max(offset ?? 0, 0); var feed = await service.GetFeedAsync(gate.Id.Value, ct, pageSize + 1, start); var items = feed.Take(pageSize).Select(item => item with { Text = StudentTextSafety.Filter(item.Text), ImageAlt = item.ImageAlt is null ? null : StudentTextSafety.Filter(item.ImageAlt) }).ToArray(); return Results.Ok(new { items, hasMore = feed.Count > pageSize, nextOffset = start + items.Length }); });
        student.MapPost("/feed", async (PostRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var filteredText = StudentTextSafety.Filter(request.Text); var errors = new Dictionary<string, string[]>(); if (string.IsNullOrWhiteSpace(filteredText) || filteredText.Trim().Length > 2000) errors["text"] = ["A post from 1 to 2,000 characters is required."]; if (request.PostKind is not ("training" or "win" or "question" or "encouragement" or "announcement")) errors["postKind"] = ["Choose a supported post type."]; if (!string.IsNullOrWhiteSpace(request.LinkUrl) && (!Uri.TryCreate(request.LinkUrl.Trim(), UriKind.Absolute, out var link) || link.Scheme is not ("http" or "https") || request.LinkUrl.Trim().Length > 1000)) errors["linkUrl"] = ["Links must be valid HTTP or HTTPS URLs up to 1,000 characters."]; var imagePrefixAllowed = request.ImageData is null || request.ImageData.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase) || request.ImageData.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase) || request.ImageData.StartsWith("data:image/gif;base64,", StringComparison.OrdinalIgnoreCase) || request.ImageData.StartsWith("data:image/webp;base64,", StringComparison.OrdinalIgnoreCase); if (!string.IsNullOrWhiteSpace(request.ImageData) && (request.ImageData.Length > 1200000 || !imagePrefixAllowed)) errors["imageData"] = ["Images must be PNG, JPEG, GIF, or WebP files under 900 KB."]; if (request.ImageAlt?.Length > 160) errors["imageAlt"] = ["Image descriptions must be 160 characters or fewer."]; if (errors.Count > 0) return Results.ValidationProblem(errors); var postId = await service.CreatePostAsync(gate.Id.Value, filteredText, request.PostKind, request.LinkUrl?.Trim(), request.ImageData, StudentTextSafety.Filter(request.ImageAlt ?? string.Empty).Trim(), ct); return Results.Created($"/api/student/feed/{postId}", new { postId }); });
        student.MapGet("/bookmarks", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetBookmarkedPostIdsAsync(gate.Id.Value, ct)); });
        student.MapPost("/feed/{postId:long}/bookmark", async (long postId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var bookmarked = await service.ToggleBookmarkAsync(gate.Id.Value, postId, ct); return Results.Ok(new { bookmarked }); });
        student.MapPost("/feed/{postId:long}/reaction", async (long postId, ReactionRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (!await service.ToggleReactionAsync(gate.Id.Value, postId, request.Code, ct)) return Results.BadRequest(new { title = "Reaction unavailable" }); return Results.Ok(new { updated = true }); });
        student.MapGet("/feed/{postId:long}/comments", async (long postId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var comments = await service.GetCommentsAsync(gate.Id.Value, postId, ct); return comments is null ? Results.Forbid() : Results.Ok(comments.Select(comment => comment with { Text = StudentTextSafety.Filter(comment.Text) })); });
        student.MapPost("/feed/{postId:long}/comments", async (long postId, CommentRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var filteredText = StudentTextSafety.Filter(request.Text); if (string.IsNullOrWhiteSpace(filteredText) || filteredText.Trim().Length > 1000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["text"] = ["A comment from 1 to 1,000 characters is required."] }); var commentId = await service.CreateCommentAsync(gate.Id.Value, postId, filteredText, ct); return commentId is null ? Results.Forbid() : Results.Created($"/api/student/feed/{postId}/comments/{commentId}", new { commentId }); });
        student.MapGet("/achievements", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetAchievementsAsync(gate.Id.Value, ct)); });
        student.MapPost("/easter-eggs/{eggId}", async (string eggId, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            var result = await service.UnlockEasterEggAsync(gate.Id.Value, eggId, ct);
            return result is null ? Results.NotFound(new { title = "Unknown easter egg" }) : Results.Ok(result);
        });
        student.MapGet("/missions", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetTrainingMissionsAsync(gate.Id.Value, ct)); });
        student.MapPost("/missions/{missionId:long}/toggle", async (long missionId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(new { completed = await service.ToggleTrainingMissionAsync(gate.Id.Value, missionId, ct) }); });
        student.MapGet("/daily-missions", async (string? date, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return DateOnly.TryParse(date, out var parsed) ? Results.Ok(await service.GetDailyMissionsAsync(gate.Id.Value, parsed.ToDateTime(TimeOnly.MinValue), ct)) : Results.ValidationProblem(new Dictionary<string, string[]> { ["date"] = ["Use a valid date in yyyy-MM-dd format."] }); });
        student.MapPost("/daily-missions/sync", async (DailyMissionSyncRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            if (!DateOnly.TryParse(request.Date, out var date) || request.Missions is null || request.Missions.Count is < 1 or > 5) return Results.ValidationProblem(new Dictionary<string, string[]> { ["missions"] = ["A valid date and one to five daily missions are required."] });
            if (request.Missions.Any(item => string.IsNullOrWhiteSpace(item.MissionKey) || item.MissionKey.Length > 200 || string.IsNullOrWhiteSpace(item.Title) || item.Title.Length > 200 || string.IsNullOrWhiteSpace(item.Description) || item.Description.Length > 1000 || string.IsNullOrWhiteSpace(item.Category) || item.Category.Length > 100)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["missions"] = ["Daily mission fields exceed the allowed length or are empty."] });
            return Results.Ok(await service.SyncDailyMissionsAsync(gate.Id.Value, date.ToDateTime(TimeOnly.MinValue), request.Missions, ct));
        });
        student.MapPost("/daily-missions/{missionId:long}/toggle", async (long missionId, string? date, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (!DateOnly.TryParse(date, out var parsed)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["date"] = ["Use a valid date in yyyy-MM-dd format."] }); var completed = await service.ToggleDailyMissionAsync(gate.Id.Value, missionId, parsed.ToDateTime(TimeOnly.MinValue), ct); return completed is null ? Results.NotFound() : Results.Ok(new { completed }); });
        student.MapGet("/practice", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok((await service.GetPracticeLogsAsync(gate.Id.Value, ct)).Select(item => item with { Skill = StudentTextSafety.Filter(item.Skill), Reflection = item.Reflection is null ? null : StudentTextSafety.Filter(item.Reflection) })); });
        student.MapPost("/practice", async (PracticeLogRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (string.IsNullOrWhiteSpace(request.Skill) || request.Skill.Trim().Length > 100 || request.Minutes is < 1 or > 240 || request.Reflection?.Length > 1000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["practice"] = ["Skill is required, minutes must be between 1 and 240, and reflection must be 1,000 characters or fewer."] }); var id = await service.CreatePracticeLogAsync(gate.Id.Value, StudentTextSafety.Filter(request.Skill), request.Minutes, string.IsNullOrWhiteSpace(request.Reflection) ? null : StudentTextSafety.Filter(request.Reflection), ct); return Results.Created($"/api/student/practice/{id}", new { practiceLogId = id }); });
        student.MapGet("/goals", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetGoalsAsync(gate.Id.Value, ct)); });
        student.MapPost("/goals", async (GoalRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 160) return Results.ValidationProblem(new Dictionary<string, string[]> { ["title"] = ["A goal from 1 to 160 characters is required."] }); var id = await service.CreateGoalAsync(gate.Id.Value, request.Title, request.TargetDate, ct); return Results.Created($"/api/student/goals/{id}", new { goalId = id }); });
        student.MapPost("/goals/{goalId:long}/toggle", async (long goalId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(new { completed = await service.ToggleGoalAsync(gate.Id.Value, goalId, ct) }); });
        student.MapGet("/timeline", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok((await service.GetTimelineAsync(gate.Id.Value, ct)).Select(item => item with { Title = StudentTextSafety.Filter(item.Title), Description = StudentTextSafety.Filter(item.Description) })); });
        student.MapGet("/gold-stars", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetGoldStarEventsAsync(gate.Id.Value, ct)); });
        student.MapGet("/gold-stars/{eventId:long}", async (long eventId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var item = await service.GetGoldStarEventAsync(gate.Id.Value, eventId, ct); return item is null ? Results.NotFound() : Results.Ok(item); });
        student.MapGet("/news", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetNewsAsync(ct)); });
        student.MapGet("/news/{postId:long}", async (long postId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var item = await service.GetNewsItemAsync(postId, ct); return item is null ? Results.NotFound() : Results.Ok(item); });
    }

    private static async Task<StudentGate> RequireAcknowledgedStudent(StudentExperienceService service, HttpContext context, CancellationToken ct)
    {
        var id = await StudentAuth.GetStudentIdAsync(context); if (id is null) return new(null, Results.Unauthorized());
        var authResult = await context.AuthenticateAsync(StudentAuth.Scheme);
        var state = await service.GetStudentAccountStateAsync(id.Value, ct);
        if (state is null) return new(null, Results.Unauthorized());
        if (authResult.Principal?.HasClaim(StudentAuth.DisclaimerClaim, StudentAuth.DisclaimerVersion) != true) return new(null, Results.Json(new { title = "Profile acknowledgment required", disclaimerRequired = true }, statusCode: StatusCodes.Status428PreconditionRequired));
        return new(id, null);
    }

    private static readonly string[] BlockedGifTerms = ["nsfw", "porn", "xxx", "nude", "nudity", "sex", "onlyfans", "fetish"];

    private static async Task<IResult> FetchGifSnapAsync(IHttpClientFactory httpClientFactory, string? query, int page, CancellationToken ct)
    {
        var safePage = Math.Clamp(page, 1, 20);
        try
        {
            var client = httpClientFactory.CreateClient("gif-search");
            var endpoint = string.IsNullOrWhiteSpace(query) ? "trending" : $"search?q={Uri.EscapeDataString(query[..Math.Min(query.Length, 80)])}";
            var separator = endpoint.Contains('?') ? "&" : "?";
            using var response = await client.GetAsync($"https://gifsnap.com/api/v1/gifs/{endpoint}{separator}page={safePage}&limit=24", ct);
            if (!response.IsSuccessStatusCode) return Results.Ok(new { configured = true, items = Array.Empty<object>(), hasMore = false, nextPage = (int?)null });
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var items = document.RootElement.TryGetProperty("data", out var results)
                ? results.EnumerateArray().Where(IsUsableGif).Select(MapGif).Where(item => !string.IsNullOrWhiteSpace(item.url)).DistinctBy(item => item.url).ToArray()
                : Array.Empty<GifSearchItem>();
            var pagination = document.RootElement.TryGetProperty("pagination", out var pageInfo) ? pageInfo : default;
            var hasMore = pagination.ValueKind == JsonValueKind.Object && pagination.TryGetProperty("has_next", out var hasNext) && hasNext.GetBoolean();
            var nextPage = pagination.ValueKind == JsonValueKind.Object && pagination.TryGetProperty("next_page", out var next) && next.ValueKind == JsonValueKind.Number && next.TryGetInt32(out var nextNumber) ? nextNumber : (int?)null;
            return Results.Ok(new { configured = true, items, hasMore, nextPage });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch { return Results.Ok(new { configured = true, items = Array.Empty<object>(), hasMore = false, nextPage = (int?)null }); }
    }

    private static bool IsUsableGif(JsonElement item)
    {
        var title = item.TryGetProperty("title", out var titleElement) ? titleElement.GetString() ?? string.Empty : string.Empty;
        if (BlockedGifTerms.Any(term => title.Contains(term, StringComparison.OrdinalIgnoreCase))) return false;
        var width = item.TryGetProperty("width", out var widthElement) && widthElement.TryGetInt32(out var parsedWidth) ? parsedWidth : 0;
        var height = item.TryGetProperty("height", out var heightElement) && heightElement.TryGetInt32(out var parsedHeight) ? parsedHeight : 0;
        return width >= 100 && height >= 80;
    }

    private static GifSearchItem MapGif(JsonElement item)
    {
        var title = item.TryGetProperty("title", out var titleElement) ? titleElement.GetString() : null;
        var url = item.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : null;
        var preview = item.TryGetProperty("preview_url", out var previewElement) ? previewElement.GetString() : url;
        var id = item.TryGetProperty("id", out var idElement) ? idElement.GetString() : Guid.NewGuid().ToString("N");
        var source = item.TryGetProperty("source", out var sourceElement) ? sourceElement.GetString() : null;
        var width = item.TryGetProperty("width", out var widthElement) && widthElement.TryGetInt32(out var parsedWidth) ? parsedWidth : 0;
        var height = item.TryGetProperty("height", out var heightElement) && heightElement.TryGetInt32(out var parsedHeight) ? parsedHeight : 0;
        return new GifSearchItem(id ?? Guid.NewGuid().ToString("N"), string.IsNullOrWhiteSpace(title) ? "Dojo GIF" : title, url, preview, source, width, height);
    }

    private static Dictionary<string, string[]> ValidateEmergencyContact(EmergencyContactRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        var hasStructuredName = !string.IsNullOrWhiteSpace(request.FirstName) && !string.IsNullOrWhiteSpace(request.LastName);
        if (!hasStructuredName && string.IsNullOrWhiteSpace(request.Name)) errors["name"] = ["First name and last name are required."];
        if (request.FirstName?.Trim().Length > 80) errors["firstName"] = ["First names must be 80 characters or fewer."];
        if (request.MiddleInitial?.Trim().Length > 4) errors["middleInitial"] = ["Middle initials must be 4 characters or fewer."];
        if (request.LastName?.Trim().Length > 80) errors["lastName"] = ["Last names must be 80 characters or fewer."];
        if (request.Pronouns?.Trim().Length > 80) errors["pronouns"] = ["Pronouns must be 80 characters or fewer."];
        if (request.Name?.Trim().Length > 100) errors["name"] = ["Contact names must be 100 characters or fewer."];
        if (string.IsNullOrWhiteSpace(request.Relationship)) errors["relationship"] = ["A relationship is required."];
        else if (request.Relationship.Trim().Length > 80) errors["relationship"] = ["Relationships must be 80 characters or fewer."];
        if (request.Phone?.Length > 50) errors["phone"] = ["Phone numbers must be 50 characters or fewer."];
        if (request.Email?.Length > 254) errors["email"] = ["Email addresses must be 254 characters or fewer."];
        if (request.SmsOptIn && string.IsNullOrWhiteSpace(request.Phone)) errors["smsOptIn"] = ["A phone number is required to opt in to SMS updates."];
        return errors;
    }
}

public sealed record StudentGate(int? Id, IResult? Result);
public sealed record GifSearchItem(string id, string title, string? url, string? preview, string? source, int width, int height);

public sealed record StudentLoginRequest(string? Username, string? Pin, int? StudentId = null);
public sealed record DisclaimerRequest(bool Accepted, string? DisclaimerVersion = null);
public sealed record PublicCheckInRequest(int StudentId, string? Pin, bool Confirmed, bool Helper = false);
public sealed record StudentCheckInRequest(bool Helper = false);
public sealed record FriendResponseRequest(bool Accept);
public sealed record MessageRequest(int RecipientId, string Message);
public sealed record MessageReportRequest(string? Reason);
public sealed record ContentReportRequest(string? Reason);
public sealed record PostEditRequest(string Text);
public sealed record PostRequest(string Text, string PostKind = "training", string? LinkUrl = null, string? ImageData = null, string? ImageAlt = null);
public sealed record ReactionRequest(string Code);
public sealed record CommentRequest(string Text);
public sealed record PracticeLogRequest(string Skill, int Minutes, string? Reflection);
public sealed record GoalRequest(string Title, DateTime? TargetDate);
public sealed record StudentProfileUpdateRequest(string? DisplayName, string? Nickname, string? Pronouns, string? Bio, string? FavoriteTechnique, string? Email, string? Phone, string? UniformSize, string? BeltSize);
public sealed record EmergencyContactRequest(string? Name, string? Relationship, string? Phone, string? Email, string? FirstName = null, string? MiddleInitial = null, string? LastName = null, string? Pronouns = null, bool NewsletterOptIn = false, bool SmsOptIn = false);
public sealed record StudentPinChangeRequest(string CurrentPin, string NewPin, string ConfirmPin);
