using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace TaskKarate.Api.Services;

public sealed class StarterDatabaseOptions
{
    public string Path { get; set; } = string.Empty;
    public string? LegacySourcePath { get; set; }
    public string ContentRootPath { get; set; } = string.Empty;
    public bool ImportLegacySchedules { get; set; }
    public bool ImportDemoStudents { get; set; }
}

public sealed class StarterStudentAccount
{
    public int StudentId { get; init; }
}

public sealed record StudentAccount(int StudentId, string Username, string DisplayName, string? RankName, bool BirthdayWeek);
public sealed record StudentAccountState(bool HasPin, bool BirthdayWeek);
public sealed record StudentPinResetResult(string Username);
public sealed record StudentSummary(int StudentId, string DisplayName, string? RankName, string? ProfileImagePath);
public sealed record StudentDirectoryItem(int StudentId, string DisplayName, string? RankName, string? Is3LevelName, string? ProfileImagePath);
public sealed record ScheduleItem(int SessionId, DateTime SessionDate, string? StartTime, string? EndTime, string ClassName, string? Description, string? Location, bool Cancelled);
public sealed record PublicRosterItem(string StudentName);
public sealed record PortalAttendanceItem(long AttendanceId, int StudentId, string StudentName, DateTime CheckedInAtUtc, string? Notes, string Status);
public sealed record CheckInResult(bool Success, bool Duplicate, bool HelperRecorded, string? Error);
public sealed record DojoCheckInSummary(bool CheckedInToday, int TotalCheckIns, int CheckInsThisMonth, int CheckedInTodayCount, string? LastCheckInDate);
public sealed record DojoCheckInResult(bool Success, bool Duplicate, string? Error, DojoCheckInSummary Summary);
public sealed record DojoLeaderboardEntry(int Rank, int StudentId, string DisplayName, int CheckIns, bool IsCurrentStudent);
public sealed record DojoLeaderboard(string Period, string PeriodLabel, IReadOnlyList<DojoLeaderboardEntry> Entries);
public sealed record PreEnrollResult(bool Success, bool Duplicate, string? Error);
public sealed record GuardianItem(string Name, string Relationship, string? Phone, string? Email);
public sealed record EmergencyContactItem(long ContactId, string Name, string Relationship, string? Phone, string? Email, string? FirstName = null, string? MiddleInitial = null, string? LastName = null, string? Pronouns = null, bool NewsletterOptIn = false, bool SmsOptIn = false);
public sealed record ProgramMembershipItem(string ProgramName, string ProgramCode, string ProgressionType, string? LevelName, string? EnrolledDate);
public sealed record StudentProfile(int StudentId, string DisplayName, string? Nickname, string? Pronouns, string? Honorific, IReadOnlyList<string> Roles, string? Bio, string? FavoriteTechnique, string? ProfileImagePath, string? RankName, string? JoinDate, int TotalClasses, int ClassesThisMonth, int UnreadMessages, int AchievementCount, IReadOnlyList<string> AttendanceDates, int ClassesIntoStripe, int ClassesPerStripe, int ClassesToNextStripe, int StripesEarned, string NextMilestone, string? AgeGroup, DateTime? BirthDate, string? Email, string? Phone, string? UniformSize, string? BeltSize, IReadOnlyList<GuardianItem> Guardians, IReadOnlyList<EmergencyContactItem> EmergencyContacts, IReadOnlyList<ProgramMembershipItem> Programs, int HelperClasses, IReadOnlyList<string> PublicTileIds);
public sealed record FriendshipItem(int StudentId, string DisplayName, string? RankName, string Status, bool Incoming);
public sealed record MessageItem(long MessageId, int SenderId, string SenderName, int RecipientId, string RecipientName, string MessageText, DateTime CreatedAt, bool IsRead, bool IsPinned, IReadOnlyList<ReactionSummary> Reactions);
public sealed record AchievementItem(string Name, string? Description, string? IconName, DateTime? AwardedAt);
public sealed record EasterEggUnlockResult(bool Unlocked, bool CollectorUnlocked, AchievementItem Achievement, AchievementItem? CollectorAchievement, int DiscoveredCount);
public sealed record NewsItem(long Id, string Title, string Body, DateTime PublishedAt);
public sealed record ReactionSummary(string Code, string Label, string Icon, int Count, bool Selected);
public sealed record FeedItem(long PostId, int AuthorId, string AuthorName, string? RankName, string Text, string PostType, DateTime CreatedAt, IReadOnlyList<ReactionSummary> Reactions, int CommentCount, string PostKind, string? LinkUrl, string? ImageData, string? ImageAlt);
public sealed record CommentItem(long CommentId, int AuthorId, string AuthorName, string Text, DateTime CreatedAt);
public sealed record StaffSocialPost(long PostId, int AuthorId, string AuthorName, string Text, string PostType, string ModerationStatus, bool Visible, DateTime CreatedAt, DateTime UpdatedAt, int CommentCount);
public sealed record StaffSocialFeedPage(IReadOnlyList<StaffSocialPost> Posts, bool HasMore);
public sealed record StaffSocialComment(long CommentId, long PostId, int AuthorId, string AuthorName, string Text, bool Visible, string ModerationStatus, DateTime CreatedAt);
public sealed record PublicProfilePost(long PostId, string Text, string PostKind, DateTime CreatedAt, int ReactionCount, int CommentCount);
public sealed record PublicAchievement(string Name, string? Description, string? IconName, DateTime? AwardedAt);
public sealed record PublicProfileTile(string Id, string Label, string Value, string Description, string Icon, string Size, int? Progress);
public sealed record PublicStudentProfile(int StudentId, string DisplayName, string? Pronouns, string? Bio, string? FavoriteTechnique, string? ProfileImagePath, string? RankName, string? JoinDate, int TotalClasses, int ClassesThisMonth, int AchievementCount, int HelperClasses, int ClassesIntoStripe, int ClassesPerStripe, int ClassesToNextStripe, int StripesEarned, string NextMilestone, IReadOnlyList<ProgramMembershipItem> Programs, IReadOnlyList<PublicAchievement> Achievements, IReadOnlyList<PublicProfilePost> RecentPosts, IReadOnlyList<PublicProfileTile> FeaturedTiles);
public sealed record StudentProfileShowcaseRequest(IReadOnlyList<string>? TileIds, IReadOnlyDictionary<string, string>? TileSizes);
public sealed record StaffSocialReport(long ReportId, string ContentType, long ContentId, int ReporterId, string ReporterName, string SubjectAuthorName, string Text, string Reason, string Status, DateTime CreatedAt);
public sealed record TrainingMissionItem(long MissionId, string Title, string Description, string Category, bool Completed, DateTime? CompletedAt);
public sealed record DailyMissionItem(long DailyMissionId, string MissionKey, string Title, string Description, string Category, DateTime MissionDate, bool Completed, DateTime? CompletedAt);
public sealed record DailyMissionSeed(string MissionKey, string Title, string Description, string Category);
public sealed record DailyMissionSyncRequest(string Date, IReadOnlyList<DailyMissionSeed> Missions);
public sealed record TimelineItem(string Type, string Title, string Description, DateTime OccurredAt, string? Link);
public sealed record GoldStarEventItem(long EventId, string Name, string Description, DateTime? EventDate, bool Awarded, bool Interested);
public sealed record PracticeLogItem(long PracticeLogId, string Skill, int Minutes, string? Reflection, DateTime LoggedAt);
public sealed record GoalItem(long GoalId, string Title, DateTime? TargetDate, bool Completed, DateTime CreatedAt, DateTime? CompletedAt);
public sealed record PortalAdminStudent(
    int StudentId,
    string FirstName,
    string LastName,
    string DisplayName,
    string? Nickname,
    string? Pronouns,
    string? Honorific,
    IReadOnlyList<string> Roles,
    string? RankName,
    string? RankAwardedDate,
    string? RankNotes,
    string? AgeGroup,
    DateTime? BirthDate,
    bool IsActive,
    string? JoinDate,
    string? UniformSize,
    string? BeltSize,
    string? Bio,
    string? FavoriteTechnique,
    int TotalClasses,
    int HelperClasses,
    int AttendanceStreak,
    int AchievementCount,
    int GoldStarCount,
    IReadOnlyList<string> Programs,
    string Status);
public sealed record PortalAdminSearchResult(string Type, int Id, string Title, string Subtitle);
public sealed record PortalAdminCheckInItem(long CheckInId, int StudentId, string StudentName, string LocationName, DateTime CheckInDate, DateTime CheckedInAtUtc);
public sealed record PortalAdminAttendanceHistoryItem(long AttendanceId, int StudentId, string StudentName, int SessionId, DateTime SessionDate, string ClassName, DateTime CheckedInAtUtc, string Status, string? Notes);
public sealed record PortalAdminAttendanceUpdateRequest(string Status, string? Notes = null);
public sealed record PortalAttendanceRemovalRequest(string Reason);
public sealed record PortalAdminReportSummary(int ActiveStudents, int PausedStudents, int SessionsThisWeek, int AttendanceThisMonth, int CheckInsThisMonth, int OpenSocialReports);
public sealed record PortalAdminMembershipSnapshot(long SnapshotId, int StudentId, string StudentName, string? ExternalCustomerId, string ExternalMembershipId, string MembershipName, string? ProgramName, string Status, DateTime? StartsOn, DateTime? EndsOn, int? AttendanceLimit, DateTime SyncedAtUtc, string Source, string? Notes);
public sealed record PortalAdminMembershipSnapshotWriteRequest(int StudentId, string? ExternalCustomerId, string ExternalMembershipId, string MembershipName, string? ProgramName, string Status, DateTime? StartsOn, DateTime? EndsOn, int? AttendanceLimit, string? Notes);
public sealed record PortalAdminDocument(long DocumentId, string Name, string Version, string Category, string? Description, string? StorageKey, bool IsActive, bool RequiredForEnrollment, int AcceptanceCount, bool AcceptedForStudent, DateTime? AcceptedAtUtc);
public sealed record PortalAdminDocumentWriteRequest(string Name, string Version, string Category, string? Description, string? StorageKey, bool RequiredForEnrollment = false);
public sealed record PortalAdminGoldStarEvent(long EventId, string Name, string Description, DateTime? EventDate, bool IsActive, int AwardCount);
public sealed record PortalAdminAchievement(string Name, string Description, string IconName, int AwardCount);
public sealed record PortalAdminAchievementDefinition(long AchievementId, string Name, string Description, string IconName, bool IsActive, int AwardCount);
public sealed record PortalAdminMission(long MissionId, string Title, string Description, string Category, bool IsActive, int CompletionCount);
public sealed record PortalAdminRank(int RankId, string Name, int SortOrder, string? DisplayColor, bool IsActive, int StudentCount);
public sealed record PortalAdminAchievementWriteRequest(string Name, string Description, string IconName = "star");
public sealed record PortalAdminMissionWriteRequest(string Title, string Description, string Category);
public sealed record PortalAdminRankWriteRequest(string Name, string? DisplayColor = null);
public sealed record PortalAdminStudentRankRequest(int RankId, DateTime? AwardedDate = null, string? Notes = null);
public sealed record StaffHelperSignupItem(string StaffUserId, string DisplayName, DateTime SignedUpAt, bool IsCurrentStaff);
public sealed record StaffHelperRosterItem(int SessionId, DateTime SessionDate, string? StartTime, string? EndTime, string ClassName, string? Location, bool Cancelled, IReadOnlyList<StaffHelperSignupItem> StaffHelpers, IReadOnlyList<PortalAttendanceItem> StudentHelpers);
public sealed record PortalStudentWriteRequest(string FirstName, string LastName, string? PreferredName, string? Nickname, string? Pronouns, string? Honorific, string AgeGroup, DateTime? BirthDate, DateTime? JoinDate, string? UniformSize, string? BeltSize, string? Bio, string? FavoriteTechnique, IReadOnlyList<string>? Roles = null);
public sealed record PortalStudentProgramWriteRequest(int ProgramId, string ProgressionType = "belt", string? LevelName = null, DateTime? EnrolledDate = null);
public sealed record PortalStudentRolesRequest(IReadOnlyList<string> Roles);
public sealed record PortalAdminGuardianStudent(int StudentId, string Name, string Relationship);
public sealed record PortalAdminGuardian(int GuardianId, string FirstName, string LastName, string? Email, string? Phone, bool IsActive, IReadOnlyList<PortalAdminGuardianStudent> Students, string? MiddleInitial = null, string? Pronouns = null, bool NewsletterOptIn = false, bool SmsOptIn = false);
public sealed record PortalGuardianWriteRequest(string FirstName, string LastName, string? Email, string? Phone, IReadOnlyList<int>? StudentIds, string? Relationship = null, string? MiddleInitial = null, string? Pronouns = null, bool NewsletterOptIn = false, bool SmsOptIn = false);
public sealed record PortalProgramItem(int ProgramId, string Name, string? Description, IReadOnlyList<PortalClassTemplateItem> Templates);
public sealed record PortalClassScheduleItem(int ScheduleId, string DayOfWeek, string StartTime, int DurationMinutes);
public sealed record PortalClassTemplateItem(int TemplateId, string Name, string DayOfWeek, string StartTime, int DurationMinutes, string? BeltScope, string ClassType, bool AppointmentOnly, int ProgramId, IReadOnlyList<PortalClassScheduleItem>? Schedules = null);
public sealed record PortalClassSessionItem(int SessionId, int ClassTemplateId, string Name, string StartTime, int DurationMinutes, DateTime SessionDateUtc, bool IsCancelled, string? CancellationReason, string ClassType, int? AssignedStudentId, string? Location);
public sealed record PortalEnrollmentItem(int EnrollmentId, int StudentId, string StudentName, int ClassTemplateId, string ClassName, bool IsActive);
public sealed record PortalClassTemplateWriteRequest(int ProgramId, string Name, string DayOfWeek, string StartTime, int DurationMinutes, IReadOnlyList<string>? BeltScope, string ClassType = "Class", bool AppointmentOnly = false);
public sealed record PortalClassSessionWriteRequest(int ClassTemplateId, DateTime SessionDateUtc, string? Notes, int? AssignedStudentId = null);
public sealed record PortalTemplateInstanceCancellationRequest(DateTime SessionDateUtc, string Reason);
public sealed record PortalSessionCancellationRequest(bool Cancelled, string? Reason);
public sealed record PortalEnrollmentWriteRequest(int StudentId, int ClassTemplateId);
public sealed record PortalContentItem(long Id, string Title, string Body, string Status, DateTime? PublishedAtUtc, DateTime? ExpiresAtUtc, string Kind);
public sealed record PortalContentWriteRequest(string Title, string Body, DateTime? ExpiresAtUtc = null);

public sealed class StudentExperienceService
{
    private readonly StarterDatabaseOptions _options;
    private readonly IPasswordHasher<StarterStudentAccount> _passwordHasher;
    private readonly ILogger<StudentExperienceService> _logger;
    private readonly IConfiguration _configuration;
    private int _ready;

    public StudentExperienceService(IOptions<StarterDatabaseOptions> options, IPasswordHasher<StarterStudentAccount> passwordHasher, ILogger<StudentExperienceService> logger, IConfiguration configuration)
    {
        _options = options.Value;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _configuration = configuration;
    }

    public string DatabasePath => _options.Path;
    private string ConnectionString => $"Data Source={DatabasePath};Cache=Shared;Foreign Keys=True";

