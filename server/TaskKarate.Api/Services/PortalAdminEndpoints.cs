namespace TaskKarate.Api.Services;

public static class PortalAdminEndpoints
{
    public static void MapPortalAdmin(this WebApplication app)
    {
        var admin = app.MapGroup("/api/portal-admin").RequireAuthorization("Staff");

        admin.MapGet("/students", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminStudentsAsync(ct)));
        admin.MapPost("/students", async (PortalStudentWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.AgeGroup)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["student"] = ["First name, last name, and age group are required."] });
            var id = await service.CreatePortalStudentAsync(request, ct); await audit.RecordAsync(context, "Create", "PortalStudent", Guid.Empty, new { legacyId = id }); return Results.Created($"/api/portal-admin/students/{id}", new { id });
        });
        admin.MapPut("/students/{studentId:int}", async (int studentId, PortalStudentWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.AgeGroup)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["student"] = ["Student details are invalid."] });
            if (!await service.UpdatePortalStudentAsync(studentId, request, ct)) return Results.NotFound(); await audit.RecordAsync(context, "Update", "PortalStudent", Guid.Empty, new { legacyId = studentId }); return Results.NoContent();
        });
        admin.MapPost("/students/{studentId:int}/active", async (int studentId, PortalActiveRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.SetPortalStudentActiveAsync(studentId, request.Active, ct)) return Results.NotFound(); await audit.RecordAsync(context, request.Active ? "Activate" : "Deactivate", "PortalStudent", Guid.Empty, new { legacyId = studentId }); return Results.NoContent();
        });
        admin.MapPost("/students/{studentId:int}/password", async (int studentId, PortalStudentPasswordRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            var errors = StudentPasswordPolicy.Validate(request.Password, request.ConfirmPassword);
            if (errors.Count > 0) return Results.ValidationProblem(errors.ToDictionary(item => item.Key, item => item.Value));
            var result = await service.ResetStudentPasswordAsync(studentId, request.Password, ct);
            if (result is null) return Results.NotFound(new { title = "Student account unavailable", detail = "The student must be active before a portal password can be assigned." });
            await audit.RecordAsync(context, "ResetPassword", "StudentAccount", Guid.Empty, new { legacyStudentId = studentId, username = result.Username, via = "administrator" });
            return Results.Ok(new { passwordChanged = true, username = result.Username });
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapGet("/attendance/students", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminStudentsAsync(ct)));
        admin.MapGet("/attendance/sessions", async (DateTime? date, StudentExperienceService service, CancellationToken ct) =>
        {
            var day = (date ?? DateTime.UtcNow).Date;
            return Results.Ok(await service.GetScheduleAsync(day, day.AddDays(1), ct));
        });
        admin.MapGet("/attendance", async (int sessionId, StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAttendanceAsync(sessionId, ct)));
        admin.MapPost("/attendance", async (PortalAttendanceRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (request.StudentId <= 0 || request.SessionId <= 0) return Results.ValidationProblem(new Dictionary<string, string[]> { ["attendance"] = ["Choose a real student and class session first."] });
            var result = await service.CheckInAsync(request.StudentId, request.SessionId, request.Helper, ct);
            if (result.Duplicate) return Results.Conflict(new { title = "Already checked in", detail = "This student already has attendance recorded for this class session." });
            if (!result.Success) return Results.ValidationProblem(new Dictionary<string, string[]> { ["attendance"] = [result.Error ?? "Choose a current, non-cancelled session and an eligible student."] });
            await audit.RecordAsync(context, "CheckIn", "PortalAttendance", Guid.Empty, new { request.StudentId, request.SessionId, request.Helper });
            return Results.Created($"/api/portal-admin/attendance?sessionId={request.SessionId}", new { checkedIn = true, helper = result.HelperRecorded });
        });
        admin.MapGet("/social/feed", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetStaffSocialFeedAsync(ct)));
        admin.MapPut("/social/posts/{postId:long}", async (long postId, StaffSocialPostRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Trim().Length > 2000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["text"] = ["Post text is required and must be 2,000 characters or fewer."] });
            if (!await service.UpdateStaffSocialPostAsync(postId, request.Text, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "Update", "SocialPost", Guid.Empty, new { postId }); return Results.NoContent();
        });
        admin.MapPost("/social/posts/{postId:long}/visibility", async (long postId, PortalActiveRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.SetStaffSocialPostVisibilityAsync(postId, request.Active, ct)) return Results.NotFound();
            await audit.RecordAsync(context, request.Active ? "Restore" : "Remove", "SocialPost", Guid.Empty, new { postId }); return Results.NoContent();
        });
        admin.MapGet("/social/posts/{postId:long}/comments", async (long postId, StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetStaffSocialCommentsAsync(postId, ct)));
        admin.MapPost("/social/comments/{commentId:long}/visibility", async (long commentId, PortalActiveRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.SetStaffSocialCommentVisibilityAsync(commentId, request.Active, ct)) return Results.NotFound();
            await audit.RecordAsync(context, request.Active ? "Restore" : "Remove", "SocialComment", Guid.Empty, new { commentId }); return Results.NoContent();
        });
        admin.MapGet("/guardians", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminGuardiansAsync(ct)));
        admin.MapGet("/guardian-students", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminGuardianStudentsAsync(ct)));
        admin.MapPost("/guardians", async (PortalGuardianWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["guardian"] = ["First name and last name are required."] });
            var id = await service.CreatePortalGuardianAsync(request, ct); await audit.RecordAsync(context, "Create", "PortalGuardian", Guid.Empty, new { legacyId = id, linkedStudents = request.StudentIds?.Count ?? 0 }); return Results.Created($"/api/portal-admin/guardians/{id}", new { id });
        });
        admin.MapPut("/guardians/{guardianId:int}", async (int guardianId, PortalGuardianWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["guardian"] = ["First name and last name are required."] });
            if (!await service.UpdatePortalGuardianAsync(guardianId, request, ct)) return Results.NotFound(); await audit.RecordAsync(context, "Update", "PortalGuardian", Guid.Empty, new { legacyId = guardianId, linkedStudents = request.StudentIds?.Count ?? 0 }); return Results.NoContent();
        });
        admin.MapGet("/gold-star-events", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminGoldStarEventsAsync(ct)));
        admin.MapPost("/gold-star-events", async (PortalGoldStarEventRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 160 || string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length > 2000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["event"] = ["A name and description are required."] });
            var id = await service.CreateGoldStarEventAsync(request.Name, request.Description, request.EventDate, ct);
            await audit.RecordAsync(context, "Create", "GoldStarEvent", Guid.Empty, new { legacyId = id, request.Name });
            return Results.Created($"/api/portal-admin/gold-star-events/{id}", new { id });
        });
        admin.MapPost("/gold-star-events/{eventId:long}/active", async (long eventId, PortalActiveRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.SetGoldStarEventActiveAsync(eventId, request.Active, ct)) return Results.NotFound();
            await audit.RecordAsync(context, request.Active ? "Activate" : "Deactivate", "GoldStarEvent", Guid.Empty, new { legacyId = eventId });
            return Results.NoContent();
        });
        admin.MapPost("/gold-star-events/{eventId:long}/award", async (long eventId, PortalAwardRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.AwardGoldStarAsync(request.StudentId, eventId, request.Note, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "Award", "GoldStar", Guid.Empty, new { request.StudentId, eventId });
            return Results.Ok(new { awarded = true });
        });
        admin.MapPost("/gold-star-events/{eventId:long}/remove", async (long eventId, PortalAwardRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.RemoveGoldStarAsync(request.StudentId, eventId, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "Remove", "GoldStar", Guid.Empty, new { request.StudentId, eventId });
            return Results.NoContent();
        });
        admin.MapGet("/achievements", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminAchievementsAsync(ct)));
        admin.MapPost("/milestones/recalculate", async (StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            var awarded = await service.RecalculateAutomaticMilestonesAsync(ct);
            await audit.RecordAsync(context, "Recalculate", "AutomaticMilestones", Guid.Empty, new { awarded });
            return Results.Ok(new { awarded });
        });
    }
}

public sealed record PortalGoldStarEventRequest(string Name, string Description, DateTime? EventDate);
public sealed record PortalActiveRequest(bool Active);
public sealed record PortalAwardRequest(int StudentId, string? Note);
public sealed record PortalStudentPasswordRequest(string Password, string ConfirmPassword);
public sealed record PortalAttendanceRequest(int StudentId, int SessionId, bool Helper = false);
public sealed record StaffSocialPostRequest(string Text);
