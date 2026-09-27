using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskKarate.Api.Data;
using TaskKarate.Api.Models;

namespace TaskKarate.Api.Services;

public static class PortalAdminEndpoints
{
    private static readonly string[] StaffRoles = ["Administrator", "Instructor", "Staff"];

    private static async Task<bool> HasDojoNewsAccessAsync(HttpContext context, UserManager<AppUser> users)
    {
        if (context.User.IsInRole("Administrator")) return true;
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var id)) return false;
        var user = await users.FindByIdAsync(id.ToString());
        return user?.IsActive == true && user.CanPublishDojoNews;
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

    public static void MapPortalAdmin(this WebApplication app)
    {
        var admin = app.MapGroup("/api/portal-admin").RequireAuthorization("Staff");

        admin.MapGet("/search", async (string? q, StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.SearchPortalAdminAsync(q ?? string.Empty, ct)));
        admin.MapGet("/check-ins/leaderboard", async (string? period, StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminDojoLeaderboardAsync(period, ct)));
        admin.MapGet("/check-ins/history", async (DateOnly? from, DateOnly? to, StudentExperienceService service, CancellationToken ct) =>
        {
            var end = to ?? DateOnly.FromDateTime(DateTime.Now).AddDays(1);
            var start = from ?? end.AddDays(-30);
            if (end <= start) return Results.ValidationProblem(new Dictionary<string, string[]> { ["range"] = ["The end date must be after the start date."] });
            if (end.DayNumber - start.DayNumber > 366) return Results.ValidationProblem(new Dictionary<string, string[]> { ["range"] = ["Check-in history is limited to one year per request."] });
            return Results.Ok(await service.GetPortalAdminCheckInsAsync(start, end, ct));
        });
        admin.MapGet("/students", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminStudentsAsync(false, ct)));
        admin.MapGet("/students/{studentId:int}/detail", async (int studentId, StudentExperienceService service, CancellationToken ct) =>
        {
            var profile = await service.GetProfileAsync(studentId, ct);
            if (profile is null) return Results.NotFound();
            var achievements = await service.GetAchievementsAsync(studentId, ct);
            var goals = await service.GetGoalsAsync(studentId, ct);
            var checkIns = await service.GetDojoCheckInSummaryAsync(studentId, ct);
            var memberships = await service.GetPortalAdminMembershipSnapshotsAsync(studentId, ct);
            var documents = await service.GetPortalAdminDocumentsAsync(studentId, ct);
            return Results.Ok(new { profile, achievements, goals, checkIns, memberships, documents });
        });
        admin.MapGet("/students/{studentId:int}/emergency-contacts", async (int studentId, StudentExperienceService service, CancellationToken ct) =>
        {
            var profile = await service.GetProfileAsync(studentId, ct);
            return profile is null ? Results.NotFound() : Results.Ok(profile.EmergencyContacts);
        });
        admin.MapPost("/students/{studentId:int}/emergency-contacts", async (int studentId, EmergencyContactRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            var errors = ValidateEmergencyContact(request);
            if (errors.Count > 0) return Results.ValidationProblem(errors);
            var profile = await service.GetProfileAsync(studentId, ct);
            if (profile is null) return Results.NotFound();
            var contact = await service.CreateEmergencyContactAsync(studentId, request, ct);
            await audit.RecordAsync(context, "Create", "EmergencyContact", Guid.Empty, new { studentId, contact.ContactId });
            return Results.Created($"/api/portal-admin/students/{studentId}/emergency-contacts/{contact.ContactId}", contact);
        });
        admin.MapPut("/students/{studentId:int}/emergency-contacts/{contactId:long}", async (int studentId, long contactId, EmergencyContactRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            var errors = ValidateEmergencyContact(request);
            if (errors.Count > 0) return Results.ValidationProblem(errors);
            var contact = await service.UpdateEmergencyContactAsync(studentId, contactId, request, ct);
            if (contact is null) return Results.NotFound();
            await audit.RecordAsync(context, "Update", "EmergencyContact", Guid.Empty, new { studentId, contactId });
            return Results.Ok(contact);
        });
        admin.MapDelete("/students/{studentId:int}/emergency-contacts/{contactId:long}", async (int studentId, long contactId, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.DeleteEmergencyContactAsync(studentId, contactId, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "Delete", "EmergencyContact", Guid.Empty, new { studentId, contactId });
            return Results.NoContent();
        });
        admin.MapPost("/students", async (PortalStudentWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.AgeGroup)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["student"] = ["First name, last name, and age group are required."] });
            if (request.Nickname?.Length > 100 || request.Pronouns?.Length > 50 || request.Honorific?.Length > 30) return Results.ValidationProblem(new Dictionary<string, string[]> { ["student"] = ["Nickname, pronouns, or honorific are too long."] });
            var id = await service.CreatePortalStudentAsync(request, ct); await audit.RecordAsync(context, "Create", "PortalStudent", Guid.Empty, new { legacyId = id }); return Results.Created($"/api/portal-admin/students/{id}", new { id });
        });
        admin.MapPut("/students/{studentId:int}", async (int studentId, PortalStudentWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.AgeGroup)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["student"] = ["Student details are invalid."] });
            if (request.Nickname?.Length > 100 || request.Pronouns?.Length > 50 || request.Honorific?.Length > 30) return Results.ValidationProblem(new Dictionary<string, string[]> { ["student"] = ["Nickname, pronouns, or honorific are too long."] });
            if (!await service.UpdatePortalStudentAsync(studentId, request, ct)) return Results.NotFound(); await audit.RecordAsync(context, "Update", "PortalStudent", Guid.Empty, new { legacyId = studentId }); return Results.NoContent();
        });
        admin.MapPost("/students/{studentId:int}/programs", async (int studentId, PortalStudentProgramWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (request.ProgramId <= 0 || string.IsNullOrWhiteSpace(request.ProgressionType)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["program"] = ["Choose a program and progression type."] });
            if (!await service.AddStudentProgramAsync(studentId, request, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "Enroll", "StudentProgram", Guid.Empty, new { legacyStudentId = studentId, request.ProgramId });
            return Results.NoContent();
        });
        admin.MapDelete("/students/{studentId:int}/programs/{programCode}", async (int studentId, string programCode, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.RemoveStudentProgramAsync(studentId, programCode, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "Remove", "StudentProgram", Guid.Empty, new { legacyStudentId = studentId, programCode });
            return Results.NoContent();
        });
        admin.MapPut("/students/{studentId:int}/roles", async (int studentId, PortalStudentRolesRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.UpdateStudentRolesAsync(studentId, request.Roles, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "Update", "StudentRoles", Guid.Empty, new { legacyStudentId = studentId, roles = request.Roles });
            return Results.NoContent();
        });
        admin.MapPost("/students/{studentId:int}/active", async (int studentId, PortalActiveRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.SetPortalStudentActiveAsync(studentId, request.Active, ct)) return Results.NotFound(); await audit.RecordAsync(context, request.Active ? "Activate" : "Deactivate", "PortalStudent", Guid.Empty, new { legacyId = studentId }); return Results.NoContent();
        });
        admin.MapPost("/students/{studentId:int}/status", async (int studentId, PortalStudentStatusRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (request.Status is not ("active" or "paused" or "deactivated")) return Results.ValidationProblem(new Dictionary<string, string[]> { ["status"] = ["Status must be active, paused, or deactivated."] });
            if (!await service.SetPortalStudentStatusAsync(studentId, request.Status, ct)) return Results.NotFound();
            await audit.RecordAsync(context, request.Status == "deactivated" ? "Deactivate" : "SetStatus", "PortalStudent", Guid.Empty, new { legacyId = studentId, request.Status });
            return Results.NoContent();
        });
        admin.MapPost("/students/{studentId:int}/pin", async (int studentId, PortalStudentPinRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            var errors = StudentPinPolicy.Validate(request.Pin, request.ConfirmPin);
            if (errors.Count > 0) return Results.ValidationProblem(errors.ToDictionary(item => item.Key, item => item.Value));
            var result = await service.ResetStudentPinAsync(studentId, request.Pin, ct);
            if (result is null) return Results.NotFound(new { title = "Student account unavailable", detail = "The student must be active before a student PIN can be assigned." });
            await audit.RecordAsync(context, "SetStudentPin", "StudentAccount", Guid.Empty, new { legacyStudentId = studentId, username = result.Username, via = "administrator" });
            return Results.Ok(new { pinChanged = true, username = result.Username });
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapGet("/attendance/students", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminStudentsAsync(true, ct)));
        admin.MapGet("/attendance/sessions", async (DateTime? date, StudentExperienceService service, CancellationToken ct) =>
        {
            var day = (date ?? DateTime.UtcNow).Date;
            return Results.Ok(await service.GetScheduleAsync(day, day.AddDays(1), ct));
        });
        admin.MapGet("/attendance", async (int sessionId, StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAttendanceAsync(sessionId, ct)));
        admin.MapGet("/attendance/history", async (DateOnly? from, DateOnly? to, int? studentId, StudentExperienceService service, CancellationToken ct) =>
        {
            var end = to ?? DateOnly.FromDateTime(DateTime.Now).AddDays(1);
            var start = from ?? end.AddDays(-30);
            if (end <= start) return Results.ValidationProblem(new Dictionary<string, string[]> { ["range"] = ["The end date must be after the start date."] });
            if (end.DayNumber - start.DayNumber > 366) return Results.ValidationProblem(new Dictionary<string, string[]> { ["range"] = ["Attendance history is limited to one year per request."] });
            return Results.Ok(await service.GetPortalAdminAttendanceHistoryAsync(start, end, studentId, ct));
        });
        admin.MapPost("/attendance", async (PortalAttendanceRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (request.StudentId <= 0 || request.SessionId <= 0) return Results.ValidationProblem(new Dictionary<string, string[]> { ["attendance"] = ["Choose a real student and class session first."] });
            var result = await service.CheckInAsync(request.StudentId, request.SessionId, request.Helper, ct);
            if (result.Duplicate) return Results.Conflict(new { title = "Already checked in", detail = "This student already has attendance recorded for this class session." });
            if (!result.Success) return Results.ValidationProblem(new Dictionary<string, string[]> { ["attendance"] = [result.Error ?? "Choose a current, non-cancelled session and an eligible student."] });
            await audit.RecordAsync(context, "CheckIn", "PortalAttendance", Guid.Empty, new { request.StudentId, request.SessionId, request.Helper });
            return Results.Created($"/api/portal-admin/attendance?sessionId={request.SessionId}", new { checkedIn = true, helper = result.HelperRecorded });
        });
        admin.MapPost("/attendance/{attendanceId:long}/status", async (long attendanceId, PortalAdminAttendanceUpdateRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (request.Status is not ("present" or "helper" or "absent" or "excused")) return Results.ValidationProblem(new Dictionary<string, string[]> { ["status"] = ["Status must be present, helper, absent, or excused."] });
            if (!await service.UpdatePortalAttendanceAsync(attendanceId, request.Status, request.Notes, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "CorrectAttendance", "PortalAttendance", Guid.Empty, new { attendanceId, request.Status, request.Notes });
            return Results.NoContent();
        });
        admin.MapDelete("/attendance/{attendanceId:long}", async (long attendanceId, [FromBody] PortalAttendanceRemovalRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Reason)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["reason"] = ["Give a reason before removing a student from the roster."] });
            if (!await service.RemovePortalAttendanceAsync(attendanceId, request.Reason, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "RemoveFromRoster", "PortalAttendance", Guid.Empty, new { attendanceId, request.Reason });
            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapGet("/reports/summary", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminReportSummaryAsync(ct)));
        admin.MapGet("/documents", async (int? studentId, StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminDocumentsAsync(studentId, ct)));
        admin.MapPost("/documents", async (PortalAdminDocumentWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Version) || string.IsNullOrWhiteSpace(request.Category)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["document"] = ["Name, version, and category are required."] });
            var id = await service.CreatePortalAdminDocumentAsync(request, ct);
            if (id is null) return Results.Conflict(new { title = "Document version already exists", detail = "Use a new version for a revised waiver or form." });
            await audit.RecordAsync(context, "Create", "PortalDocument", Guid.Empty, new { documentId = id, request.Name, request.Version });
            return Results.Created($"/api/portal-admin/documents/{id}", new { id });
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapPost("/documents/{documentId:long}/active", async (long documentId, PortalActiveRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.SetPortalAdminDocumentActiveAsync(documentId, request.Active, ct)) return Results.NotFound();
            await audit.RecordAsync(context, request.Active ? "Activate" : "Deactivate", "PortalDocument", Guid.Empty, new { documentId });
            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapPost("/students/{studentId:int}/documents/{documentId:long}/accept", async (int studentId, long documentId, PortalDocumentAcceptanceRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            var staffUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await service.AcceptPortalAdminDocumentAsync(documentId, studentId, request.GuardianId, staffUserId, request.Notes, ct)) return Results.NotFound(new { title = "Document or student unavailable", detail = "The document must be active and the student must exist before acceptance can be recorded." });
            await audit.RecordAsync(context, "AcceptDocument", "PortalDocument", Guid.Empty, new { documentId, studentId, request.GuardianId });
            return Results.NoContent();
        });
        admin.MapGet("/mystudio/membership-snapshots", async (int? studentId, StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminMembershipSnapshotsAsync(studentId, ct)));
        admin.MapPost("/mystudio/membership-snapshots", async (PortalAdminMembershipSnapshotWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (request.StudentId <= 0 || string.IsNullOrWhiteSpace(request.ExternalMembershipId) || string.IsNullOrWhiteSpace(request.MembershipName) || string.IsNullOrWhiteSpace(request.Status)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["membership"] = ["Student, external membership ID, membership name, and status are required."] });
            var id = await service.UpsertPortalAdminMembershipSnapshotAsync(request, ct);
            if (id is null) return Results.NotFound(new { title = "Student unavailable", detail = "The membership snapshot could not be linked to an existing student." });
            await audit.RecordAsync(context, "SyncMembershipSnapshot", "MyStudioMembership", Guid.Empty, new { request.StudentId, request.ExternalMembershipId, request.Status });
            return Results.Ok(new { snapshotId = id, sourceOfTruth = "MyStudio" });
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapGet("/helpers", async (DateTime? from, DateTime? to, StudentExperienceService service, HttpContext context, CancellationToken ct) =>
        {
            var start = (from ?? DateTime.UtcNow.Date).Date;
            var end = (to ?? start.AddDays(14)).Date;
            if (end <= start) end = start.AddDays(14);
            if (end > start.AddDays(60)) end = start.AddDays(60);
            var staffUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Results.Ok(await service.GetStaffHelperRosterAsync(start, end, staffUserId, ct));
        });
        admin.MapPost("/helpers/{sessionId:int}/signup", async (int sessionId, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            var staffUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(staffUserId)) return Results.Unauthorized();
            var displayName = context.User.Identity?.Name ?? "Staff volunteer";
            if (!await service.AddStaffHelperSignupAsync(sessionId, staffUserId, displayName, ct)) return Results.Conflict(new { title = "Helper signup already exists", detail = "You are already listed as a helper for this class, or the class is no longer available for signup." });
            await audit.RecordAsync(context, "Signup", "StaffHelper", Guid.Empty, new { sessionId });
            return Results.Ok(new { signedUp = true });
        });
        admin.MapDelete("/helpers/{sessionId:int}/signup", async (int sessionId, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            var staffUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(staffUserId)) return Results.Unauthorized();
            if (!await service.RemoveStaffHelperSignupAsync(sessionId, staffUserId, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "RemoveSignup", "StaffHelper", Guid.Empty, new { sessionId });
            return Results.NoContent();
        });
        admin.MapGet("/social/feed", async (string? q, int? offset, int? limit, StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetStaffSocialFeedAsync(q, offset ?? 0, limit ?? 25, ct)));
        admin.MapGet("/social/reports", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetStaffSocialReportsAsync(ct)));
        admin.MapPost("/social/reports/{reportId:long}/resolve", async (long reportId, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.ResolveStaffSocialReportAsync(reportId, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "Resolve", "SocialReport", Guid.Empty, new { reportId }); return Results.NoContent();
        });
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
            if (!string.IsNullOrWhiteSpace(request.Relationship) && request.Relationship.Trim().Length > 80) return Results.ValidationProblem(new Dictionary<string, string[]> { ["relationship"] = ["Relationships must be 80 characters or fewer."] });
            var id = await service.CreatePortalGuardianAsync(request, ct); await audit.RecordAsync(context, "Create", "PortalGuardian", Guid.Empty, new { legacyId = id, linkedStudents = request.StudentIds?.Count ?? 0 }); return Results.Created($"/api/portal-admin/guardians/{id}", new { id });
        });
        admin.MapPut("/guardians/{guardianId:int}", async (int guardianId, PortalGuardianWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["guardian"] = ["First name and last name are required."] });
            if (!string.IsNullOrWhiteSpace(request.Relationship) && request.Relationship.Trim().Length > 80) return Results.ValidationProblem(new Dictionary<string, string[]> { ["relationship"] = ["Relationships must be 80 characters or fewer."] });
            if (!await service.UpdatePortalGuardianAsync(guardianId, request, ct)) return Results.NotFound(); await audit.RecordAsync(context, "Update", "PortalGuardian", Guid.Empty, new { legacyId = guardianId, linkedStudents = request.StudentIds?.Count ?? 0 }); return Results.NoContent();
        });
        admin.MapGet("/programs", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalProgramsAsync(ct)));
        admin.MapPost("/class-templates", async (PortalClassTemplateWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (request.ClassType is not ("Class" or "Seminar" or "Private lesson" or "Belt testing")) return Results.ValidationProblem(new Dictionary<string, string[]> { ["classType"] = ["Choose Class, Seminar, Private lesson, or Belt testing."] });
            var id = await service.CreatePortalClassTemplateAsync(request, ct); if (id is null) return Results.ValidationProblem(new Dictionary<string, string[]> { ["template"] = ["Program, name, day, time, and duration must be valid."] });
            await audit.RecordAsync(context, "Create", "PortalClassTemplate", Guid.Empty, new { legacyId = id.Value, request.ClassType }); return Results.Created($"/api/portal-admin/class-templates/{id.Value}", new { id = id.Value });
        });
        admin.MapPut("/class-templates/{templateId:int}", async (int templateId, PortalClassTemplateWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (request.ClassType is not ("Class" or "Seminar" or "Private lesson" or "Belt testing")) return Results.ValidationProblem(new Dictionary<string, string[]> { ["classType"] = ["Choose Class, Seminar, Private lesson, or Belt testing."] });
            if (!await service.UpdatePortalClassTemplateAsync(templateId, request, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "Update", "PortalClassTemplate", Guid.Empty, new { legacyId = templateId, request.ClassType }); return Results.NoContent();
        });
        admin.MapDelete("/class-templates/{templateId:int}", async (int templateId, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.RetirePortalClassTemplateAsync(templateId, ct)) return Results.NotFound();
            await audit.RecordAsync(context, "Retire", "PortalClassTemplate", Guid.Empty, new { legacyId = templateId }); return Results.NoContent();
        });
        admin.MapPost("/class-templates/{templateId:int}/cancel-instance", async (int templateId, PortalTemplateInstanceCancellationRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Reason)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["reason"] = ["Give staff a reason for cancelling this class instance."] });
            var sessionId = await service.CancelPortalTemplateInstanceAsync(templateId, request.SessionDateUtc, request.Reason, ct); if (sessionId is null) return Results.NotFound();
            await audit.RecordAsync(context, "Cancel", "PortalClassSession", Guid.Empty, new { legacyId = sessionId.Value, templateId, request.SessionDateUtc, request.Reason }); return Results.Ok(new { sessionId = sessionId.Value, cancelled = true });
        });
        admin.MapGet("/sessions", async (DateTime? date, StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalSessionsAsync((date ?? DateTime.UtcNow).Date, ct)));
        admin.MapPost("/sessions", async (PortalClassSessionWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            var id = await service.CreatePortalSessionAsync(request, ct); if (id is null) return Results.Conflict(new { title = "Session could not be created", detail = "Check the template, date, assignment, or whether that class occurrence already exists." });
            await audit.RecordAsync(context, "Create", "PortalClassSession", Guid.Empty, new { legacyId = id.Value }); return Results.Created($"/api/portal-admin/sessions/{id.Value}", new { id = id.Value });
        });
        admin.MapPost("/sessions/{sessionId:int}/cancellation", async (int sessionId, PortalSessionCancellationRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (request.Cancelled && string.IsNullOrWhiteSpace(request.Reason)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["reason"] = ["Give staff a reason for cancelling this session."] });
            var updated = await service.SetPortalSessionCancellationAsync(sessionId, request.Cancelled, request.Reason, ct);
            if (updated is null) return Results.NotFound();
            await audit.RecordAsync(context, request.Cancelled ? "Cancel" : "Restore", "PortalClassSession", Guid.Empty, new { sessionId, request.Reason });
            return Results.Ok(new { sessionId, cancelled = request.Cancelled });
        });
        admin.MapGet("/news/access", async (UserManager<AppUser> users, HttpContext context) => Results.Ok(new { canManage = await HasDojoNewsAccessAsync(context, users) }));
        admin.MapPost("/staff-users", async (StaffUserCreateRequest request, UserManager<AppUser> users, HttpContext context, AuditService audit) =>
        {
            var requestedRoles = request.Roles.Where(role => !string.IsNullOrWhiteSpace(role)).Select(role => role.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.DisplayName) || string.IsNullOrWhiteSpace(request.Password) || requestedRoles.Length == 0 || requestedRoles.Any(role => !StaffRoles.Contains(role, StringComparer.OrdinalIgnoreCase))) return Results.ValidationProblem(new Dictionary<string, string[]> { ["staff"] = [$"Email, display name, password, and at least one valid staff role are required: {string.Join(", ", StaffRoles)}."] });
            var user = new AppUser { UserName = request.DisplayName.Trim(), Email = request.Email.Trim(), EmailConfirmed = true, IsActive = true };
            var create = await users.CreateAsync(user, request.Password);
            if (!create.Succeeded)
            {
                if (create.Errors.Any(error => error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase))) return Results.Conflict(new { title = "Staff account already exists", detail = "Use the existing account or choose another email address." });
                return Results.ValidationProblem(create.Errors.GroupBy(error => error.Code).ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));
            }
            var roles = await users.AddToRolesAsync(user, requestedRoles);
            if (!roles.Succeeded) { await users.DeleteAsync(user); return Results.Problem(string.Join("; ", roles.Errors.Select(item => item.Description)), statusCode: StatusCodes.Status500InternalServerError); }
            await audit.RecordAsync(context, "Create", "StaffUser", user.Id, new { user.Email, roles = requestedRoles });
            return Results.Created($"/api/portal-admin/staff-users/{user.Id}", new { userId = user.Id, email = user.Email, roles = requestedRoles });
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapGet("/staff-users", async (ApplicationDbContext db, UserManager<AppUser> users, CancellationToken ct) =>
        {
            var candidates = await db.Users.AsNoTracking().OrderBy(x => x.UserName).ToListAsync(ct);
            var list = new List<StaffAccessUser>();
            foreach (var user in candidates)
            {
                var roles = (await users.GetRolesAsync(user)).Where(role => StaffRoles.Contains(role, StringComparer.OrdinalIgnoreCase)).OrderBy(role => role).ToArray();
                if (roles.Length == 0) continue;
                list.Add(new StaffAccessUser(user.Id, user.UserName ?? user.Email ?? "Staff user", user.Email, user.IsActive, user.CanPublishDojoNews, roles));
            }
            return Results.Ok(list);
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapPut("/staff-users/{userId:guid}/news-access", async (Guid userId, StaffNewsAccessRequest request, UserManager<AppUser> users, HttpContext context, AuditService audit) =>
        {
            var user = await users.FindByIdAsync(userId.ToString());
            if (user is null) return Results.NotFound();
            var roles = await users.GetRolesAsync(user);
            if (!roles.Any(role => StaffRoles.Contains(role, StringComparer.OrdinalIgnoreCase))) return Results.NotFound();
            user.CanPublishDojoNews = request.Enabled;
            var result = await users.UpdateAsync(user);
            if (!result.Succeeded) return Results.Problem(string.Join("; ", result.Errors.Select(item => item.Description)), statusCode: StatusCodes.Status500InternalServerError);
            await audit.RecordAsync(context, request.Enabled ? "Grant" : "Revoke", "StaffDojoNewsPermission", userId);
            return Results.Ok(new { userId, canManage = user.CanPublishDojoNews });
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapPut("/staff-users/{userId:guid}/active", async (Guid userId, PortalActiveRequest request, UserManager<AppUser> users, HttpContext context, AuditService audit) =>
        {
            var currentUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.Equals(currentUserId, userId.ToString(), StringComparison.OrdinalIgnoreCase) && !request.Active) return Results.Conflict(new { title = "You cannot deactivate your own account", detail = "Another administrator must make that change." });
            var user = await users.FindByIdAsync(userId.ToString());
            if (user is null) return Results.NotFound();
            user.IsActive = request.Active;
            var result = await users.UpdateAsync(user);
            if (!result.Succeeded) return Results.Problem(string.Join("; ", result.Errors.Select(item => item.Description)), statusCode: StatusCodes.Status500InternalServerError);
            await audit.RecordAsync(context, request.Active ? "Activate" : "Deactivate", "StaffUser", userId);
            return Results.Ok(new { userId, isActive = user.IsActive });
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapPut("/staff-users/{userId:guid}/roles", async (Guid userId, StaffRoleUpdateRequest request, UserManager<AppUser> users, HttpContext context, AuditService audit) =>
        {
            var requestedRoles = request.Roles.Where(role => !string.IsNullOrWhiteSpace(role)).Select(role => role.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (requestedRoles.Length == 0 || requestedRoles.Any(role => !StaffRoles.Contains(role, StringComparer.OrdinalIgnoreCase))) return Results.ValidationProblem(new Dictionary<string, string[]> { ["roles"] = [$"Choose at least one valid staff role: {string.Join(", ", StaffRoles)}."] });
            var currentUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.Equals(currentUserId, userId.ToString(), StringComparison.OrdinalIgnoreCase) && !requestedRoles.Contains("Administrator", StringComparer.OrdinalIgnoreCase)) return Results.Conflict(new { title = "You cannot remove your own administrator role", detail = "Another administrator must make that change." });
            var user = await users.FindByIdAsync(userId.ToString());
            if (user is null) return Results.NotFound();
            var currentRoles = await users.GetRolesAsync(user);
            var currentStaffRoles = currentRoles.Where(role => StaffRoles.Contains(role, StringComparer.OrdinalIgnoreCase)).ToArray();
            var remove = currentStaffRoles.Where(role => !requestedRoles.Contains(role, StringComparer.OrdinalIgnoreCase)).ToArray();
            var add = requestedRoles.Where(role => !currentStaffRoles.Contains(role, StringComparer.OrdinalIgnoreCase)).ToArray();
            if (remove.Length > 0) { var result = await users.RemoveFromRolesAsync(user, remove); if (!result.Succeeded) return Results.Problem(string.Join("; ", result.Errors.Select(item => item.Description)), statusCode: StatusCodes.Status500InternalServerError); }
            if (add.Length > 0) { var result = await users.AddToRolesAsync(user, add); if (!result.Succeeded) return Results.Problem(string.Join("; ", result.Errors.Select(item => item.Description)), statusCode: StatusCodes.Status500InternalServerError); }
            await audit.RecordAsync(context, "SetRoles", "StaffUser", userId, new { roles = requestedRoles });
            return Results.Ok(new { userId, roles = requestedRoles });
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapGet("/announcements", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalContentAsync("announcement", true, ct)));
        admin.MapPost("/announcements", async (PortalContentWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["content"] = ["Title and body are required."] }); var id = await service.CreatePortalContentAsync("announcement", request, ct); await audit.RecordAsync(context, "Create", "PortalAnnouncement", Guid.Empty, new { legacyId = id }); return Results.Created($"/api/portal-admin/announcements/{id}", new { id });
        });
        admin.MapPost("/announcements/{contentId:long}/publish", async (long contentId, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.PublishPortalContentAsync("announcement", contentId, ct)) return Results.NotFound(); await audit.RecordAsync(context, "Publish", "PortalAnnouncement", Guid.Empty, new { legacyId = contentId }); return Results.NoContent();
        });
        admin.MapDelete("/announcements/{contentId:long}", async (long contentId, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.DeletePortalContentAsync("announcement", contentId, ct)) return Results.NotFound(); await audit.RecordAsync(context, "Delete", "PortalAnnouncement", Guid.Empty, new { legacyId = contentId }); return Results.NoContent();
        });
        admin.MapGet("/news", async (StudentExperienceService service, UserManager<AppUser> users, HttpContext context, CancellationToken ct) =>
        {
            if (!await HasDojoNewsAccessAsync(context, users)) return Results.Forbid();
            return Results.Ok(await service.GetPortalContentAsync("news", true, ct));
        });
        admin.MapPost("/news", async (PortalContentWriteRequest request, StudentExperienceService service, UserManager<AppUser> users, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await HasDojoNewsAccessAsync(context, users)) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["content"] = ["Title and body are required."] }); var id = await service.CreatePortalContentAsync("news", request, ct); await audit.RecordAsync(context, "Create", "PortalNews", Guid.Empty, new { legacyId = id }); return Results.Created($"/api/portal-admin/news/{id}", new { id });
        });
        admin.MapPost("/news/{contentId:long}/publish", async (long contentId, StudentExperienceService service, UserManager<AppUser> users, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await HasDojoNewsAccessAsync(context, users)) return Results.Forbid();
            if (!await service.PublishPortalContentAsync("news", contentId, ct)) return Results.NotFound(); await audit.RecordAsync(context, "Publish", "PortalNews", Guid.Empty, new { legacyId = contentId }); return Results.NoContent();
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
            if (!await service.AwardGoldStarAsync(request.StudentId, eventId, request.Note, request.EventDate, ct)) return Results.NotFound();
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
        admin.MapGet("/configuration/achievements", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminAchievementDefinitionsAsync(ct)));
        admin.MapPost("/configuration/achievements", async (PortalAdminAchievementWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 160 || string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length > 2000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["achievement"] = ["Name and description are required."] });
            try { var id = await service.CreatePortalAdminAchievementAsync(request, ct); await audit.RecordAsync(context, "Create", "AchievementDefinition", Guid.Empty, new { legacyId = id, request.Name }); return Results.Created($"/api/portal-admin/configuration/achievements/{id}", new { id }); }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19) { return Results.Conflict(new { title = "Achievement already exists" }); }
        });
        admin.MapPost("/configuration/achievements/{achievementId:long}/active", async (long achievementId, PortalActiveRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.SetPortalAdminAchievementActiveAsync(achievementId, request.Active, ct)) return Results.NotFound(); await audit.RecordAsync(context, request.Active ? "Activate" : "Deactivate", "AchievementDefinition", Guid.Empty, new { legacyId = achievementId }); return Results.NoContent();
        });
        admin.MapGet("/configuration/missions", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminMissionsAsync(ct)));
        admin.MapPost("/configuration/missions", async (PortalAdminMissionWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description) || string.IsNullOrWhiteSpace(request.Category)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["mission"] = ["Title, description, and category are required."] });
            try { var id = await service.CreatePortalAdminMissionAsync(request, ct); await audit.RecordAsync(context, "Create", "TrainingMission", Guid.Empty, new { legacyId = id, request.Title }); return Results.Created($"/api/portal-admin/configuration/missions/{id}", new { id }); }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19) { return Results.Conflict(new { title = "Mission already exists" }); }
        });
        admin.MapPost("/configuration/missions/{missionId:long}/active", async (long missionId, PortalActiveRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.SetPortalAdminMissionActiveAsync(missionId, request.Active, ct)) return Results.NotFound(); await audit.RecordAsync(context, request.Active ? "Activate" : "Deactivate", "TrainingMission", Guid.Empty, new { legacyId = missionId }); return Results.NoContent();
        });
        admin.MapGet("/configuration/ranks", async (StudentExperienceService service, CancellationToken ct) => Results.Ok(await service.GetPortalAdminRanksAsync(ct)));
        admin.MapPost("/configuration/ranks", async (PortalAdminRankWriteRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 120) return Results.ValidationProblem(new Dictionary<string, string[]> { ["rank"] = ["Rank name is required and must be 120 characters or fewer."] });
            try { var id = await service.CreatePortalAdminRankAsync(request, ct); await audit.RecordAsync(context, "Create", "RankDefinition", Guid.Empty, new { legacyId = id, request.Name }); return Results.Created($"/api/portal-admin/configuration/ranks/{id}", new { id }); }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19) { return Results.Conflict(new { title = "Rank already exists" }); }
        });
        admin.MapPost("/configuration/ranks/{rankId:int}/active", async (int rankId, PortalActiveRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (!await service.SetPortalAdminRankActiveAsync(rankId, request.Active, ct)) return Results.NotFound(); await audit.RecordAsync(context, request.Active ? "Activate" : "Deactivate", "RankDefinition", Guid.Empty, new { legacyId = rankId }); return Results.NoContent();
        });
        admin.MapPost("/students/{studentId:int}/rank", async (int studentId, PortalAdminStudentRankRequest request, StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            if (request.RankId < 0) return Results.ValidationProblem(new Dictionary<string, string[]> { ["rankId"] = ["Choose an active rank or leave the rank blank."] });
            if (!await service.AssignPortalAdminRankAsync(studentId, request, ct)) return Results.NotFound(); await audit.RecordAsync(context, "Assign", "StudentRank", Guid.Empty, new { legacyStudentId = studentId, request.RankId }); return Results.NoContent();
        });
        admin.MapGet("/audit", async (int? limit, ApplicationDbContext db, UserManager<AppUser> users, CancellationToken ct) =>
        {
            var take = Math.Clamp(limit ?? 100, 1, 500);
            var events = await db.AuditEvents.AsNoTracking().OrderByDescending(x => x.OccurredAtUtc).Take(take).ToListAsync(ct);
            var actorIds = events.Where(x => x.ActorUserId.HasValue).Select(x => x.ActorUserId!.Value).Distinct().ToArray();
            var actors = await db.Users.AsNoTracking().Where(x => actorIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Email ?? x.UserName ?? "Staff", ct);
            return Results.Ok(events.Select(item => new { item.Id, item.Action, item.Entity, item.EntityId, item.OccurredAtUtc, Actor = item.ActorUserId.HasValue && actors.TryGetValue(item.ActorUserId.Value, out var name) ? name : "System", item.MetadataJson }));
        }).RequireAuthorization(policy => policy.RequireRole("Administrator"));
        admin.MapPost("/milestones/recalculate", async (StudentExperienceService service, HttpContext context, AuditService audit, CancellationToken ct) =>
        {
            var awarded = await service.RecalculateAutomaticMilestonesAsync(ct);
            await audit.RecordAsync(context, "Recalculate", "AutomaticMilestones", Guid.Empty, new { awarded });
            return Results.Ok(new { awarded });
        });
    }
}

public sealed record StaffAccessUser(Guid UserId, string DisplayName, string? Email, bool IsActive, bool CanPublishDojoNews, IReadOnlyList<string> Roles);
public sealed record StaffNewsAccessRequest(bool Enabled);
public sealed record StaffRoleUpdateRequest(IReadOnlyList<string> Roles);
public sealed record StaffUserCreateRequest(string Email, string DisplayName, string Password, IReadOnlyList<string> Roles);

public sealed record PortalGoldStarEventRequest(string Name, string Description, DateTime? EventDate);
public sealed record PortalActiveRequest(bool Active);
public sealed record PortalStudentStatusRequest(string Status);
public sealed record PortalAwardRequest(int StudentId, string? Note, DateTime? EventDate = null);
public sealed record PortalStudentPinRequest(string Pin, string ConfirmPin);
public sealed record PortalAttendanceRequest(int StudentId, int SessionId, bool Helper = false);
public sealed record PortalDocumentAcceptanceRequest(int? GuardianId, string? Notes);
public sealed record StaffSocialPostRequest(string Text);
public sealed record PortalProgramWriteRequest(string Name, string? Description);