    public async Task EnsureReadyAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref _ready, 1, 1) == 1) return;
        var directory = System.IO.Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        await using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await ImportLegacySourceDatabaseAsync(connection, cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS users (user_id INTEGER PRIMARY KEY AUTOINCREMENT, username TEXT UNIQUE, password_hash TEXT, display_name TEXT NOT NULL, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS instructors (instructor_id INTEGER PRIMARY KEY AUTOINCREMENT, display_name TEXT NOT NULL, active INTEGER NOT NULL DEFAULT 1);
CREATE TABLE IF NOT EXISTS ranks (rank_id INTEGER PRIMARY KEY AUTOINCREMENT, rank_name TEXT NOT NULL UNIQUE, rank_order INTEGER NOT NULL UNIQUE, display_color TEXT, active INTEGER NOT NULL DEFAULT 1);
CREATE TABLE IF NOT EXISTS students (student_id INTEGER PRIMARY KEY AUTOINCREMENT, first_name TEXT NOT NULL, middle_name TEXT, last_name TEXT NOT NULL, preferred_name TEXT, birth_date TEXT, email TEXT, phone TEXT, start_date TEXT, active INTEGER NOT NULL DEFAULT 1, archived_at TEXT, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, roles TEXT);
CREATE TABLE IF NOT EXISTS guardians (guardian_id INTEGER PRIMARY KEY AUTOINCREMENT, first_name TEXT NOT NULL, middle_initial TEXT, last_name TEXT NOT NULL, pronouns TEXT, email TEXT, phone TEXT, newsletter_opt_in INTEGER NOT NULL DEFAULT 0, sms_opt_in INTEGER NOT NULL DEFAULT 0, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS student_guardians (student_id INTEGER NOT NULL, guardian_id INTEGER NOT NULL, relationship TEXT NOT NULL DEFAULT 'Guardian', is_primary INTEGER NOT NULL DEFAULT 0, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(student_id, guardian_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(guardian_id) REFERENCES guardians(guardian_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_emergency_contacts (contact_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, contact_name TEXT NOT NULL, first_name TEXT, middle_initial TEXT, last_name TEXT, pronouns TEXT, relationship TEXT NOT NULL, phone TEXT, email TEXT, newsletter_opt_in INTEGER NOT NULL DEFAULT 0, sms_opt_in INTEGER NOT NULL DEFAULT 0, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_program_memberships (student_program_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, program_code TEXT NOT NULL, program_name TEXT NOT NULL, progression_type TEXT NOT NULL, level_name TEXT, enrolled_date TEXT, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, UNIQUE(student_id, program_code), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_profiles (student_id INTEGER PRIMARY KEY, display_name TEXT, bio TEXT, profile_image_path TEXT, favorite_technique TEXT, public_tile_ids TEXT, public_tile_sizes TEXT, profile_visible INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_rank_history (student_rank_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, rank_id INTEGER NOT NULL, awarded_date TEXT NOT NULL, awarded_by_user_id INTEGER, notes TEXT, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(rank_id) REFERENCES ranks(rank_id));
CREATE TABLE IF NOT EXISTS classes (class_id INTEGER PRIMARY KEY AUTOINCREMENT, class_name TEXT NOT NULL, description TEXT, active INTEGER NOT NULL DEFAULT 1);
CREATE TABLE IF NOT EXISTS class_schedule (schedule_id INTEGER PRIMARY KEY AUTOINCREMENT, class_id INTEGER NOT NULL, day_of_week INTEGER NOT NULL, start_time TEXT NOT NULL, end_time TEXT, instructor_id INTEGER, location_name TEXT, active INTEGER NOT NULL DEFAULT 1, FOREIGN KEY(class_id) REFERENCES classes(class_id));
CREATE TABLE IF NOT EXISTS class_sessions (session_id INTEGER PRIMARY KEY AUTOINCREMENT, class_id INTEGER NOT NULL, schedule_id INTEGER, session_date TEXT NOT NULL, start_time TEXT, end_time TEXT, instructor_id INTEGER, location_name TEXT, cancelled INTEGER NOT NULL DEFAULT 0, cancellation_reason TEXT, notes TEXT, assigned_student_id INTEGER, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(class_id) REFERENCES classes(class_id), UNIQUE(class_id, session_date, start_time));
CREATE TABLE IF NOT EXISTS attendance (attendance_id INTEGER PRIMARY KEY AUTOINCREMENT, session_id INTEGER NOT NULL, student_id INTEGER NOT NULL, check_in_time TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, check_out_time TEXT, status TEXT NOT NULL DEFAULT 'present', checked_in_by_user_id INTEGER, notes TEXT, FOREIGN KEY(session_id) REFERENCES class_sessions(session_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, UNIQUE(session_id, student_id));
CREATE TABLE IF NOT EXISTS dojo_check_ins (check_in_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, location_name TEXT NOT NULL DEFAULT 'Main studio', check_in_date TEXT NOT NULL, checked_in_at TEXT NOT NULL, UNIQUE(student_id, check_in_date), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_class_signups (signup_id INTEGER PRIMARY KEY AUTOINCREMENT, session_id INTEGER NOT NULL, student_id INTEGER NOT NULL, signed_up_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, status TEXT NOT NULL DEFAULT 'reserved', FOREIGN KEY(session_id) REFERENCES class_sessions(session_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, UNIQUE(session_id, student_id));
CREATE TABLE IF NOT EXISTS achievements (achievement_id INTEGER PRIMARY KEY AUTOINCREMENT, achievement_name TEXT NOT NULL UNIQUE, description TEXT, icon_name TEXT, active INTEGER NOT NULL DEFAULT 1);
CREATE TABLE IF NOT EXISTS student_achievements (student_id INTEGER NOT NULL, achievement_id INTEGER NOT NULL, awarded_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(student_id, achievement_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(achievement_id) REFERENCES achievements(achievement_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS gold_star_events (event_id INTEGER PRIMARY KEY AUTOINCREMENT, event_name TEXT NOT NULL UNIQUE, description TEXT NOT NULL, event_date TEXT, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS student_gold_stars (student_id INTEGER NOT NULL, event_id INTEGER NOT NULL, awarded_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, note TEXT, PRIMARY KEY(student_id, event_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(event_id) REFERENCES gold_star_events(event_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_event_interests (student_id INTEGER NOT NULL, event_id INTEGER NOT NULL, status TEXT NOT NULL DEFAULT 'interested', created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(student_id, event_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(event_id) REFERENCES gold_star_events(event_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS training_missions (mission_id INTEGER PRIMARY KEY AUTOINCREMENT, title TEXT NOT NULL UNIQUE, description TEXT NOT NULL, category TEXT NOT NULL, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS student_mission_progress (student_id INTEGER NOT NULL, mission_id INTEGER NOT NULL, completed INTEGER NOT NULL DEFAULT 0, completed_at TEXT, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(student_id, mission_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(mission_id) REFERENCES training_missions(mission_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_daily_missions (daily_mission_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, mission_date TEXT NOT NULL, mission_key TEXT NOT NULL, title TEXT NOT NULL, description TEXT NOT NULL, category TEXT NOT NULL, completed INTEGER NOT NULL DEFAULT 0, completed_at TEXT, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, UNIQUE(student_id, mission_date, mission_key), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS posts (post_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER, user_id INTEGER, post_text TEXT NOT NULL, post_type TEXT NOT NULL DEFAULT 'student', post_kind TEXT NOT NULL DEFAULT 'training', link_url TEXT, image_data TEXT, image_alt TEXT, moderation_status TEXT NOT NULL DEFAULT 'approved', visible INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS student_accounts (student_id INTEGER PRIMARY KEY, username TEXT NOT NULL UNIQUE COLLATE NOCASE, password_hash TEXT NOT NULL, pin_hash TEXT, must_change_password INTEGER NOT NULL DEFAULT 0, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_disclaimer_acceptances (acceptance_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, disclaimer_version TEXT NOT NULL, accepted_at TEXT NOT NULL, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_friendships (student_id INTEGER NOT NULL, friend_id INTEGER NOT NULL, status TEXT NOT NULL, requested_by INTEGER NOT NULL, created_at TEXT NOT NULL, updated_at TEXT NOT NULL, PRIMARY KEY(student_id, friend_id), CHECK(student_id <> friend_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(friend_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_messages (message_id INTEGER PRIMARY KEY AUTOINCREMENT, sender_id INTEGER NOT NULL, recipient_id INTEGER NOT NULL, message_text TEXT NOT NULL, created_at TEXT NOT NULL, read_at TEXT, FOREIGN KEY(sender_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(recipient_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_message_reports (message_id INTEGER NOT NULL, reporter_id INTEGER NOT NULL, reason TEXT NOT NULL DEFAULT 'safety', created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(message_id, reporter_id), FOREIGN KEY(message_id) REFERENCES student_messages(message_id) ON DELETE CASCADE, FOREIGN KEY(reporter_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_content_reports (report_id INTEGER PRIMARY KEY AUTOINCREMENT, content_type TEXT NOT NULL, content_id INTEGER NOT NULL, reporter_id INTEGER NOT NULL, reason TEXT NOT NULL DEFAULT 'safety', status TEXT NOT NULL DEFAULT 'open', created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, resolved_at TEXT, UNIQUE(content_type, content_id, reporter_id), FOREIGN KEY(reporter_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS admin_alerts (alert_id INTEGER PRIMARY KEY AUTOINCREMENT, alert_type TEXT NOT NULL, title TEXT NOT NULL, body TEXT NOT NULL, entity_type TEXT NOT NULL, entity_id INTEGER NOT NULL, status TEXT NOT NULL DEFAULT 'open', created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, resolved_at TEXT);
CREATE TABLE IF NOT EXISTS student_message_reactions (message_id INTEGER NOT NULL, student_id INTEGER NOT NULL, reaction_code TEXT NOT NULL, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(message_id, student_id), CHECK(reaction_code IN ('fist_bump', 'respect', 'fire', 'heart', 'clap')), FOREIGN KEY(message_id) REFERENCES student_messages(message_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_message_pins (message_id INTEGER NOT NULL, student_id INTEGER NOT NULL, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(message_id, student_id), FOREIGN KEY(message_id) REFERENCES student_messages(message_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS post_reactions (post_id INTEGER NOT NULL, student_id INTEGER NOT NULL, reaction_code TEXT NOT NULL, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(post_id, student_id), CHECK(reaction_code IN ('fist_bump', 'respect', 'fire')), FOREIGN KEY(post_id) REFERENCES posts(post_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS post_comments (comment_id INTEGER PRIMARY KEY AUTOINCREMENT, post_id INTEGER NOT NULL, student_id INTEGER NOT NULL, comment_text TEXT NOT NULL, visible INTEGER NOT NULL DEFAULT 1, moderation_status TEXT NOT NULL DEFAULT 'approved', created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(post_id) REFERENCES posts(post_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_practice_logs (practice_log_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, skill TEXT NOT NULL, minutes INTEGER NOT NULL, reflection TEXT, logged_at TEXT NOT NULL, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_goals (goal_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, title TEXT NOT NULL, target_date TEXT, completed INTEGER NOT NULL DEFAULT 0, created_at TEXT NOT NULL, completed_at TEXT, UNIQUE(student_id, title), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS post_bookmarks (post_id INTEGER NOT NULL, student_id INTEGER NOT NULL, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(post_id, student_id), FOREIGN KEY(post_id) REFERENCES posts(post_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS staff_helper_signups (signup_id INTEGER PRIMARY KEY AUTOINCREMENT, session_id INTEGER NOT NULL, staff_user_id TEXT NOT NULL, staff_display_name TEXT NOT NULL, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, UNIQUE(session_id, staff_user_id), FOREIGN KEY(session_id) REFERENCES class_sessions(session_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS portal_programs (program_id INTEGER PRIMARY KEY AUTOINCREMENT, program_name TEXT NOT NULL UNIQUE COLLATE NOCASE, description TEXT, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS portal_class_templates (template_id INTEGER PRIMARY KEY AUTOINCREMENT, program_id INTEGER NOT NULL, class_id INTEGER NOT NULL UNIQUE, template_name TEXT NOT NULL, day_of_week INTEGER NOT NULL, start_time TEXT NOT NULL, duration_minutes INTEGER NOT NULL, belt_scope TEXT, class_type TEXT NOT NULL DEFAULT 'Class', appointment_only INTEGER NOT NULL DEFAULT 0, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(program_id) REFERENCES portal_programs(program_id), FOREIGN KEY(class_id) REFERENCES classes(class_id));
CREATE TABLE IF NOT EXISTS portal_enrollments (enrollment_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, template_id INTEGER NOT NULL, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, UNIQUE(student_id, template_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(template_id) REFERENCES portal_class_templates(template_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS portal_content (content_id INTEGER PRIMARY KEY AUTOINCREMENT, kind TEXT NOT NULL, title TEXT NOT NULL, body TEXT NOT NULL, status TEXT NOT NULL DEFAULT 'Draft', published_at TEXT, expires_at TEXT, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, CHECK(kind IN ('news', 'announcement')));
CREATE TABLE IF NOT EXISTS mystudio_membership_snapshots (snapshot_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, external_customer_id TEXT, external_membership_id TEXT NOT NULL, membership_name TEXT NOT NULL, program_name TEXT, status TEXT NOT NULL, starts_on TEXT, ends_on TEXT, attendance_limit INTEGER, synced_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, source TEXT NOT NULL DEFAULT 'MyStudio', notes TEXT, UNIQUE(student_id, external_membership_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS portal_documents (document_id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, version TEXT NOT NULL, category TEXT NOT NULL DEFAULT 'Waiver', description TEXT, storage_key TEXT, active INTEGER NOT NULL DEFAULT 1, required_for_enrollment INTEGER NOT NULL DEFAULT 0, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, UNIQUE(name, version));
CREATE TABLE IF NOT EXISTS portal_document_acceptances (acceptance_id INTEGER PRIMARY KEY AUTOINCREMENT, document_id INTEGER NOT NULL, student_id INTEGER NOT NULL, guardian_id INTEGER, accepted_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, accepted_by_staff_user_id TEXT, acceptance_method TEXT NOT NULL DEFAULT 'Staff', notes TEXT, UNIQUE(document_id, student_id), FOREIGN KEY(document_id) REFERENCES portal_documents(document_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(guardian_id) REFERENCES guardians(guardian_id) ON DELETE SET NULL);
INSERT OR IGNORE INTO achievements(achievement_name, description, icon_name) VALUES
    ('First Studio Check-in', 'You checked in at the studio for the first time.', 'dojo-checkin-one'),
    ('5 Studio Check-ins', 'Five studio check-ins. You are building a habit.', 'dojo-checkin-five'),
    ('25 Studio Check-ins', 'Twenty-five studio check-ins. The dojo knows your HIYAH!.', 'dojo-checkin-twenty-five'),
    ('100 Studio Check-ins', 'One hundred studio check-ins. That is serious dojo consistency.', 'dojo-checkin-hundred'),
    ('Bell Finder', 'You found the hidden dojo bell.', 'bell'),
    ('Belt Whisperer', 'You made the belt colors orbit.', 'belt'),
    ('Scroll of the Long Way', 'You followed the progress trail all the way to black belt.', 'scroll'),
    ('Dojo Explorer', 'You visited every Student Hub door in one session.', 'compass'),
    ('Secret Kata', 'You discovered the reverse navigation pattern.', 'kata'),
    ('Volume Control', 'You found the hidden HIYAH! overdrive.', 'speaker'),
    ('Sticker Sensei', 'You opened the hidden dojo sticker drawer.', 'sticker'),
    ('Card-Carrying Student', 'You flipped your Student Hub identity card.', 'card'),
    ('Wall Wisdom', 'You activated the dojo motto machine.', 'wisdom'),
    ('Master of Hidden Dojos', 'You discovered every Student Hub easter egg.', 'hidden-dojo');
CREATE INDEX IF NOT EXISTS ix_student_accounts_username ON student_accounts(username);
CREATE INDEX IF NOT EXISTS ix_class_sessions_date ON class_sessions(session_date, cancelled);
CREATE INDEX IF NOT EXISTS ix_attendance_student ON attendance(student_id, session_id);
CREATE INDEX IF NOT EXISTS ix_dojo_check_ins_student_date ON dojo_check_ins(student_id, check_in_date DESC);
CREATE INDEX IF NOT EXISTS ix_student_emergency_contacts_student ON student_emergency_contacts(student_id, contact_id);
CREATE INDEX IF NOT EXISTS ix_student_class_signups_session ON student_class_signups(session_id, student_id);
CREATE INDEX IF NOT EXISTS ix_messages_thread ON student_messages(sender_id, recipient_id, created_at);
CREATE INDEX IF NOT EXISTS ix_posts_news ON posts(post_type, visible, moderation_status, created_at);
CREATE INDEX IF NOT EXISTS ix_post_reactions_post ON post_reactions(post_id);
CREATE INDEX IF NOT EXISTS ix_post_comments_post ON post_comments(post_id, created_at);
CREATE INDEX IF NOT EXISTS ix_timeline_attendance ON attendance(student_id, check_in_time);
CREATE INDEX IF NOT EXISTS ix_practice_logs_student ON student_practice_logs(student_id, logged_at);
CREATE INDEX IF NOT EXISTS ix_goals_student ON student_goals(student_id, completed, target_date);
CREATE INDEX IF NOT EXISTS ix_daily_missions_student_date ON student_daily_missions(student_id, mission_date, completed);
CREATE INDEX IF NOT EXISTS ix_post_bookmarks_student ON post_bookmarks(student_id, created_at);
CREATE INDEX IF NOT EXISTS ix_staff_helper_signups_session ON staff_helper_signups(session_id, created_at);
CREATE INDEX IF NOT EXISTS ix_portal_class_templates_program ON portal_class_templates(program_id, active);
CREATE INDEX IF NOT EXISTS ix_portal_enrollments_template ON portal_enrollments(template_id, active);
CREATE INDEX IF NOT EXISTS ix_portal_content_kind_status ON portal_content(kind, status, published_at);
CREATE INDEX IF NOT EXISTS ix_mystudio_membership_student ON mystudio_membership_snapshots(student_id, status, ends_on);
CREATE INDEX IF NOT EXISTS ix_portal_document_acceptances_student ON portal_document_acceptances(student_id, document_id);
";
        await command.ExecuteNonQueryAsync(cancellationToken);
        await EnsureLegacyStudentColumnsAsync(connection, cancellationToken);
        await EnsureLegacyContactColumnsAsync(connection, cancellationToken);
        await EnsureStudentProfileColumnsAsync(connection, cancellationToken);
        await EnsureLegacyClassSessionColumnsAsync(connection, cancellationToken);
        await EnsureStudentAccountColumnsAsync(connection, cancellationToken);
        await EnsureLegacyReactionSchemaAsync(connection, cancellationToken);
        await EnsurePostColumnsAsync(connection, cancellationToken);
        await EnsureGoldStarColumnsAsync(connection, cancellationToken);
        await ImportDevelopmentDataAsync(connection, cancellationToken);
        await NormalizeLegacyDataAsync(connection, cancellationToken);
        await EnsureCanonicalClassCatalogAsync(connection, cancellationToken);
        await SeedDevelopmentGoldStarEventsAsync(connection, cancellationToken);
        await MaterializeUpcomingSessionsAsync(connection, cancellationToken);
        await SeedDemoDataAsync(connection, cancellationToken);
        await EnsureBootstrapAccountAsync(connection, cancellationToken);
        Interlocked.Exchange(ref _ready, 1);
    }

    private async Task ImportLegacySourceDatabaseAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.LegacySourcePath) || !File.Exists(_options.LegacySourcePath)) return;

        var sourcePath = Path.GetFullPath(_options.LegacySourcePath);
        var targetPath = Path.GetFullPath(DatabasePath);
        if (string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase)) return;

        await using var attach = connection.CreateCommand();
        attach.CommandText = "ATTACH DATABASE $path AS legacy_source";
        attach.Parameters.AddWithValue("$path", sourcePath);
        await attach.ExecuteNonQueryAsync(cancellationToken);

        try
        {
            await using (var disableForeignKeys = connection.CreateCommand())
            {
                disableForeignKeys.CommandText = "PRAGMA foreign_keys = OFF";
                await disableForeignKeys.ExecuteNonQueryAsync(cancellationToken);
            }

            var tables = new List<(string Name, string Sql)>();
            await using (var list = connection.CreateCommand())
            {
                list.CommandText = "SELECT name, sql FROM legacy_source.sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND sql IS NOT NULL ORDER BY name";
                await using var reader = await list.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken)) tables.Add((reader.GetString(0), reader.GetString(1)));
            }

            foreach (var table in tables)
            {
                var createSql = table.Sql.Replace("CREATE TABLE ", "CREATE TABLE IF NOT EXISTS ", StringComparison.OrdinalIgnoreCase);
                await using var create = connection.CreateCommand();
                create.CommandText = createSql;
                await create.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var table in tables)
            {
                var name = QuoteIdentifier(table.Name);
                await using var copy = connection.CreateCommand();
                copy.CommandText = $"INSERT OR IGNORE INTO {name} SELECT * FROM legacy_source.{name}";
                await copy.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        finally
        {
            await using (var enableForeignKeys = connection.CreateCommand())
            {
                enableForeignKeys.CommandText = "PRAGMA foreign_keys = ON";
                await enableForeignKeys.ExecuteNonQueryAsync(cancellationToken);
            }

            await using var detach = connection.CreateCommand();
            detach.CommandText = "DETACH DATABASE legacy_source";
            await detach.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static string QuoteIdentifier(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";

    private static async Task NormalizeLegacyDataAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
DELETE FROM student_program_memberships
WHERE program_code = 'is3'
  AND student_id IN (SELECT student_id FROM students WHERE lower(replace(age_group, ' ', '')) NOT IN ('teensadults', 'teenadults', 'adult'));
DELETE FROM student_guardians
WHERE guardian_id IN (
    SELECT duplicate.guardian_id
    FROM guardians duplicate
    JOIN guardians canonical ON canonical.guardian_id < duplicate.guardian_id
        AND lower(trim(canonical.first_name)) = lower(trim(duplicate.first_name))
        AND lower(trim(canonical.last_name)) = lower(trim(duplicate.last_name))
        AND coalesce(canonical.email, '') = coalesce(duplicate.email, '')
        AND coalesce(canonical.phone, '') = coalesce(duplicate.phone, '')
);
DELETE FROM guardians
WHERE guardian_id IN (
    SELECT duplicate.guardian_id
    FROM guardians duplicate
    JOIN guardians canonical ON canonical.guardian_id < duplicate.guardian_id
        AND lower(trim(canonical.first_name)) = lower(trim(duplicate.first_name))
        AND lower(trim(canonical.last_name)) = lower(trim(duplicate.last_name))
        AND coalesce(canonical.email, '') = coalesce(duplicate.email, '')
        AND coalesce(canonical.phone, '') = coalesce(duplicate.phone, '')
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_guardians_identity ON guardians(first_name, last_name, email, phone);
";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureLegacyReactionSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var columns = connection.CreateCommand();
        columns.CommandText = "PRAGMA table_info(post_reactions)";
        var hasReactionCode = false;
        await using (var reader = await columns.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                if (string.Equals(reader.GetString(1), "reaction_code", StringComparison.OrdinalIgnoreCase)) hasReactionCode = true;
            }
        }
        if (hasReactionCode) return;

        await using var migrate = connection.CreateCommand();
        migrate.CommandText = @"
ALTER TABLE post_reactions RENAME TO post_reactions_legacy;
CREATE TABLE post_reactions (post_id INTEGER NOT NULL, student_id INTEGER NOT NULL, reaction_code TEXT NOT NULL, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(post_id, student_id), CHECK(reaction_code IN ('fist_bump', 'respect', 'fire')), FOREIGN KEY(post_id) REFERENCES posts(post_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
INSERT OR IGNORE INTO post_reactions(post_id, student_id, reaction_code, created_at)
SELECT post_id, student_id,
       CASE lower(reaction) WHEN 'like' THEN 'fist_bump' WHEN 'fire' THEN 'fire' ELSE 'respect' END,
       max(created_at)
FROM post_reactions_legacy
GROUP BY post_id, student_id;
DROP TABLE post_reactions_legacy;
CREATE INDEX IF NOT EXISTS ix_post_reactions_post ON post_reactions(post_id, reaction_code);";
        await migrate.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsurePostColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var columns = new[]
        {
            (Name: "post_kind", Definition: "TEXT NOT NULL DEFAULT 'training'"),
            (Name: "link_url", Definition: "TEXT"),
            (Name: "image_data", Definition: "TEXT"),
            (Name: "image_alt", Definition: "TEXT")
        };

        foreach (var column in columns)
        {
            await using var check = connection.CreateCommand();
            check.CommandText = "SELECT COUNT(1) FROM pragma_table_info('posts') WHERE name = $name";
            check.Parameters.AddWithValue("$name", column.Name);
            if (Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) > 0) continue;

            await using var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE posts ADD COLUMN {column.Name} {column.Definition}";
            await alter.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsureGoldStarColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(1) FROM pragma_table_info('student_gold_stars') WHERE name = 'event_date'";
        if (Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 0)
        {
            await using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE student_gold_stars ADD COLUMN event_date TEXT";
            await alter.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsureStudentProfileColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        foreach (var column in new[]
        {
            (Name: "public_tile_ids", Definition: "TEXT"),
            (Name: "public_tile_sizes", Definition: "TEXT")
        })
        {
            await using var check = connection.CreateCommand();
            check.CommandText = "SELECT COUNT(1) FROM pragma_table_info('student_profiles') WHERE name = $name";
            check.Parameters.AddWithValue("$name", column.Name);
            if (Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) > 0) continue;
            await using var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE student_profiles ADD COLUMN {column.Name} {column.Definition}";
            await alter.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsureCanonicalClassCatalogAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var classes = connection.CreateCommand();
        classes.CommandText = "SELECT class_id, class_name, description FROM classes WHERE active = 1 ORDER BY class_id";
        var rows = new List<(int Id, string Name, string? Description)>();
        await using (var reader = await classes.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add((reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2)));
            }
        }

        foreach (var row in rows)
        {
            var separator = row.Name.IndexOf('—');
            var programName = separator > 0 ? row.Name[..separator].Trim() : row.Name.Trim();
            programName = programName switch
            {
                "Teens & Adults" => "Teen/Adult",
                "IS3 • Eskrima" => "IS3",
                "Sparring Program" or "Weapons Program" or "Special Schedules" or "Task Karate" => string.Empty,
                _ when programName.Contains("Kids", StringComparison.OrdinalIgnoreCase) => "Kids",
                _ when programName.Contains("Teen", StringComparison.OrdinalIgnoreCase) || programName.Contains("Adult", StringComparison.OrdinalIgnoreCase) => "Teen/Adult",
                _ when programName.Contains("IS3", StringComparison.OrdinalIgnoreCase) => "IS3",
                _ => string.Empty
            };
            if (string.IsNullOrWhiteSpace(programName)) continue;

            await using var program = connection.CreateCommand();
            program.CommandText = "INSERT OR IGNORE INTO portal_programs(program_name, description) VALUES ($name, $description)";
            program.Parameters.AddWithValue("$name", programName);
            program.Parameters.AddWithValue("$description", (object?)row.Description ?? DBNull.Value);
            await program.ExecuteNonQueryAsync(cancellationToken);

            await using var schedule = connection.CreateCommand();
            schedule.CommandText = "SELECT day_of_week, start_time, end_time FROM class_schedule WHERE class_id = $class AND active = 1 ORDER BY schedule_id LIMIT 1";
            schedule.Parameters.AddWithValue("$class", row.Id);
            await using var scheduleReader = await schedule.ExecuteReaderAsync(cancellationToken);
            var day = 1;
            var start = "17:00";
            var duration = 60;
            if (!await scheduleReader.ReadAsync(cancellationToken)) continue;
            day = scheduleReader.GetInt32(0);
            start = scheduleReader.GetString(1);
            var end = scheduleReader.IsDBNull(2) ? null : scheduleReader.GetString(2);
            duration = CalculateDurationMinutes(start, end);

            await using var template = connection.CreateCommand();
            template.CommandText = @"
INSERT OR IGNORE INTO portal_class_templates(program_id, class_id, template_name, day_of_week, start_time, duration_minutes, belt_scope, class_type, appointment_only)
SELECT program_id, $class, $template, $day, $start, $duration, NULL,
       CASE WHEN lower($template) LIKE '%seminar%' THEN 'Seminar' WHEN lower($template) LIKE '%private%' THEN 'Private lesson' ELSE 'Class' END,
       CASE WHEN lower($template) LIKE '%private%' THEN 1 ELSE 0 END
FROM portal_programs WHERE program_name = $program";
            template.Parameters.AddWithValue("$class", row.Id);
            template.Parameters.AddWithValue("$template", row.Name);
            template.Parameters.AddWithValue("$day", day);
            template.Parameters.AddWithValue("$start", start);
            template.Parameters.AddWithValue("$duration", duration);
            template.Parameters.AddWithValue("$program", programName);
            await template.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static int CalculateDurationMinutes(string start, string? end)
    {
        if (string.IsNullOrWhiteSpace(end) || !TimeSpan.TryParse(start, CultureInfo.InvariantCulture, out var startTime) || !TimeSpan.TryParse(end, CultureInfo.InvariantCulture, out var endTime)) return 60;
        var duration = (int)(endTime - startTime).TotalMinutes;
        return duration > 0 && duration <= 480 ? duration : 60;
    }

    private async Task SeedDevelopmentGoldStarEventsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        if (!_options.ImportDemoStudents) return;
        var resetDemoAttendance = Environment.GetEnvironmentVariable("TASK_KARATE_RESET_DEMO_ATTENDANCE") is "1" or "true";
        await using var command = connection.CreateCommand();
        command.CommandText = (resetDemoAttendance ? "DELETE FROM attendance WHERE student_id = (SELECT student_id FROM students WHERE first_name = 'Emma' AND last_name = 'Thompson');\n" : string.Empty) + @"
INSERT OR IGNORE INTO gold_star_events(event_name, description, event_date) VALUES
    ('Tunnel Hike', 'A special gold-star event for students who take on the studio tunnel hike.', date('now', '-30 days')),
    ('1,000 Kick Challenge', 'Complete the studio 1,000-kick challenge with focus and good technique.', date('now', '-14 days')),
    ('Torchlight Parade', 'Students attending the Task Karate Torchlight Parade appearance can earn a gold star.', date('now', '+3 days'));

INSERT OR IGNORE INTO achievements(achievement_name, description, icon_name) VALUES
    ('First Class!', 'You completed your very first class at TASK.', 'star'),
    ('10 Classes Strong', 'You completed ten classes and built a real training rhythm.', 'ten'),
    ('100 Classes', 'One hundred classes completed. That is serious consistency.', 'hundred'),
    ('1,000 Classes', 'One thousand classes completed. A true dojo milestone.', 'thousand'),
    ('10,000 Classes', 'Ten thousand classes completed. An extraordinary lifetime achievement.', 'ten-thousand'),
    ('1 Helper Class', 'You helped teach and support your first class.', 'helper-one'),
    ('10 Helper Classes', 'Ten classes helped. Your leadership is becoming part of the dojo.', 'helper-ten'),
    ('100 Helper Classes', 'One hundred classes helped. Students can count on you.', 'helper-hundred'),
    ('1,000 Helper Classes', 'One thousand classes helped. A remarkable service record.', 'helper-thousand'),
    ('10,000 Helper Classes', 'Ten thousand classes helped. An extraordinary lifetime of leadership.', 'helper-ten-thousand'),
    ('30-Day Rhythm', 'You attended on thirty distinct days and kept showing up.', 'calendar'),
    ('90-Day Rhythm', 'You attended on ninety distinct days and made training a habit.', 'calendar-star'),
    ('Warmup Warrior', 'Ready to go on time for warmups all month.', 'flame'),
    ('Form Explorer', 'You began learning a new form and can perform the opening section.', 'compass'),
    ('Community Spark', 'You showed up for a special event that brought the dojo together.', 'spark'),
    ('3 Classes', 'Three classes completed and the rhythm is starting.', 'three'),
    ('5 Classes', 'Five classes completed with focus.', 'five'),
    ('25 Classes', 'Twenty-five classes completed.', 'twenty-five'),
    ('50 Classes', 'Fifty classes completed.', 'fifty'),
    ('250 Classes', 'Two hundred and fifty classes completed.', 'two-fifty'),
    ('500 Classes', 'Five hundred classes completed.', 'five-hundred'),
    ('2,500 Classes', 'Two thousand five hundred classes completed.', 'two-five-hundred'),
     ('5,000 Classes', 'Five thousand classes completed.', 'five-thousand'),
     ('1-Year Dojo Anniversary', 'One year of training with the Task Karate community.', 'anniversary'),
     ('3-Year Dojo Anniversary', 'Three years of steady training and growth at Task Karate.', 'anniversary'),
     ('5-Year Dojo Anniversary', 'Five years of commitment to the dojo.', 'anniversary'),
     ('10-Year Dojo Anniversary', 'Ten years of learning, teaching, and showing up.', 'anniversary'),
     ('7-Day Rhythm', 'You trained on seven consecutive days with attendance.', 'calendar'),
    ('14-Day Rhythm', 'You trained on fourteen consecutive days.', 'calendar'),
    ('60-Day Rhythm', 'You built a sixty-day attendance rhythm.', 'calendar-star'),
    ('180-Day Rhythm', 'You built a 180-day attendance rhythm.', 'calendar-star'),
    ('Year of Consistency', 'A full year of showing up and doing the work.', 'year'),
    ('First Practice Log', 'You recorded your first focused practice session.', 'journal'),
    ('10 Practice Logs', 'Ten practice sessions recorded outside class.', 'journal'),
    ('50 Practice Logs', 'Fifty practice sessions recorded.', 'journal'),
    ('10 Practice Hours', 'Ten hours of focused practice recorded.', 'clock'),
    ('50 Practice Hours', 'Fifty hours of focused practice recorded.', 'clock'),
    ('First Goal', 'You set your first personal training goal.', 'target'),
    ('Goal Getter', 'You completed a personal training goal.', 'target'),
    ('5 Goals Completed', 'Five training goals completed.', 'target'),
    ('10 Goals Completed', 'Ten training goals completed.', 'target'),
    ('Early Bird', 'You arrived ready before class time all month.', 'sunrise'),
    ('First Stripe', 'You earned your first stripe through consistent training.', 'stripe'),
    ('Stripe Collector', 'You earned five stripes on your journey.', 'stripe'),
    ('New Belt Day', 'You advanced to a new belt rank.', 'belt'),
    ('Black Belt Candidate', 'Your instructor marked you ready for black belt preparation.', 'belt'),
    ('Tunnel Hike', 'You completed the studio tunnel hike.', 'mountain'),
    ('Torchlight Parade', 'You represented Task Karate in the torchlight parade.', 'torch'),
    ('1,000 Kick Challenge', 'You completed the 1,000 kick challenge.', 'kick'),
    ('Seminar Explorer', 'You participated in a special training seminar.', 'seminar'),
    ('Private Lesson Focus', 'You completed a focused private lesson.', 'focus'),
    ('Helping Hand', 'You helped a fellow student learn safely.', 'hand'),
    ('Respect in Action', 'You demonstrated respect when it mattered.', 'respect'),
    ('Leadership Moment', 'You took a positive leadership role in class.', 'leadership'),
    ('Buddy Builder', 'You trained consistently with a partner.', 'buddy'),
    ('IS3 Student Level 1', 'You advanced to IS3 Student Level 1.', 'is3'),
    ('IS3 Student Level 2', 'You advanced to IS3 Student Level 2.', 'is3'),
    ('First Stick Flow', 'You learned your first IS3 stick-flow sequence.', 'is3'),
    ('Safe Spacing', 'You demonstrated safe training distance in IS3.', 'is3'),
    ('Pattern Keeper', 'You held a complete IS3 pattern with control.', 'is3');

INSERT OR IGNORE INTO training_missions(title, description, category) VALUES
    ('Front Kick Snap & Return', 'Chamber, snap, and rechamber with balance. Try 3 sets of 10 on each leg.', 'Karate fundamentals'),
    ('Green Belt Combo 2', 'Jab, cross, rear roundhouse, and backfist with smooth transitions.', 'Next belt'),
    ('IS3 Stick Flow Basics', 'Practice your ready position, safe spacing, and the opening sequence with an instructor.', 'IS3 level track');

INSERT OR IGNORE INTO student_mission_progress(student_id, mission_id, completed, completed_at)
SELECT student.student_id, mission.mission_id, 0, NULL
FROM students student CROSS JOIN training_missions mission
WHERE student.first_name = 'Emma' AND student.last_name = 'Thompson';

INSERT OR IGNORE INTO student_goals(student_id, title, target_date, completed, created_at, completed_at)
SELECT student.student_id, 'Attend two classes this week', date('now', '+6 days'), 0, strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-2 days'), NULL
FROM students student
WHERE student.first_name = 'Emma' AND student.last_name = 'Thompson';

INSERT OR IGNORE INTO student_goals(student_id, title, target_date, completed, created_at, completed_at)
SELECT student.student_id, 'Practice my roundhouse with control', date('now', '+14 days'), 0, strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-5 days'), NULL
FROM students student
WHERE student.first_name = 'Emma' AND student.last_name = 'Thompson';

INSERT INTO student_practice_logs(student_id, skill, minutes, reflection, logged_at)
SELECT student.student_id, 'Roundhouse kick', 12, 'Better balance on the landing today.', strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-1 day')
FROM students student
WHERE student.first_name = 'Emma' AND student.last_name = 'Thompson'
  AND NOT EXISTS (SELECT 1 FROM student_practice_logs WHERE skill = 'Roundhouse kick' AND reflection = 'Better balance on the landing today.');

INSERT INTO student_practice_logs(student_id, skill, minutes, reflection, logged_at)
SELECT student.student_id, 'IS3 ready position', 8, 'Stayed relaxed and kept safe spacing.', strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-3 days')
FROM students student
WHERE student.first_name = 'Emma' AND student.last_name = 'Thompson'
  AND NOT EXISTS (SELECT 1 FROM student_practice_logs WHERE skill = 'IS3 ready position' AND reflection = 'Stayed relaxed and kept safe spacing.');

INSERT INTO guardians(first_name, last_name, email, phone, active)
SELECT 'Alex', 'Thompson', 'alex.thompson@example.test', '555-0102', 1
WHERE NOT EXISTS (SELECT 1 FROM guardians WHERE first_name = 'Alex' AND last_name = 'Thompson' AND email = 'alex.thompson@example.test');

INSERT OR IGNORE INTO student_guardians(student_id, guardian_id, relationship, is_primary)
SELECT student.student_id, guardian.guardian_id, 'Parent / Guardian', 1
FROM students student CROSS JOIN guardians guardian
WHERE student.first_name = 'Emma' AND student.last_name = 'Thompson'
  AND guardian.first_name = 'Alex' AND guardian.last_name = 'Thompson';

INSERT OR IGNORE INTO student_program_memberships(student_id, program_code, program_name, progression_type, level_name, enrolled_date)
SELECT student.student_id, 'teen-adult', 'Teen/Adult', 'belt', 'Green Belt', student.start_date
FROM students student
WHERE student.first_name = 'Emma' AND student.last_name = 'Thompson';

INSERT OR IGNORE INTO student_program_memberships(student_id, program_code, program_name, progression_type, level_name, enrolled_date)
SELECT student.student_id, 'is3', 'IS3', 'level', 'IS3 Student Level 0', student.start_date
FROM students student
WHERE student.first_name = 'Emma' AND student.last_name = 'Thompson'
  AND lower(replace(student.age_group, ' ', '')) IN ('teensadults', 'teenadults', 'adult');

INSERT OR IGNORE INTO classes(class_name, description, active)
VALUES ('Development Attendance Sample', 'Development-only attendance records used to demonstrate the 30-day training view. You can remove this class from a local demo database at any time.', 1);

INSERT OR IGNORE INTO class_sessions(class_id, session_date, start_time, end_time, location_name, cancelled)
SELECT class_id, date('now', '-10 days'), '18:00', '19:00', 'Main dojo', 0
FROM classes WHERE class_name = 'Development Attendance Sample';

INSERT OR IGNORE INTO class_sessions(class_id, session_date, start_time, end_time, location_name, cancelled)
SELECT class_id, date('now', '-3 days'), '18:00', '19:00', 'Main dojo', 0
FROM classes WHERE class_name = 'Development Attendance Sample';

UPDATE student_rank_history
SET awarded_date = date('now', '-30 days')
WHERE student_id = (SELECT student_id FROM students WHERE first_name = 'Emma' AND last_name = 'Thompson')
  AND rank_id = (SELECT rank_id FROM ranks WHERE rank_name = 'Green Belt');

INSERT OR IGNORE INTO attendance(session_id, student_id, check_in_time, status)
SELECT class_session.session_id, student.student_id, class_session.session_date || 'T18:00:00Z', 'present'
FROM class_sessions class_session
JOIN classes class_item ON class_item.class_id = class_session.class_id
CROSS JOIN students student
WHERE class_item.class_name = 'Development Attendance Sample'
  AND student.first_name = 'Emma' AND student.last_name = 'Thompson';

INSERT OR IGNORE INTO student_gold_stars(student_id, event_id, note)
SELECT student.student_id, event.event_id, 'Development-only sample award.'
FROM students student CROSS JOIN gold_star_events event
WHERE student.first_name = 'Emma' AND student.last_name = 'Thompson'
  AND event.event_name IN ('Tunnel Hike', '1,000 Kick Challenge');

INSERT OR IGNORE INTO student_gold_stars(student_id, event_id, note)
SELECT student.student_id, event.event_id, 'Development-only sample award.'
FROM students student CROSS JOIN gold_star_events event
WHERE student.first_name = 'Logan' AND student.last_name = 'Perez'
  AND event.event_name = 'Torchlight Parade';

INSERT OR IGNORE INTO student_achievements(student_id, achievement_id, awarded_at)
SELECT student.student_id, achievement.achievement_id, strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-60 days')
FROM students student CROSS JOIN achievements achievement
WHERE student.first_name = 'Emma' AND student.last_name = 'Thompson'
  AND achievement.achievement_name IN ('First Class!', 'Warmup Warrior');

INSERT OR IGNORE INTO student_achievements(student_id, achievement_id, awarded_at)
SELECT student.student_id, achievement.achievement_id, strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-25 days')
FROM students student CROSS JOIN achievements achievement
WHERE student.first_name = 'Emma' AND student.last_name = 'Thompson'
  AND achievement.achievement_name = 'Community Spark';

INSERT OR IGNORE INTO student_achievements(student_id, achievement_id, awarded_at)
SELECT student.student_id, achievement.achievement_id, strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-45 days')
FROM students student CROSS JOIN achievements achievement
WHERE student.first_name = 'Logan' AND student.last_name = 'Perez'
  AND achievement.achievement_name = 'Form Explorer';

INSERT OR IGNORE INTO student_friendships(student_id, friend_id, status, requested_by, created_at, updated_at)
SELECT emma.student_id, logan.student_id, 'accepted', emma.student_id,
       strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-21 days'), strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-21 days')
FROM students emma CROSS JOIN students logan
WHERE emma.first_name = 'Emma' AND emma.last_name = 'Thompson'
  AND logan.first_name = 'Logan' AND logan.last_name = 'Perez';

INSERT OR IGNORE INTO student_friendships(student_id, friend_id, status, requested_by, created_at, updated_at)
SELECT emma.student_id, naomi.student_id, 'accepted', naomi.student_id,
       strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-16 days'), strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-16 days')
FROM students emma CROSS JOIN students naomi
WHERE emma.first_name = 'Emma' AND emma.last_name = 'Thompson'
  AND naomi.first_name = 'Naomi' AND naomi.last_name = 'Wu';

INSERT OR IGNORE INTO student_friendships(student_id, friend_id, status, requested_by, created_at, updated_at)
SELECT emma.student_id, brohan.student_id, 'pending', emma.student_id,
       strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-2 days'), strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-2 days')
FROM students emma CROSS JOIN students brohan
WHERE emma.first_name = 'Emma' AND emma.last_name = 'Thompson'
  AND brohan.first_name = 'Brohan' AND brohan.last_name = 'the Flex Monk';

INSERT INTO student_messages(sender_id, recipient_id, message_text, created_at)
SELECT emma.student_id, logan.student_id, 'Are you going to the Torchlight Parade practice?', strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-4 days')
FROM students emma CROSS JOIN students logan
WHERE emma.first_name = 'Emma' AND emma.last_name = 'Thompson'
  AND logan.first_name = 'Logan' AND logan.last_name = 'Perez'
  AND NOT EXISTS (SELECT 1 FROM student_messages WHERE message_text = 'Are you going to the Torchlight Parade practice?');

INSERT INTO student_messages(sender_id, recipient_id, message_text, created_at)
SELECT logan.student_id, emma.student_id, 'Absolutely! I will see you there. Bring your loudest parade energy!', strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-3 days')
FROM students emma CROSS JOIN students logan
WHERE emma.first_name = 'Emma' AND emma.last_name = 'Thompson'
  AND logan.first_name = 'Logan' AND logan.last_name = 'Perez'
  AND NOT EXISTS (SELECT 1 FROM student_messages WHERE message_text = 'Absolutely! I will see you there. Bring your loudest parade energy!');

INSERT INTO posts(student_id, post_text, post_type, moderation_status, visible, created_at, updated_at)
SELECT logan.student_id, 'Torchlight Parade practice is looking great. See everyone Thursday!', 'student', 'approved', 1, strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-2 hours'), strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-2 hours')
FROM students logan
WHERE logan.first_name = 'Logan' AND logan.last_name = 'Perez'
  AND NOT EXISTS (SELECT 1 FROM posts WHERE post_text = 'Torchlight Parade practice is looking great. See everyone Thursday!');

INSERT INTO posts(student_id, post_text, post_type, moderation_status, visible, created_at, updated_at)
SELECT naomi.student_id, 'Purple Belt form practice after class today. Small improvements add up!', 'student', 'approved', 1, strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-5 hours'), strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-5 hours')
FROM students naomi
WHERE naomi.first_name = 'Naomi' AND naomi.last_name = 'Wu'
  AND NOT EXISTS (SELECT 1 FROM posts WHERE post_text = 'Purple Belt form practice after class today. Small improvements add up!');

INSERT INTO posts(student_id, post_text, post_type, moderation_status, visible, created_at, updated_at)
SELECT (SELECT student_id FROM students ORDER BY student_id LIMIT 1), 'Torchlight Parade Gold Star Event — Students who attend the Task Karate parade appearance this Thursday will receive a Gold Star on their student profile.', 'news', 'approved', 1, strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-1 day'), strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-1 day')
WHERE NOT EXISTS (SELECT 1 FROM posts WHERE post_text = 'Torchlight Parade Gold Star Event — Students who attend the Task Karate parade appearance this Thursday will receive a Gold Star on their student profile.');

INSERT INTO posts(student_id, post_text, post_type, moderation_status, visible, created_at, updated_at)
SELECT (SELECT student_id FROM students ORDER BY student_id LIMIT 1), 'Belt Testing Focus This Week — Open Training to review your next-rank requirements, stripe progress, and practice assignments before your next class.', 'news', 'approved', 1, strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-3 days'), strftime('%Y-%m-%dT%H:%M:%fZ', 'now', '-3 days')
WHERE NOT EXISTS (SELECT 1 FROM posts WHERE post_text = 'Belt Testing Focus This Week — Open Training to review your next-rank requirements, stripe progress, and practice assignments before your next class.');

INSERT OR IGNORE INTO post_reactions(post_id, student_id, reaction_code)
SELECT post.post_id, emma.student_id, 'fist_bump'
FROM posts post CROSS JOIN students emma
WHERE post.post_text = 'Torchlight Parade practice is looking great. See everyone Thursday!'
  AND emma.first_name = 'Emma' AND emma.last_name = 'Thompson';

INSERT OR IGNORE INTO post_reactions(post_id, student_id, reaction_code)
SELECT post.post_id, naomi.student_id, 'respect'
FROM posts post CROSS JOIN students naomi
WHERE post.post_text = 'Torchlight Parade practice is looking great. See everyone Thursday!'
  AND naomi.first_name = 'Naomi' AND naomi.last_name = 'Wu';

INSERT OR IGNORE INTO post_reactions(post_id, student_id, reaction_code)
SELECT post.post_id, emma.student_id, 'respect'
FROM posts post CROSS JOIN students emma
WHERE post.post_text = 'Belt Testing Focus This Week — Open Training to review your next-rank requirements, stripe progress, and practice assignments before your next class.'
  AND emma.first_name = 'Emma' AND emma.last_name = 'Thompson';

INSERT OR IGNORE INTO post_bookmarks(post_id, student_id)
SELECT post.post_id, emma.student_id
FROM posts post CROSS JOIN students emma
WHERE post.post_text = 'Belt Testing Focus This Week — Open Training to review your next-rank requirements, stripe progress, and practice assignments before your next class.'
  AND emma.first_name = 'Emma' AND emma.last_name = 'Thompson';
";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task ImportDevelopmentDataAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        if (_options.ImportLegacySchedules) await ImportLegacySchedulesAsync(connection, cancellationToken);
        if (_options.ImportDemoStudents) await ImportDemoStudentsAsync(connection, cancellationToken);
    }

    private async Task ImportLegacySchedulesAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(_options.ContentRootPath, "..", "..", "..", "..", "data", "schedules.json"));
        if (!File.Exists(path)) path = System.IO.Path.GetFullPath(System.IO.Path.Combine(_options.ContentRootPath, "..", "..", "data", "schedules.json"));
        if (!File.Exists(path)) { _logger.LogWarning("Starter schedule import was enabled, but {Path} was not found.", path); return; }
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(path, cancellationToken));
        foreach (var programProperty in document.RootElement.EnumerateObject())
        {
            var programName = programProperty.Value.GetProperty("title").GetString() ?? programProperty.Name;
            foreach (var group in programProperty.Value.GetProperty("groups").EnumerateArray())
            {
                if (!group.TryGetProperty("schedule", out var schedule) || !schedule.EnumerateObject().Any()) continue;
                var groupName = group.GetProperty("name").GetString() ?? "Class";
                var className = $"{programName} — {groupName}";
                var description = programProperty.Value.TryGetProperty("description", out var descriptionProperty) ? descriptionProperty.GetString() : null;
                var classId = await FindOrInsertClassAsync(connection, className, description, cancellationToken);
                foreach (var day in schedule.EnumerateObject())
                {
                    if (!Enum.TryParse<DayOfWeek>(day.Name, true, out var dayOfWeek)) continue;
                    foreach (var occurrence in day.Value.EnumerateArray())
                    {
                        var timeText = occurrence.GetProperty("time").GetString() ?? "";
                        if (!DateTime.TryParse(timeText, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out var parsedTime)) continue;
                        var duration = occurrence.TryGetProperty("isHour", out var isHour) && isHour.GetBoolean() ? 60 : 45;
                        var start = parsedTime.ToString("HH:mm", CultureInfo.InvariantCulture);
                        var end = parsedTime.AddMinutes(duration).ToString("HH:mm", CultureInfo.InvariantCulture);
                        await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO class_schedule(class_id, day_of_week, start_time, end_time, location_name, active) SELECT $class, $day, $start, $end, 'Main dojo', 1 WHERE NOT EXISTS (SELECT 1 FROM class_schedule WHERE class_id = $class AND day_of_week = $day AND start_time = $start AND active = 1)"; command.Parameters.AddWithValue("$class", classId); command.Parameters.AddWithValue("$day", (int)dayOfWeek); command.Parameters.AddWithValue("$start", start); command.Parameters.AddWithValue("$end", end); await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }
        }
    }

    private async Task<int> FindOrInsertClassAsync(SqliteConnection connection, string className, string? description, CancellationToken cancellationToken)
    {
        await using var find = connection.CreateCommand(); find.CommandText = "SELECT class_id FROM classes WHERE class_name = $name LIMIT 1"; find.Parameters.AddWithValue("$name", className); var existing = await find.ExecuteScalarAsync(cancellationToken); if (existing is not null) return Convert.ToInt32(existing, CultureInfo.InvariantCulture);
        await using var insert = connection.CreateCommand(); insert.CommandText = "INSERT INTO classes(class_name, description, active) VALUES ($name, $description, 1)"; insert.Parameters.AddWithValue("$name", className); insert.Parameters.AddWithValue("$description", (object?)description ?? DBNull.Value); await insert.ExecuteNonQueryAsync(cancellationToken);
        await using var select = connection.CreateCommand(); select.CommandText = "SELECT class_id FROM classes WHERE class_name = $name LIMIT 1"; select.Parameters.AddWithValue("$name", className); return Convert.ToInt32(await select.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    private async Task ImportDemoStudentsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(_options.ContentRootPath, "..", "..", "..", "..", "data", "portal-students.json"));
        if (!File.Exists(path)) path = System.IO.Path.GetFullPath(System.IO.Path.Combine(_options.ContentRootPath, "..", "..", "data", "portal-students.json"));
        if (!File.Exists(path)) { _logger.LogWarning("Starter demo-student import was enabled, but {Path} was not found.", path); return; }
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(path, cancellationToken));
        foreach (var item in document.RootElement.GetProperty("students").EnumerateArray())
        {
            var fullName = item.GetProperty("name").GetString() ?? "Demo Student"; var parts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries); if (parts.Length < 2) continue;
            await using var find = connection.CreateCommand(); find.CommandText = "SELECT student_id FROM students WHERE first_name = $first AND last_name = $last LIMIT 1"; find.Parameters.AddWithValue("$first", parts[0]); find.Parameters.AddWithValue("$last", parts[1]); var value = await find.ExecuteScalarAsync(cancellationToken); int studentId;
            if (value is not null) studentId = Convert.ToInt32(value, CultureInfo.InvariantCulture); else { await using var insert = connection.CreateCommand(); insert.CommandText = "INSERT INTO students(first_name, last_name, preferred_name, start_date, active) VALUES ($first, $last, $preferred, $join, 1); SELECT last_insert_rowid();"; insert.Parameters.AddWithValue("$first", parts[0]); insert.Parameters.AddWithValue("$last", parts[1]); insert.Parameters.AddWithValue("$preferred", item.TryGetProperty("displayName", out var display) ? display.GetString() ?? fullName : fullName); insert.Parameters.AddWithValue("$join", item.TryGetProperty("joinDate", out var join) ? join.GetString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd") : DateTime.UtcNow.ToString("yyyy-MM-dd")); studentId = Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture); }
            await using var details = connection.CreateCommand(); details.CommandText = "UPDATE students SET age_group = COALESCE($ageGroup, age_group), uniform_size = COALESCE($uniform, uniform_size), belt_size = COALESCE($belt, belt_size), birth_date = COALESCE($birth, birth_date) WHERE student_id = $id"; details.Parameters.AddWithValue("$id", studentId); details.Parameters.AddWithValue("$ageGroup", item.TryGetProperty("ageGroup", out var existingAgeGroup) ? existingAgeGroup.GetString() ?? (object)DBNull.Value : DBNull.Value); details.Parameters.AddWithValue("$uniform", item.TryGetProperty("uniformSize", out var existingUniform) ? existingUniform.GetString() ?? (object)DBNull.Value : DBNull.Value); details.Parameters.AddWithValue("$belt", item.TryGetProperty("beltSize", out var existingBelt) ? existingBelt.GetString() ?? (object)DBNull.Value : DBNull.Value); details.Parameters.AddWithValue("$birth", item.TryGetProperty("birthday", out var existingBirth) ? existingBirth.GetString() ?? (object)DBNull.Value : DBNull.Value); await details.ExecuteNonQueryAsync(cancellationToken);
            await using var profile = connection.CreateCommand(); profile.CommandText = "INSERT OR IGNORE INTO student_profiles(student_id, display_name, bio, favorite_technique) VALUES ($id, $name, $bio, $technique)"; profile.Parameters.AddWithValue("$id", studentId); profile.Parameters.AddWithValue("$name", item.TryGetProperty("displayName", out var profileName) ? profileName.GetString() ?? fullName : fullName); profile.Parameters.AddWithValue("$bio", item.TryGetProperty("bio", out var bio) ? bio.GetString() ?? "Development-only imported demo profile." : "Development-only imported demo profile."); profile.Parameters.AddWithValue("$technique", item.TryGetProperty("funStats", out var stats) && stats.TryGetProperty("favoriteTechnique", out var technique) ? technique.GetString() ?? (object)DBNull.Value : DBNull.Value); await profile.ExecuteNonQueryAsync(cancellationToken);
            if (item.TryGetProperty("rank", out var rankProperty)) await AddRankHistoryAsync(connection, studentId, rankProperty.GetString(), cancellationToken);
            await EnsureImportedDemoAccountAsync(connection, item, studentId, cancellationToken);
        }
    }

    private async Task EnsureImportedDemoAccountAsync(SqliteConnection connection, JsonElement item, int studentId, CancellationToken cancellationToken)
    {
        var demoPin = Environment.GetEnvironmentVariable("TASK_KARATE_DEMO_STUDENT_PIN");
        if (string.IsNullOrWhiteSpace(demoPin) || !StudentPinPolicy.Validate(demoPin, demoPin).Count.Equals(0) || !item.TryGetProperty("roles", out var roles) || !roles.EnumerateArray().Any(role => string.Equals(role.GetString(), "student", StringComparison.OrdinalIgnoreCase))) return;
        var sourceId = item.TryGetProperty("id", out var idProperty) ? idProperty.GetString() : null;
        var username = string.IsNullOrWhiteSpace(sourceId) ? $"demo.student.{studentId}" : $"demo.{sourceId}";
        var hash = _passwordHasher.HashPassword(new StarterStudentAccount { StudentId = studentId }, demoPin);
        await using var insert = connection.CreateCommand(); insert.CommandText = @"INSERT INTO student_accounts(student_id, username, password_hash, pin_hash, must_change_password)
VALUES ($id, $username, $hash, $hash, 0)
ON CONFLICT(student_id) DO UPDATE SET pin_hash = COALESCE(student_accounts.pin_hash, excluded.pin_hash), active = 1, updated_at = CURRENT_TIMESTAMP"; insert.Parameters.AddWithValue("$id", studentId); insert.Parameters.AddWithValue("$username", username); insert.Parameters.AddWithValue("$hash", hash); await insert.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task AddRankHistoryAsync(SqliteConnection connection, int studentId, string? rankName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rankName)) return;
        await using var existing = connection.CreateCommand(); existing.CommandText = "SELECT rank_id FROM ranks WHERE rank_name = $name LIMIT 1"; existing.Parameters.AddWithValue("$name", rankName); var existingId = await existing.ExecuteScalarAsync(cancellationToken); int rankId;
        if (existingId is not null) rankId = Convert.ToInt32(existingId, CultureInfo.InvariantCulture);
        else
        {
            await using var next = connection.CreateCommand(); next.CommandText = "SELECT COALESCE(MAX(rank_order), 0) + 1 FROM ranks"; var order = Convert.ToInt32(await next.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
            await using var insert = connection.CreateCommand(); insert.CommandText = "INSERT INTO ranks(rank_name, rank_order, active) VALUES ($name, $order, 1)"; insert.Parameters.AddWithValue("$name", rankName); insert.Parameters.AddWithValue("$order", order); await insert.ExecuteNonQueryAsync(cancellationToken);
            await using var select = connection.CreateCommand(); select.CommandText = "SELECT rank_id FROM ranks WHERE rank_name = $name LIMIT 1"; select.Parameters.AddWithValue("$name", rankName); rankId = Convert.ToInt32(await select.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }
        await using var history = connection.CreateCommand(); history.CommandText = "INSERT INTO student_rank_history(student_id, rank_id, awarded_date) SELECT $student, $rank, date('now') WHERE NOT EXISTS (SELECT 1 FROM student_rank_history WHERE student_id = $student AND rank_id = $rank)"; history.Parameters.AddWithValue("$student", studentId); history.Parameters.AddWithValue("$rank", rankId); await history.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task MaterializeUpcomingSessionsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var start = DateTime.UtcNow.Date; await using var schedules = connection.CreateCommand(); schedules.CommandText = "SELECT class_id, day_of_week, start_time, end_time, instructor_id, location_name FROM class_schedule WHERE active = 1"; var rows = new List<(int ClassId, int Day, string Start, string End, object? Instructor, object? Location)>(); await using (var reader = await schedules.ExecuteReaderAsync(cancellationToken)) while (await reader.ReadAsync(cancellationToken)) rows.Add((reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetValue(4), reader.IsDBNull(5) ? null : reader.GetValue(5)));
        foreach (var row in rows) for (var offset = 0; offset < 35; offset++) { var date = start.AddDays(offset); if ((int)date.DayOfWeek != row.Day) continue; await using var insert = connection.CreateCommand(); insert.CommandText = "INSERT OR IGNORE INTO class_sessions(class_id, session_date, start_time, end_time, instructor_id, location_name, cancelled) VALUES ($class, $date, $start, $end, $instructor, $location, 0)"; insert.Parameters.AddWithValue("$class", row.ClassId); insert.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); insert.Parameters.AddWithValue("$start", row.Start); insert.Parameters.AddWithValue("$end", row.End); insert.Parameters.AddWithValue("$instructor", row.Instructor ?? DBNull.Value); insert.Parameters.AddWithValue("$location", row.Location ?? DBNull.Value); await insert.ExecuteNonQueryAsync(cancellationToken); }
    }

    private static async Task SeedDemoDataAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("TASK_KARATE_STARTER_SEED_DEMO"), "true", StringComparison.OrdinalIgnoreCase) && Environment.GetEnvironmentVariable("TASK_KARATE_STARTER_SEED_DEMO") != "1") return;
        await using var count = connection.CreateCommand(); count.CommandText = "SELECT COUNT(1) FROM students"; if (Convert.ToInt32(await count.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) > 0) return;
        var tomorrow = DateTime.UtcNow.Date.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); var later = DateTime.UtcNow.Date.AddDays(2).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await using var command = connection.CreateCommand(); command.CommandText = @"
INSERT OR IGNORE INTO ranks(rank_name, rank_order, display_color) VALUES ('White Belt', 1, '#f1f5f9');
INSERT INTO students(first_name, last_name, preferred_name, start_date) VALUES ('Demo', 'Student', 'Demo Student', date('now'));
INSERT INTO student_profiles(student_id, display_name, bio, favorite_technique) VALUES (last_insert_rowid(), 'Demo Student', 'Development-only sample profile.', 'Front stance');
INSERT INTO classes(class_name, description) VALUES ('Foundations', 'Development-only sample session.');
INSERT INTO class_sessions(class_id, session_date, start_time, end_time, location_name) VALUES (last_insert_rowid(), $tomorrow, '17:30', '18:30', 'Main dojo');
INSERT INTO class_sessions(class_id, session_date, start_time, end_time, location_name) VALUES ((SELECT class_id FROM classes ORDER BY class_id DESC LIMIT 1), $later, '09:30', '10:30', 'Main dojo');
INSERT INTO student_rank_history(student_id, rank_id, awarded_date) VALUES ((SELECT student_id FROM students ORDER BY student_id DESC LIMIT 1), (SELECT rank_id FROM ranks WHERE rank_name = 'White Belt'), date('now'));
"; command.Parameters.AddWithValue("$tomorrow", tomorrow); command.Parameters.AddWithValue("$later", later); await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task EnsureBootstrapAccountAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var username = Environment.GetEnvironmentVariable("TASK_KARATE_STUDENT_USERNAME");
        var pin = Environment.GetEnvironmentVariable("TASK_KARATE_STUDENT_PIN");
        var idText = Environment.GetEnvironmentVariable("TASK_KARATE_STUDENT_ID");
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(pin) || StudentPinPolicy.Validate(pin, pin).Count != 0 || !int.TryParse(idText, out var studentId)) return;
        await using var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(1) FROM students WHERE student_id = $id AND active = 1";
        check.Parameters.AddWithValue("$id", studentId);
        if (Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) != 1) return;
        await using var exists = connection.CreateCommand();
        exists.CommandText = "SELECT COUNT(1) FROM student_accounts WHERE student_id = $id AND pin_hash IS NOT NULL";
        exists.Parameters.AddWithValue("$id", studentId);
        if (Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 1) return;
        var hash = _passwordHasher.HashPassword(new StarterStudentAccount { StudentId = studentId }, pin);
        await using var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO student_accounts(student_id, username, password_hash, pin_hash, must_change_password) VALUES ($id, $username, $hash, $hash, 0)";
        insert.Parameters.AddWithValue("$id", studentId); insert.Parameters.AddWithValue("$username", username.Trim()); insert.Parameters.AddWithValue("$hash", hash);
        await insert.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Created local student account for student id {StudentId}; credentials were read from environment only.", studentId);
    }

    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        await EnsureReadyAsync(cancellationToken);
        var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public async Task<StudentAccount?> AuthenticateAsync(string username, string pin, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT a.student_id, a.username, a.pin_hash, s.first_name, s.last_name, s.nickname, s.honorific, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), s.birth_date FROM student_accounts a JOIN students s ON s.student_id = a.student_id LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE a.username = $username COLLATE NOCASE AND a.pin_hash IS NOT NULL AND a.active = 1 AND s.active = 1";
        command.Parameters.AddWithValue("$username", username.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var account = new StarterStudentAccount { StudentId = reader.GetInt32(0) };
        var result = _passwordHasher.VerifyHashedPassword(account, reader.GetString(2), pin);
        var rankName = reader.IsDBNull(8) ? null : reader.GetString(8);
        var displayName = BuildStudentDisplayName(reader.GetString(3), reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.GetString(7), rankName);
        return result == PasswordVerificationResult.Failed ? null : new StudentAccount(reader.GetInt32(0), reader.GetString(1), displayName, rankName, IsBirthdayWeek(reader.IsDBNull(9) ? null : reader.GetString(9)));
    }

    public async Task<StudentAccount?> AuthenticateByStudentIdAsync(int studentId, string pin, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT a.student_id, a.username, a.pin_hash, s.first_name, s.last_name, s.nickname, s.honorific, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), s.birth_date FROM student_accounts a JOIN students s ON s.student_id = a.student_id LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE a.student_id = $studentId AND a.pin_hash IS NOT NULL AND a.active = 1 AND s.active = 1 AND COALESCE(p.profile_visible, 1) = 1";
        command.Parameters.AddWithValue("$studentId", studentId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var account = new StarterStudentAccount { StudentId = reader.GetInt32(0) };
        var result = _passwordHasher.VerifyHashedPassword(account, reader.GetString(2), pin);
        var rankName = reader.IsDBNull(8) ? null : reader.GetString(8);
        var displayName = BuildStudentDisplayName(reader.GetString(3), reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.GetString(7), rankName);
        return result == PasswordVerificationResult.Failed ? null : new StudentAccount(reader.GetInt32(0), reader.GetString(1), displayName, rankName, IsBirthdayWeek(reader.IsDBNull(9) ? null : reader.GetString(9)));
    }

    public async Task<bool> ChangeStudentPinAsync(int studentId, string currentPin, string newPin, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT pin_hash FROM student_accounts WHERE student_id = $id AND active = 1";
        command.Parameters.AddWithValue("$id", studentId);
        var storedHash = await command.ExecuteScalarAsync(cancellationToken) as string;
        if (string.IsNullOrWhiteSpace(storedHash)) return false;
        var account = new StarterStudentAccount { StudentId = studentId };
        if (_passwordHasher.VerifyHashedPassword(account, storedHash, currentPin) == PasswordVerificationResult.Failed) return false;
        var pinHash = _passwordHasher.HashPassword(account, newPin);
        await using var update = connection.CreateCommand();
        update.CommandText = "UPDATE student_accounts SET pin_hash = $hash, updated_at = $now WHERE student_id = $id AND active = 1";
        update.Parameters.AddWithValue("$hash", pinHash); update.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); update.Parameters.AddWithValue("$id", studentId);
        return await update.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> VerifyStudentPinAsync(int studentId, string pin, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT a.pin_hash FROM student_accounts a JOIN students s ON s.student_id = a.student_id WHERE a.student_id = $id AND a.active = 1 AND s.active = 1";
        command.Parameters.AddWithValue("$id", studentId);
        var storedHash = await command.ExecuteScalarAsync(cancellationToken) as string;
        if (string.IsNullOrWhiteSpace(storedHash)) return false;
        return _passwordHasher.VerifyHashedPassword(new StarterStudentAccount { StudentId = studentId }, storedHash, pin) != PasswordVerificationResult.Failed;
    }

    public async Task<StudentAccountState?> GetStudentAccountStateAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT a.pin_hash IS NOT NULL, s.birth_date FROM student_accounts a JOIN students s ON s.student_id = a.student_id WHERE a.student_id = $id AND a.active = 1 AND s.active = 1";
        command.Parameters.AddWithValue("$id", studentId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? new StudentAccountState(reader.GetBoolean(0), IsBirthdayWeek(reader.IsDBNull(1) ? null : reader.GetString(1))) : null;
    }

    public async Task<StudentPinResetResult?> ResetStudentPinAsync(int studentId, string pin, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var lookup = connection.CreateCommand();
        lookup.Transaction = (SqliteTransaction)transaction;
        lookup.CommandText = @"SELECT s.first_name, s.last_name, a.username, a.password_hash
FROM students s LEFT JOIN student_accounts a ON a.student_id = s.student_id
WHERE s.student_id = $id AND s.active = 1";
        lookup.Parameters.AddWithValue("$id", studentId);
        await using var reader = await lookup.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var firstName = reader.GetString(0); var lastName = reader.GetString(1);
        var username = reader.IsDBNull(2) ? BuildStudentUsername(firstName, lastName, studentId) : reader.GetString(2);
        var passwordHash = reader.IsDBNull(3) ? _passwordHasher.HashPassword(new StarterStudentAccount { StudentId = studentId }, pin) : reader.GetString(3);
        await reader.DisposeAsync();
        var pinHash = _passwordHasher.HashPassword(new StarterStudentAccount { StudentId = studentId }, pin);
        await using var save = connection.CreateCommand(); save.Transaction = (SqliteTransaction)transaction;
        save.CommandText = @"INSERT INTO student_accounts(student_id, username, password_hash, pin_hash, must_change_password, active, updated_at)
VALUES ($id, $username, $passwordHash, $pinHash, 0, 1, $now)
ON CONFLICT(student_id) DO UPDATE SET pin_hash = excluded.pin_hash, password_hash = excluded.password_hash, must_change_password = 0, active = 1, updated_at = excluded.updated_at";
        save.Parameters.AddWithValue("$id", studentId); save.Parameters.AddWithValue("$username", username); save.Parameters.AddWithValue("$passwordHash", passwordHash); save.Parameters.AddWithValue("$pinHash", pinHash); save.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        await save.ExecuteNonQueryAsync(cancellationToken); await transaction.CommitAsync(cancellationToken); return new StudentPinResetResult(username);
    }

    private static string BuildStudentUsername(string firstName, string lastName, int studentId)
    {
        static string Clean(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
        var first = Clean(firstName);
        var last = Clean(lastName);
        return $"{(string.IsNullOrWhiteSpace(first) ? "student" : first)}.{(string.IsNullOrWhiteSpace(last) ? "account" : last)}.{studentId}";
    }

    private static bool IsBirthdayWeek(string? birthDateText)
    {
        if (!DateTime.TryParse(birthDateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthDate)) return false;
        var today = DateTime.Now.Date;
        var day = birthDate.Month == 2 && birthDate.Day == 29 && !DateTime.IsLeapYear(today.Year) ? 28 : birthDate.Day;
        var birthday = new DateTime(today.Year, birthDate.Month, day);
        return Math.Abs((today - birthday).TotalDays) <= 3;
    }

    public async Task<IReadOnlyList<StudentDirectoryItem>> GetStudentDirectoryAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT s.student_id, s.first_name, s.last_name, s.nickname, s.honorific, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)),
            (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1),
            (SELECT m.level_name FROM student_program_memberships m WHERE m.student_id = s.student_id AND m.active = 1 AND LOWER(m.program_code) = 'is3' ORDER BY m.enrolled_date DESC LIMIT 1),
            p.profile_image_path
            FROM students s
            JOIN student_accounts a ON a.student_id = s.student_id AND a.active = 1
            LEFT JOIN student_profiles p ON p.student_id = s.student_id
            WHERE s.active = 1 AND a.pin_hash IS NOT NULL AND COALESCE(p.profile_visible, 1) = 1
            ORDER BY UPPER(COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name))) COLLATE NOCASE";
        var list = new List<StudentDirectoryItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) { var rank = reader.IsDBNull(6) ? null : reader.GetString(6); list.Add(new(reader.GetInt32(0), BuildStudentDisplayName(reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4), reader.GetString(5), rank), rank, reader.IsDBNull(7) ? null : reader.GetString(7), reader.IsDBNull(8) ? null : reader.GetString(8))); }
        return list;
    }

    public async Task<bool> HasDisclaimerAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM student_disclaimer_acceptances WHERE student_id = $id AND disclaimer_version = $version"; command.Parameters.AddWithValue("$id", studentId); command.Parameters.AddWithValue("$version", StudentAuth.DisclaimerVersion);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) > 0;
    }

    public async Task AcceptDisclaimerAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO student_disclaimer_acceptances(student_id, disclaimer_version, accepted_at) VALUES ($id, $version, $now)"; command.Parameters.AddWithValue("$id", studentId); command.Parameters.AddWithValue("$version", StudentAuth.DisclaimerVersion); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleItem>> GetScheduleAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "SELECT s.session_id, s.session_date, s.start_time, s.end_time, c.class_name, c.description, s.location_name, s.cancelled FROM class_sessions s JOIN classes c ON c.class_id = s.class_id WHERE date(s.session_date) >= date($from) AND date(s.session_date) < date($to) AND c.active = 1 AND s.cancelled = 0 ORDER BY date(s.session_date), COALESCE(s.start_time, '')"; command.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd")); command.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd"));
        var list = new List<ScheduleItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt32(0), DateTime.Parse(reader.GetString(1), CultureInfo.InvariantCulture), reader.IsDBNull(2) ? null : reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.GetInt32(7) != 0)); return list;
    }

    public async Task<IReadOnlyList<PortalProgramItem>> GetPortalProgramsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT p.program_id, p.program_name, p.description,
       t.template_id, t.template_name, t.day_of_week, t.start_time, t.duration_minutes,
       t.belt_scope, t.class_type, t.appointment_only,
       s.schedule_id, s.day_of_week, s.start_time, s.end_time
FROM portal_programs p
LEFT JOIN portal_class_templates t ON t.program_id = p.program_id AND t.active = 1
LEFT JOIN class_schedule s ON s.class_id = t.class_id AND s.active = 1
WHERE p.active = 1
ORDER BY p.program_name COLLATE NOCASE, COALESCE(s.day_of_week, t.day_of_week), COALESCE(s.start_time, t.start_time), t.template_name COLLATE NOCASE";
        var programs = new Dictionary<int, (string Name, string? Description, List<PortalClassTemplateItem> Templates)>();
        var templateLookup = new Dictionary<(int ProgramId, int TemplateId), PortalClassTemplateItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var programId = reader.GetInt32(0);
            if (!programs.TryGetValue(programId, out var program))
            {
                program = (reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), []);
                programs.Add(programId, program);
            }
            if (!reader.IsDBNull(3))
            {
                var day = Enum.IsDefined(typeof(DayOfWeek), reader.GetInt32(5)) ? ((DayOfWeek)reader.GetInt32(5)).ToString() : "Monday";
                var templateKey = (programId, reader.GetInt32(3));
                if (!templateLookup.TryGetValue(templateKey, out var template))
                {
                    template = new(reader.GetInt32(3), reader.GetString(4), day, reader.GetString(6), reader.GetInt32(7), reader.IsDBNull(8) ? null : reader.GetString(8), reader.GetString(9), reader.GetInt32(10) != 0, programId, new List<PortalClassScheduleItem>());
                    program.Templates.Add(template);
                    templateLookup[templateKey] = template;
                }
                if (!reader.IsDBNull(11))
                {
                    var schedules = (List<PortalClassScheduleItem>)template.Schedules!;
                    var scheduleDay = Enum.IsDefined(typeof(DayOfWeek), reader.GetInt32(12)) ? ((DayOfWeek)reader.GetInt32(12)).ToString() : day;
                    schedules.Add(new(reader.GetInt32(11), scheduleDay, reader.GetString(13), CalculateDurationMinutes(reader.GetString(13), reader.IsDBNull(14) ? null : reader.GetString(14))));
                }
            }
        }
        return programs.Select(item => new PortalProgramItem(item.Key, item.Value.Name, item.Value.Description, item.Value.Templates)).ToList();
    }

    public async Task<int> CreatePortalProgramAsync(string name, string? description, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO portal_programs(program_name, description) VALUES ($name, $description); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name", name.Trim());
        command.Parameters.AddWithValue("$description", (object?)NullIfBlank(description) ?? DBNull.Value);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public async Task<int?> CreatePortalClassTemplateAsync(PortalClassTemplateWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ProgramId <= 0 || string.IsNullOrWhiteSpace(request.Name) || request.DurationMinutes is < 1 or > 480 || request.ClassType is not ("Class" or "Seminar" or "Private lesson" or "Belt testing")) return null;
        if (!Enum.TryParse<DayOfWeek>(request.DayOfWeek, true, out var day)) return null;
        var start = NormalizeClock(request.StartTime);
        if (start is null) return null;
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var program = connection.CreateCommand();
        program.Transaction = (SqliteTransaction)transaction;
        program.CommandText = "SELECT program_name FROM portal_programs WHERE program_id = $id AND active = 1";
        program.Parameters.AddWithValue("$id", request.ProgramId);
        var programName = await program.ExecuteScalarAsync(cancellationToken) as string;
        if (programName is null) return null;
        await using var classInsert = connection.CreateCommand();
        classInsert.Transaction = (SqliteTransaction)transaction;
        classInsert.CommandText = "INSERT INTO classes(class_name, description, active) VALUES ($name, $description, 1); SELECT last_insert_rowid();";
        classInsert.Parameters.AddWithValue("$name", request.Name.Trim());
        classInsert.Parameters.AddWithValue("$description", "Managed in the Task Karate portal.");
        var classId = Convert.ToInt32(await classInsert.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        await using var schedule = connection.CreateCommand();
        schedule.Transaction = (SqliteTransaction)transaction;
        schedule.CommandText = "INSERT INTO class_schedule(class_id, day_of_week, start_time, end_time, location_name, active) VALUES ($class, $day, $start, $end, 'Main dojo', 1)";
        schedule.Parameters.AddWithValue("$class", classId); schedule.Parameters.AddWithValue("$day", (int)day); schedule.Parameters.AddWithValue("$start", start); schedule.Parameters.AddWithValue("$end", AddMinutes(start, request.DurationMinutes));
        await schedule.ExecuteNonQueryAsync(cancellationToken);
        await using var template = connection.CreateCommand();
        template.Transaction = (SqliteTransaction)transaction;
        template.CommandText = @"INSERT INTO portal_class_templates(program_id, class_id, template_name, day_of_week, start_time, duration_minutes, belt_scope, class_type, appointment_only)
VALUES ($program, $class, $name, $day, $start, $duration, $belt, $type, $appointment); SELECT last_insert_rowid();";
        var beltScope = NormalizeBeltScope(request.BeltScope);
        template.Parameters.AddWithValue("$program", request.ProgramId); template.Parameters.AddWithValue("$class", classId); template.Parameters.AddWithValue("$name", request.Name.Trim()); template.Parameters.AddWithValue("$day", (int)day); template.Parameters.AddWithValue("$start", start); template.Parameters.AddWithValue("$duration", request.DurationMinutes); template.Parameters.AddWithValue("$belt", (object?)NullIfBlank(beltScope) ?? DBNull.Value); template.Parameters.AddWithValue("$type", request.ClassType is "Seminar" or "Private lesson" or "Belt testing" ? request.ClassType : "Class"); template.Parameters.AddWithValue("$appointment", request.AppointmentOnly ? 1 : 0);
        var templateId = Convert.ToInt32(await template.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        await transaction.CommitAsync(cancellationToken);
        await MaterializeUpcomingSessionsAsync(connection, cancellationToken);
        return templateId;
    }

    public async Task<bool> UpdatePortalClassTemplateAsync(int templateId, PortalClassTemplateWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (templateId <= 0 || request.ProgramId <= 0 || string.IsNullOrWhiteSpace(request.Name) || request.DurationMinutes is < 1 or > 480 || request.ClassType is not ("Class" or "Seminar" or "Private lesson" or "Belt testing")) return false;
        if (!Enum.TryParse<DayOfWeek>(request.DayOfWeek, true, out var day)) return false;
        var start = NormalizeClock(request.StartTime);
        if (start is null) return false;
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var lookup = connection.CreateCommand();
        lookup.Transaction = (SqliteTransaction)transaction;
        lookup.CommandText = "SELECT t.class_id, p.program_id FROM portal_class_templates t JOIN portal_programs p ON p.program_id = $program AND p.active = 1 WHERE t.template_id = $template AND t.active = 1";
        lookup.Parameters.AddWithValue("$template", templateId);
        lookup.Parameters.AddWithValue("$program", request.ProgramId);
        await using var reader = await lookup.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return false;
        var classId = reader.GetInt32(0);
        await reader.DisposeAsync();
        var beltScope = NormalizeBeltScope(request.BeltScope);
        await using var updateClass = connection.CreateCommand();
        updateClass.Transaction = (SqliteTransaction)transaction;
        updateClass.CommandText = "UPDATE classes SET class_name = $name, active = 1, updated_at = CURRENT_TIMESTAMP WHERE class_id = $class";
        updateClass.Parameters.AddWithValue("$name", request.Name.Trim()); updateClass.Parameters.AddWithValue("$class", classId);
        await updateClass.ExecuteNonQueryAsync(cancellationToken);
        await using var updateSchedule = connection.CreateCommand();
        updateSchedule.Transaction = (SqliteTransaction)transaction;
        // A class may meet on several recurring days. Editing its primary schedule
        // must not collapse the other active days into the same day/time.
        updateSchedule.CommandText = "UPDATE class_schedule SET day_of_week = $day, start_time = $start, end_time = $end, active = 1 WHERE schedule_id = (SELECT schedule_id FROM class_schedule WHERE class_id = $class AND active = 1 ORDER BY schedule_id LIMIT 1)";
        updateSchedule.Parameters.AddWithValue("$day", (int)day); updateSchedule.Parameters.AddWithValue("$start", start); updateSchedule.Parameters.AddWithValue("$end", AddMinutes(start, request.DurationMinutes)); updateSchedule.Parameters.AddWithValue("$class", classId);
        if (await updateSchedule.ExecuteNonQueryAsync(cancellationToken) == 0)
        {
            await using var insertSchedule = connection.CreateCommand();
            insertSchedule.Transaction = (SqliteTransaction)transaction;
            insertSchedule.CommandText = "INSERT INTO class_schedule(class_id, day_of_week, start_time, end_time, location_name, active) VALUES ($class, $day, $start, $end, 'Main dojo', 1)";
            insertSchedule.Parameters.AddWithValue("$class", classId); insertSchedule.Parameters.AddWithValue("$day", (int)day); insertSchedule.Parameters.AddWithValue("$start", start); insertSchedule.Parameters.AddWithValue("$end", AddMinutes(start, request.DurationMinutes));
            await insertSchedule.ExecuteNonQueryAsync(cancellationToken);
        }
        await using var updateTemplate = connection.CreateCommand();
        updateTemplate.Transaction = (SqliteTransaction)transaction;
        updateTemplate.CommandText = "UPDATE portal_class_templates SET program_id = $program, template_name = $name, day_of_week = $day, start_time = $start, duration_minutes = $duration, belt_scope = $belt, class_type = $type, appointment_only = $appointment, active = 1, updated_at = CURRENT_TIMESTAMP WHERE template_id = $template";
        updateTemplate.Parameters.AddWithValue("$program", request.ProgramId); updateTemplate.Parameters.AddWithValue("$name", request.Name.Trim()); updateTemplate.Parameters.AddWithValue("$day", (int)day); updateTemplate.Parameters.AddWithValue("$start", start); updateTemplate.Parameters.AddWithValue("$duration", request.DurationMinutes); updateTemplate.Parameters.AddWithValue("$belt", (object?)NullIfBlank(beltScope) ?? DBNull.Value); updateTemplate.Parameters.AddWithValue("$type", request.ClassType); updateTemplate.Parameters.AddWithValue("$appointment", request.AppointmentOnly ? 1 : 0); updateTemplate.Parameters.AddWithValue("$template", templateId);
        var updated = await updateTemplate.ExecuteNonQueryAsync(cancellationToken) > 0;
        await transaction.CommitAsync(cancellationToken);
        return updated;
    }

    public async Task<bool> RetirePortalClassTemplateAsync(int templateId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var lookup = connection.CreateCommand(); lookup.Transaction = (SqliteTransaction)transaction; lookup.CommandText = "SELECT class_id FROM portal_class_templates WHERE template_id = $template AND active = 1"; lookup.Parameters.AddWithValue("$template", templateId);
        var value = await lookup.ExecuteScalarAsync(cancellationToken);
        if (value is null) return false;
        var classId = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        await using var retireTemplate = connection.CreateCommand(); retireTemplate.Transaction = (SqliteTransaction)transaction; retireTemplate.CommandText = "UPDATE portal_class_templates SET active = 0, updated_at = CURRENT_TIMESTAMP WHERE template_id = $template"; retireTemplate.Parameters.AddWithValue("$template", templateId); await retireTemplate.ExecuteNonQueryAsync(cancellationToken);
        await using var retireClass = connection.CreateCommand(); retireClass.Transaction = (SqliteTransaction)transaction; retireClass.CommandText = "UPDATE classes SET active = 0, updated_at = CURRENT_TIMESTAMP WHERE class_id = $class; UPDATE class_schedule SET active = 0 WHERE class_id = $class; UPDATE class_sessions SET cancelled = 1, cancellation_reason = 'Class template retired' WHERE class_id = $class AND date(session_date) >= date('now') AND cancelled = 0"; retireClass.Parameters.AddWithValue("$class", classId); await retireClass.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<int?> CancelPortalTemplateInstanceAsync(int templateId, DateTime sessionDateUtc, string reason, CancellationToken cancellationToken = default)
    {
        if (templateId <= 0 || string.IsNullOrWhiteSpace(reason)) return null;
        var sessionDate = sessionDateUtc.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var template = connection.CreateCommand(); template.Transaction = (SqliteTransaction)transaction; template.CommandText = "SELECT t.class_id, s.start_time, s.end_time, s.location_name FROM portal_class_templates t JOIN class_schedule s ON s.class_id = t.class_id AND s.active = 1 WHERE t.template_id = $template AND t.active = 1 ORDER BY s.schedule_id LIMIT 1"; template.Parameters.AddWithValue("$template", templateId);
        await using var reader = await template.ExecuteReaderAsync(cancellationToken); if (!await reader.ReadAsync(cancellationToken)) return null; var classId = reader.GetInt32(0); var start = reader.IsDBNull(1) ? null : reader.GetString(1); var end = reader.IsDBNull(2) ? null : reader.GetString(2); var location = reader.IsDBNull(3) ? null : reader.GetString(3); await reader.DisposeAsync();
        await using var existing = connection.CreateCommand(); existing.Transaction = (SqliteTransaction)transaction; existing.CommandText = "SELECT session_id FROM class_sessions WHERE class_id = $class AND date(session_date) = date($date) LIMIT 1"; existing.Parameters.AddWithValue("$class", classId); existing.Parameters.AddWithValue("$date", sessionDate); var existingId = await existing.ExecuteScalarAsync(cancellationToken);
        int sessionId;
        if (existingId is not null)
        {
            sessionId = Convert.ToInt32(existingId, CultureInfo.InvariantCulture);
            await using var update = connection.CreateCommand(); update.Transaction = (SqliteTransaction)transaction; update.CommandText = "UPDATE class_sessions SET cancelled = 1, cancellation_reason = $reason WHERE session_id = $session"; update.Parameters.AddWithValue("$reason", reason.Trim()); update.Parameters.AddWithValue("$session", sessionId); await update.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            await using var insert = connection.CreateCommand(); insert.Transaction = (SqliteTransaction)transaction; insert.CommandText = "INSERT INTO class_sessions(class_id, session_date, start_time, end_time, location_name, cancelled, cancellation_reason) VALUES ($class, $date, $start, $end, $location, 1, $reason); SELECT last_insert_rowid();"; insert.Parameters.AddWithValue("$class", classId); insert.Parameters.AddWithValue("$date", sessionDate); insert.Parameters.AddWithValue("$start", (object?)start ?? DBNull.Value); insert.Parameters.AddWithValue("$end", (object?)end ?? DBNull.Value); insert.Parameters.AddWithValue("$location", (object?)location ?? DBNull.Value); insert.Parameters.AddWithValue("$reason", reason.Trim()); sessionId = Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }
        await transaction.CommitAsync(cancellationToken);
        return sessionId;
    }

    public async Task<IReadOnlyList<PortalClassSessionItem>> GetPortalSessionsAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT s.session_id, t.template_id, c.class_name, COALESCE(s.start_time, t.start_time), t.duration_minutes, s.session_date, s.cancelled, s.cancellation_reason, t.class_type, s.assigned_student_id, s.location_name
FROM class_sessions s JOIN classes c ON c.class_id = s.class_id JOIN portal_class_templates t ON t.class_id = s.class_id
WHERE date(s.session_date) = date($date) AND c.active = 1
ORDER BY COALESCE(s.start_time, t.start_time), s.session_id";
        command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        var list = new List<PortalClassSessionItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4), DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture), reader.GetInt32(6) != 0, reader.IsDBNull(7) ? null : reader.GetString(7), reader.GetString(8), reader.IsDBNull(9) ? null : reader.GetInt32(9), reader.IsDBNull(10) ? null : reader.GetString(10)));
        return list;
    }

    public async Task<bool?> SetPortalSessionCancellationAsync(int sessionId, bool cancelled, string? reason, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.Transaction = (SqliteTransaction)transaction;
        command.CommandText = "UPDATE class_sessions SET cancelled = $cancelled, cancellation_reason = $reason WHERE session_id = $session";
        command.Parameters.AddWithValue("$cancelled", cancelled ? 1 : 0);
        command.Parameters.AddWithValue("$reason", (object?)NullIfBlank(reason) ?? DBNull.Value);
        command.Parameters.AddWithValue("$session", sessionId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) return null;

        await using var reservations = connection.CreateCommand();
        reservations.Transaction = (SqliteTransaction)transaction;
        reservations.CommandText = @"UPDATE student_class_signups
SET status = CASE WHEN $cancelled = 1 THEN 'cancelled' ELSE 'reserved' END
WHERE session_id = $session AND status IN ('reserved', 'cancelled')";
        reservations.Parameters.AddWithValue("$cancelled", cancelled ? 1 : 0);
        reservations.Parameters.AddWithValue("$session", sessionId);
        await reservations.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<int?> CreatePortalSessionAsync(PortalClassSessionWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var template = connection.CreateCommand();
        template.CommandText = "SELECT class_id, class_type, appointment_only FROM portal_class_templates WHERE template_id = $id AND active = 1";
        template.Parameters.AddWithValue("$id", request.ClassTemplateId);
        await using var reader = await template.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var classId = reader.GetInt32(0); var classType = reader.GetString(1);
        await reader.DisposeAsync();
        if (classType == "Private lesson" && request.AssignedStudentId is null) return null;
        if (request.AssignedStudentId is not null)
        {
            await using var student = connection.CreateCommand(); student.CommandText = "SELECT COUNT(1) FROM students WHERE student_id = $id AND active = 1 AND COALESCE(status, 'active') = 'active'"; student.Parameters.AddWithValue("$id", request.AssignedStudentId.Value);
            if (Convert.ToInt32(await student.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) != 1) return null;
        }
        await using var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO class_sessions(class_id, session_date, start_time, end_time, location_name, cancelled, notes, assigned_student_id) SELECT class_id, $date, start_time, end_time, location_name, 0, $notes, $assignedStudent FROM class_schedule WHERE class_id = $class AND active = 1 ORDER BY schedule_id LIMIT 1; SELECT last_insert_rowid();";
        insert.Parameters.AddWithValue("$class", classId); insert.Parameters.AddWithValue("$date", request.SessionDateUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); insert.Parameters.AddWithValue("$notes", (object?)NullIfBlank(request.Notes) ?? DBNull.Value); insert.Parameters.AddWithValue("$assignedStudent", (object?)request.AssignedStudentId ?? DBNull.Value);
        try { return Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture); } catch (SqliteException ex) when (ex.SqliteErrorCode == 19) { return null; }
    }

    public async Task<IReadOnlyList<PortalEnrollmentItem>> GetPortalEnrollmentsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT e.enrollment_id, e.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), e.template_id, t.template_name, e.active
FROM portal_enrollments e JOIN students s ON s.student_id = e.student_id LEFT JOIN student_profiles p ON p.student_id = s.student_id JOIN portal_class_templates t ON t.template_id = e.template_id
WHERE e.active = 1 ORDER BY t.template_name COLLATE NOCASE, s.last_name COLLATE NOCASE, s.first_name COLLATE NOCASE";
        var list = new List<PortalEnrollmentItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3), reader.GetString(4), reader.GetInt32(5) != 0)); return list;
    }

    public async Task<int?> AddPortalEnrollmentAsync(PortalEnrollmentWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var check = connection.CreateCommand(); check.CommandText = "SELECT COUNT(1) FROM students WHERE student_id = $student AND active = 1 AND COALESCE(status, 'active') = 'active'; SELECT COUNT(1) FROM portal_class_templates WHERE template_id = $template AND active = 1"; check.Parameters.AddWithValue("$student", request.StudentId); check.Parameters.AddWithValue("$template", request.ClassTemplateId);
        await using var checkReader = await check.ExecuteReaderAsync(cancellationToken); if (!await checkReader.ReadAsync(cancellationToken)) return null;
        var studentExists = checkReader.GetInt32(0) == 1; var templateExists = await checkReader.NextResultAsync(cancellationToken) && await checkReader.ReadAsync(cancellationToken) && checkReader.GetInt32(0) == 1; await checkReader.DisposeAsync(); if (!studentExists || !templateExists) return null;
        await using var command = connection.CreateCommand(); command.CommandText = @"INSERT INTO portal_enrollments(student_id, template_id, active) VALUES ($student, $template, 1)
ON CONFLICT(student_id, template_id) DO UPDATE SET active = 1, updated_at = CURRENT_TIMESTAMP; SELECT enrollment_id FROM portal_enrollments WHERE student_id = $student AND template_id = $template"; command.Parameters.AddWithValue("$student", request.StudentId); command.Parameters.AddWithValue("$template", request.ClassTemplateId); try { return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture); } catch (SqliteException ex) when (ex.SqliteErrorCode == 19) { return null; }
    }

    public async Task<bool> RemovePortalEnrollmentAsync(int enrollmentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "UPDATE portal_enrollments SET active = 0, updated_at = CURRENT_TIMESTAMP WHERE enrollment_id = $id AND active = 1"; command.Parameters.AddWithValue("$id", enrollmentId); return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    private static readonly string[] BeltScopeOrder = ["White Belt", "Yellow Belt", "Orange Belt", "Green Belt", "Purple Belt", "Blue Belt", "Red Belt", "Brown Belt", "Black Belt"];

    private static string? NormalizeBeltScope(IEnumerable<string>? values)
    {
        var requested = (values ?? Array.Empty<string>()).Select(value => value.Trim()).Where(value => value.Length > 0).ToArray();
        if (requested.Length == 0) return null;
        var selected = BeltScopeOrder.Where(option => requested.Any(value => string.Equals(value, option, StringComparison.OrdinalIgnoreCase) || (option == "Black Belt" && value.StartsWith("Black Belt", StringComparison.OrdinalIgnoreCase)))).ToArray();
        return selected.Length == 0 ? null : string.Join(", ", selected);
    }

    private static string? NormalizeClock(string value)
    {
        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out var parsed)) return null;
        return parsed.ToString("HH:mm", CultureInfo.InvariantCulture);
    }

    private static string AddMinutes(string start, int minutes)
    {
        return (TimeSpan.Parse(start, CultureInfo.InvariantCulture) + TimeSpan.FromMinutes(minutes)).ToString(@"hh\:mm", CultureInfo.InvariantCulture);
    }

    public async Task<IReadOnlyList<PortalAttendanceItem>> GetPortalAttendanceAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT a.attendance_id, a.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), a.check_in_time, a.status, a.notes
FROM attendance a JOIN students s ON s.student_id = a.student_id LEFT JOIN student_profiles p ON p.student_id = s.student_id
WHERE a.session_id = $session AND a.status IN ('present', 'helper') ORDER BY s.last_name, s.first_name";
        command.Parameters.AddWithValue("$session", sessionId);
        var list = new List<PortalAttendanceItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.IsDBNull(5) ? null : reader.GetString(5), reader.GetString(4)));
        return list;
    }

    public async Task<IReadOnlyList<PublicRosterItem>> GetPublicRosterAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT DISTINCT COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name))
FROM attendance a
JOIN class_sessions cs ON cs.session_id = a.session_id
JOIN students s ON s.student_id = a.student_id
LEFT JOIN student_profiles p ON p.student_id = s.student_id
WHERE a.session_id = $session
  AND date(cs.session_date) = date('now', 'localtime')
  AND a.status IN ('present', 'helper')
ORDER BY 1 COLLATE NOCASE";
        command.Parameters.AddWithValue("$session", sessionId);
        var list = new List<PublicRosterItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0)));
        return list;
    }

    public async Task<IReadOnlyList<PortalAdminAttendanceHistoryItem>> GetPortalAdminAttendanceHistoryAsync(DateOnly from, DateOnly to, int? studentId = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT a.attendance_id, a.student_id, COALESCE(p.display_name, TRIM(st.first_name || ' ' || st.last_name)), cs.session_id, cs.session_date, c.class_name, a.check_in_time, a.status, a.notes
FROM attendance a
JOIN class_sessions cs ON cs.session_id = a.session_id
JOIN classes c ON c.class_id = cs.class_id
JOIN students st ON st.student_id = a.student_id
LEFT JOIN student_profiles p ON p.student_id = st.student_id
WHERE date(cs.session_date) >= date($from) AND date(cs.session_date) < date($to)
  AND ($student IS NULL OR a.student_id = $student)
ORDER BY date(cs.session_date) DESC, a.check_in_time DESC, 3 COLLATE NOCASE
LIMIT 500";
        command.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$student", (object?)studentId ?? DBNull.Value);
        var list = new List<PortalAdminAttendanceHistoryItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3), DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture), reader.GetString(5), DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture), reader.GetString(7), reader.IsDBNull(8) ? null : reader.GetString(8)));
        }
        return list;
    }

    public async Task<bool> UpdatePortalAttendanceAsync(long attendanceId, string status, string? notes, CancellationToken cancellationToken = default)
    {
        if (status is not ("present" or "helper" or "absent" or "excused")) return false;
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE attendance SET status = $status, notes = $notes WHERE attendance_id = $id";
        command.Parameters.AddWithValue("$status", status);
        command.Parameters.AddWithValue("$notes", (object?)NullIfBlank(notes) ?? DBNull.Value);
        command.Parameters.AddWithValue("$id", attendanceId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> RemovePortalAttendanceAsync(long attendanceId, string reason, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE attendance SET status = 'absent', notes = $notes WHERE attendance_id = $id AND status IN ('present', 'helper')";
        command.Parameters.AddWithValue("$notes", $"Removed from class roster: {reason.Trim()}");
        command.Parameters.AddWithValue("$id", attendanceId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<PortalAdminReportSummary> GetPortalAdminReportSummaryAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT
 (SELECT COUNT(1) FROM students WHERE active = 1 AND COALESCE(status, 'active') = 'active'),
 (SELECT COUNT(1) FROM students WHERE active = 1 AND COALESCE(status, 'active') = 'paused'),
 (SELECT COUNT(1) FROM class_sessions WHERE date(session_date) >= date('now', 'localtime', 'weekday 1', '-7 days') AND date(session_date) < date('now', 'localtime', 'weekday 1') AND cancelled = 0),
 (SELECT COUNT(1) FROM attendance WHERE status IN ('present', 'helper') AND strftime('%Y-%m', check_in_time, 'localtime') = strftime('%Y-%m', 'now', 'localtime')),
 (SELECT COUNT(1) FROM dojo_check_ins WHERE strftime('%Y-%m', check_in_date) = strftime('%Y-%m', 'now', 'localtime')),
 (SELECT COUNT(1) FROM student_content_reports WHERE status = 'open')";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return new(0, 0, 0, 0, 0, 0);
        return new(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5));
    }

    public async Task<IReadOnlyList<PortalAdminMembershipSnapshot>> GetPortalAdminMembershipSnapshotsAsync(int? studentId = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT m.snapshot_id, m.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), m.external_customer_id, m.external_membership_id, m.membership_name, m.program_name, m.status, m.starts_on, m.ends_on, m.attendance_limit, m.synced_at, m.source, m.notes
FROM mystudio_membership_snapshots m
JOIN students s ON s.student_id = m.student_id
LEFT JOIN student_profiles p ON p.student_id = s.student_id
WHERE ($student IS NULL OR m.student_id = $student)
ORDER BY CASE WHEN m.status = 'active' THEN 0 ELSE 1 END, 3 COLLATE NOCASE, m.membership_name COLLATE NOCASE";
        command.Parameters.AddWithValue("$student", (object?)studentId ?? DBNull.Value);
        var list = new List<PortalAdminMembershipSnapshot>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.GetString(7), ParseNullableDate(reader, 8), ParseNullableDate(reader, 9), reader.IsDBNull(10) ? null : reader.GetInt32(10), DateTime.Parse(reader.GetString(11), CultureInfo.InvariantCulture), reader.GetString(12), reader.IsDBNull(13) ? null : reader.GetString(13)));
        }
        return list;
    }

    public async Task<long?> UpsertPortalAdminMembershipSnapshotAsync(PortalAdminMembershipSnapshotWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (request.StudentId <= 0 || string.IsNullOrWhiteSpace(request.ExternalMembershipId) || string.IsNullOrWhiteSpace(request.MembershipName) || string.IsNullOrWhiteSpace(request.Status)) return null;
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO mystudio_membership_snapshots(student_id, external_customer_id, external_membership_id, membership_name, program_name, status, starts_on, ends_on, attendance_limit, synced_at, source, notes)
VALUES ($student, $customer, $membershipId, $membershipName, $program, $status, $starts, $ends, $limit, $synced, 'MyStudio', $notes)
ON CONFLICT(student_id, external_membership_id) DO UPDATE SET external_customer_id = excluded.external_customer_id, membership_name = excluded.membership_name, program_name = excluded.program_name, status = excluded.status, starts_on = excluded.starts_on, ends_on = excluded.ends_on, attendance_limit = excluded.attendance_limit, synced_at = excluded.synced_at, notes = excluded.notes;
SELECT snapshot_id FROM mystudio_membership_snapshots WHERE student_id = $student AND external_membership_id = $membershipId";
        command.Parameters.AddWithValue("$student", request.StudentId);
        command.Parameters.AddWithValue("$customer", (object?)NullIfBlank(request.ExternalCustomerId) ?? DBNull.Value);
        command.Parameters.AddWithValue("$membershipId", request.ExternalMembershipId.Trim());
        command.Parameters.AddWithValue("$membershipName", request.MembershipName.Trim());
        command.Parameters.AddWithValue("$program", (object?)NullIfBlank(request.ProgramName) ?? DBNull.Value);
        command.Parameters.AddWithValue("$status", request.Status.Trim());
        command.Parameters.AddWithValue("$starts", (object?)request.StartsOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? DBNull.Value);
        command.Parameters.AddWithValue("$ends", (object?)request.EndsOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? DBNull.Value);
        command.Parameters.AddWithValue("$limit", (object?)request.AttendanceLimit ?? DBNull.Value);
        command.Parameters.AddWithValue("$synced", DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$notes", (object?)NullIfBlank(request.Notes) ?? DBNull.Value);
        try { return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture); } catch (SqliteException ex) when (ex.SqliteErrorCode == 19) { return null; }
    }

    private static DateTime? ParseNullableDate(SqliteDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : DateTime.TryParse(reader.GetString(ordinal), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;

    public async Task<IReadOnlyList<PortalAdminDocument>> GetPortalAdminDocumentsAsync(int? studentId = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT d.document_id, d.name, d.version, d.category, d.description, d.storage_key, d.active, d.required_for_enrollment,
 COUNT(a.acceptance_id), MAX(CASE WHEN a.student_id = $student THEN a.accepted_at END)
FROM portal_documents d
LEFT JOIN portal_document_acceptances a ON a.document_id = d.document_id
GROUP BY d.document_id, d.name, d.version, d.category, d.description, d.storage_key, d.active, d.required_for_enrollment
ORDER BY d.active DESC, d.category COLLATE NOCASE, d.name COLLATE NOCASE, d.version DESC";
        command.Parameters.AddWithValue("$student", (object?)studentId ?? DBNull.Value);
        var list = new List<PortalAdminDocument>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetString(5), reader.GetInt32(6) != 0, reader.GetInt32(7) != 0, reader.GetInt32(8), !reader.IsDBNull(9), ParseNullableDate(reader, 9)));
        }
        return list;
    }

    public async Task<long?> CreatePortalAdminDocumentAsync(PortalAdminDocumentWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Version) || string.IsNullOrWhiteSpace(request.Category)) return null;
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO portal_documents(name, version, category, description, storage_key, required_for_enrollment)
VALUES ($name, $version, $category, $description, $storage, $required); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name", request.Name.Trim());
        command.Parameters.AddWithValue("$version", request.Version.Trim());
        command.Parameters.AddWithValue("$category", request.Category.Trim());
        command.Parameters.AddWithValue("$description", (object?)NullIfBlank(request.Description) ?? DBNull.Value);
        command.Parameters.AddWithValue("$storage", (object?)NullIfBlank(request.StorageKey) ?? DBNull.Value);
        command.Parameters.AddWithValue("$required", request.RequiredForEnrollment ? 1 : 0);
        try { return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture); } catch (SqliteException ex) when (ex.SqliteErrorCode == 19) { return null; }
    }

    public async Task<bool> SetPortalAdminDocumentActiveAsync(long documentId, bool active, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE portal_documents SET active = $active, updated_at = CURRENT_TIMESTAMP WHERE document_id = $id";
        command.Parameters.AddWithValue("$active", active ? 1 : 0);
        command.Parameters.AddWithValue("$id", documentId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> AcceptPortalAdminDocumentAsync(long documentId, int studentId, int? guardianId, string? staffUserId, string? notes, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO portal_document_acceptances(document_id, student_id, guardian_id, accepted_at, accepted_by_staff_user_id, acceptance_method, notes)
SELECT $document, $student, $guardian, $now, $staff, 'Staff', $notes
WHERE EXISTS (SELECT 1 FROM portal_documents WHERE document_id = $document AND active = 1)
  AND EXISTS (SELECT 1 FROM students WHERE student_id = $student)
ON CONFLICT(document_id, student_id) DO UPDATE SET guardian_id = excluded.guardian_id, accepted_at = excluded.accepted_at, accepted_by_staff_user_id = excluded.accepted_by_staff_user_id, notes = excluded.notes";
        command.Parameters.AddWithValue("$document", documentId);
        command.Parameters.AddWithValue("$student", studentId);
        command.Parameters.AddWithValue("$guardian", (object?)guardianId ?? DBNull.Value);
        command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$staff", (object?)NullIfBlank(staffUserId) ?? DBNull.Value);
        command.Parameters.AddWithValue("$notes", (object?)NullIfBlank(notes) ?? DBNull.Value);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<IReadOnlyList<StaffHelperRosterItem>> GetStaffHelperRosterAsync(DateTime from, DateTime to, string? currentStaffUserId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        var sessions = new List<StaffHelperRosterItem>();
        await using (var sessionCommand = connection.CreateCommand())
        {
            sessionCommand.CommandText = @"SELECT s.session_id, s.session_date, s.start_time, s.end_time, c.class_name, s.location_name, s.cancelled
FROM class_sessions s JOIN classes c ON c.class_id = s.class_id
WHERE date(s.session_date) >= date($from) AND date(s.session_date) < date($to) AND c.active = 1
ORDER BY date(s.session_date), COALESCE(s.start_time, ''), s.session_id";
            sessionCommand.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            sessionCommand.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            await using var reader = await sessionCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                sessions.Add(new(
                    reader.GetInt32(0),
                    DateTime.Parse(reader.GetString(1), CultureInfo.InvariantCulture),
                    reader.IsDBNull(2) ? null : reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3),
                    reader.GetString(4),
                    reader.IsDBNull(5) ? null : reader.GetString(5),
                    reader.GetInt32(6) != 0,
                    [],
                    []));
            }
        }

        if (sessions.Count == 0) return sessions;
        var staffBySession = sessions.ToDictionary(item => item.SessionId, _ => new List<StaffHelperSignupItem>());
        await using (var helperCommand = connection.CreateCommand())
        {
            helperCommand.CommandText = @"SELECT h.session_id, h.staff_user_id, h.staff_display_name, h.created_at
FROM staff_helper_signups h JOIN class_sessions s ON s.session_id = h.session_id
WHERE date(s.session_date) >= date($from) AND date(s.session_date) < date($to)
ORDER BY h.created_at, h.staff_display_name";
            helperCommand.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            helperCommand.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            await using var reader = await helperCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var sessionId = reader.GetInt32(0);
                if (staffBySession.ContainsKey(sessionId))
                {
                    var staffUserId = reader.GetString(1);
                    staffBySession[sessionId].Add(new(staffUserId, reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), string.Equals(staffUserId, currentStaffUserId, StringComparison.OrdinalIgnoreCase)));
                }
            }
        }

        var studentsBySession = sessions.ToDictionary(item => item.SessionId, _ => new List<PortalAttendanceItem>());
        await using (var studentCommand = connection.CreateCommand())
        {
            studentCommand.CommandText = @"SELECT a.session_id, a.attendance_id, a.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), a.check_in_time, a.notes
FROM attendance a JOIN class_sessions cs ON cs.session_id = a.session_id JOIN students s ON s.student_id = a.student_id LEFT JOIN student_profiles p ON p.student_id = s.student_id
WHERE a.status = 'helper' AND date(cs.session_date) >= date($from) AND date(cs.session_date) < date($to)
ORDER BY a.check_in_time, s.last_name, s.first_name";
            studentCommand.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            studentCommand.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            await using var reader = await studentCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var sessionId = reader.GetInt32(0);
                if (studentsBySession.ContainsKey(sessionId)) studentsBySession[sessionId].Add(new(reader.GetInt64(1), reader.GetInt32(2), reader.GetString(3), DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture), reader.IsDBNull(5) ? null : reader.GetString(5), "helper"));
            }
        }

        return sessions.Select(item => item with { StaffHelpers = staffBySession[item.SessionId], StudentHelpers = studentsBySession[item.SessionId] }).ToList();
    }

    public async Task<bool> AddStaffHelperSignupAsync(int sessionId, string staffUserId, string displayName, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"INSERT OR IGNORE INTO staff_helper_signups(session_id, staff_user_id, staff_display_name)
SELECT $session, $user, $name
WHERE EXISTS (SELECT 1 FROM class_sessions s JOIN classes c ON c.class_id = s.class_id WHERE s.session_id = $session AND c.active = 1 AND s.cancelled = 0 AND date(s.session_date) >= date('now', 'localtime'))";
        command.Parameters.AddWithValue("$session", sessionId);
        command.Parameters.AddWithValue("$user", staffUserId);
        command.Parameters.AddWithValue("$name", displayName);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> RemoveStaffHelperSignupAsync(int sessionId, string staffUserId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM staff_helper_signups WHERE session_id = $session AND staff_user_id = $user";
        command.Parameters.AddWithValue("$session", sessionId);
        command.Parameters.AddWithValue("$user", staffUserId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<IReadOnlyList<int>> GetAttendanceSessionIdsAsync(int studentId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "SELECT a.session_id FROM attendance a JOIN class_sessions s ON s.session_id = a.session_id WHERE a.student_id = $student AND a.status IN ('present', 'helper') AND date(s.session_date) >= date($from) AND date(s.session_date) < date($to) ORDER BY s.session_date, s.session_id";
        command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd")); command.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd"));
        var ids = new List<int>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) ids.Add(reader.GetInt32(0)); return ids;
    }

    public async Task<StudentProfile?> GetProfileAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
		command.CommandText = @"SELECT s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), p.bio, p.favorite_technique, p.profile_image_path, (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), s.start_date, (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND a.status = 'present'), (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND a.status = 'present' AND strftime('%Y-%m', a.check_in_time) = strftime('%Y-%m', 'now')), (SELECT COUNT(1) FROM student_messages m WHERE m.recipient_id = s.student_id AND m.read_at IS NULL), (SELECT COUNT(1) FROM student_achievements a WHERE a.student_id = s.student_id), (SELECT h.awarded_date FROM student_rank_history h WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1) FROM students s LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE s.student_id = $id AND s.active = 1"; command.Parameters.AddWithValue("$id", studentId);
        int profileId; string displayName; string? bio; string? favoriteTechnique; string? image; string? rank; string? joinDate; int totalClasses; int classesThisMonth; int unread; int achievements; string? rankAwardedDate;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken)) return null;
            profileId = reader.GetInt32(0); displayName = reader.GetString(1); bio = reader.IsDBNull(2) ? null : reader.GetString(2); favoriteTechnique = reader.IsDBNull(3) ? null : reader.GetString(3); image = reader.IsDBNull(4) ? null : reader.GetString(4); rank = reader.IsDBNull(5) ? null : reader.GetString(5); joinDate = reader.IsDBNull(6) ? null : reader.GetString(6); totalClasses = reader.GetInt32(7); classesThisMonth = reader.GetInt32(8); unread = reader.GetInt32(9); achievements = reader.GetInt32(10); rankAwardedDate = reader.IsDBNull(11) ? null : reader.GetString(11);
        }
        var dates = new List<string>(); await using (var datesCommand = connection.CreateCommand()) { datesCommand.CommandText = "SELECT substr(check_in_time, 1, 10) FROM attendance WHERE student_id = $id AND status = 'present' ORDER BY check_in_time DESC LIMIT 30"; datesCommand.Parameters.AddWithValue("$id", studentId); await using var datesReader = await datesCommand.ExecuteReaderAsync(cancellationToken); while (await datesReader.ReadAsync(cancellationToken)) dates.Add(datesReader.GetString(0)); }
        var helperClasses = 0; await using (var helperCommand = connection.CreateCommand()) { helperCommand.CommandText = "SELECT COUNT(1) FROM attendance WHERE student_id = $id AND status = 'helper'"; helperCommand.Parameters.AddWithValue("$id", studentId); helperClasses = Convert.ToInt32(await helperCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture); }
        var classesPerStripe = rank?.Contains("Black", StringComparison.OrdinalIgnoreCase) == true ? 0 : rank?.Contains("Brown", StringComparison.OrdinalIgnoreCase) == true ? 12 : 8;
        var classesSinceRank = totalClasses;
        if (!string.IsNullOrWhiteSpace(rankAwardedDate))
        {
			await using var rankClasses = connection.CreateCommand(); rankClasses.CommandText = "SELECT COUNT(1) FROM attendance WHERE student_id = $id AND status = 'present' AND date(check_in_time) >= date($awarded)"; rankClasses.Parameters.AddWithValue("$id", studentId); rankClasses.Parameters.AddWithValue("$awarded", rankAwardedDate); classesSinceRank = Convert.ToInt32(await rankClasses.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }
        var classesIntoStripe = classesPerStripe == 0 ? 0 : classesSinceRank % classesPerStripe;
        var classesToNextStripe = classesPerStripe == 0 ? 0 : classesPerStripe - classesIntoStripe;
        var stripesEarned = classesPerStripe == 0 ? 0 : classesSinceRank / classesPerStripe;
        var nextMilestone = classesPerStripe == 0 ? "Next degree · instructor tracked" : "Next stripe";
        string? ageGroup; DateTime? birthDate; string? email; string? phone; string? uniformSize; string? beltSize; string? nickname; string? pronouns; string? honorific; IReadOnlyList<string> roles;
        await using (var details = connection.CreateCommand()) { details.CommandText = "SELECT age_group, birth_date, email, phone, uniform_size, belt_size, nickname, pronouns, honorific, roles, first_name, last_name FROM students WHERE student_id = $id"; details.Parameters.AddWithValue("$id", studentId); await using var detailsReader = await details.ExecuteReaderAsync(cancellationToken); await detailsReader.ReadAsync(cancellationToken); ageGroup = detailsReader.IsDBNull(0) ? null : detailsReader.GetString(0); birthDate = detailsReader.IsDBNull(1) || !DateTime.TryParse(detailsReader.GetString(1), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedBirthDate) ? null : parsedBirthDate; email = detailsReader.IsDBNull(2) ? null : detailsReader.GetString(2); phone = detailsReader.IsDBNull(3) ? null : detailsReader.GetString(3); uniformSize = detailsReader.IsDBNull(4) ? null : detailsReader.GetString(4); beltSize = detailsReader.IsDBNull(5) ? null : detailsReader.GetString(5); nickname = detailsReader.IsDBNull(6) ? null : detailsReader.GetString(6); pronouns = detailsReader.IsDBNull(7) ? null : detailsReader.GetString(7); honorific = detailsReader.IsDBNull(8) ? null : detailsReader.GetString(8); roles = detailsReader.IsDBNull(9) ? ["student"] : NormalizeStudentRoles(detailsReader.GetString(9).Split(',', StringSplitOptions.RemoveEmptyEntries)); displayName = BuildStudentDisplayName(detailsReader.GetString(10), detailsReader.GetString(11), nickname, honorific, displayName, rank); }
        var isAdult = birthDate.HasValue
            ? birthDate.Value.Date <= DateTime.Today.AddYears(-18)
            : string.Equals(ageGroup?.Trim(), "adult", StringComparison.OrdinalIgnoreCase);
        var guardians = new List<GuardianItem>();
        if (!isAdult)
        {
            await using var guardianCommand = connection.CreateCommand(); guardianCommand.CommandText = "SELECT DISTINCT g.first_name, g.last_name, sg.relationship, g.phone, g.email FROM student_guardians sg JOIN guardians g ON g.guardian_id = sg.guardian_id WHERE sg.student_id = $id AND g.active = 1 ORDER BY sg.is_primary DESC, g.last_name, g.first_name"; guardianCommand.Parameters.AddWithValue("$id", studentId); await using var guardianReader = await guardianCommand.ExecuteReaderAsync(cancellationToken); while (await guardianReader.ReadAsync(cancellationToken)) guardians.Add(new($"{guardianReader.GetString(0)} {guardianReader.GetString(1)}", guardianReader.GetString(2), guardianReader.IsDBNull(3) ? null : guardianReader.GetString(3), guardianReader.IsDBNull(4) ? null : guardianReader.GetString(4)));
        }
        var emergencyContacts = new List<EmergencyContactItem>();
        if (isAdult)
        {
            await using var emergencyCommand = connection.CreateCommand(); emergencyCommand.CommandText = "SELECT contact_id, contact_name, relationship, phone, email, first_name, middle_initial, last_name, pronouns, newsletter_opt_in, sms_opt_in FROM student_emergency_contacts WHERE student_id = $id ORDER BY contact_id"; emergencyCommand.Parameters.AddWithValue("$id", studentId); await using var emergencyReader = await emergencyCommand.ExecuteReaderAsync(cancellationToken); while (await emergencyReader.ReadAsync(cancellationToken)) emergencyContacts.Add(ReadEmergencyContact(emergencyReader));
        }
        var programs = new List<ProgramMembershipItem>(); await using (var programCommand = connection.CreateCommand()) { programCommand.CommandText = "SELECT program_name, program_code, progression_type, level_name, enrolled_date FROM student_program_memberships WHERE student_id = $id AND active = 1 ORDER BY program_name"; programCommand.Parameters.AddWithValue("$id", studentId); await using var programReader = await programCommand.ExecuteReaderAsync(cancellationToken); while (await programReader.ReadAsync(cancellationToken)) programs.Add(new(programReader.GetString(0), programReader.GetString(1), programReader.GetString(2), programReader.IsDBNull(3) ? null : programReader.GetString(3), programReader.IsDBNull(4) ? null : programReader.GetString(4))); }
        // A student with a belt is necessarily enrolled in the core karate track, even when
        // an older local database predates the explicit membership row.
        if (programs.Count == 0 && !string.IsNullOrWhiteSpace(rank))
        {
            var coreProgram = string.Equals(ageGroup, "Kids", StringComparison.OrdinalIgnoreCase) ? (Name: "Kids", Code: "kids") : (Name: "Teen/Adult", Code: "teen-adult");
            programs.Add(new(coreProgram.Name, coreProgram.Code, "belt", rank, joinDate));
        }
        var publicShowcase = await ReadPublicShowcaseAsync(connection, studentId, cancellationToken);
        return new(profileId, displayName, nickname, pronouns, honorific, roles, bio, favoriteTechnique, image, rank, joinDate, totalClasses, classesThisMonth, unread, achievements, dates, classesIntoStripe, classesPerStripe, classesToNextStripe, stripesEarned, nextMilestone, ageGroup, birthDate, email, phone, uniformSize, beltSize, guardians, emergencyContacts, programs, helperClasses, publicShowcase.Ids);
    }

    public async Task<bool> IsAdultStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT age_group, birth_date FROM students WHERE student_id = $id AND active = 1";
        command.Parameters.AddWithValue("$id", studentId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return false;
        var ageGroup = reader.IsDBNull(0) ? null : reader.GetString(0);
        var birthDate = reader.IsDBNull(1) || !DateTime.TryParse(reader.GetString(1), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedBirthDate) ? (DateTime?)null : parsedBirthDate;
        return birthDate.HasValue ? birthDate.Value.Date <= DateTime.Today.AddYears(-18) : string.Equals(ageGroup?.Trim(), "adult", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<EmergencyContactItem>> GetEmergencyContactsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT contact_id, contact_name, relationship, phone, email, first_name, middle_initial, last_name, pronouns, newsletter_opt_in, sms_opt_in FROM student_emergency_contacts WHERE student_id = $student ORDER BY contact_id";
        command.Parameters.AddWithValue("$student", studentId);
        var contacts = new List<EmergencyContactItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) contacts.Add(ReadEmergencyContact(reader));
        return contacts;
    }

    public async Task<EmergencyContactItem> CreateEmergencyContactAsync(int studentId, EmergencyContactRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        var name = ContactName(request.FirstName, request.MiddleInitial, request.LastName, request.Name);
        command.CommandText = "INSERT INTO student_emergency_contacts(student_id, contact_name, first_name, middle_initial, last_name, pronouns, relationship, phone, email, newsletter_opt_in, sms_opt_in, updated_at) VALUES ($student, $name, $first, $middle, $last, $pronouns, $relationship, $phone, $email, $newsletter, $sms, $now); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$name", name); command.Parameters.AddWithValue("$first", (object?)NullIfBlank(request.FirstName) ?? DBNull.Value); command.Parameters.AddWithValue("$middle", (object?)NullIfBlank(request.MiddleInitial) ?? DBNull.Value); command.Parameters.AddWithValue("$last", (object?)NullIfBlank(request.LastName) ?? DBNull.Value); command.Parameters.AddWithValue("$pronouns", (object?)NullIfBlank(request.Pronouns) ?? DBNull.Value); command.Parameters.AddWithValue("$relationship", request.Relationship!.Trim()); command.Parameters.AddWithValue("$phone", (object?)NullIfBlank(request.Phone) ?? DBNull.Value); command.Parameters.AddWithValue("$email", (object?)NullIfBlank(request.Email) ?? DBNull.Value); command.Parameters.AddWithValue("$newsletter", request.NewsletterOptIn ? 1 : 0); command.Parameters.AddWithValue("$sms", request.SmsOptIn ? 1 : 0); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        var contactId = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        return new(contactId, name, request.Relationship.Trim(), NullIfBlank(request.Phone), NullIfBlank(request.Email), NullIfBlank(request.FirstName), NullIfBlank(request.MiddleInitial), NullIfBlank(request.LastName), NullIfBlank(request.Pronouns), request.NewsletterOptIn, request.SmsOptIn);
    }

    public async Task<EmergencyContactItem?> UpdateEmergencyContactAsync(int studentId, long contactId, EmergencyContactRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        var name = ContactName(request.FirstName, request.MiddleInitial, request.LastName, request.Name);
        command.CommandText = "UPDATE student_emergency_contacts SET contact_name = $name, first_name = $first, middle_initial = $middle, last_name = $last, pronouns = $pronouns, relationship = $relationship, phone = $phone, email = $email, newsletter_opt_in = $newsletter, sms_opt_in = $sms, updated_at = $now WHERE contact_id = $contact AND student_id = $student";
        command.Parameters.AddWithValue("$name", name); command.Parameters.AddWithValue("$first", (object?)NullIfBlank(request.FirstName) ?? DBNull.Value); command.Parameters.AddWithValue("$middle", (object?)NullIfBlank(request.MiddleInitial) ?? DBNull.Value); command.Parameters.AddWithValue("$last", (object?)NullIfBlank(request.LastName) ?? DBNull.Value); command.Parameters.AddWithValue("$pronouns", (object?)NullIfBlank(request.Pronouns) ?? DBNull.Value); command.Parameters.AddWithValue("$relationship", request.Relationship!.Trim()); command.Parameters.AddWithValue("$phone", (object?)NullIfBlank(request.Phone) ?? DBNull.Value); command.Parameters.AddWithValue("$email", (object?)NullIfBlank(request.Email) ?? DBNull.Value); command.Parameters.AddWithValue("$newsletter", request.NewsletterOptIn ? 1 : 0); command.Parameters.AddWithValue("$sms", request.SmsOptIn ? 1 : 0); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); command.Parameters.AddWithValue("$contact", contactId); command.Parameters.AddWithValue("$student", studentId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) return null;
        return new(contactId, name, request.Relationship.Trim(), NullIfBlank(request.Phone), NullIfBlank(request.Email), NullIfBlank(request.FirstName), NullIfBlank(request.MiddleInitial), NullIfBlank(request.LastName), NullIfBlank(request.Pronouns), request.NewsletterOptIn, request.SmsOptIn);
    }

    public async Task<bool> DeleteEmergencyContactAsync(int studentId, long contactId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand(); command.CommandText = "DELETE FROM student_emergency_contacts WHERE contact_id = $contact AND student_id = $student"; command.Parameters.AddWithValue("$contact", contactId); command.Parameters.AddWithValue("$student", studentId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private static readonly string[] DefaultPublicTileIds = ["rank", "next-stripe", "total-classes", "monthly-classes", "streak", "achievements", "programs", "favorite-technique", "dojo-motto"];
    private static readonly HashSet<string> PublicTileAllowlist = new(StringComparer.OrdinalIgnoreCase)
    {
        "rank", "next-stripe", "total-classes", "helper-classes", "monthly-classes", "streak", "programs", "favorite-technique", "achievements", "training-track", "dojo-motto"
    };

    private static async Task<(IReadOnlyList<string> Ids, IReadOnlyDictionary<string, string> Sizes)> ReadPublicShowcaseAsync(SqliteConnection connection, int studentId, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT public_tile_ids, public_tile_sizes FROM student_profiles WHERE student_id = $id";
        command.Parameters.AddWithValue("$id", studentId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return (DefaultPublicTileIds, new Dictionary<string, string>());

        var ids = reader.IsDBNull(0) ? null : JsonSerializer.Deserialize<string[]>(reader.GetString(0));
        var sizes = reader.IsDBNull(1) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(reader.GetString(1));
        var safeIds = (ids ?? DefaultPublicTileIds).Where(id => !string.IsNullOrWhiteSpace(id) && PublicTileAllowlist.Contains(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return (safeIds.Length > 0 ? safeIds : DefaultPublicTileIds, sizes ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<PublicProfileTile> BuildPublicShowcase(IReadOnlyList<string> ids, IReadOnlyDictionary<string, string> sizes, string? rank, string? favoriteTechnique, int totalClasses, int classesThisMonth, int helperClasses, int attendanceStreak, int classesIntoStripe, int classesToNextStripe, int classesPerStripe, int achievementCount, IReadOnlyList<ProgramMembershipItem> programs)
    {
        static string Classes(int count) => $"{count} {(count == 1 ? "class" : "classes")}";
        var programNames = programs.Count == 0 ? "Karate progression" : string.Join(" · ", programs.Select(program => program.ProgramName));
        var values = new Dictionary<string, (string Label, string Value, string Description, string Icon, string DefaultSize, int? Progress)>(StringComparer.OrdinalIgnoreCase)
        {
            ["rank"] = ("Current rank", rank ?? "Rank in progress", "The belt or level you are building toward.", "🥋", "feature", null),
            ["next-stripe"] = ("Next stripe", classesPerStripe == 0 ? "Instructor tracked" : $"{classesToNextStripe} classes to go", "Your next training milestone is on the horizon.", "⚡", "medium", classesPerStripe == 0 ? null : Math.Clamp((int)Math.Round(classesIntoStripe * 100d / classesPerStripe), 0, 100)),
            ["total-classes"] = ("Mat time", Classes(totalClasses), "Every class is another rep toward confidence.", "◎", "small", null),
            ["helper-classes"] = ("Helper classes", $"{Classes(helperClasses)} helped", "Showing up for the dojo community.", "✦", "small", null),
            ["monthly-classes"] = ("This month", Classes(classesThisMonth), "Your current training momentum.", "▦", "small", null),
            ["streak"] = ("Training streak", $"{attendanceStreak} {(attendanceStreak == 1 ? "day" : "days")}", "The rhythm you are building on the mat.", "⚡", "small", null),
            ["programs"] = ("Programs", $"{programs.Count} active tracks", "The paths that make up your dojo journey.", "⌘", "medium", null),
            ["favorite-technique"] = ("Favorite focus", favoriteTechnique ?? "Choose a favorite focus", "The technique you are excited to sharpen.", "✺", "medium", null),
            ["achievements"] = ("Achievements", $"{achievementCount} collected", "Milestones worth celebrating.", "★", "medium", null),
            ["training-track"] = ("Training track", programNames, "The tracks you are currently exploring.", "⌁", "wide", null),
            ["dojo-motto"] = ("Dojo motto", "Train with intention.", "Small steps. Strong habits. Keep moving.", "☼", "wide", null)
        };

        return ids.Where(values.ContainsKey).Select(id =>
        {
            var tile = values[id];
            var size = sizes.TryGetValue(id, out var savedSize) && savedSize is "small" or "medium" or "wide" or "feature" ? savedSize : tile.DefaultSize;
            return new PublicProfileTile(id, tile.Label, tile.Value, tile.Description, tile.Icon, size, tile.Progress);
        }).ToArray();
    }

    public async Task UpdateProfileShowcaseAsync(int studentId, StudentProfileShowcaseRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        var ids = (request.TileIds ?? Array.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id) && PublicTileAllowlist.Contains(id)).Distinct(StringComparer.OrdinalIgnoreCase).Take(20).ToArray();
        if (ids.Length == 0) ids = DefaultPublicTileIds;
        var sizes = (request.TileSizes ?? new Dictionary<string, string>()).Where(item => PublicTileAllowlist.Contains(item.Key) && (item.Value is "small" or "medium" or "wide" or "feature")).ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT OR IGNORE INTO student_profiles(student_id, profile_visible) VALUES ($id, 1);
UPDATE student_profiles SET public_tile_ids = $ids, public_tile_sizes = $sizes, updated_at = $now WHERE student_id = $id;";
        command.Parameters.AddWithValue("$id", studentId);
        command.Parameters.AddWithValue("$ids", JsonSerializer.Serialize(ids));
        command.Parameters.AddWithValue("$sizes", JsonSerializer.Serialize(sizes));
        command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<PublicStudentProfile?> GetPublicProfileAsync(int viewerStudentId, int profileStudentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), p.bio, p.favorite_technique, p.profile_image_path,
  (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1),
  s.start_date, (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND a.status = 'present'),
  (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND a.status = 'present' AND strftime('%Y-%m', a.check_in_time) = strftime('%Y-%m', 'now')),
  (SELECT COUNT(1) FROM student_achievements a WHERE a.student_id = s.student_id),
  (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND a.status = 'helper'),
  (SELECT h.awarded_date FROM student_rank_history h WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1)
FROM students s LEFT JOIN student_profiles p ON p.student_id = s.student_id
WHERE s.student_id = $id AND s.active = 1 AND (COALESCE(p.profile_visible, 1) = 1 OR s.student_id = $viewer)";
        command.Parameters.AddWithValue("$id", profileStudentId); command.Parameters.AddWithValue("$viewer", viewerStudentId);
        int id; string displayName; string? pronouns; string? bio; string? favoriteTechnique; string? image; string? rank; string? joinDate; int totalClasses; int classesThisMonth; int achievements; int helperClasses; string? rankAwardedDate;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken)) return null;
            id = reader.GetInt32(0); displayName = reader.GetString(1); bio = reader.IsDBNull(2) ? null : reader.GetString(2); favoriteTechnique = reader.IsDBNull(3) ? null : reader.GetString(3); image = reader.IsDBNull(4) ? null : reader.GetString(4); rank = reader.IsDBNull(5) ? null : reader.GetString(5); joinDate = reader.IsDBNull(6) ? null : reader.GetString(6); totalClasses = reader.GetInt32(7); classesThisMonth = reader.GetInt32(8); achievements = reader.GetInt32(9); helperClasses = reader.GetInt32(10); rankAwardedDate = reader.IsDBNull(11) ? null : reader.GetString(11);
        }
        string? ageGroup;
        await using (var identity = connection.CreateCommand()) { identity.CommandText = "SELECT first_name, last_name, nickname, pronouns, honorific, age_group FROM students WHERE student_id = $id"; identity.Parameters.AddWithValue("$id", id); await using var identityReader = await identity.ExecuteReaderAsync(cancellationToken); await identityReader.ReadAsync(cancellationToken); pronouns = identityReader.IsDBNull(3) ? null : identityReader.GetString(3); ageGroup = identityReader.IsDBNull(5) ? null : identityReader.GetString(5); displayName = BuildStudentDisplayName(identityReader.GetString(0), identityReader.GetString(1), identityReader.IsDBNull(2) ? null : identityReader.GetString(2), identityReader.IsDBNull(4) ? null : identityReader.GetString(4), displayName, rank); }
        var classesPerStripe = rank?.Contains("Black", StringComparison.OrdinalIgnoreCase) == true ? 0 : rank?.Contains("Brown", StringComparison.OrdinalIgnoreCase) == true ? 12 : 8;
        var classesSinceRank = totalClasses;
        if (!string.IsNullOrWhiteSpace(rankAwardedDate))
        {
            await using var rankClasses = connection.CreateCommand(); rankClasses.CommandText = "SELECT COUNT(1) FROM attendance WHERE student_id = $id AND status = 'present' AND date(check_in_time) >= date($awarded)"; rankClasses.Parameters.AddWithValue("$id", id); rankClasses.Parameters.AddWithValue("$awarded", rankAwardedDate); classesSinceRank = Convert.ToInt32(await rankClasses.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }
        var classesIntoStripe = classesPerStripe == 0 ? 0 : classesSinceRank % classesPerStripe;
        var classesToNextStripe = classesPerStripe == 0 ? 0 : classesPerStripe - classesIntoStripe;
        var stripesEarned = classesPerStripe == 0 ? 0 : classesSinceRank / classesPerStripe;
        var nextMilestone = classesPerStripe == 0 ? "Next degree · instructor tracked" : "Next stripe";
        var programs = new List<ProgramMembershipItem>();
        await using (var programCommand = connection.CreateCommand())
        {
            programCommand.CommandText = "SELECT program_name, program_code, progression_type, level_name, enrolled_date FROM student_program_memberships WHERE student_id = $id AND active = 1 ORDER BY program_name"; programCommand.Parameters.AddWithValue("$id", id);
            await using var programReader = await programCommand.ExecuteReaderAsync(cancellationToken); while (await programReader.ReadAsync(cancellationToken)) programs.Add(new(programReader.GetString(0), programReader.GetString(1), programReader.GetString(2), programReader.IsDBNull(3) ? null : programReader.GetString(3), programReader.IsDBNull(4) ? null : programReader.GetString(4)));
        }
        if (programs.Count == 0 && !string.IsNullOrWhiteSpace(rank))
        {
            var coreProgram = string.Equals(ageGroup, "Kids", StringComparison.OrdinalIgnoreCase) ? (Name: "Kids", Code: "kids") : (Name: "Teen/Adult", Code: "teen-adult");
            programs.Add(new(coreProgram.Name, coreProgram.Code, "belt", rank, joinDate));
        }
        var recentPosts = new List<PublicProfilePost>();
        await using (var postCommand = connection.CreateCommand())
        {
            postCommand.CommandText = @"SELECT p.post_id, p.post_text, COALESCE(p.post_kind, 'training'), p.created_at,
  (SELECT COUNT(1) FROM post_reactions r WHERE r.post_id = p.post_id),
  (SELECT COUNT(1) FROM post_comments c WHERE c.post_id = p.post_id AND c.visible = 1 AND c.moderation_status = 'approved')
FROM posts p WHERE p.student_id = $id AND p.post_type = 'student' AND p.visible = 1 AND p.moderation_status = 'approved' ORDER BY p.created_at DESC LIMIT 8"; postCommand.Parameters.AddWithValue("$id", id);
            await using var postReader = await postCommand.ExecuteReaderAsync(cancellationToken); while (await postReader.ReadAsync(cancellationToken)) recentPosts.Add(new(postReader.GetInt64(0), postReader.GetString(1), postReader.GetString(2), DateTime.Parse(postReader.GetString(3), CultureInfo.InvariantCulture), postReader.GetInt32(4), postReader.GetInt32(5)));
        }
        var publicAchievements = (await GetAchievementsAsync(id, cancellationToken))
            .Select(item => new PublicAchievement(item.Name, item.Description, item.IconName, item.AwardedAt))
            .ToArray();
        var showcase = await ReadPublicShowcaseAsync(connection, id, cancellationToken);
        var attendanceStreak = await GetAttendanceStreakAsync(connection, id, cancellationToken);
        var featuredTiles = BuildPublicShowcase(showcase.Ids, showcase.Sizes, rank, favoriteTechnique, totalClasses, classesThisMonth, helperClasses, attendanceStreak, classesIntoStripe, classesToNextStripe, classesPerStripe, achievements, programs);
        return new(id, displayName, pronouns, bio, favoriteTechnique, image, rank, joinDate, totalClasses, classesThisMonth, achievements, helperClasses, classesIntoStripe, classesPerStripe, classesToNextStripe, stripesEarned, nextMilestone, programs, publicAchievements, recentPosts, featuredTiles);
    }

    public async Task UpdateProfileAsync(int studentId, StudentProfileUpdateRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using (var profile = connection.CreateCommand())
        {
            profile.Transaction = (SqliteTransaction)transaction;
            profile.CommandText = @"
INSERT OR IGNORE INTO student_profiles(student_id, display_name, profile_visible)
VALUES ($id, NULL, 1);
UPDATE student_profiles
SET display_name = $displayName,
    bio = $bio,
    favorite_technique = $favoriteTechnique,
    updated_at = $now
WHERE student_id = $id;";
            profile.Parameters.AddWithValue("$id", studentId);
            profile.Parameters.AddWithValue("$displayName", (object?)NullIfBlank(request.DisplayName) ?? DBNull.Value);
            profile.Parameters.AddWithValue("$bio", (object?)NullIfBlank(request.Bio) ?? DBNull.Value);
            profile.Parameters.AddWithValue("$favoriteTechnique", (object?)NullIfBlank(request.FavoriteTechnique) ?? DBNull.Value);
            profile.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
            await profile.ExecuteNonQueryAsync(cancellationToken);
        }
        await using (var student = connection.CreateCommand())
        {
            student.Transaction = (SqliteTransaction)transaction;
            student.CommandText = @"UPDATE students SET nickname = $nickname, pronouns = $pronouns, email = $email, phone = $phone, uniform_size = $uniformSize, belt_size = $beltSize, updated_at = $now WHERE student_id = $id AND active = 1";
            student.Parameters.AddWithValue("$id", studentId);
            student.Parameters.AddWithValue("$nickname", (object?)NullIfBlank(request.Nickname) ?? DBNull.Value);
            student.Parameters.AddWithValue("$pronouns", (object?)NullIfBlank(request.Pronouns) ?? DBNull.Value);
            student.Parameters.AddWithValue("$email", (object?)NullIfBlank(request.Email) ?? DBNull.Value);
            student.Parameters.AddWithValue("$phone", (object?)NullIfBlank(request.Phone) ?? DBNull.Value);
            student.Parameters.AddWithValue("$uniformSize", (object?)NullIfBlank(request.UniformSize) ?? DBNull.Value);
            student.Parameters.AddWithValue("$beltSize", (object?)NullIfBlank(request.BeltSize) ?? DBNull.Value);
            student.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
            await student.ExecuteNonQueryAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static IReadOnlyList<string> NormalizeStudentRoles(IEnumerable<string>? roles)
    {
        var allowed = new[] { "student", "assistant_instructor", "instructor", "news_admin" };
        var selected = (roles ?? ["student"])
            .Select(role => role.Trim().ToLowerInvariant())
            .Where(role => allowed.Contains(role, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (!selected.Contains("student", StringComparer.OrdinalIgnoreCase)) selected.Insert(0, "student");
        return selected;
    }

    private static string ContactName(string? firstName, string? middleInitial, string? lastName, string? fallback)
    {
        var parts = new[] { NullIfBlank(firstName), NullIfBlank(middleInitial), NullIfBlank(lastName) }.Where(value => !string.IsNullOrWhiteSpace(value));
        return string.Join(' ', parts).Trim() is { Length: > 0 } name ? name : (NullIfBlank(fallback) ?? "Unnamed contact");
    }

    private static EmergencyContactItem ReadEmergencyContact(SqliteDataReader reader)
    {
        return new(
            reader.GetInt64(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? null : reader.GetString(8),
            !reader.IsDBNull(9) && reader.GetInt32(9) != 0,
            !reader.IsDBNull(10) && reader.GetInt32(10) != 0);
    }

    private static string BuildStudentDisplayName(string firstName, string lastName, string? nickname, string? honorific, string? profileDisplayName, string? rankName)
    {
        if (rankName?.Contains("Black", StringComparison.OrdinalIgnoreCase) == true && !string.IsNullOrWhiteSpace(honorific)) return $"{honorific.Trim()} {lastName.Trim()}";
        return NullIfBlank(nickname) ?? NullIfBlank(profileDisplayName) ?? $"{firstName.Trim()} {lastName.Trim()}";
    }

    public async Task<PreEnrollResult> PreEnrollAsync(int studentId, int sessionId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var session = connection.CreateCommand();
        session.CommandText = @"SELECT c.class_name,
    (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = $student ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1)
FROM class_sessions s JOIN classes c ON c.class_id = s.class_id JOIN students st ON st.student_id = $student
WHERE s.session_id = $session
  AND date(s.session_date) = date('now', 'localtime')
  AND s.start_time IS NOT NULL
  AND time(s.start_time) > time('now', 'localtime')
  AND time(s.start_time) = (SELECT MIN(time(next_s.start_time)) FROM class_sessions next_s WHERE date(next_s.session_date) = date('now', 'localtime') AND next_s.cancelled = 0 AND next_s.start_time IS NOT NULL AND time(next_s.start_time) > time('now', 'localtime'))
  AND s.cancelled = 0
  AND st.active = 1
  AND COALESCE(NULLIF(st.status, ''), 'active') = 'active'";
        session.Parameters.AddWithValue("$student", studentId);
        session.Parameters.AddWithValue("$session", sessionId);
        await using var reader = await session.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return new(false, false, "Only the next upcoming class can accept a reservation.");
        var className = reader.GetString(0);
        var currentRank = reader.IsDBNull(1) ? null : reader.GetString(1);
        var hasStudentBelt = TryGetBeltRankOrder(currentRank, out var studentOrder);
        var hasClassBeltThreshold = TryGetLowestClassBeltOrder(className, out var requiredOrder, out var requiredRank);
        if (hasClassBeltThreshold && (!hasStudentBelt || studentOrder < requiredOrder))
            return new(false, false, $"This class is for {requiredRank} and above. A {currentRank ?? "student without a belt rank"} cannot reserve it.");

        await reader.DisposeAsync();
        await using var signup = connection.CreateCommand();
        signup.CommandText = "INSERT INTO student_class_signups(session_id, student_id, signed_up_at, status) VALUES ($session, $student, $now, 'reserved')";
        signup.Parameters.AddWithValue("$session", sessionId);
        signup.Parameters.AddWithValue("$student", studentId);
        signup.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        try
        {
            await signup.ExecuteNonQueryAsync(cancellationToken);
            return new(true, false, null);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return new(false, true, "This student is already on the list for this class.");
        }
    }

    public Task<CheckInResult> CheckInAsync(int studentId, int sessionId, CancellationToken cancellationToken = default) => CheckInAsync(studentId, sessionId, false, cancellationToken);

    public async Task<DojoCheckInSummary> GetDojoCheckInSummaryAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT
  EXISTS(SELECT 1 FROM dojo_check_ins WHERE student_id = $student AND check_in_date = date('now', 'localtime')),
  (SELECT COUNT(1) FROM dojo_check_ins WHERE student_id = $student),
  (SELECT COUNT(1) FROM dojo_check_ins WHERE student_id = $student AND strftime('%Y-%m', check_in_date) = strftime('%Y-%m', 'now', 'localtime')),
  (SELECT COUNT(1) FROM dojo_check_ins WHERE check_in_date = date('now', 'localtime')),
  (SELECT check_in_date FROM dojo_check_ins WHERE student_id = $student ORDER BY check_in_date DESC, checked_in_at DESC LIMIT 1)";
        command.Parameters.AddWithValue("$student", studentId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return new(false, 0, 0, 0, null);
        return new(reader.GetInt32(0) == 1, reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.IsDBNull(4) ? null : reader.GetString(4));
    }

    public async Task<DojoCheckInResult> CheckInAtDojoAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        var today = DateTime.Now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO dojo_check_ins(student_id, location_name, check_in_date, checked_in_at) VALUES ($student, 'Main studio', $date, $now)";
        command.Parameters.AddWithValue("$student", studentId);
        command.Parameters.AddWithValue("$date", today);
        command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
            await AwardAutomaticMilestonesAsync(connection, studentId, cancellationToken);
            return new(true, false, null, await GetDojoCheckInSummaryAsync(studentId, cancellationToken));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return new(false, true, "You have already checked in at the studio today. Come back tomorrow for another HIYAH!.", await GetDojoCheckInSummaryAsync(studentId, cancellationToken));
        }
    }

    public async Task<DojoLeaderboard> GetDojoLeaderboardAsync(int currentStudentId, string? period, CancellationToken cancellationToken = default)
    {
        var normalizedPeriod = string.Equals(period, "year", StringComparison.OrdinalIgnoreCase) ? "year" : "month";
        var now = DateTime.Now;
        var start = normalizedPeriod == "year" ? new DateTime(now.Year, 1, 1) : new DateTime(now.Year, now.Month, 1);
        var end = normalizedPeriod == "year" ? start.AddYears(1) : start.AddMonths(1);
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), COUNT(d.check_in_id)
FROM students s
LEFT JOIN student_profiles p ON p.student_id = s.student_id
LEFT JOIN dojo_check_ins d ON d.student_id = s.student_id AND d.check_in_date >= $start AND d.check_in_date < $end
WHERE s.active = 1 AND (COALESCE(p.profile_visible, 1) = 1 OR s.student_id = $current)
GROUP BY s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name))
HAVING COUNT(d.check_in_id) > 0 OR s.student_id = $current
ORDER BY COUNT(d.check_in_id) DESC, UPPER(COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name))) COLLATE NOCASE
LIMIT 100";
        command.Parameters.AddWithValue("$start", start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$end", end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$current", currentStudentId);
        var rows = new List<(int StudentId, string DisplayName, int CheckIns)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) rows.Add((reader.GetInt32(0), reader.GetString(1), Convert.ToInt32(reader.GetValue(2), CultureInfo.InvariantCulture)));
        var entries = new List<DojoLeaderboardEntry>();
        var rank = 0; var previousCount = -1;
        for (var index = 0; index < rows.Count; index++)
        {
            if (rows[index].CheckIns != previousCount) rank = index + 1;
            previousCount = rows[index].CheckIns;
            entries.Add(new(rank, rows[index].StudentId, rows[index].StudentId == currentStudentId ? $"{rows[index].DisplayName} (You)" : rows[index].DisplayName, rows[index].CheckIns, rows[index].StudentId == currentStudentId));
        }
        var label = normalizedPeriod == "year" ? start.ToString("yyyy", CultureInfo.InvariantCulture) : start.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        return new(normalizedPeriod, label, entries);
    }

    public async Task<DojoLeaderboard> GetPortalAdminDojoLeaderboardAsync(string? period, CancellationToken cancellationToken = default)
    {
        var normalizedPeriod = string.Equals(period, "year", StringComparison.OrdinalIgnoreCase) ? "year" : "month";
        var now = DateTime.Now;
        var start = normalizedPeriod == "year" ? new DateTime(now.Year, 1, 1) : new DateTime(now.Year, now.Month, 1);
        var end = normalizedPeriod == "year" ? start.AddYears(1) : start.AddMonths(1);
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), COUNT(d.check_in_id)
FROM students s
LEFT JOIN student_profiles p ON p.student_id = s.student_id
LEFT JOIN dojo_check_ins d ON d.student_id = s.student_id AND d.check_in_date >= $start AND d.check_in_date < $end
WHERE s.active = 1
GROUP BY s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name))
HAVING COUNT(d.check_in_id) > 0
ORDER BY COUNT(d.check_in_id) DESC, UPPER(COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name))) COLLATE NOCASE
LIMIT 100";
        command.Parameters.AddWithValue("$start", start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$end", end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        var entries = new List<DojoLeaderboardEntry>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rank = 0; var previousCount = -1; var index = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            var count = reader.GetInt32(2);
            if (count != previousCount) rank = index + 1;
            entries.Add(new(rank, reader.GetInt32(0), reader.GetString(1), count, false));
            previousCount = count; index++;
        }
        var label = normalizedPeriod == "year" ? start.ToString("yyyy", CultureInfo.InvariantCulture) : start.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        return new(normalizedPeriod, label, entries);
    }

    public async Task<IReadOnlyList<PortalAdminCheckInItem>> GetPortalAdminCheckInsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT d.check_in_id, d.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), d.location_name, d.check_in_date, d.checked_in_at
