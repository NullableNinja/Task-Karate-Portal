using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TaskKarate.Api.Services;

public static class StudentAuth
{
    public const string Scheme = "TaskKarateStudent";

    public static async Task<int?> GetStudentIdAsync(HttpContext context)
    {
        var result = await context.AuthenticateAsync(Scheme);
        return result.Succeeded && int.TryParse(result.Principal?.FindFirstValue("student_id"), out var id) ? id : null;
    }
}

public static class StudentExperienceEndpoints
{
    public static void MapStudentExperience(this WebApplication app)
    {
        var publicApi = app.MapGroup("/api/student/public");
        publicApi.MapGet("/schedule", async (StudentExperienceService service, DateTime? from, DateTime? to, CancellationToken ct) => Results.Ok(await service.GetScheduleAsync((from ?? DateTime.UtcNow.Date).Date, (to ?? DateTime.UtcNow.Date.AddDays(30)).Date.AddDays(1), ct)));
        publicApi.MapGet("/news", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetNewsAsync(ct)));

        var auth = app.MapGroup("/api/student/auth");
        auth.MapPost("/login", async (StudentLoginRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["credentials"] = ["Username and password are required."] });
            var account = await service.AuthenticateAsync(request.Username, request.Password, ct);
            if (account is null) return Results.Problem("Invalid student credentials.", statusCode: StatusCodes.Status401Unauthorized);
            var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("student_id", account.StudentId.ToString()), new Claim(ClaimTypes.Name, account.DisplayName)], StudentAuth.Scheme));
            await context.SignInAsync(StudentAuth.Scheme, principal, new AuthenticationProperties { IsPersistent = request.RememberMe, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
            return Results.Ok(new { authenticated = true, studentId = account.StudentId, displayName = account.DisplayName, rankName = account.RankName, disclaimerRequired = !await service.HasDisclaimerAsync(account.StudentId, ct) });
        });
        auth.MapGet("/me", async (StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var id = await StudentAuth.GetStudentIdAsync(context); if (id is null) return Results.Unauthorized();
            return Results.Ok(new { authenticated = true, studentId = id.Value, displayName = context.User.Identity?.Name, disclaimerRequired = !await service.HasDisclaimerAsync(id.Value, ct) });
        });
        auth.MapPost("/logout", async (HttpContext context) => { await context.SignOutAsync(StudentAuth.Scheme); return Results.NoContent(); });

        var student = app.MapGroup("/api/student");
        student.MapPost("/disclaimer", async (DisclaimerRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var id = await StudentAuth.GetStudentIdAsync(context); if (id is null) return Results.Unauthorized();
            if (!request.Accepted) return Results.BadRequest(new { title = "Acknowledgment required", detail = "Accept the profile privacy acknowledgment before opening a student profile." });
            await service.AcceptDisclaimerAsync(id.Value, ct); return Results.Ok(new { accepted = true });
        });

        student.MapGet("/profile", async (StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            var profile = await service.GetProfileAsync(gate.Id.Value, ct); return profile is null ? Results.NotFound() : Results.Ok(profile);
        });
        student.MapPut("/profile", async (StudentProfileUpdateRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            var errors = new Dictionary<string, string[]>();
            if (request.DisplayName?.Length > 100) errors["displayName"] = ["Display name must be 100 characters or fewer."];
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
        student.MapPost("/schedule/{sessionId:int}/check-in", async (int sessionId, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!;
            var result = await service.CheckInAsync(gate.Id.Value, sessionId, ct); if (result.Duplicate) return Results.Conflict(new { title = "Already checked in", detail = "This student already has attendance recorded for this class session." }); if (!result.Success) return Results.NotFound(new { title = "Class session unavailable" }); return Results.Ok(new { checkedIn = true });
        });
        student.MapGet("/social/search", async (string? q, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2) return Results.Ok(Array.Empty<StudentSummary>()); return Results.Ok(await service.SearchStudentsAsync(gate.Id.Value, q.Trim(), ct));
        });
        student.MapGet("/friends", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetFriendsAsync(gate.Id.Value, ct)); });
        student.MapPost("/friends/{friendId:int}/request", async (int friendId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var status = await service.RequestFriendAsync(gate.Id.Value, friendId, ct); return status == "invalid" ? Results.BadRequest() : status is "pending" ? Results.Accepted() : Results.Conflict(new { title = "Friend request already exists", status }); });
        student.MapPost("/friends/{friendId:int}/respond", async (int friendId, FriendResponseRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return await service.RespondToFriendAsync(gate.Id.Value, friendId, request.Accept, ct) ? Results.NoContent() : Results.NotFound(); });
        student.MapGet("/messages/{friendId:int}", async (int friendId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (!await service.AreFriendsAsync(gate.Id.Value, friendId, ct)) return Results.Forbid(); return Results.Ok(await service.GetMessagesAsync(gate.Id.Value, friendId, ct)); });
        student.MapPost("/messages", async (MessageRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (request.RecipientId <= 0 || string.IsNullOrWhiteSpace(request.Message) || request.Message.Trim().Length > 2000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["message"] = ["A message from 1 to 2,000 characters is required."] }); if (!await service.AreFriendsAsync(gate.Id.Value, request.RecipientId, ct)) return Results.Forbid(); var messageId = await service.SendMessageAsync(gate.Id.Value, request.RecipientId, request.Message, ct); return Results.Created($"/api/student/messages/{messageId}", new { messageId }); });
        student.MapGet("/feed", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetFeedAsync(gate.Id.Value, ct)); });
        student.MapPost("/feed", async (PostRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Trim().Length > 2000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["text"] = ["A post from 1 to 2,000 characters is required."] }); var postId = await service.CreatePostAsync(gate.Id.Value, request.Text, ct); return Results.Created($"/api/student/feed/{postId}", new { postId }); });
        student.MapGet("/bookmarks", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetBookmarkedPostIdsAsync(gate.Id.Value, ct)); });
        student.MapPost("/feed/{postId:long}/bookmark", async (long postId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var bookmarked = await service.ToggleBookmarkAsync(gate.Id.Value, postId, ct); return Results.Ok(new { bookmarked }); });
        student.MapPost("/feed/{postId:long}/reaction", async (long postId, ReactionRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (!await service.ToggleReactionAsync(gate.Id.Value, postId, request.Code, ct)) return Results.BadRequest(new { title = "Reaction unavailable" }); return Results.Ok(new { updated = true }); });
        student.MapGet("/feed/{postId:long}/comments", async (long postId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var comments = await service.GetCommentsAsync(gate.Id.Value, postId, ct); return comments is null ? Results.Forbid() : Results.Ok(comments); });
        student.MapPost("/feed/{postId:long}/comments", async (long postId, CommentRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Trim().Length > 1000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["text"] = ["A comment from 1 to 1,000 characters is required."] }); var commentId = await service.CreateCommentAsync(gate.Id.Value, postId, request.Text, ct); return commentId is null ? Results.Forbid() : Results.Created($"/api/student/feed/{postId}/comments/{commentId}", new { commentId }); });
        student.MapGet("/achievements", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetAchievementsAsync(gate.Id.Value, ct)); });
        student.MapGet("/missions", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetTrainingMissionsAsync(gate.Id.Value, ct)); });
        student.MapPost("/missions/{missionId:long}/toggle", async (long missionId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(new { completed = await service.ToggleTrainingMissionAsync(gate.Id.Value, missionId, ct) }); });
        student.MapGet("/practice", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetPracticeLogsAsync(gate.Id.Value, ct)); });
        student.MapPost("/practice", async (PracticeLogRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (string.IsNullOrWhiteSpace(request.Skill) || request.Skill.Trim().Length > 100 || request.Minutes is < 1 or > 240 || request.Reflection?.Length > 1000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["practice"] = ["Skill is required, minutes must be between 1 and 240, and reflection must be 1,000 characters or fewer."] }); var id = await service.CreatePracticeLogAsync(gate.Id.Value, request.Skill, request.Minutes, request.Reflection, ct); return Results.Created($"/api/student/practice/{id}", new { practiceLogId = id }); });
        student.MapGet("/goals", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetGoalsAsync(gate.Id.Value, ct)); });
        student.MapPost("/goals", async (GoalRequest request, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 160) return Results.ValidationProblem(new Dictionary<string, string[]> { ["title"] = ["A goal from 1 to 160 characters is required."] }); var id = await service.CreateGoalAsync(gate.Id.Value, request.Title, request.TargetDate, ct); return Results.Created($"/api/student/goals/{id}", new { goalId = id }); });
        student.MapPost("/goals/{goalId:long}/toggle", async (long goalId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(new { completed = await service.ToggleGoalAsync(gate.Id.Value, goalId, ct) }); });
        student.MapGet("/timeline", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetTimelineAsync(gate.Id.Value, ct)); });
        student.MapGet("/gold-stars", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetGoldStarEventsAsync(gate.Id.Value, ct)); });
        student.MapGet("/gold-stars/{eventId:long}", async (long eventId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var item = await service.GetGoldStarEventAsync(gate.Id.Value, eventId, ct); return item is null ? Results.NotFound() : Results.Ok(item); });
        student.MapPost("/gold-stars/{eventId:long}/interest", async (long eventId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(new { interested = await service.ToggleGoldStarInterestAsync(gate.Id.Value, eventId, ct) }); });
        student.MapGet("/news", async (StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; return Results.Ok(await service.GetNewsAsync(ct)); });
        student.MapGet("/news/{postId:long}", async (long postId, StudentExperienceService service, HttpContext context, CancellationToken ct) => { var gate = await RequireAcknowledgedStudent(service, context, ct); if (gate.Id is null) return gate.Result!; var item = await service.GetNewsItemAsync(postId, ct); return item is null ? Results.NotFound() : Results.Ok(item); });
    }

    private static async Task<StudentGate> RequireAcknowledgedStudent(StudentExperienceService service, HttpContext context, CancellationToken ct)
    {
        var id = await StudentAuth.GetStudentIdAsync(context); if (id is null) return new(null, Results.Unauthorized());
        if (!await service.HasDisclaimerAsync(id.Value, ct)) return new(null, Results.Json(new { title = "Profile acknowledgment required", disclaimerRequired = true }, statusCode: StatusCodes.Status428PreconditionRequired));
        return new(id, null);
    }
}

public sealed record StudentGate(int? Id, IResult? Result);

public sealed record StudentLoginRequest(string Username, string Password, bool RememberMe = false);
public sealed record DisclaimerRequest(bool Accepted);
public sealed record FriendResponseRequest(bool Accept);
public sealed record MessageRequest(int RecipientId, string Message);
public sealed record PostRequest(string Text);
public sealed record ReactionRequest(string Code);
public sealed record CommentRequest(string Text);
public sealed record PracticeLogRequest(string Skill, int Minutes, string? Reflection);
public sealed record GoalRequest(string Title, DateTime? TargetDate);
public sealed record StudentProfileUpdateRequest(string? DisplayName, string? Bio, string? FavoriteTechnique, string? Email, string? Phone, string? UniformSize, string? BeltSize);