FROM dojo_check_ins d JOIN students s ON s.student_id = d.student_id LEFT JOIN student_profiles p ON p.student_id = s.student_id
WHERE d.check_in_date >= $from AND d.check_in_date < $to
ORDER BY d.check_in_date DESC, d.checked_in_at DESC, 3 COLLATE NOCASE
LIMIT 500";
        command.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        var items = new List<PortalAdminCheckInItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var date = DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture);
            var timestamp = DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture);
            items.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), date, timestamp));
        }
        return items;
    }

    public async Task<CheckInResult> CheckInAsync(int studentId, int sessionId, bool helper, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var session = connection.CreateCommand();
        session.CommandText = @"SELECT c.class_name,
    (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = $student ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1)
FROM class_sessions s JOIN classes c ON c.class_id = s.class_id JOIN students st ON st.student_id = $student
WHERE s.session_id = $session
  AND date(s.session_date) = date('now', 'localtime')
  AND (s.start_time IS NULL OR (time('now', 'localtime') >= time(s.start_time) AND (s.end_time IS NULL OR time('now', 'localtime') < time(s.end_time))))
  AND s.cancelled = 0
  AND st.active = 1
  AND COALESCE(NULLIF(st.status, ''), 'active') = 'active'";
        session.Parameters.AddWithValue("$student", studentId);
        session.Parameters.AddWithValue("$session", sessionId);
        await using var sessionReader = await session.ExecuteReaderAsync(cancellationToken);
        if (!await sessionReader.ReadAsync(cancellationToken)) return new(false, false, false, "Only a class currently in progress can accept a check-in.");
        var className = sessionReader.GetString(0);
        var currentRank = sessionReader.IsDBNull(1) ? null : sessionReader.GetString(1);
        var hasStudentBelt = TryGetBeltRankOrder(currentRank, out var studentOrder);
        var hasClassBeltThreshold = TryGetLowestClassBeltOrder(className, out var requiredOrder, out var requiredRank);
        var recordAsHelper = helper || (hasClassBeltThreshold && !IsAllBeltClass(className) && hasStudentBelt && studentOrder > requiredOrder);
        if (!recordAsHelper && hasClassBeltThreshold && (!hasStudentBelt || studentOrder < requiredOrder))
            return new(false, false, false, $"This class is for {requiredRank} and above. A {currentRank ?? "student without a belt rank"} cannot check in here.");
        if (recordAsHelper)
        {
            if (!hasStudentBelt || !hasClassBeltThreshold)
                return new(false, false, false, "Helper check-in is only available for belt-based classes when the student has a current belt rank.");
            if (studentOrder <= requiredOrder)
                return new(false, false, false, $"A {currentRank} student cannot help a {requiredRank} class. Helpers must outrank the class threshold.");
        }

        await sessionReader.DisposeAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO attendance(session_id, student_id, check_in_time, status) VALUES ($session, $student, $now, $status)";
        command.Parameters.AddWithValue("$session", sessionId);
        command.Parameters.AddWithValue("$student", studentId);
        command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$status", recordAsHelper ? "helper" : "present");
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
            await using var reservation = connection.CreateCommand();
            reservation.CommandText = "UPDATE student_class_signups SET status = 'attended' WHERE session_id = $session AND student_id = $student AND status = 'reserved'";
            reservation.Parameters.AddWithValue("$session", sessionId);
            reservation.Parameters.AddWithValue("$student", studentId);
            await reservation.ExecuteNonQueryAsync(cancellationToken);
            await AwardAutomaticMilestonesAsync(connection, studentId, cancellationToken);
            return new(true, false, recordAsHelper, null);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return new(false, true, false, "This student is already checked in for this class session.");
        }
    }

    private static bool TryGetLowestClassBeltOrder(string className, out int order, out string rank)
    {
        order = int.MaxValue; rank = string.Empty;
        foreach (var (name, value) in BeltRankOrders)
        {
            if (className.Contains(name, StringComparison.OrdinalIgnoreCase) && value < order) { order = value; rank = $"{name} Belt"; }
        }
        if (order == int.MaxValue) { order = 0; rank = string.Empty; return false; }
        return true;
    }

    private static bool IsAllBeltClass(string className)
    {
        var normalized = className.ToLowerInvariant();
        return normalized.Contains("all belt") || (normalized.Contains("belt") && normalized.Contains("up") && (normalized.Contains("white") || normalized.Contains("gold") || normalized.Contains("orange")));
    }

    private static bool TryGetBeltRankOrder(string? rankName, out int order)
    {
        order = 0;
        if (string.IsNullOrWhiteSpace(rankName)) return false;
        foreach (var (name, value) in BeltRankOrders)
        {
            if (rankName.Contains(name, StringComparison.OrdinalIgnoreCase)) { order = value; return true; }
        }
        return false;
    }

    private static readonly IReadOnlyList<(string Name, int Order)> BeltRankOrders =
    [
        ("White", 10), ("Gold", 20), ("Orange", 30), ("Green", 40), ("Purple", 50),
        ("Blue", 60), ("Red", 70), ("Brown", 80), ("Black", 90)
    ];

    public async Task<IReadOnlyList<StudentSummary>> SearchStudentsAsync(int currentStudentId, string query, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), p.profile_image_path FROM students s LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE s.active = 1 AND s.student_id <> $id AND COALESCE(p.profile_visible, 1) = 1 AND (COALESCE(p.display_name, '') LIKE $query OR s.first_name LIKE $query OR s.last_name LIKE $query) ORDER BY 2 LIMIT 25"; command.Parameters.AddWithValue("$id", currentStudentId); command.Parameters.AddWithValue("$query", $"%{query.Trim()}%"); var list = new List<StudentSummary>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3))); return list;
    }

    public async Task<IReadOnlyList<FriendshipItem>> GetFriendsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT CASE WHEN f.student_id = $id THEN f.friend_id ELSE f.student_id END, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC LIMIT 1), f.status, CASE WHEN f.requested_by <> $id THEN 1 ELSE 0 END FROM student_friendships f JOIN students s ON s.student_id = CASE WHEN f.student_id = $id THEN f.friend_id ELSE f.student_id END LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE f.friend_id = $id OR f.student_id = $id ORDER BY f.status, 2"; command.Parameters.AddWithValue("$id", studentId); var list = new List<FriendshipItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.GetString(3), reader.GetInt32(4) != 0)); return list;
    }

    public async Task<string> RequestFriendAsync(int studentId, int friendId, CancellationToken cancellationToken = default)
    {
        if (studentId == friendId) return "invalid"; await using var connection = await OpenAsync(cancellationToken); await using var exists = connection.CreateCommand(); exists.CommandText = "SELECT status FROM student_friendships WHERE student_id = $a AND friend_id = $b"; exists.Parameters.AddWithValue("$a", studentId); exists.Parameters.AddWithValue("$b", friendId); var current = await exists.ExecuteScalarAsync(cancellationToken); if (current is not null) return Convert.ToString(current, CultureInfo.InvariantCulture) ?? "invalid";
        await using var reverse = connection.CreateCommand(); reverse.CommandText = "SELECT status FROM student_friendships WHERE student_id = $b AND friend_id = $a"; reverse.Parameters.AddWithValue("$a", studentId); reverse.Parameters.AddWithValue("$b", friendId); var reverseStatus = await reverse.ExecuteScalarAsync(cancellationToken); if (reverseStatus is not null) return Convert.ToString(reverseStatus, CultureInfo.InvariantCulture) ?? "invalid";
        await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO student_friendships(student_id, friend_id, status, requested_by, created_at, updated_at) VALUES ($a, $b, 'pending', $a, $now, $now)"; command.Parameters.AddWithValue("$a", studentId); command.Parameters.AddWithValue("$b", friendId); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken); return "pending";
    }

    public async Task<bool> RespondToFriendAsync(int studentId, int friendId, bool accept, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "UPDATE student_friendships SET status = $status, updated_at = $now WHERE student_id = $friendId AND friend_id = $studentId AND status = 'pending'"; command.Parameters.AddWithValue("$status", accept ? "accepted" : "declined"); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); command.Parameters.AddWithValue("$friendId", friendId); command.Parameters.AddWithValue("$studentId", studentId); return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> AreFriendsAsync(int first, int second, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT COUNT(1) FROM student_friendships WHERE status = 'accepted' AND ((student_id = $a AND friend_id = $b) OR (student_id = $b AND friend_id = $a))"; command.Parameters.AddWithValue("$a", first); command.Parameters.AddWithValue("$b", second); return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) > 0;
    }

    public async Task<IReadOnlyList<MessageItem>> GetMessagesAsync(int studentId, int friendId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using (var markRead = connection.CreateCommand())
        {
            markRead.CommandText = "UPDATE student_messages SET read_at = $now WHERE recipient_id = $me AND sender_id = $friend AND read_at IS NULL";
            markRead.Parameters.AddWithValue("$me", studentId); markRead.Parameters.AddWithValue("$friend", friendId); markRead.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
            await markRead.ExecuteNonQueryAsync(cancellationToken);
        }
        await using var command = connection.CreateCommand(); command.CommandText = "SELECT m.message_id, m.sender_id, COALESCE(ps.display_name, TRIM(ss.first_name || ' ' || ss.last_name)), m.recipient_id, COALESCE(pr.display_name, TRIM(sr.first_name || ' ' || sr.last_name)), m.message_text, m.created_at, m.read_at IS NOT NULL, EXISTS (SELECT 1 FROM student_message_pins pin WHERE pin.message_id = m.message_id AND pin.student_id = $me) FROM student_messages m JOIN students ss ON ss.student_id = m.sender_id JOIN students sr ON sr.student_id = m.recipient_id LEFT JOIN student_profiles ps ON ps.student_id = ss.student_id LEFT JOIN student_profiles pr ON pr.student_id = sr.student_id WHERE ((m.sender_id = $me AND m.recipient_id = $friend) OR (m.sender_id = $friend AND m.recipient_id = $me)) ORDER BY m.created_at LIMIT 200"; command.Parameters.AddWithValue("$me", studentId); command.Parameters.AddWithValue("$friend", friendId); var list = new List<MessageItem>(); await using (var reader = await command.ExecuteReaderAsync(cancellationToken)) while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3), reader.GetString(4), reader.GetString(5), DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture), reader.GetBoolean(7), reader.GetBoolean(8), Array.Empty<ReactionSummary>())); for (var index = 0; index < list.Count; index++) list[index] = list[index] with { Reactions = await GetMessageReactionSummaryAsync(connection, list[index].MessageId, studentId, cancellationToken) }; return list;
    }

    public async Task<long> SendMessageAsync(int senderId, int recipientId, string text, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO student_messages(sender_id, recipient_id, message_text, created_at) VALUES ($sender, $recipient, $text, $now)"; command.Parameters.AddWithValue("$sender", senderId); command.Parameters.AddWithValue("$recipient", recipientId); command.Parameters.AddWithValue("$text", text.Trim()); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken); await using var idCommand = connection.CreateCommand(); idCommand.CommandText = "SELECT last_insert_rowid()"; return Convert.ToInt64(await idCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public Task<bool> ReportMessageAsync(int studentId, long messageId, string reason, CancellationToken cancellationToken = default) => CreateContentReportAsync("message", messageId, studentId, reason, cancellationToken);
    public Task<bool> ReportPostAsync(int studentId, long postId, string reason, CancellationToken cancellationToken = default) => CreateContentReportAsync("post", postId, studentId, reason, cancellationToken);
    public Task<bool> ReportCommentAsync(int studentId, long commentId, string reason, CancellationToken cancellationToken = default) => CreateContentReportAsync("comment", commentId, studentId, reason, cancellationToken);

    private async Task<bool> CreateContentReportAsync(string contentType, long contentId, int reporterId, string reason, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var lookup = connection.CreateCommand();
        lookup.CommandText = contentType switch
        {
            "post" => "SELECT COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name), 'Task Karate'), po.post_text FROM posts po LEFT JOIN students s ON s.student_id = po.student_id LEFT JOIN student_profiles p ON p.student_id = po.student_id WHERE po.post_id = $id AND po.post_type = 'student'",
            "comment" => "SELECT COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name), 'Student'), c.comment_text FROM post_comments c JOIN students s ON s.student_id = c.student_id LEFT JOIN student_profiles p ON p.student_id = c.student_id WHERE c.comment_id = $id",
            _ => "SELECT COALESCE(TRIM(s.first_name || ' ' || s.last_name), 'Student'), m.message_text FROM student_messages m JOIN students s ON s.student_id = m.sender_id WHERE m.message_id = $id AND (m.sender_id = $reporter OR m.recipient_id = $reporter)"
        };
        lookup.Parameters.AddWithValue("$id", contentId); if (contentType == "message") lookup.Parameters.AddWithValue("$reporter", reporterId);
        await using var reader = await lookup.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return false;
        var subjectAuthor = reader.GetString(0); var contentText = reader.GetString(1); await reader.DisposeAsync();
        var cleanReason = string.IsNullOrWhiteSpace(reason) ? "safety" : reason.Trim()[..Math.Min(80, reason.Trim().Length)];
        var now = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var report = connection.CreateCommand(); report.Transaction = (SqliteTransaction)transaction; report.CommandText = "INSERT OR IGNORE INTO student_content_reports(content_type, content_id, reporter_id, reason, status, created_at) VALUES ($type, $content, $reporter, $reason, 'open', $now)"; report.Parameters.AddWithValue("$type", contentType); report.Parameters.AddWithValue("$content", contentId); report.Parameters.AddWithValue("$reporter", reporterId); report.Parameters.AddWithValue("$reason", cleanReason); report.Parameters.AddWithValue("$now", now);
        if (await report.ExecuteNonQueryAsync(cancellationToken) == 0) return true;
        await using var alert = connection.CreateCommand(); alert.Transaction = (SqliteTransaction)transaction; alert.CommandText = "INSERT INTO admin_alerts(alert_type, title, body, entity_type, entity_id, status, created_at) VALUES ('moderation', $title, $body, $entity, $id, 'open', $now)"; alert.Parameters.AddWithValue("$title", "Reported " + contentType); alert.Parameters.AddWithValue("$body", contentText[..Math.Min(500, contentText.Length)]); alert.Parameters.AddWithValue("$entity", contentType); alert.Parameters.AddWithValue("$id", contentId); alert.Parameters.AddWithValue("$now", now); await alert.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await TrySendReportEmailAsync(contentType, contentId, subjectAuthor, contentText, cleanReason, cancellationToken);
        return true;
    }

    private async Task TrySendReportEmailAsync(string contentType, long contentId, string subjectAuthor, string contentText, string reason, CancellationToken cancellationToken)
    {
        var host = _configuration["Notifications:Smtp:Host"] ?? Environment.GetEnvironmentVariable("TASK_KARATE_SMTP_HOST");
        var destination = _configuration["Notifications:SchoolReportEmail"] ?? Environment.GetEnvironmentVariable("TASK_KARATE_SCHOOL_REPORT_EMAIL");
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(destination)) { _logger.LogInformation("Moderation report {ContentType} {ContentId} recorded without email; configure Notifications:Smtp:Host and Notifications:SchoolReportEmail to notify the school.", contentType, contentId); return; }
        try
        {
            using var client = new SmtpClient(host, int.TryParse(_configuration["Notifications:Smtp:Port"], out var port) ? port : 587) { EnableSsl = true };
            var user = _configuration["Notifications:Smtp:User"] ?? Environment.GetEnvironmentVariable("TASK_KARATE_SMTP_USER"); var password = _configuration["Notifications:Smtp:Password"] ?? Environment.GetEnvironmentVariable("TASK_KARATE_SMTP_PASSWORD"); if (!string.IsNullOrWhiteSpace(user)) client.Credentials = new NetworkCredential(user, password);
            var from = _configuration["Notifications:Smtp:From"] ?? user ?? destination;
            using var mail = new MailMessage(from, destination, $"Task Karate moderation report: {contentType}", $"A student reported a {contentType} by {subjectAuthor}.\n\nReason: {reason}\nContent ID: {contentId}\n\n{contentText}");
            await client.SendMailAsync(mail, cancellationToken);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not send moderation report email for {ContentType} {ContentId}; the admin alert remains available.", contentType, contentId); }
    }

    public async Task<bool> ToggleMessagePinAsync(int studentId, long messageId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var access = connection.CreateCommand(); access.CommandText = "SELECT COUNT(1) FROM student_messages WHERE message_id = $message AND (sender_id = $student OR recipient_id = $student)"; access.Parameters.AddWithValue("$message", messageId); access.Parameters.AddWithValue("$student", studentId);
        if (Convert.ToInt32(await access.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) != 1) return false;
        await using var existing = connection.CreateCommand(); existing.CommandText = "SELECT COUNT(1) FROM student_message_pins WHERE message_id = $message AND student_id = $student"; existing.Parameters.AddWithValue("$message", messageId); existing.Parameters.AddWithValue("$student", studentId);
        var pinned = Convert.ToInt32(await existing.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 1;
        await using var command = connection.CreateCommand(); command.Parameters.AddWithValue("$message", messageId); command.Parameters.AddWithValue("$student", studentId);
        if (pinned) command.CommandText = "DELETE FROM student_message_pins WHERE message_id = $message AND student_id = $student";
        else { command.CommandText = "INSERT INTO student_message_pins(message_id, student_id, created_at) VALUES ($message, $student, $now)"; command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); }
        await command.ExecuteNonQueryAsync(cancellationToken); return !pinned;
    }

    public async Task<bool> MessageBelongsToStudentAsync(int studentId, long messageId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT COUNT(1) FROM student_messages WHERE message_id = $message AND (sender_id = $student OR recipient_id = $student)"; command.Parameters.AddWithValue("$message", messageId); command.Parameters.AddWithValue("$student", studentId); return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 1;
    }

    public async Task<bool> ToggleMessageReactionAsync(int studentId, long messageId, string reactionCode, CancellationToken cancellationToken = default)
    {
        if (reactionCode is not ("fist_bump" or "respect" or "fire" or "heart" or "clap")) return false;
        await using var connection = await OpenAsync(cancellationToken);
        await using var access = connection.CreateCommand(); access.CommandText = "SELECT COUNT(1) FROM student_messages WHERE message_id = $message AND (sender_id = $student OR recipient_id = $student)"; access.Parameters.AddWithValue("$message", messageId); access.Parameters.AddWithValue("$student", studentId); if (Convert.ToInt32(await access.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) != 1) return false;
        await using var existing = connection.CreateCommand(); existing.CommandText = "SELECT reaction_code FROM student_message_reactions WHERE message_id = $message AND student_id = $student"; existing.Parameters.AddWithValue("$message", messageId); existing.Parameters.AddWithValue("$student", studentId); var currentValue = await existing.ExecuteScalarAsync(cancellationToken); var current = currentValue is null || currentValue == DBNull.Value ? null : Convert.ToString(currentValue, CultureInfo.InvariantCulture);
        await using var command = connection.CreateCommand();
        if (current == reactionCode) command.CommandText = "DELETE FROM student_message_reactions WHERE message_id = $message AND student_id = $student";
        else if (current is not null) { command.CommandText = "UPDATE student_message_reactions SET reaction_code = $reaction, created_at = $now WHERE message_id = $message AND student_id = $student"; command.Parameters.AddWithValue("$reaction", reactionCode); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); }
        else { command.CommandText = "INSERT INTO student_message_reactions(message_id, student_id, reaction_code, created_at) VALUES ($message, $student, $reaction, $now)"; command.Parameters.AddWithValue("$reaction", reactionCode); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); }
        command.Parameters.AddWithValue("$message", messageId); command.Parameters.AddWithValue("$student", studentId); await command.ExecuteNonQueryAsync(cancellationToken); return true;
    }

    private static async Task<IReadOnlyList<ReactionSummary>> GetMessageReactionSummaryAsync(SqliteConnection connection, long messageId, int studentId, CancellationToken cancellationToken)
    {
        var list = new List<ReactionSummary>(); await using var command = connection.CreateCommand(); command.CommandText = "SELECT reaction_code, COUNT(1), MAX(CASE WHEN student_id = $student THEN 1 ELSE 0 END) FROM student_message_reactions WHERE message_id = $message GROUP BY reaction_code"; command.Parameters.AddWithValue("$message", messageId); command.Parameters.AddWithValue("$student", studentId); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) { var code = reader.GetString(0); var label = code switch { "fist_bump" => "Fist bump", "respect" => "Nice work", "fire" => "Fire", "heart" => "Care", "clap" => "Applause", _ => code }; var icon = code switch { "fist_bump" => "👊", "respect" => "🌟", "fire" => "🔥", "heart" => "❤️", "clap" => "👏", _ => "✦" }; list.Add(new(code, label, icon, reader.GetInt32(1), reader.GetInt32(2) != 0)); } return list;
    }

    private static async Task EnsureLegacyStudentColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        foreach (var column in new[] { (Name: "age_group", Definition: "TEXT"), (Name: "uniform_size", Definition: "TEXT"), (Name: "belt_size", Definition: "TEXT"), (Name: "nickname", Definition: "TEXT"), (Name: "pronouns", Definition: "TEXT"), (Name: "honorific", Definition: "TEXT"), (Name: "roles", Definition: "TEXT"), (Name: "active", Definition: "INTEGER NOT NULL DEFAULT 1"), (Name: "status", Definition: "TEXT NOT NULL DEFAULT 'active'") })
        {
            await using var check = connection.CreateCommand(); check.CommandText = "SELECT COUNT(1) FROM pragma_table_info('students') WHERE name = $name"; check.Parameters.AddWithValue("$name", column.Name);
            if (Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 0)
            {
                await using var alter = connection.CreateCommand(); alter.CommandText = $"ALTER TABLE students ADD COLUMN {column.Name} {column.Definition}"; await alter.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        await using (var statusNormalize = connection.CreateCommand()) { statusNormalize.CommandText = "UPDATE students SET status = CASE WHEN active = 1 THEN 'active' ELSE 'deactivated' END WHERE status IS NULL OR TRIM(status) = ''"; await statusNormalize.ExecuteNonQueryAsync(cancellationToken); }
        await using var guardianCheck = connection.CreateCommand(); guardianCheck.CommandText = "SELECT COUNT(1) FROM pragma_table_info('guardians') WHERE name = 'active'";
        if (Convert.ToInt32(await guardianCheck.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 0)
        {
            await using var guardianAlter = connection.CreateCommand(); guardianAlter.CommandText = "ALTER TABLE guardians ADD COLUMN active INTEGER NOT NULL DEFAULT 1"; await guardianAlter.ExecuteNonQueryAsync(cancellationToken);
        }
        await using var linkCheck = connection.CreateCommand(); linkCheck.CommandText = "SELECT COUNT(1) FROM pragma_table_info('student_guardians') WHERE name = 'is_primary'";
        if (Convert.ToInt32(await linkCheck.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 0)
        {
            await using var linkAlter = connection.CreateCommand(); linkAlter.CommandText = "ALTER TABLE student_guardians ADD COLUMN is_primary INTEGER NOT NULL DEFAULT 0"; await linkAlter.ExecuteNonQueryAsync(cancellationToken);
            await using var linkCopy = connection.CreateCommand(); linkCopy.CommandText = "UPDATE student_guardians SET is_primary = is_primary_contact WHERE is_primary_contact IS NOT NULL"; await linkCopy.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsureLegacyContactColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var columns = new[]
        {
            (Table: "guardians", Name: "middle_initial", Definition: "TEXT"),
            (Table: "guardians", Name: "pronouns", Definition: "TEXT"),
            (Table: "guardians", Name: "newsletter_opt_in", Definition: "INTEGER NOT NULL DEFAULT 0"),
            (Table: "guardians", Name: "sms_opt_in", Definition: "INTEGER NOT NULL DEFAULT 0"),
            (Table: "student_emergency_contacts", Name: "first_name", Definition: "TEXT"),
            (Table: "student_emergency_contacts", Name: "middle_initial", Definition: "TEXT"),
            (Table: "student_emergency_contacts", Name: "last_name", Definition: "TEXT"),
            (Table: "student_emergency_contacts", Name: "pronouns", Definition: "TEXT"),
            (Table: "student_emergency_contacts", Name: "newsletter_opt_in", Definition: "INTEGER NOT NULL DEFAULT 0"),
            (Table: "student_emergency_contacts", Name: "sms_opt_in", Definition: "INTEGER NOT NULL DEFAULT 0")
        };
        foreach (var column in columns)
        {
            await using var check = connection.CreateCommand();
            check.CommandText = $"SELECT COUNT(1) FROM pragma_table_info('{column.Table}') WHERE name = $name";
            check.Parameters.AddWithValue("$name", column.Name);
            if (Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 0)
            {
                await using var alter = connection.CreateCommand();
                alter.CommandText = $"ALTER TABLE {column.Table} ADD COLUMN {column.Name} {column.Definition}";
                await alter.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }

    private static async Task EnsureLegacyClassSessionColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        foreach (var column in new[] { (Name: "assigned_student_id", Definition: "INTEGER"), (Name: "cancellation_reason", Definition: "TEXT") })
        {
            await using var check = connection.CreateCommand();
            check.CommandText = "SELECT COUNT(1) FROM pragma_table_info('class_sessions') WHERE name = $name";
            check.Parameters.AddWithValue("$name", column.Name);
            if (Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) > 0) continue;
            await using var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE class_sessions ADD COLUMN {column.Name} {column.Definition}";
            await alter.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task EnsureStudentAccountColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        foreach (var column in new[] { (Name: "pin_hash", Definition: "TEXT"), (Name: "must_change_password", Definition: "INTEGER NOT NULL DEFAULT 0") })
        {
            await using var check = connection.CreateCommand(); check.CommandText = "SELECT COUNT(1) FROM pragma_table_info('student_accounts') WHERE name = $name"; check.Parameters.AddWithValue("$name", column.Name);
            if (Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 0)
            {
                await using var alter = connection.CreateCommand(); alter.CommandText = $"ALTER TABLE student_accounts ADD COLUMN {column.Name} {column.Definition}"; await alter.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }

    public async Task<IReadOnlyList<FeedItem>> GetFeedAsync(int studentId, CancellationToken cancellationToken = default, int limit = 50, int offset = 0)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT p.post_id, COALESCE(p.student_id, 0), COALESCE(sp.display_name, TRIM(s.first_name || ' ' || s.last_name), 'Task Karate'), r.rank_name, p.post_text, p.post_type, p.created_at,
  COALESCE(p.post_kind, CASE WHEN p.post_type = 'news' THEN 'announcement' ELSE 'training' END), p.link_url, p.image_data, p.image_alt,
  (SELECT COUNT(1) FROM post_comments c WHERE c.post_id = p.post_id AND c.visible = 1 AND c.moderation_status = 'approved')
FROM posts p
LEFT JOIN students s ON s.student_id = p.student_id
LEFT JOIN student_profiles sp ON sp.student_id = p.student_id
LEFT JOIN student_rank_history h ON h.student_id = p.student_id AND h.student_rank_id = (SELECT MAX(h2.student_rank_id) FROM student_rank_history h2 WHERE h2.student_id = p.student_id)
LEFT JOIN ranks r ON r.rank_id = h.rank_id
WHERE p.visible = 1 AND p.moderation_status = 'approved' AND (p.post_type = 'news' OR (p.post_type = 'student' AND (p.student_id = $id OR EXISTS (SELECT 1 FROM student_friendships f WHERE f.status = 'accepted' AND ((f.student_id = $id AND f.friend_id = p.student_id) OR (f.friend_id = $id AND f.student_id = p.student_id))))))
ORDER BY p.created_at DESC LIMIT $limit OFFSET $offset"; command.Parameters.AddWithValue("$id", studentId); command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 51)); command.Parameters.AddWithValue("$offset", Math.Max(0, offset));
        var rows = new List<(long Id, int AuthorId, string AuthorName, string? RankName, string Text, string Type, DateTime CreatedAt, string Kind, string? LinkUrl, string? ImageData, string? ImageAlt, int CommentCount)>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken)) while (await reader.ReadAsync(cancellationToken)) rows.Add((reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.GetString(5), DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture), reader.IsDBNull(7) ? "training" : reader.GetString(7), reader.IsDBNull(8) ? null : reader.GetString(8), reader.IsDBNull(9) ? null : reader.GetString(9), reader.IsDBNull(10) ? null : reader.GetString(10), reader.GetInt32(11)));
        var list = new List<FeedItem>();
        foreach (var row in rows) list.Add(new(row.Id, row.AuthorId, row.AuthorName, row.RankName, row.Text, row.Type, row.CreatedAt, await GetReactionSummaryAsync(connection, row.Id, studentId, cancellationToken), row.CommentCount, row.Kind, row.LinkUrl, row.ImageData, row.ImageAlt));
        return list;
    }

    public async Task<IReadOnlyList<StaffSocialReport>> GetStaffSocialReportsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT r.report_id, r.content_type, r.content_id, r.reporter_id,
  COALESCE(rp.display_name, TRIM(rs.first_name || ' ' || rs.last_name), 'Student'),
  CASE r.content_type WHEN 'post' THEN COALESCE(pp.display_name, TRIM(ps.first_name || ' ' || ps.last_name), 'Student') WHEN 'comment' THEN COALESCE(cp.display_name, TRIM(cs.first_name || ' ' || cs.last_name), 'Student') ELSE COALESCE(mp.display_name, TRIM(ms.first_name || ' ' || ms.last_name), 'Student') END,
  CASE r.content_type WHEN 'post' THEN po.post_text WHEN 'comment' THEN co.comment_text ELSE m.message_text END,
  r.reason, r.status, r.created_at
FROM student_content_reports r
JOIN students rs ON rs.student_id = r.reporter_id LEFT JOIN student_profiles rp ON rp.student_id = rs.student_id
LEFT JOIN posts po ON r.content_type = 'post' AND po.post_id = r.content_id LEFT JOIN students ps ON ps.student_id = po.student_id LEFT JOIN student_profiles pp ON pp.student_id = ps.student_id
LEFT JOIN post_comments co ON r.content_type = 'comment' AND co.comment_id = r.content_id LEFT JOIN students cs ON cs.student_id = co.student_id LEFT JOIN student_profiles cp ON cp.student_id = cs.student_id
LEFT JOIN student_messages m ON r.content_type = 'message' AND m.message_id = r.content_id LEFT JOIN students ms ON ms.student_id = m.sender_id LEFT JOIN student_profiles mp ON mp.student_id = ms.student_id
WHERE r.status = 'open' ORDER BY r.created_at DESC LIMIT 100";
        var list = new List<StaffSocialReport>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetInt64(2), reader.GetInt32(3), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetString(8), DateTime.Parse(reader.GetString(9), CultureInfo.InvariantCulture))); return list;
    }

    public async Task<bool> ResolveStaffSocialReportAsync(long reportId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); var now = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture); string? contentType = null; long contentId = 0;
        await using (var lookup = connection.CreateCommand()) { lookup.CommandText = "SELECT content_type, content_id FROM student_content_reports WHERE report_id = $id AND status = 'open'"; lookup.Parameters.AddWithValue("$id", reportId); await using var reader = await lookup.ExecuteReaderAsync(cancellationToken); if (!await reader.ReadAsync(cancellationToken)) return false; contentType = reader.GetString(0); contentId = reader.GetInt64(1); }
        await using var command = connection.CreateCommand(); command.CommandText = "UPDATE student_content_reports SET status = 'resolved', resolved_at = $now WHERE report_id = $id AND status = 'open'"; command.Parameters.AddWithValue("$id", reportId); command.Parameters.AddWithValue("$now", now); if (await command.ExecuteNonQueryAsync(cancellationToken) != 1) return false;
        await using var alert = connection.CreateCommand(); alert.CommandText = "UPDATE admin_alerts SET status = 'resolved', resolved_at = $now WHERE entity_type = $type AND entity_id = $content AND status = 'open'"; alert.Parameters.AddWithValue("$type", contentType); alert.Parameters.AddWithValue("$content", contentId); alert.Parameters.AddWithValue("$now", now); await alert.ExecuteNonQueryAsync(cancellationToken); return true;
    }

    public async Task<StaffSocialFeedPage> GetStaffSocialFeedAsync(string? search, int offset = 0, int limit = 25, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT p.post_id, COALESCE(p.student_id, 0), COALESCE(sp.display_name, TRIM(s.first_name || ' ' || s.last_name), 'Task Karate'), p.post_text, p.post_type, p.moderation_status, p.visible, p.created_at, p.updated_at,
  (SELECT COUNT(1) FROM post_comments c WHERE c.post_id = p.post_id)
FROM posts p
LEFT JOIN students s ON s.student_id = p.student_id
LEFT JOIN student_profiles sp ON sp.student_id = p.student_id
WHERE ($search = '' OR COALESCE(sp.display_name, TRIM(s.first_name || ' ' || s.last_name), 'Task Karate') LIKE $pattern OR p.post_text LIKE $pattern)
ORDER BY p.created_at DESC LIMIT $limit OFFSET $offset";
        var cleanSearch = search?.Trim() ?? string.Empty;
        command.Parameters.AddWithValue("$search", cleanSearch);
        command.Parameters.AddWithValue("$pattern", $"%{cleanSearch}%");
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 100) + 1);
        command.Parameters.AddWithValue("$offset", Math.Max(0, offset));
        var list = new List<StaffSocialPost>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6) != 0, DateTime.Parse(reader.GetString(7), CultureInfo.InvariantCulture), DateTime.Parse(reader.GetString(8), CultureInfo.InvariantCulture), reader.GetInt32(9)));
        var take = Math.Clamp(limit, 1, 100);
        return new(list.Take(take).ToList(), list.Count > take);
    }

    public async Task<bool> UpdateStaffSocialPostAsync(long postId, string text, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE posts SET post_text = $text, updated_at = $now WHERE post_id = $id"; command.Parameters.AddWithValue("$text", text.Trim()); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); command.Parameters.AddWithValue("$id", postId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> SetStaffSocialPostVisibilityAsync(long postId, bool visible, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE posts SET visible = $visible, moderation_status = $status, updated_at = $now WHERE post_id = $id"; command.Parameters.AddWithValue("$visible", visible ? 1 : 0); command.Parameters.AddWithValue("$status", visible ? "approved" : "removed"); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); command.Parameters.AddWithValue("$id", postId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<IReadOnlyList<StaffSocialComment>> GetStaffSocialCommentsAsync(long postId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "SELECT c.comment_id, c.post_id, c.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), c.comment_text, c.visible, c.moderation_status, c.created_at FROM post_comments c JOIN students s ON s.student_id = c.student_id LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE c.post_id = $post ORDER BY c.created_at"; command.Parameters.AddWithValue("$post", postId);
        var list = new List<StaffSocialComment>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetInt64(1), reader.GetInt32(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5) != 0, reader.GetString(6), DateTime.Parse(reader.GetString(7), CultureInfo.InvariantCulture)));
        return list;
    }

    public async Task<bool> SetStaffSocialCommentVisibilityAsync(long commentId, bool visible, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE post_comments SET visible = $visible, moderation_status = $status WHERE comment_id = $id"; command.Parameters.AddWithValue("$visible", visible ? 1 : 0); command.Parameters.AddWithValue("$status", visible ? "approved" : "removed"); command.Parameters.AddWithValue("$id", commentId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    private static async Task<IReadOnlyList<ReactionSummary>> GetReactionSummaryAsync(SqliteConnection connection, long postId, int studentId, CancellationToken cancellationToken)
    {
        var list = new List<ReactionSummary>();
        await using var command = connection.CreateCommand(); command.CommandText = "SELECT reaction_code, COUNT(1), MAX(CASE WHEN student_id = $student THEN 1 ELSE 0 END) FROM post_reactions WHERE post_id = $post GROUP BY reaction_code"; command.Parameters.AddWithValue("$post", postId); command.Parameters.AddWithValue("$student", studentId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var code = reader.GetString(0); var label = code switch { "fist_bump" => "Fist bump", "respect" => "Nice work", "fire" => "On fire", _ => code };
            var icon = code switch { "fist_bump" => "👊", "respect" => "🙌", "fire" => "⚡", _ => "✦" };
            list.Add(new(code, label, icon, reader.GetInt32(1), reader.GetInt32(2) != 0));
        }
        return list;
    }

    public async Task<bool> ToggleReactionAsync(int studentId, long postId, string reactionCode, CancellationToken cancellationToken = default)
    {
        if (reactionCode is not ("fist_bump" or "respect" or "fire")) return false;
        await using var connection = await OpenAsync(cancellationToken);
        await using var access = connection.CreateCommand(); access.CommandText = "SELECT COUNT(1) FROM posts p WHERE p.post_id = $post AND p.visible = 1 AND p.moderation_status = 'approved' AND (p.post_type = 'news' OR p.student_id = $student OR EXISTS (SELECT 1 FROM student_friendships f WHERE f.status = 'accepted' AND ((f.student_id = $student AND f.friend_id = p.student_id) OR (f.friend_id = $student AND f.student_id = p.student_id))))"; access.Parameters.AddWithValue("$post", postId); access.Parameters.AddWithValue("$student", studentId); if (Convert.ToInt32(await access.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 0) return false;
        await using var existing = connection.CreateCommand(); existing.CommandText = "SELECT reaction_code FROM post_reactions WHERE post_id = $post AND student_id = $student"; existing.Parameters.AddWithValue("$post", postId); existing.Parameters.AddWithValue("$student", studentId); var currentValue = await existing.ExecuteScalarAsync(cancellationToken); var current = currentValue is null || currentValue == DBNull.Value ? null : Convert.ToString(currentValue, CultureInfo.InvariantCulture);
        await using var command = connection.CreateCommand();
        if (current == reactionCode) { command.CommandText = "DELETE FROM post_reactions WHERE post_id = $post AND student_id = $student"; }
        else if (current is not null) { command.CommandText = "UPDATE post_reactions SET reaction_code = $reaction, created_at = $now WHERE post_id = $post AND student_id = $student"; command.Parameters.AddWithValue("$reaction", reactionCode); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); }
        else { command.CommandText = "INSERT INTO post_reactions(post_id, student_id, reaction_code, created_at) VALUES ($post, $student, $reaction, $now)"; command.Parameters.AddWithValue("$reaction", reactionCode); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); }
        command.Parameters.AddWithValue("$post", postId); command.Parameters.AddWithValue("$student", studentId); await command.ExecuteNonQueryAsync(cancellationToken); return true;
    }

    public async Task<IReadOnlyList<CommentItem>?> GetCommentsAsync(int studentId, long postId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        if (!await CanAccessPostAsync(connection, studentId, postId, cancellationToken)) return null;
        await using var command = connection.CreateCommand(); command.CommandText = "SELECT c.comment_id, c.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), c.comment_text, c.created_at FROM post_comments c JOIN students s ON s.student_id = c.student_id LEFT JOIN student_profiles p ON p.student_id = c.student_id WHERE c.post_id = $post AND c.visible = 1 AND c.moderation_status = 'approved' ORDER BY c.created_at LIMIT 100"; command.Parameters.AddWithValue("$post", postId); var list = new List<CommentItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture))); return list;
    }

    public async Task<long?> CreateCommentAsync(int studentId, long postId, string text, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        if (!await CanAccessPostAsync(connection, studentId, postId, cancellationToken)) return null;
        await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO post_comments(post_id, student_id, comment_text, created_at) VALUES ($post, $student, $text, $now)"; command.Parameters.AddWithValue("$post", postId); command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$text", text.Trim()); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken); await using var idCommand = connection.CreateCommand(); idCommand.CommandText = "SELECT last_insert_rowid()"; return Convert.ToInt64(await idCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    private static async Task<bool> CanAccessPostAsync(SqliteConnection connection, int studentId, long postId, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand(); command.CommandText = "SELECT COUNT(1) FROM posts p WHERE p.post_id = $post AND p.visible = 1 AND p.moderation_status = 'approved' AND (p.post_type = 'news' OR p.student_id = $student OR EXISTS (SELECT 1 FROM student_friendships f WHERE f.status = 'accepted' AND ((f.student_id = $student AND f.friend_id = p.student_id) OR (f.friend_id = $student AND f.student_id = p.student_id))))"; command.Parameters.AddWithValue("$post", postId); command.Parameters.AddWithValue("$student", studentId); return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 1;
    }

    public async Task<long> CreatePostAsync(int studentId, string text, string postKind = "training", string? linkUrl = null, string? imageData = null, string? imageAlt = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO posts(student_id, post_text, post_type, post_kind, link_url, image_data, image_alt, moderation_status, visible, created_at, updated_at) VALUES ($student, $text, 'student', $kind, $link, $image, $alt, 'approved', 1, $now, $now)"; command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$text", text.Trim()); command.Parameters.AddWithValue("$kind", postKind); command.Parameters.AddWithValue("$link", (object?)linkUrl ?? DBNull.Value); command.Parameters.AddWithValue("$image", (object?)imageData ?? DBNull.Value); command.Parameters.AddWithValue("$alt", (object?)imageAlt ?? DBNull.Value); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken); await using var idCommand = connection.CreateCommand(); idCommand.CommandText = "SELECT last_insert_rowid()"; return Convert.ToInt64(await idCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public async Task<bool> UpdatePostAsync(int studentId, long postId, string text, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE posts SET post_text = $text, updated_at = $now WHERE post_id = $post AND student_id = $student AND post_type = 'student'";
        command.Parameters.AddWithValue("$text", text.Trim()); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); command.Parameters.AddWithValue("$post", postId); command.Parameters.AddWithValue("$student", studentId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> DeletePostAsync(int studentId, long postId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using (var owner = connection.CreateCommand())
        {
            owner.CommandText = "SELECT COUNT(1) FROM posts WHERE post_id = $post AND student_id = $student AND post_type = 'student'";
            owner.Parameters.AddWithValue("$post", postId); owner.Parameters.AddWithValue("$student", studentId);
            if (Convert.ToInt32(await owner.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) != 1) return false;
        }
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        foreach (var sql in new[] { "DELETE FROM post_reactions WHERE post_id = $post", "DELETE FROM post_bookmarks WHERE post_id = $post", "DELETE FROM post_comments WHERE post_id = $post", "DELETE FROM posts WHERE post_id = $post AND student_id = $student AND post_type = 'student'" })
        {
            await using var command = connection.CreateCommand(); command.Transaction = (SqliteTransaction)transaction; command.CommandText = sql; command.Parameters.AddWithValue("$post", postId); command.Parameters.AddWithValue("$student", studentId); await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken); return true;
    }

    public async Task<bool> HidePostAsync(int studentId, long postId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE posts SET visible = 0, moderation_status = 'removed', updated_at = $now WHERE post_id = $post AND student_id = $student AND post_type = 'student'";
        command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); command.Parameters.AddWithValue("$post", postId); command.Parameters.AddWithValue("$student", studentId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<IReadOnlyList<PracticeLogItem>> GetPracticeLogsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT practice_log_id, skill, minutes, reflection, logged_at FROM student_practice_logs WHERE student_id = $student ORDER BY logged_at DESC LIMIT 30"; command.Parameters.AddWithValue("$student", studentId); var list = new List<PracticeLogItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2), reader.IsDBNull(3) ? null : reader.GetString(3), DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture))); return list;
    }

    public async Task<long> CreatePracticeLogAsync(int studentId, string skill, int minutes, string? reflection, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO student_practice_logs(student_id, skill, minutes, reflection, logged_at) VALUES ($student, $skill, $minutes, $reflection, $now)"; command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$skill", skill.Trim()); command.Parameters.AddWithValue("$minutes", minutes); command.Parameters.AddWithValue("$reflection", (object?)reflection?.Trim() ?? DBNull.Value); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken); await using var idCommand = connection.CreateCommand(); idCommand.CommandText = "SELECT last_insert_rowid()"; return Convert.ToInt64(await idCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public async Task<IReadOnlyList<GoalItem>> GetGoalsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT goal_id, title, target_date, completed, created_at, completed_at FROM student_goals WHERE student_id = $student ORDER BY completed, target_date IS NULL, target_date, created_at DESC"; command.Parameters.AddWithValue("$student", studentId); var list = new List<GoalItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.IsDBNull(2) ? null : DateTime.Parse(reader.GetString(2), CultureInfo.InvariantCulture), reader.GetInt32(3) != 0, DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture), reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture))); return list;
    }

    public async Task<long> CreateGoalAsync(int studentId, string title, DateTime? targetDate, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO student_goals(student_id, title, target_date, completed, created_at) VALUES ($student, $title, $target, 0, $now) ON CONFLICT(student_id, title) DO UPDATE SET target_date = excluded.target_date"; command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$title", title.Trim()); command.Parameters.AddWithValue("$target", targetDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? (object)DBNull.Value); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken); await using var idCommand = connection.CreateCommand(); idCommand.CommandText = "SELECT goal_id FROM student_goals WHERE student_id = $student AND title = $title"; idCommand.Parameters.AddWithValue("$student", studentId); idCommand.Parameters.AddWithValue("$title", title.Trim()); return Convert.ToInt64(await idCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public async Task<bool> ToggleGoalAsync(int studentId, long goalId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var check = connection.CreateCommand(); check.CommandText = "SELECT completed FROM student_goals WHERE goal_id = $goal AND student_id = $student"; check.Parameters.AddWithValue("$goal", goalId); check.Parameters.AddWithValue("$student", studentId); var value = await check.ExecuteScalarAsync(cancellationToken); if (value is null) return false; var completed = Convert.ToInt32(value, CultureInfo.InvariantCulture) == 0; await using var command = connection.CreateCommand(); command.CommandText = "UPDATE student_goals SET completed = $completed, completed_at = $completedAt WHERE goal_id = $goal AND student_id = $student"; command.Parameters.AddWithValue("$completed", completed ? 1 : 0); command.Parameters.AddWithValue("$completedAt", completed ? DateTime.UtcNow.ToString("O") : (object)DBNull.Value); command.Parameters.AddWithValue("$goal", goalId); command.Parameters.AddWithValue("$student", studentId); await command.ExecuteNonQueryAsync(cancellationToken); return completed;
    }

    public async Task<IReadOnlyList<long>> GetBookmarkedPostIdsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT post_id FROM post_bookmarks WHERE student_id = $student ORDER BY created_at DESC"; command.Parameters.AddWithValue("$student", studentId); var list = new List<long>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(reader.GetInt64(0)); return list;
    }

    public async Task<bool> ToggleBookmarkAsync(int studentId, long postId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); if (!await CanAccessPostAsync(connection, studentId, postId, cancellationToken)) return false; await using var exists = connection.CreateCommand(); exists.CommandText = "SELECT COUNT(1) FROM post_bookmarks WHERE post_id = $post AND student_id = $student"; exists.Parameters.AddWithValue("$post", postId); exists.Parameters.AddWithValue("$student", studentId); var bookmarked = Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) > 0; await using var command = connection.CreateCommand(); command.Parameters.AddWithValue("$post", postId); command.Parameters.AddWithValue("$student", studentId); if (bookmarked) command.CommandText = "DELETE FROM post_bookmarks WHERE post_id = $post AND student_id = $student"; else { command.CommandText = "INSERT INTO post_bookmarks(post_id, student_id) VALUES ($post, $student)"; } await command.ExecuteNonQueryAsync(cancellationToken); return !bookmarked;
    }

    public async Task<IReadOnlyList<AchievementItem>> GetAchievementsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT a.achievement_name, a.description, a.icon_name, sa.awarded_at FROM student_achievements sa JOIN achievements a ON a.achievement_id = sa.achievement_id WHERE sa.student_id = $id AND a.achievement_name <> 'Bubble Sensei' ORDER BY sa.awarded_at DESC"; command.Parameters.AddWithValue("$id", studentId); var list = new List<AchievementItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture))); return list;
    }

    private static readonly IReadOnlyDictionary<string, (string Name, string Description, string Icon)> EasterEggAchievements = new Dictionary<string, (string, string, string)>(StringComparer.OrdinalIgnoreCase)
    {
        ["egg-bell-finder"] = ("Bell Finder", "You found the hidden dojo bell.", "bell"),
        ["egg-belt-whisperer"] = ("Belt Whisperer", "You made the belt colors orbit.", "belt"),
        ["egg-scroll-long-way"] = ("Scroll of the Long Way", "You followed the progress trail all the way to black belt.", "scroll"),
        ["egg-dojo-explorer"] = ("Dojo Explorer", "You visited every Student Hub door in one session.", "compass"),
        ["egg-secret-kata"] = ("Secret Kata", "You discovered the reverse navigation pattern.", "kata"),
        ["egg-volume-control"] = ("Volume Control", "You found the hidden HIYAH! overdrive.", "speaker"),
        ["egg-sticker-sensei"] = ("Sticker Sensei", "You opened the hidden dojo sticker drawer.", "sticker"),
        ["egg-card-carrying-student"] = ("Card-Carrying Student", "You flipped your Student Hub identity card.", "card"),
        ["egg-wall-wisdom"] = ("Wall Wisdom", "You activated the dojo motto machine.", "wisdom")
    };

    public async Task<EasterEggUnlockResult?> UnlockEasterEggAsync(int studentId, string eggId, CancellationToken cancellationToken = default)
    {
        if (!EasterEggAchievements.TryGetValue(eggId.Trim(), out var egg)) return null;
        await using var connection = await OpenAsync(cancellationToken);
        var now = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        await using (var seed = connection.CreateCommand())
        {
            seed.CommandText = "INSERT OR IGNORE INTO achievements(achievement_name, description, icon_name) VALUES ($name, $description, $icon)";
            seed.Parameters.AddWithValue("$name", egg.Name); seed.Parameters.AddWithValue("$description", egg.Description); seed.Parameters.AddWithValue("$icon", egg.Icon);
            await seed.ExecuteNonQueryAsync(cancellationToken);
        }

        int achievementId;
        await using (var lookup = connection.CreateCommand())
        {
            lookup.CommandText = "SELECT achievement_id FROM achievements WHERE achievement_name = $name"; lookup.Parameters.AddWithValue("$name", egg.Name);
            achievementId = Convert.ToInt32(await lookup.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }

        bool unlocked;
        await using (var award = connection.CreateCommand())
        {
            award.CommandText = "INSERT OR IGNORE INTO student_achievements(student_id, achievement_id, awarded_at) VALUES ($student, $achievement, $now)";
            award.Parameters.AddWithValue("$student", studentId); award.Parameters.AddWithValue("$achievement", achievementId); award.Parameters.AddWithValue("$now", now);
            unlocked = await award.ExecuteNonQueryAsync(cancellationToken) > 0;
        }

        var achievement = await ReadAchievementAsync(connection, studentId, achievementId, cancellationToken);
        if (achievement is null) return null;

        int discoveredCount;
        await using (var count = connection.CreateCommand())
        {
            count.CommandText = "SELECT COUNT(1) FROM student_achievements sa JOIN achievements a ON a.achievement_id = sa.achievement_id WHERE sa.student_id = $student AND a.achievement_name IN ('Bell Finder', 'Belt Whisperer', 'Scroll of the Long Way', 'Dojo Explorer', 'Secret Kata', 'Volume Control', 'Sticker Sensei', 'Card-Carrying Student', 'Wall Wisdom')";
            count.Parameters.AddWithValue("$student", studentId); discoveredCount = Convert.ToInt32(await count.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }

        var collector = (AchievementItem?)null;
        var collectorUnlocked = false;
        if (discoveredCount >= 9)
        {
            await using var collectorSeed = connection.CreateCommand();
            collectorSeed.CommandText = "INSERT OR IGNORE INTO achievements(achievement_name, description, icon_name) VALUES ('Master of Hidden Dojos', 'You discovered every Student Hub easter egg.', 'hidden-dojo')";
            await collectorSeed.ExecuteNonQueryAsync(cancellationToken);
            await using var collectorAward = connection.CreateCommand();
            collectorAward.CommandText = "INSERT OR IGNORE INTO student_achievements(student_id, achievement_id, awarded_at) SELECT $student, achievement_id, $now FROM achievements WHERE achievement_name = 'Master of Hidden Dojos'";
            collectorAward.Parameters.AddWithValue("$student", studentId); collectorAward.Parameters.AddWithValue("$now", now); collectorUnlocked = await collectorAward.ExecuteNonQueryAsync(cancellationToken) > 0;
            await using var collectorLookup = connection.CreateCommand(); collectorLookup.CommandText = "SELECT achievement_id FROM achievements WHERE achievement_name = 'Master of Hidden Dojos'"; var collectorId = Convert.ToInt32(await collectorLookup.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture); collector = await ReadAchievementAsync(connection, studentId, collectorId, cancellationToken);
        }

        return new(unlocked, collectorUnlocked, achievement, collector, discoveredCount);
    }

    private static async Task<AchievementItem?> ReadAchievementAsync(SqliteConnection connection, int studentId, int achievementId, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand(); command.CommandText = "SELECT a.achievement_name, a.description, a.icon_name, sa.awarded_at FROM student_achievements sa JOIN achievements a ON a.achievement_id = sa.achievement_id WHERE sa.student_id = $student AND a.achievement_id = $achievement"; command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$achievement", achievementId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken); if (!await reader.ReadAsync(cancellationToken)) return null;
        return new(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture));
    }

    public async Task<IReadOnlyList<TrainingMissionItem>> GetTrainingMissionsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT m.mission_id, m.title, m.description, m.category, COALESCE(p.completed, 0), p.completed_at FROM training_missions m LEFT JOIN student_mission_progress p ON p.mission_id = m.mission_id AND p.student_id = $student WHERE m.active = 1 ORDER BY m.category, m.mission_id"; command.Parameters.AddWithValue("$student", studentId); var list = new List<TrainingMissionItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4) != 0, reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture))); return list;
    }

    public async Task<bool> ToggleTrainingMissionAsync(int studentId, long missionId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var check = connection.CreateCommand(); check.CommandText = "SELECT completed FROM student_mission_progress WHERE student_id = $student AND mission_id = $mission"; check.Parameters.AddWithValue("$student", studentId); check.Parameters.AddWithValue("$mission", missionId); var current = Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken) ?? 0, CultureInfo.InvariantCulture); var completed = current == 0;
        await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO student_mission_progress(student_id, mission_id, completed, completed_at, updated_at) VALUES ($student, $mission, $completed, $completedAt, $now) ON CONFLICT(student_id, mission_id) DO UPDATE SET completed = excluded.completed, completed_at = excluded.completed_at, updated_at = excluded.updated_at"; command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$mission", missionId); command.Parameters.AddWithValue("$completed", completed ? 1 : 0); command.Parameters.AddWithValue("$completedAt", completed ? DateTime.UtcNow.ToString("O") : DBNull.Value); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken); return completed;
    }

    public async Task<IReadOnlyList<DailyMissionItem>> SyncDailyMissionsAsync(int studentId, DateTime missionDate, IReadOnlyList<DailyMissionSeed> seeds, CancellationToken cancellationToken = default)
    {
        var date = missionDate.Date.ToString("yyyy-MM-dd");
        await using var connection = await OpenAsync(cancellationToken); await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var selectedSeeds = seeds.Take(5).ToList();
        await using (var removeStale = connection.CreateCommand())
        {
            removeStale.Transaction = (SqliteTransaction)transaction;
            var keyParameters = selectedSeeds.Select((_, index) => $"$key{index}").ToArray();
            removeStale.CommandText = $"DELETE FROM student_daily_missions WHERE student_id = $student AND mission_date = $date AND mission_key NOT IN ({string.Join(", ", keyParameters)})";
            removeStale.Parameters.AddWithValue("$student", studentId); removeStale.Parameters.AddWithValue("$date", date);
            for (var index = 0; index < selectedSeeds.Count; index++) removeStale.Parameters.AddWithValue(keyParameters[index], selectedSeeds[index].MissionKey.Trim());
            await removeStale.ExecuteNonQueryAsync(cancellationToken);
        }
        foreach (var seed in selectedSeeds)
        {
            await using var command = connection.CreateCommand(); command.Transaction = (SqliteTransaction)transaction;
            command.CommandText = "INSERT INTO student_daily_missions(student_id, mission_date, mission_key, title, description, category) VALUES ($student, $date, $key, $title, $description, $category) ON CONFLICT(student_id, mission_date, mission_key) DO UPDATE SET title = excluded.title, description = excluded.description, category = excluded.category";
            command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$date", date); command.Parameters.AddWithValue("$key", seed.MissionKey.Trim()); command.Parameters.AddWithValue("$title", seed.Title.Trim()); command.Parameters.AddWithValue("$description", seed.Description.Trim()); command.Parameters.AddWithValue("$category", seed.Category.Trim()); await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
        return await GetDailyMissionsAsync(studentId, missionDate, cancellationToken);
    }

    public async Task<IReadOnlyList<DailyMissionItem>> GetDailyMissionsAsync(int studentId, DateTime missionDate, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT daily_mission_id, mission_key, title, description, category, mission_date, completed, completed_at FROM student_daily_missions WHERE student_id = $student AND mission_date = $date ORDER BY daily_mission_id"; command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$date", missionDate.Date.ToString("yyyy-MM-dd"));
        var list = new List<DailyMissionItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture), reader.GetInt32(6) != 0, reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7), CultureInfo.InvariantCulture))); return list;
    }

    public async Task<bool?> ToggleDailyMissionAsync(int studentId, long missionId, DateTime missionDate, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var check = connection.CreateCommand(); check.CommandText = "SELECT completed FROM student_daily_missions WHERE daily_mission_id = $mission AND student_id = $student AND mission_date = $date"; check.Parameters.AddWithValue("$mission", missionId); check.Parameters.AddWithValue("$student", studentId); check.Parameters.AddWithValue("$date", missionDate.Date.ToString("yyyy-MM-dd")); var currentValue = await check.ExecuteScalarAsync(cancellationToken); if (currentValue is null) return null;
        var completed = Convert.ToInt32(currentValue, CultureInfo.InvariantCulture) == 0; await using var command = connection.CreateCommand(); command.CommandText = "UPDATE student_daily_missions SET completed = $completed, completed_at = $completedAt WHERE daily_mission_id = $mission AND student_id = $student AND mission_date = $date"; command.Parameters.AddWithValue("$completed", completed ? 1 : 0); command.Parameters.AddWithValue("$completedAt", completed ? DateTime.UtcNow.ToString("O") : DBNull.Value); command.Parameters.AddWithValue("$mission", missionId); command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$date", missionDate.Date.ToString("yyyy-MM-dd")); await command.ExecuteNonQueryAsync(cancellationToken); return completed;
    }

    public async Task<IReadOnlyList<TimelineItem>> GetTimelineAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); var list = new List<TimelineItem>();
		await using (var attendance = connection.CreateCommand()) { attendance.CommandText = "SELECT CASE WHEN a.status = 'helper' THEN 'helping' ELSE 'attendance' END, CASE WHEN a.status = 'helper' THEN 'Helped with ' || c.class_name ELSE 'Checked into ' || c.class_name END, CASE WHEN a.status = 'helper' THEN 'Helper attendance recorded. This supports the dojo but does not count toward stripe progress.' ELSE COALESCE(c.description, 'Training attendance recorded.') END, a.check_in_time, '/schedule' FROM attendance a JOIN class_sessions cs ON cs.session_id = a.session_id JOIN classes c ON c.class_id = cs.class_id WHERE a.student_id = $student AND a.status IN ('present', 'helper') ORDER BY a.check_in_time DESC LIMIT 10"; attendance.Parameters.AddWithValue("$student", studentId); await using var reader = await attendance.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.GetString(4))); }
        await using (var achievements = connection.CreateCommand()) { achievements.CommandText = "SELECT 'achievement', a.achievement_name, COALESCE(a.description, 'Milestone awarded by the dojo.'), sa.awarded_at, '/student/achievements' FROM student_achievements sa JOIN achievements a ON a.achievement_id = sa.achievement_id WHERE sa.student_id = $student AND a.achievement_name <> 'Bubble Sensei' ORDER BY sa.awarded_at DESC LIMIT 10"; achievements.Parameters.AddWithValue("$student", studentId); await using var reader = await achievements.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.GetString(4))); }
        await using (var stars = connection.CreateCommand()) { stars.CommandText = "SELECT 'gold_star', e.event_name, e.description, g.awarded_at, '/student/achievements/' || e.event_id FROM student_gold_stars g JOIN gold_star_events e ON e.event_id = g.event_id WHERE g.student_id = $student ORDER BY g.awarded_at DESC LIMIT 10"; stars.Parameters.AddWithValue("$student", studentId); await using var reader = await stars.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.GetString(4))); }
        await using (var posts = connection.CreateCommand()) { posts.CommandText = "SELECT 'post', 'Shared a dojo update', p.post_text, p.created_at, '/student/social' FROM posts p WHERE p.student_id = $student AND p.visible = 1 ORDER BY p.created_at DESC LIMIT 10"; posts.Parameters.AddWithValue("$student", studentId); await using var reader = await posts.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.GetString(4))); }
        return list.OrderByDescending(item => item.OccurredAt).Take(20).ToList();
    }

    public async Task<IReadOnlyList<GoldStarEventItem>> GetGoldStarEventsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT e.event_id, e.event_name, e.description, COALESCE(g.event_date, e.event_date), 1, 0 FROM gold_star_events e JOIN student_gold_stars g ON g.event_id = e.event_id AND g.student_id = $student WHERE e.active = 1 ORDER BY g.awarded_at DESC, e.event_name"; command.Parameters.AddWithValue("$student", studentId); var list = new List<GoldStarEventItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), true, false)); return list;
    }

    public async Task<GoldStarEventItem?> GetGoldStarEventAsync(int studentId, long eventId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "SELECT e.event_id, e.event_name, e.description, COALESCE(g.event_date, e.event_date), 1, 0 FROM gold_star_events e JOIN student_gold_stars g ON g.event_id = e.event_id AND g.student_id = $student WHERE e.event_id = $event AND e.active = 1";
        command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$event", eventId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.GetBoolean(4), reader.GetBoolean(5));
    }

    public async Task<bool> ToggleGoldStarInterestAsync(int studentId, long eventId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var exists = connection.CreateCommand(); exists.CommandText = "SELECT status FROM student_event_interests WHERE student_id = $student AND event_id = $event"; exists.Parameters.AddWithValue("$student", studentId); exists.Parameters.AddWithValue("$event", eventId); var currentValue = await exists.ExecuteScalarAsync(cancellationToken); var current = currentValue is null || currentValue == DBNull.Value ? null : Convert.ToString(currentValue, CultureInfo.InvariantCulture);
        await using var command = connection.CreateCommand(); command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$event", eventId); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        if (current == "interested") command.CommandText = "DELETE FROM student_event_interests WHERE student_id = $student AND event_id = $event";
        else command.CommandText = "INSERT INTO student_event_interests(student_id, event_id, status, created_at, updated_at) VALUES ($student, $event, 'interested', $now, $now) ON CONFLICT(student_id, event_id) DO UPDATE SET status = 'interested', updated_at = excluded.updated_at";
        await command.ExecuteNonQueryAsync(cancellationToken); return current != "interested";
    }

    public async Task<IReadOnlyList<NewsItem>> GetNewsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT post_id, post_text, created_at FROM posts WHERE visible = 1 AND moderation_status = 'approved' AND post_type = 'news' ORDER BY created_at DESC LIMIT 50"; var list = new List<NewsItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(ToNewsItem(reader.GetInt64(0), reader.GetString(1), reader.GetString(2))); return list;
    }

    public async Task<NewsItem?> GetNewsItemAsync(long postId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT post_id, post_text, created_at FROM posts WHERE post_id = $id AND visible = 1 AND moderation_status = 'approved' AND post_type = 'news'"; command.Parameters.AddWithValue("$id", postId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken); if (!await reader.ReadAsync(cancellationToken)) return null; return ToNewsItem(reader.GetInt64(0), reader.GetString(1), reader.GetString(2));
    }

    public async Task<IReadOnlyList<PortalContentItem>> GetPortalContentAsync(string kind, bool includeDrafts, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "SELECT content_id, title, body, status, published_at, expires_at, kind FROM portal_content WHERE kind = $kind" + (includeDrafts ? string.Empty : " AND status = 'Published' AND (expires_at IS NULL OR datetime(expires_at) > datetime('now'))") + " ORDER BY COALESCE(published_at, updated_at) DESC, content_id DESC";
        command.Parameters.AddWithValue("$kind", kind); var list = new List<PortalContentItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), ParseNullableDate(reader.IsDBNull(4) ? null : reader.GetString(4)), ParseNullableDate(reader.IsDBNull(5) ? null : reader.GetString(5)), reader.GetString(6)));
        return list;
    }

    public async Task<long> CreatePortalContentAsync(string kind, PortalContentWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO portal_content(kind, title, body, status, expires_at) VALUES ($kind, $title, $body, 'Draft', $expires); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$kind", kind); command.Parameters.AddWithValue("$title", request.Title.Trim()); command.Parameters.AddWithValue("$body", request.Body.Trim()); command.Parameters.AddWithValue("$expires", request.ExpiresAtUtc?.ToString("O") ?? (object)DBNull.Value);
        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public async Task<bool> DeletePortalContentAsync(string kind, long contentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM portal_content WHERE content_id = $id AND kind = $kind";
        command.Parameters.AddWithValue("$id", contentId); command.Parameters.AddWithValue("$kind", kind);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> PublishPortalContentAsync(string kind, long contentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var lookup = connection.CreateCommand(); lookup.Transaction = (SqliteTransaction)transaction; lookup.CommandText = "SELECT title, body, status FROM portal_content WHERE content_id = $id AND kind = $kind"; lookup.Parameters.AddWithValue("$id", contentId); lookup.Parameters.AddWithValue("$kind", kind);
        await using var reader = await lookup.ExecuteReaderAsync(cancellationToken); if (!await reader.ReadAsync(cancellationToken)) return false; var title = reader.GetString(0); var body = reader.GetString(1); var wasPublished = string.Equals(reader.GetString(2), "Published", StringComparison.OrdinalIgnoreCase); await reader.DisposeAsync();
        var now = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        await using var update = connection.CreateCommand(); update.Transaction = (SqliteTransaction)transaction; update.CommandText = "UPDATE portal_content SET status = 'Published', published_at = COALESCE(published_at, $now), updated_at = $now WHERE content_id = $id AND kind = $kind"; update.Parameters.AddWithValue("$now", now); update.Parameters.AddWithValue("$id", contentId); update.Parameters.AddWithValue("$kind", kind); if (await update.ExecuteNonQueryAsync(cancellationToken) != 1) return false;
        if (!wasPublished && kind == "news")
        {
            await using var systemAuthor = connection.CreateCommand(); systemAuthor.Transaction = (SqliteTransaction)transaction; systemAuthor.CommandText = "INSERT OR IGNORE INTO users(username, password_hash, display_name, active) VALUES ('task-karate-system', NULL, 'Task Karate', 1)"; await systemAuthor.ExecuteNonQueryAsync(cancellationToken);
            await using var post = connection.CreateCommand(); post.Transaction = (SqliteTransaction)transaction; post.CommandText = "INSERT INTO posts(student_id, user_id, post_text, post_type, post_kind, moderation_status, visible, created_at, updated_at) VALUES (NULL, (SELECT user_id FROM users WHERE username = 'task-karate-system'), $text, 'news', 'announcement', 'approved', 1, $now, $now)"; post.Parameters.AddWithValue("$text", $"{title} — {body}"); post.Parameters.AddWithValue("$now", now); await post.ExecuteNonQueryAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken); return true;
    }

    private static DateTime? ParseNullableDate(string? value) => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed) ? parsed : null;

    private static NewsItem ToNewsItem(long id, string text, string createdAt)
    {
        var separator = text.IndexOf(" — ", StringComparison.Ordinal);
        return separator > 0
            ? new(id, text[..separator], text[(separator + 3)..], DateTime.Parse(createdAt, CultureInfo.InvariantCulture))
            : new(id, "Dojo news", text, DateTime.Parse(createdAt, CultureInfo.InvariantCulture));
    }

    public async Task<int> CreatePortalStudentAsync(PortalStudentWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO students(first_name, last_name, preferred_name, nickname, pronouns, honorific, roles, age_group, birth_date, start_date, uniform_size, belt_size, active, status) VALUES ($first, $last, $preferred, $nickname, $pronouns, $honorific, $roles, $age, $birth, $join, $uniform, $belt, 1, 'active'); SELECT last_insert_rowid();"; command.Parameters.AddWithValue("$first", request.FirstName.Trim()); command.Parameters.AddWithValue("$last", request.LastName.Trim()); command.Parameters.AddWithValue("$preferred", (object?)NullIfBlank(request.PreferredName) ?? DBNull.Value); command.Parameters.AddWithValue("$nickname", (object?)NullIfBlank(request.Nickname) ?? DBNull.Value); command.Parameters.AddWithValue("$pronouns", (object?)NullIfBlank(request.Pronouns) ?? DBNull.Value); command.Parameters.AddWithValue("$honorific", (object?)NullIfBlank(request.Honorific) ?? DBNull.Value); command.Parameters.AddWithValue("$roles", string.Join(',', NormalizeStudentRoles(request.Roles))); command.Parameters.AddWithValue("$age", request.AgeGroup.Trim()); command.Parameters.AddWithValue("$birth", request.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? (object)DBNull.Value); command.Parameters.AddWithValue("$join", (request.JoinDate ?? DateTime.UtcNow.Date).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); command.Parameters.AddWithValue("$uniform", (object?)NullIfBlank(request.UniformSize) ?? DBNull.Value); command.Parameters.AddWithValue("$belt", (object?)NullIfBlank(request.BeltSize) ?? DBNull.Value); var id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture); await using var profile = connection.CreateCommand(); profile.CommandText = "INSERT INTO student_profiles(student_id, display_name, bio, favorite_technique, profile_visible) VALUES ($id, $display, $bio, $technique, 1)"; profile.Parameters.AddWithValue("$id", id); profile.Parameters.AddWithValue("$display", (object?)NullIfBlank(request.Nickname) ?? (object?)NullIfBlank(request.PreferredName) ?? $"{request.FirstName.Trim()} {request.LastName.Trim()}"); profile.Parameters.AddWithValue("$bio", (object?)NullIfBlank(request.Bio) ?? DBNull.Value); profile.Parameters.AddWithValue("$technique", (object?)NullIfBlank(request.FavoriteTechnique) ?? DBNull.Value); await profile.ExecuteNonQueryAsync(cancellationToken); return id;
    }

    public async Task<bool> UpdatePortalStudentAsync(int studentId, PortalStudentWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "UPDATE students SET first_name = $first, last_name = $last, preferred_name = $preferred, nickname = $nickname, pronouns = $pronouns, honorific = $honorific, roles = $roles, age_group = $age, birth_date = $birth, start_date = $join, uniform_size = $uniform, belt_size = $belt, updated_at = $now WHERE student_id = $id"; command.Parameters.AddWithValue("$first", request.FirstName.Trim()); command.Parameters.AddWithValue("$last", request.LastName.Trim()); command.Parameters.AddWithValue("$preferred", (object?)NullIfBlank(request.PreferredName) ?? DBNull.Value); command.Parameters.AddWithValue("$nickname", (object?)NullIfBlank(request.Nickname) ?? DBNull.Value); command.Parameters.AddWithValue("$pronouns", (object?)NullIfBlank(request.Pronouns) ?? DBNull.Value); command.Parameters.AddWithValue("$honorific", (object?)NullIfBlank(request.Honorific) ?? DBNull.Value); command.Parameters.AddWithValue("$roles", string.Join(',', NormalizeStudentRoles(request.Roles))); command.Parameters.AddWithValue("$age", request.AgeGroup.Trim()); command.Parameters.AddWithValue("$birth", request.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? (object)DBNull.Value); command.Parameters.AddWithValue("$join", (request.JoinDate ?? DateTime.UtcNow.Date).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); command.Parameters.AddWithValue("$uniform", (object?)NullIfBlank(request.UniformSize) ?? DBNull.Value); command.Parameters.AddWithValue("$belt", (object?)NullIfBlank(request.BeltSize) ?? DBNull.Value); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); command.Parameters.AddWithValue("$id", studentId); if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) return false; await using var profile = connection.CreateCommand(); profile.CommandText = "INSERT INTO student_profiles(student_id, display_name, bio, favorite_technique, profile_visible) VALUES ($id, $display, $bio, $technique, 1) ON CONFLICT(student_id) DO UPDATE SET display_name = excluded.display_name, bio = excluded.bio, favorite_technique = excluded.favorite_technique, updated_at = excluded.updated_at"; profile.Parameters.AddWithValue("$id", studentId); profile.Parameters.AddWithValue("$display", (object?)NullIfBlank(request.Nickname) ?? (object?)NullIfBlank(request.PreferredName) ?? $"{request.FirstName.Trim()} {request.LastName.Trim()}"); profile.Parameters.AddWithValue("$bio", (object?)NullIfBlank(request.Bio) ?? DBNull.Value); profile.Parameters.AddWithValue("$technique", (object?)NullIfBlank(request.FavoriteTechnique) ?? DBNull.Value); profile.Parameters.AddWithValue("$updated_at", DateTime.UtcNow.ToString("O")); await profile.ExecuteNonQueryAsync(cancellationToken); return true;
    }

    public async Task<bool> SetPortalStudentActiveAsync(int studentId, bool active, CancellationToken cancellationToken = default)
    {
        return await SetPortalStudentStatusAsync(studentId, active ? "active" : "deactivated", cancellationToken);
    }

    public async Task<bool> SetPortalStudentStatusAsync(int studentId, string status, CancellationToken cancellationToken = default)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (normalized is not ("active" or "paused" or "deactivated")) return false;
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "UPDATE students SET status = $status, active = CASE WHEN $status = 'deactivated' THEN 0 ELSE 1 END, archived_at = CASE WHEN $status = 'deactivated' THEN $now ELSE NULL END, updated_at = $now WHERE student_id = $id"; command.Parameters.AddWithValue("$status", normalized); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); command.Parameters.AddWithValue("$id", studentId); return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IReadOnlyList<PortalAdminStudent>> GetPortalAdminStudentsAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), (SELECT h.awarded_date FROM student_rank_history h WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), (SELECT h.notes FROM student_rank_history h WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), s.age_group, s.birth_date, s.active, s.start_date, s.uniform_size, s.belt_size, p.bio, p.favorite_technique, (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND a.status = 'present'), (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND a.status = 'helper'), (SELECT COUNT(1) FROM student_achievements a WHERE a.student_id = s.student_id), (SELECT COUNT(1) FROM student_gold_stars g WHERE g.student_id = s.student_id), COALESCE((SELECT group_concat(m.program_name, ', ') FROM student_program_memberships m WHERE m.student_id = s.student_id AND m.active = 1), ''), COALESCE(NULLIF(s.status, ''), CASE WHEN s.active = 1 THEN 'active' ELSE 'deactivated' END), s.first_name, s.last_name, s.nickname, s.pronouns, s.honorific, s.roles FROM students s LEFT JOIN student_profiles p ON p.student_id = s.student_id" + (activeOnly ? " WHERE COALESCE(NULLIF(s.status, ''), CASE WHEN s.active = 1 THEN 'active' ELSE 'deactivated' END) = 'active'" : string.Empty) + " ORDER BY 2";
        var list = new List<PortalAdminStudent>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var studentId = reader.GetInt32(0);
            IReadOnlyList<string> programs = reader.IsDBNull(17) ? Array.Empty<string>() : reader.GetString(17).Split(", ", StringSplitOptions.RemoveEmptyEntries);
            var rankName = reader.IsDBNull(2) ? null : reader.GetString(2);
            var nickname = reader.IsDBNull(21) ? null : reader.GetString(21);
            var pronouns = reader.IsDBNull(22) ? null : reader.GetString(22);
            var honorific = reader.IsDBNull(23) ? null : reader.GetString(23);
            IReadOnlyList<string> roles = reader.IsDBNull(24) ? ["student"] : NormalizeStudentRoles(reader.GetString(24).Split(',', StringSplitOptions.RemoveEmptyEntries));
            list.Add(new(
                studentId,
                reader.GetString(19),
                reader.GetString(20),
                BuildStudentDisplayName(reader.GetString(19), reader.GetString(20), nickname, honorific, reader.GetString(1), rankName),
                nickname,
                pronouns,
                honorific,
                roles,
                rankName,
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) || !DateTime.TryParse(reader.GetString(6), CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthDate) ? null : birthDate,
                reader.GetInt32(7) != 0,
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetString(9),
                reader.IsDBNull(10) ? null : reader.GetString(10),
                reader.IsDBNull(11) ? null : reader.GetString(11),
                reader.IsDBNull(12) ? null : reader.GetString(12),
                reader.GetInt32(13),
                reader.GetInt32(14),
                await GetAttendanceStreakAsync(connection, studentId, cancellationToken),
                reader.GetInt32(15),
                reader.GetInt32(16),
                programs,
                reader.IsDBNull(18) ? "active" : reader.GetString(18)));
        }
        return list;
    }

    public async Task<IReadOnlyList<PortalAdminSearchResult>> SearchPortalAdminAsync(string query, CancellationToken cancellationToken = default)
    {
        var clean = query.Trim();
        if (clean.Length < 2) return Array.Empty<PortalAdminSearchResult>();
        await using var connection = await OpenAsync(cancellationToken);
        var like = $"%{clean}%";
        var results = new List<PortalAdminSearchResult>();
        async Task ReadAsync(string sql, Action<SqliteDataReader> add)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("$query", like);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken)) add(reader);
        }

        await ReadAsync(@"SELECT s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)),
  COALESCE((SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC LIMIT 1), 'Rank not assigned')
FROM students s LEFT JOIN student_profiles p ON p.student_id = s.student_id
WHERE COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)) LIKE $query OR s.email LIKE $query OR s.phone LIKE $query
ORDER BY 2 COLLATE NOCASE LIMIT 20", reader => results.Add(new("student", reader.GetInt32(0), reader.GetString(1), reader.GetString(2))));
        await ReadAsync(@"SELECT g.guardian_id, TRIM(g.first_name || ' ' || g.last_name), COALESCE(g.email, g.phone, 'Family contact')
FROM guardians g WHERE TRIM(g.first_name || ' ' || g.last_name) LIKE $query OR g.email LIKE $query OR g.phone LIKE $query
ORDER BY 2 COLLATE NOCASE LIMIT 12", reader => results.Add(new("guardian", reader.GetInt32(0), reader.GetString(1), reader.GetString(2))));
        await ReadAsync(@"SELECT program_id, program_name, COALESCE(description, 'Program') FROM portal_programs WHERE active = 1 AND (program_name LIKE $query OR description LIKE $query) ORDER BY program_name COLLATE NOCASE LIMIT 12", reader => results.Add(new("program", reader.GetInt32(0), reader.GetString(1), reader.GetString(2))));
        await ReadAsync(@"SELECT post_id, COALESCE(post_text, 'Community post'), created_at FROM posts WHERE visible = 1 AND post_text LIKE $query ORDER BY created_at DESC LIMIT 12", reader => results.Add(new("post", checked((int)reader.GetInt64(0)), reader.GetString(1).Length > 100 ? reader.GetString(1)[..100] + '…' : reader.GetString(1), reader.GetString(2))));
        return results.Take(50).ToList();
    }

    public async Task<IReadOnlyList<PortalAdminGoldStarEvent>> GetPortalAdminGoldStarEventsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT e.event_id, e.event_name, e.description, e.event_date, e.active, (SELECT COUNT(1) FROM student_gold_stars g WHERE g.event_id = e.event_id) FROM gold_star_events e ORDER BY e.active DESC, e.event_date IS NULL, e.event_date, e.event_name";
        var list = new List<PortalAdminGoldStarEvent>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.GetInt32(4) != 0, reader.GetInt32(5)));
        return list;
    }

    public async Task<IReadOnlyList<PortalAdminGuardian>> GetPortalAdminGuardiansAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT g.guardian_id, g.first_name, g.middle_initial, g.last_name, g.pronouns, g.email, g.phone, g.newsletter_opt_in, g.sms_opt_in, g.active, s.student_id, TRIM(s.first_name || ' ' || s.last_name), sg.relationship FROM guardians g LEFT JOIN student_guardians sg ON sg.guardian_id = g.guardian_id LEFT JOIN students s ON s.student_id = sg.student_id WHERE g.active = 1 ORDER BY g.last_name, g.first_name, s.last_name, s.first_name";
        var guardians = new Dictionary<int, (string FirstName, string? MiddleInitial, string LastName, string? Pronouns, string? Email, string? Phone, bool NewsletterOptIn, bool SmsOptIn, bool IsActive, List<PortalAdminGuardianStudent> Students)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var guardianId = reader.GetInt32(0);
            if (!guardians.TryGetValue(guardianId, out var guardian))
            {
                guardian = (reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), !reader.IsDBNull(7) && reader.GetInt32(7) != 0, !reader.IsDBNull(8) && reader.GetInt32(8) != 0, reader.GetInt32(9) != 0, new List<PortalAdminGuardianStudent>());
                guardians.Add(guardianId, guardian);
            }

            if (!reader.IsDBNull(10) && !guardian.Students.Any(student => student.StudentId == reader.GetInt32(10)))
                guardian.Students.Add(new(reader.GetInt32(10), reader.GetString(11), reader.IsDBNull(12) ? "Guardian" : reader.GetString(12)));
        }

        return guardians.Select(pair => new PortalAdminGuardian(pair.Key, pair.Value.FirstName, pair.Value.LastName, pair.Value.Email, pair.Value.Phone, pair.Value.IsActive, pair.Value.Students, pair.Value.MiddleInitial, pair.Value.Pronouns, pair.Value.NewsletterOptIn, pair.Value.SmsOptIn)).ToList();
    }

    public async Task<IReadOnlyList<PortalAdminGuardianStudent>> GetPortalAdminGuardianStudentsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT s.student_id, s.first_name, s.last_name, s.nickname, s.honorific, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1) FROM students s LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE s.active = 1 ORDER BY s.last_name, s.first_name";
        var students = new List<PortalAdminGuardianStudent>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) { var rank = reader.IsDBNull(6) ? null : reader.GetString(6); students.Add(new(reader.GetInt32(0), BuildStudentDisplayName(reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4), reader.GetString(5), rank), "Guardian")); }
        return students;
    }

    public async Task<int> CreatePortalGuardianAsync(PortalGuardianWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        var relationship = string.IsNullOrWhiteSpace(request.Relationship) ? "Guardian" : request.Relationship.Trim();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO guardians(first_name, middle_initial, last_name, pronouns, email, phone, newsletter_opt_in, sms_opt_in, active, updated_at) VALUES ($first, $middle, $last, $pronouns, $email, $phone, $newsletter, $sms, 1, $now); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$first", request.FirstName.Trim()); command.Parameters.AddWithValue("$middle", (object?)NullIfBlank(request.MiddleInitial) ?? DBNull.Value); command.Parameters.AddWithValue("$last", request.LastName.Trim()); command.Parameters.AddWithValue("$pronouns", (object?)NullIfBlank(request.Pronouns) ?? DBNull.Value); command.Parameters.AddWithValue("$email", (object?)NullIfBlank(request.Email) ?? DBNull.Value); command.Parameters.AddWithValue("$phone", (object?)NullIfBlank(request.Phone) ?? DBNull.Value); command.Parameters.AddWithValue("$newsletter", request.NewsletterOptIn ? 1 : 0); command.Parameters.AddWithValue("$sms", request.SmsOptIn ? 1 : 0); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        var guardianId = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        await ReplaceGuardianLinksAsync(connection, guardianId, request.StudentIds, relationship, cancellationToken);
        return guardianId;
    }

    public async Task<bool> UpdatePortalGuardianAsync(int guardianId, PortalGuardianWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        var relationship = string.IsNullOrWhiteSpace(request.Relationship) ? "Guardian" : request.Relationship.Trim();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE guardians SET first_name = $first, middle_initial = $middle, last_name = $last, pronouns = $pronouns, email = $email, phone = $phone, newsletter_opt_in = $newsletter, sms_opt_in = $sms, updated_at = $now WHERE guardian_id = $id";
        command.Parameters.AddWithValue("$first", request.FirstName.Trim()); command.Parameters.AddWithValue("$middle", (object?)NullIfBlank(request.MiddleInitial) ?? DBNull.Value); command.Parameters.AddWithValue("$last", request.LastName.Trim()); command.Parameters.AddWithValue("$pronouns", (object?)NullIfBlank(request.Pronouns) ?? DBNull.Value); command.Parameters.AddWithValue("$email", (object?)NullIfBlank(request.Email) ?? DBNull.Value); command.Parameters.AddWithValue("$phone", (object?)NullIfBlank(request.Phone) ?? DBNull.Value); command.Parameters.AddWithValue("$newsletter", request.NewsletterOptIn ? 1 : 0); command.Parameters.AddWithValue("$sms", request.SmsOptIn ? 1 : 0); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); command.Parameters.AddWithValue("$id", guardianId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) return false;
        await ReplaceGuardianLinksAsync(connection, guardianId, request.StudentIds, relationship, cancellationToken);
        return true;
    }

    public async Task<bool> AddStudentProgramAsync(int studentId, PortalStudentProgramWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ProgramId <= 0 || string.IsNullOrWhiteSpace(request.ProgressionType)) return false;
        await using var connection = await OpenAsync(cancellationToken);
        await using var program = connection.CreateCommand();
        program.CommandText = "SELECT program_name FROM portal_programs WHERE program_id = $program AND active = 1; SELECT COUNT(1) FROM students WHERE student_id = $student AND active = 1";
        program.Parameters.AddWithValue("$program", request.ProgramId);
        program.Parameters.AddWithValue("$student", studentId);
        await using var reader = await program.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return false;
        var programName = reader.GetString(0);
        if (!await reader.NextResultAsync(cancellationToken) || !await reader.ReadAsync(cancellationToken) || reader.GetInt32(0) != 1) return false;
        await reader.DisposeAsync();
        var programCode = programName switch
        {
            "Kids" => "kids",
            "Teen/Adult" => "teen-adult",
            "IS3" => "is3",
            _ => $"program-{request.ProgramId}"
        };
        var enrolledDate = (request.EnrolledDate ?? DateTime.UtcNow.Date).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE student_program_memberships
SET program_code = $code, program_name = $name, progression_type = $progression, level_name = $level, enrolled_date = $enrolled, active = 1
WHERE student_id = $student AND program_name = $name;
INSERT INTO student_program_memberships(student_id, program_code, program_name, progression_type, level_name, enrolled_date, active)
SELECT $student, $code, $name, $progression, $level, $enrolled, 1
WHERE changes() = 0
  AND NOT EXISTS (SELECT 1 FROM student_program_memberships WHERE student_id = $student AND program_code = $code);";
        command.Parameters.AddWithValue("$student", studentId);
        command.Parameters.AddWithValue("$code", programCode);
        command.Parameters.AddWithValue("$name", programName);
        command.Parameters.AddWithValue("$progression", request.ProgressionType.Trim());
        command.Parameters.AddWithValue("$level", (object?)NullIfBlank(request.LevelName) ?? DBNull.Value);
        command.Parameters.AddWithValue("$enrolled", enrolledDate);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateStudentRolesAsync(int studentId, IEnumerable<string>? roles, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE students SET roles = $roles, updated_at = $now WHERE student_id = $id";
        command.Parameters.AddWithValue("$roles", string.Join(',', NormalizeStudentRoles(roles)));
        command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$id", studentId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> RemoveStudentProgramAsync(int studentId, string programCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(programCode)) return false;
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE student_program_memberships
SET active = 0
WHERE student_id = $student
  AND active = 1
  AND (program_code = $code OR program_code = CASE $code
        WHEN 'program-1' THEN 'kids'
        WHEN 'program-7' THEN 'teen-adult'
        WHEN 'program-17' THEN 'is3'
        ELSE $code END)";
        command.Parameters.AddWithValue("$student", studentId);
        command.Parameters.AddWithValue("$code", programCode.Trim());
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private static async Task ReplaceGuardianLinksAsync(SqliteConnection connection, int guardianId, IReadOnlyList<int>? studentIds, string relationship, CancellationToken cancellationToken)
    {
        await using var delete = connection.CreateCommand(); delete.CommandText = "DELETE FROM student_guardians WHERE guardian_id = $guardian"; delete.Parameters.AddWithValue("$guardian", guardianId); await delete.ExecuteNonQueryAsync(cancellationToken);
        foreach (var studentId in (studentIds ?? Array.Empty<int>()).Distinct())
        {
            await using var link = connection.CreateCommand(); link.CommandText = "INSERT OR IGNORE INTO student_guardians(student_id, guardian_id, relationship, is_primary) SELECT $student, $guardian, $relationship, CASE WHEN NOT EXISTS (SELECT 1 FROM student_guardians WHERE student_id = $student) THEN 1 ELSE 0 END FROM students WHERE student_id = $student AND active = 1"; link.Parameters.AddWithValue("$student", studentId); link.Parameters.AddWithValue("$guardian", guardianId); link.Parameters.AddWithValue("$relationship", relationship); await link.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task<long> CreateGoldStarEventAsync(string name, string description, DateTime? eventDate, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO gold_star_events(event_name, description, event_date, active) VALUES ($name, $description, $date, 1); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name", name.Trim()); command.Parameters.AddWithValue("$description", description.Trim()); command.Parameters.AddWithValue("$date", eventDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? (object)DBNull.Value);
        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public async Task<bool> SetGoldStarEventActiveAsync(long eventId, bool active, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "UPDATE gold_star_events SET active = $active WHERE event_id = $event"; command.Parameters.AddWithValue("$active", active ? 1 : 0); command.Parameters.AddWithValue("$event", eventId); return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> AwardGoldStarAsync(int studentId, long eventId, string? note, DateTime? eventDate, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var check = connection.CreateCommand(); check.CommandText = "SELECT COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), e.event_name FROM students s LEFT JOIN student_profiles p ON p.student_id = s.student_id CROSS JOIN gold_star_events e WHERE s.student_id = $student AND s.active = 1 AND e.event_id = $event"; check.Parameters.AddWithValue("$student", studentId); check.Parameters.AddWithValue("$event", eventId);
        await using var reader = await check.ExecuteReaderAsync(cancellationToken); if (!await reader.ReadAsync(cancellationToken)) return false; var studentName = reader.GetString(0); var eventName = reader.GetString(1);
        await reader.DisposeAsync();
        await using var award = connection.CreateCommand(); award.CommandText = "INSERT OR IGNORE INTO student_gold_stars(student_id, event_id, note, event_date, awarded_at) VALUES ($student, $event, $note, $eventDate, $now)"; award.Parameters.AddWithValue("$student", studentId); award.Parameters.AddWithValue("$event", eventId); award.Parameters.AddWithValue("$note", (object?)note?.Trim() ?? DBNull.Value); award.Parameters.AddWithValue("$eventDate", eventDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? (object)DBNull.Value); award.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); var changed = await award.ExecuteNonQueryAsync(cancellationToken) > 0;
        if (changed) await AddNewsPostAsync(connection, $"Gold Star earned — {studentName}", $"{studentName} received a Gold Star for {eventName}.", cancellationToken);
        return changed;
    }

    public async Task<bool> RemoveGoldStarAsync(int studentId, long eventId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "DELETE FROM student_gold_stars WHERE student_id = $student AND event_id = $event"; command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$event", eventId); return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IReadOnlyList<PortalAdminAchievement>> GetPortalAdminAchievementsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT a.achievement_name, COALESCE(a.description, ''), COALESCE(a.icon_name, 'star'), (SELECT COUNT(1) FROM student_achievements sa WHERE sa.achievement_id = a.achievement_id) FROM achievements a WHERE a.active = 1 ORDER BY a.achievement_id"; var list = new List<PortalAdminAchievement>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3))); return list;
    }

    public async Task<IReadOnlyList<PortalAdminAchievementDefinition>> GetPortalAdminAchievementDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "SELECT a.achievement_id, a.achievement_name, COALESCE(a.description, ''), COALESCE(a.icon_name, 'star'), a.active, (SELECT COUNT(1) FROM student_achievements sa WHERE sa.achievement_id = a.achievement_id) FROM achievements a ORDER BY a.active DESC, a.achievement_id";
        var list = new List<PortalAdminAchievementDefinition>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4) != 0, reader.GetInt32(5)));
        return list;
    }

    public async Task<long> CreatePortalAdminAchievementAsync(PortalAdminAchievementWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO achievements(achievement_name, description, icon_name, active) VALUES ($name, $description, $icon, 1); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name", request.Name.Trim()); command.Parameters.AddWithValue("$description", request.Description.Trim()); command.Parameters.AddWithValue("$icon", string.IsNullOrWhiteSpace(request.IconName) ? "star" : request.IconName.Trim());
        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public async Task<bool> SetPortalAdminAchievementActiveAsync(long achievementId, bool active, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "UPDATE achievements SET active = $active WHERE achievement_id = $id"; command.Parameters.AddWithValue("$active", active ? 1 : 0); command.Parameters.AddWithValue("$id", achievementId); return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<IReadOnlyList<PortalAdminMission>> GetPortalAdminMissionsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT m.mission_id, m.title, m.description, m.category, m.active, (SELECT COUNT(1) FROM student_mission_progress p WHERE p.mission_id = m.mission_id AND p.completed = 1) FROM training_missions m ORDER BY m.active DESC, m.category, m.title COLLATE NOCASE";
        var list = new List<PortalAdminMission>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4) != 0, reader.GetInt32(5))); return list;
    }

    public async Task<long> CreatePortalAdminMissionAsync(PortalAdminMissionWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO training_missions(title, description, category, active) VALUES ($title, $description, $category, 1); SELECT last_insert_rowid();"; command.Parameters.AddWithValue("$title", request.Title.Trim()); command.Parameters.AddWithValue("$description", request.Description.Trim()); command.Parameters.AddWithValue("$category", request.Category.Trim()); return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public async Task<bool> SetPortalAdminMissionActiveAsync(long missionId, bool active, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "UPDATE training_missions SET active = $active WHERE mission_id = $id"; command.Parameters.AddWithValue("$active", active ? 1 : 0); command.Parameters.AddWithValue("$id", missionId); return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<IReadOnlyList<PortalAdminRank>> GetPortalAdminRanksAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT r.rank_id, r.rank_name, r.rank_order, r.display_color, r.active, (SELECT COUNT(DISTINCT h.student_id) FROM student_rank_history h WHERE h.rank_id = r.rank_id AND h.student_rank_id = (SELECT MAX(h2.student_rank_id) FROM student_rank_history h2 WHERE h2.student_id = h.student_id)) FROM ranks r ORDER BY r.active DESC, r.rank_order, r.rank_name COLLATE NOCASE";
        var list = new List<PortalAdminRank>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetInt32(4) != 0, reader.GetInt32(5))); return list;
    }

    public async Task<int> CreatePortalAdminRankAsync(PortalAdminRankWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO ranks(rank_name, rank_order, display_color, active) SELECT $name, COALESCE(MAX(rank_order), 0) + 1, $color, 1 FROM ranks; SELECT last_insert_rowid();"; command.Parameters.AddWithValue("$name", request.Name.Trim()); command.Parameters.AddWithValue("$color", (object?)NullIfBlank(request.DisplayColor) ?? DBNull.Value); return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public async Task<bool> SetPortalAdminRankActiveAsync(int rankId, bool active, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "UPDATE ranks SET active = $active WHERE rank_id = $id"; command.Parameters.AddWithValue("$active", active ? 1 : 0); command.Parameters.AddWithValue("$id", rankId); return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> AssignPortalAdminRankAsync(int studentId, PortalAdminStudentRankRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var check = connection.CreateCommand(); check.Transaction = (SqliteTransaction)transaction; check.CommandText = "SELECT COUNT(1) FROM students WHERE student_id = $student AND active = 1" + (request.RankId > 0 ? "; SELECT COUNT(1) FROM ranks WHERE rank_id = $rank AND active = 1" : string.Empty); check.Parameters.AddWithValue("$student", studentId); check.Parameters.AddWithValue("$rank", request.RankId);
        await using var reader = await check.ExecuteReaderAsync(cancellationToken); if (!await reader.ReadAsync(cancellationToken) || reader.GetInt32(0) != 1) return false; if (request.RankId > 0 && (!await reader.NextResultAsync(cancellationToken) || !await reader.ReadAsync(cancellationToken) || reader.GetInt32(0) != 1)) return false; await reader.DisposeAsync();
        if (request.RankId <= 0)
        {
            await using var clear = connection.CreateCommand(); clear.Transaction = (SqliteTransaction)transaction; clear.CommandText = "DELETE FROM student_rank_history WHERE student_rank_id = (SELECT student_rank_id FROM student_rank_history WHERE student_id = $student ORDER BY awarded_date DESC, student_rank_id DESC LIMIT 1)"; clear.Parameters.AddWithValue("$student", studentId); await clear.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            await using var latest = connection.CreateCommand(); latest.Transaction = (SqliteTransaction)transaction; latest.CommandText = "SELECT student_rank_id, rank_id FROM student_rank_history WHERE student_id = $student ORDER BY awarded_date DESC, student_rank_id DESC LIMIT 1"; latest.Parameters.AddWithValue("$student", studentId);
            await using var latestReader = await latest.ExecuteReaderAsync(cancellationToken);
            long? latestId = null; var latestRankId = 0;
            if (await latestReader.ReadAsync(cancellationToken)) { latestId = latestReader.GetInt64(0); latestRankId = latestReader.GetInt32(1); }
            await latestReader.DisposeAsync();
            if (latestId.HasValue && latestRankId == request.RankId)
            {
                await using var update = connection.CreateCommand(); update.Transaction = (SqliteTransaction)transaction; update.CommandText = "UPDATE student_rank_history SET awarded_date = $date, notes = $notes WHERE student_rank_id = $id"; update.Parameters.AddWithValue("$date", (request.AwardedDate ?? DateTime.UtcNow.Date).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); update.Parameters.AddWithValue("$notes", (object?)NullIfBlank(request.Notes) ?? DBNull.Value); update.Parameters.AddWithValue("$id", latestId.Value); await update.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                await using var insert = connection.CreateCommand(); insert.Transaction = (SqliteTransaction)transaction; insert.CommandText = "INSERT INTO student_rank_history(student_id, rank_id, awarded_date, notes) VALUES ($student, $rank, $date, $notes)"; insert.Parameters.AddWithValue("$student", studentId); insert.Parameters.AddWithValue("$rank", request.RankId); insert.Parameters.AddWithValue("$date", (request.AwardedDate ?? DateTime.UtcNow.Date).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); insert.Parameters.AddWithValue("$notes", (object?)NullIfBlank(request.Notes) ?? DBNull.Value); await insert.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        await transaction.CommitAsync(cancellationToken); return true;
    }

    public async Task<int> RecalculateAutomaticMilestonesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT student_id FROM students WHERE active = 1"; var ids = new List<int>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) ids.Add(reader.GetInt32(0)); var awarded = 0; foreach (var id in ids) awarded += await AwardAutomaticMilestonesAsync(connection, id, cancellationToken); return awarded;
    }

    public async Task<int> RefreshAutomaticMilestonesAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        return await AwardAutomaticMilestonesAsync(connection, studentId, cancellationToken);
    }

    private static async Task<int> AwardAutomaticMilestonesAsync(SqliteConnection connection, int studentId, CancellationToken cancellationToken)
    {
		await using var countCommand = connection.CreateCommand(); countCommand.CommandText = "SELECT COUNT(1) FROM attendance WHERE student_id = $student AND status = 'present'"; countCommand.Parameters.AddWithValue("$student", studentId); var totalClasses = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
		await using var helperCountCommand = connection.CreateCommand(); helperCountCommand.CommandText = "SELECT COUNT(1) FROM attendance WHERE student_id = $student AND status = 'helper'"; helperCountCommand.Parameters.AddWithValue("$student", studentId); var helperClasses = Convert.ToInt32(await helperCountCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
		await using var dojoCheckInCountCommand = connection.CreateCommand(); dojoCheckInCountCommand.CommandText = "SELECT COUNT(1) FROM dojo_check_ins WHERE student_id = $student"; dojoCheckInCountCommand.Parameters.AddWithValue("$student", studentId); var dojoCheckIns = Convert.ToInt32(await dojoCheckInCountCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        var streak = await GetAttendanceStreakAsync(connection, studentId, cancellationToken);
        await using var startCommand = connection.CreateCommand(); startCommand.CommandText = "SELECT start_date FROM students WHERE student_id = $student"; startCommand.Parameters.AddWithValue("$student", studentId);
        var startValue = await startCommand.ExecuteScalarAsync(cancellationToken);
        var yearsAtDojo = 0;
        if (startValue is string startText && DateTime.TryParse(startText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
        {
            var today = DateTime.Now.Date;
            yearsAtDojo = today.Year - startDate.Year;
            if (today < startDate.AddYears(yearsAtDojo).Date) yearsAtDojo--;
        }
        var milestones = new List<(string Name, bool Qualifies)> { ("First Class!", totalClasses >= 1), ("3 Classes", totalClasses >= 3), ("5 Classes", totalClasses >= 5), ("10 Classes Strong", totalClasses >= 10), ("25 Classes", totalClasses >= 25), ("50 Classes", totalClasses >= 50), ("100 Classes", totalClasses >= 100), ("250 Classes", totalClasses >= 250), ("500 Classes", totalClasses >= 500), ("1,000 Classes", totalClasses >= 1000), ("2,500 Classes", totalClasses >= 2500), ("5,000 Classes", totalClasses >= 5000), ("10,000 Classes", totalClasses >= 10000), ("1 Helper Class", helperClasses >= 1), ("10 Helper Classes", helperClasses >= 10), ("100 Helper Classes", helperClasses >= 100), ("1,000 Helper Classes", helperClasses >= 1000), ("10,000 Helper Classes", helperClasses >= 10000), ("First Studio Check-in", dojoCheckIns >= 1), ("5 Studio Check-ins", dojoCheckIns >= 5), ("25 Studio Check-ins", dojoCheckIns >= 25), ("100 Studio Check-ins", dojoCheckIns >= 100), ("7-Day Rhythm", streak >= 7), ("14-Day Rhythm", streak >= 14), ("30-Day Rhythm", streak >= 30), ("60-Day Rhythm", streak >= 60), ("90-Day Rhythm", streak >= 90), ("180-Day Rhythm", streak >= 180), ("1-Year Dojo Anniversary", yearsAtDojo >= 1), ("3-Year Dojo Anniversary", yearsAtDojo >= 3), ("5-Year Dojo Anniversary", yearsAtDojo >= 5), ("10-Year Dojo Anniversary", yearsAtDojo >= 10) };
        var awarded = 0;
        foreach (var milestone in milestones.Where(x => x.Qualifies))
        {
            await using var command = connection.CreateCommand(); command.CommandText = "INSERT OR IGNORE INTO student_achievements(student_id, achievement_id, awarded_at) SELECT $student, achievement_id, $now FROM achievements WHERE achievement_name = $name"; command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$name", milestone.Name); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); if (await command.ExecuteNonQueryAsync(cancellationToken) > 0) { awarded++; await AddNewsPostAsync(connection, $"Milestone unlocked — {milestone.Name}", $"A training milestone was automatically awarded after attendance criteria were met.", cancellationToken); }
        }
        return awarded;
    }

    private static async Task<int> GetAttendanceStreakAsync(SqliteConnection connection, int studentId, CancellationToken cancellationToken)
    {
		await using var command = connection.CreateCommand(); command.CommandText = "SELECT DISTINCT date(check_in_time) FROM attendance WHERE student_id = $student AND status = 'present' ORDER BY date(check_in_time) DESC"; command.Parameters.AddWithValue("$student", studentId); var dates = new List<DateOnly>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) if (DateOnly.TryParse(reader.GetString(0), out var date)) dates.Add(date); if (dates.Count == 0) return 0; var streak = 1; for (var index = 1; index < dates.Count; index++) { if (dates[index - 1].DayNumber - dates[index].DayNumber != 1) break; streak++; } return streak;
    }

    private static async Task AddNewsPostAsync(SqliteConnection connection, string title, string body, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO posts(student_id, post_text, post_type, moderation_status, visible, created_at, updated_at) SELECT (SELECT student_id FROM students ORDER BY student_id LIMIT 1), $text, 'news', 'approved', 1, $now, $now WHERE EXISTS (SELECT 1 FROM students)"; command.Parameters.AddWithValue("$text", $"{title} — {body}"); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
