using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace TaskKarate.Api.Services;

public sealed class StarterDatabaseOptions
{
    public string Path { get; set; } = string.Empty;
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
public sealed record PortalAttendanceItem(long AttendanceId, int StudentId, string StudentName, DateTime CheckedInAtUtc, string? Notes, string Status);
public sealed record CheckInResult(bool Success, bool Duplicate, bool HelperRecorded, string? Error);
public sealed record GuardianItem(string Name, string Relationship, string? Phone, string? Email);
public sealed record ProgramMembershipItem(string ProgramName, string ProgramCode, string ProgressionType, string? LevelName, string? EnrolledDate);
public sealed record StudentProfile(int StudentId, string DisplayName, string? Bio, string? FavoriteTechnique, string? ProfileImagePath, string? RankName, string? JoinDate, int TotalClasses, int ClassesThisMonth, int UnreadMessages, int AchievementCount, IReadOnlyList<string> AttendanceDates, int ClassesIntoStripe, int ClassesPerStripe, int ClassesToNextStripe, string NextMilestone, string? AgeGroup, DateTime? BirthDate, string? Email, string? Phone, string? UniformSize, string? BeltSize, IReadOnlyList<GuardianItem> Guardians, IReadOnlyList<ProgramMembershipItem> Programs);
public sealed record FriendshipItem(int StudentId, string DisplayName, string? RankName, string Status, bool Incoming);
public sealed record MessageItem(long MessageId, int SenderId, string SenderName, int RecipientId, string RecipientName, string MessageText, DateTime CreatedAt, bool IsRead);
public sealed record AchievementItem(string Name, string? Description, string? IconName, DateTime? AwardedAt);
public sealed record NewsItem(long Id, string Title, string Body, DateTime PublishedAt);
public sealed record ReactionSummary(string Code, string Label, string Icon, int Count, bool Selected);
public sealed record FeedItem(long PostId, int AuthorId, string AuthorName, string? RankName, string Text, string PostType, DateTime CreatedAt, IReadOnlyList<ReactionSummary> Reactions, int CommentCount);
public sealed record CommentItem(long CommentId, int AuthorId, string AuthorName, string Text, DateTime CreatedAt);
public sealed record StaffSocialPost(long PostId, int AuthorId, string AuthorName, string Text, string PostType, string ModerationStatus, bool Visible, DateTime CreatedAt, DateTime UpdatedAt, int CommentCount);
public sealed record StaffSocialComment(long CommentId, long PostId, int AuthorId, string AuthorName, string Text, bool Visible, string ModerationStatus, DateTime CreatedAt);
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
    string DisplayName,
    string? RankName,
    string? AgeGroup,
    DateTime? BirthDate,
    bool IsActive,
    string? JoinDate,
    string? UniformSize,
    string? BeltSize,
    string? Bio,
    string? FavoriteTechnique,
    int TotalClasses,
    int AttendanceStreak,
    int AchievementCount,
    int GoldStarCount,
    IReadOnlyList<string> Programs,
    string Status);
public sealed record PortalAdminGoldStarEvent(long EventId, string Name, string Description, DateTime? EventDate, bool IsActive, int AwardCount);
public sealed record PortalAdminAchievement(string Name, string Description, string IconName, int AwardCount);
public sealed record StaffHelperSignupItem(string StaffUserId, string DisplayName, DateTime SignedUpAt, bool IsCurrentStaff);
public sealed record StaffHelperRosterItem(int SessionId, DateTime SessionDate, string? StartTime, string? EndTime, string ClassName, string? Location, bool Cancelled, IReadOnlyList<StaffHelperSignupItem> StaffHelpers, IReadOnlyList<PortalAttendanceItem> StudentHelpers);
public sealed record PortalStudentWriteRequest(string FirstName, string LastName, string? PreferredName, string AgeGroup, DateTime? BirthDate, DateTime? JoinDate, string? UniformSize, string? BeltSize, string? Bio, string? FavoriteTechnique);
public sealed record PortalAdminGuardianStudent(int StudentId, string Name, string Relationship);
public sealed record PortalAdminGuardian(int GuardianId, string FirstName, string LastName, string? Email, string? Phone, bool IsActive, IReadOnlyList<PortalAdminGuardianStudent> Students);
public sealed record PortalGuardianWriteRequest(string FirstName, string LastName, string? Email, string? Phone, IReadOnlyList<int>? StudentIds);

public sealed class StudentExperienceService
{
    private readonly StarterDatabaseOptions _options;
    private readonly IPasswordHasher<StarterStudentAccount> _passwordHasher;
    private readonly ILogger<StudentExperienceService> _logger;
    private int _ready;

    public StudentExperienceService(IOptions<StarterDatabaseOptions> options, IPasswordHasher<StarterStudentAccount> passwordHasher, ILogger<StudentExperienceService> logger)
    {
        _options = options.Value;
        _passwordHasher = passwordHasher;
        _logger = logger;
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
        await using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS users (user_id INTEGER PRIMARY KEY AUTOINCREMENT, username TEXT UNIQUE, password_hash TEXT, display_name TEXT NOT NULL, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS instructors (instructor_id INTEGER PRIMARY KEY AUTOINCREMENT, display_name TEXT NOT NULL, active INTEGER NOT NULL DEFAULT 1);
CREATE TABLE IF NOT EXISTS ranks (rank_id INTEGER PRIMARY KEY AUTOINCREMENT, rank_name TEXT NOT NULL UNIQUE, rank_order INTEGER NOT NULL UNIQUE, display_color TEXT, active INTEGER NOT NULL DEFAULT 1);
CREATE TABLE IF NOT EXISTS students (student_id INTEGER PRIMARY KEY AUTOINCREMENT, first_name TEXT NOT NULL, middle_name TEXT, last_name TEXT NOT NULL, preferred_name TEXT, birth_date TEXT, email TEXT, phone TEXT, start_date TEXT, active INTEGER NOT NULL DEFAULT 1, archived_at TEXT, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS guardians (guardian_id INTEGER PRIMARY KEY AUTOINCREMENT, first_name TEXT NOT NULL, last_name TEXT NOT NULL, email TEXT, phone TEXT, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS student_guardians (student_id INTEGER NOT NULL, guardian_id INTEGER NOT NULL, relationship TEXT NOT NULL DEFAULT 'Guardian', is_primary INTEGER NOT NULL DEFAULT 0, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(student_id, guardian_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(guardian_id) REFERENCES guardians(guardian_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_program_memberships (student_program_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, program_code TEXT NOT NULL, program_name TEXT NOT NULL, progression_type TEXT NOT NULL, level_name TEXT, enrolled_date TEXT, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, UNIQUE(student_id, program_code), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_profiles (student_id INTEGER PRIMARY KEY, display_name TEXT, bio TEXT, profile_image_path TEXT, favorite_technique TEXT, profile_visible INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_rank_history (student_rank_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, rank_id INTEGER NOT NULL, awarded_date TEXT NOT NULL, awarded_by_user_id INTEGER, notes TEXT, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(rank_id) REFERENCES ranks(rank_id));
CREATE TABLE IF NOT EXISTS classes (class_id INTEGER PRIMARY KEY AUTOINCREMENT, class_name TEXT NOT NULL, description TEXT, active INTEGER NOT NULL DEFAULT 1);
CREATE TABLE IF NOT EXISTS class_schedule (schedule_id INTEGER PRIMARY KEY AUTOINCREMENT, class_id INTEGER NOT NULL, day_of_week INTEGER NOT NULL, start_time TEXT NOT NULL, end_time TEXT, instructor_id INTEGER, location_name TEXT, active INTEGER NOT NULL DEFAULT 1, FOREIGN KEY(class_id) REFERENCES classes(class_id));
CREATE TABLE IF NOT EXISTS class_sessions (session_id INTEGER PRIMARY KEY AUTOINCREMENT, class_id INTEGER NOT NULL, schedule_id INTEGER, session_date TEXT NOT NULL, start_time TEXT, end_time TEXT, instructor_id INTEGER, location_name TEXT, cancelled INTEGER NOT NULL DEFAULT 0, cancellation_reason TEXT, notes TEXT, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(class_id) REFERENCES classes(class_id), UNIQUE(class_id, session_date, start_time));
CREATE TABLE IF NOT EXISTS attendance (attendance_id INTEGER PRIMARY KEY AUTOINCREMENT, session_id INTEGER NOT NULL, student_id INTEGER NOT NULL, check_in_time TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, check_out_time TEXT, status TEXT NOT NULL DEFAULT 'present', checked_in_by_user_id INTEGER, notes TEXT, FOREIGN KEY(session_id) REFERENCES class_sessions(session_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, UNIQUE(session_id, student_id));
CREATE TABLE IF NOT EXISTS achievements (achievement_id INTEGER PRIMARY KEY AUTOINCREMENT, achievement_name TEXT NOT NULL UNIQUE, description TEXT, icon_name TEXT, active INTEGER NOT NULL DEFAULT 1);
CREATE TABLE IF NOT EXISTS student_achievements (student_id INTEGER NOT NULL, achievement_id INTEGER NOT NULL, awarded_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(student_id, achievement_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(achievement_id) REFERENCES achievements(achievement_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS gold_star_events (event_id INTEGER PRIMARY KEY AUTOINCREMENT, event_name TEXT NOT NULL UNIQUE, description TEXT NOT NULL, event_date TEXT, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS student_gold_stars (student_id INTEGER NOT NULL, event_id INTEGER NOT NULL, awarded_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, note TEXT, PRIMARY KEY(student_id, event_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(event_id) REFERENCES gold_star_events(event_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_event_interests (student_id INTEGER NOT NULL, event_id INTEGER NOT NULL, status TEXT NOT NULL DEFAULT 'interested', created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(student_id, event_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(event_id) REFERENCES gold_star_events(event_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS training_missions (mission_id INTEGER PRIMARY KEY AUTOINCREMENT, title TEXT NOT NULL UNIQUE, description TEXT NOT NULL, category TEXT NOT NULL, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS student_mission_progress (student_id INTEGER NOT NULL, mission_id INTEGER NOT NULL, completed INTEGER NOT NULL DEFAULT 0, completed_at TEXT, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(student_id, mission_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(mission_id) REFERENCES training_missions(mission_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_daily_missions (daily_mission_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, mission_date TEXT NOT NULL, mission_key TEXT NOT NULL, title TEXT NOT NULL, description TEXT NOT NULL, category TEXT NOT NULL, completed INTEGER NOT NULL DEFAULT 0, completed_at TEXT, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, UNIQUE(student_id, mission_date, mission_key), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS posts (post_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER, user_id INTEGER, post_text TEXT NOT NULL, post_type TEXT NOT NULL DEFAULT 'student', moderation_status TEXT NOT NULL DEFAULT 'approved', visible INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS student_accounts (student_id INTEGER PRIMARY KEY, username TEXT NOT NULL UNIQUE COLLATE NOCASE, password_hash TEXT NOT NULL, pin_hash TEXT, must_change_password INTEGER NOT NULL DEFAULT 0, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_disclaimer_acceptances (acceptance_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, disclaimer_version TEXT NOT NULL, accepted_at TEXT NOT NULL, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_friendships (student_id INTEGER NOT NULL, friend_id INTEGER NOT NULL, status TEXT NOT NULL, requested_by INTEGER NOT NULL, created_at TEXT NOT NULL, updated_at TEXT NOT NULL, PRIMARY KEY(student_id, friend_id), CHECK(student_id <> friend_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(friend_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_messages (message_id INTEGER PRIMARY KEY AUTOINCREMENT, sender_id INTEGER NOT NULL, recipient_id INTEGER NOT NULL, message_text TEXT NOT NULL, created_at TEXT NOT NULL, read_at TEXT, FOREIGN KEY(sender_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(recipient_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS post_reactions (post_id INTEGER NOT NULL, student_id INTEGER NOT NULL, reaction_code TEXT NOT NULL, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(post_id, student_id), CHECK(reaction_code IN ('fist_bump', 'respect', 'fire')), FOREIGN KEY(post_id) REFERENCES posts(post_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS post_comments (comment_id INTEGER PRIMARY KEY AUTOINCREMENT, post_id INTEGER NOT NULL, student_id INTEGER NOT NULL, comment_text TEXT NOT NULL, visible INTEGER NOT NULL DEFAULT 1, moderation_status TEXT NOT NULL DEFAULT 'approved', created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(post_id) REFERENCES posts(post_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_practice_logs (practice_log_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, skill TEXT NOT NULL, minutes INTEGER NOT NULL, reflection TEXT, logged_at TEXT NOT NULL, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_goals (goal_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, title TEXT NOT NULL, target_date TEXT, completed INTEGER NOT NULL DEFAULT 0, created_at TEXT NOT NULL, completed_at TEXT, UNIQUE(student_id, title), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS post_bookmarks (post_id INTEGER NOT NULL, student_id INTEGER NOT NULL, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(post_id, student_id), FOREIGN KEY(post_id) REFERENCES posts(post_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS staff_helper_signups (signup_id INTEGER PRIMARY KEY AUTOINCREMENT, session_id INTEGER NOT NULL, staff_user_id TEXT NOT NULL, staff_display_name TEXT NOT NULL, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, UNIQUE(session_id, staff_user_id), FOREIGN KEY(session_id) REFERENCES class_sessions(session_id) ON DELETE CASCADE);
CREATE INDEX IF NOT EXISTS ix_student_accounts_username ON student_accounts(username);
CREATE INDEX IF NOT EXISTS ix_class_sessions_date ON class_sessions(session_date, cancelled);
CREATE INDEX IF NOT EXISTS ix_attendance_student ON attendance(student_id, session_id);
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
";
        await command.ExecuteNonQueryAsync(cancellationToken);
        await EnsureLegacyStudentColumnsAsync(connection, cancellationToken);
        await EnsureStudentAccountColumnsAsync(connection, cancellationToken);
        await EnsureLegacyReactionSchemaAsync(connection, cancellationToken);
        await ImportDevelopmentDataAsync(connection, cancellationToken);
        await NormalizeLegacyDataAsync(connection, cancellationToken);
        await SeedDevelopmentGoldStarEventsAsync(connection, cancellationToken);
        await MaterializeUpcomingSessionsAsync(connection, cancellationToken);
        await SeedDemoDataAsync(connection, cancellationToken);
        await EnsureBootstrapAccountAsync(connection, cancellationToken);
        Interlocked.Exchange(ref _ready, 1);
    }

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
SELECT student.student_id, 'karate', 'Task Karate', 'belt', 'Green Belt', student.start_date
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
                var groupName = group.GetProperty("name").GetString() ?? "Class";
                var className = $"{programName} — {groupName}";
                var description = programProperty.Value.TryGetProperty("description", out var descriptionProperty) ? descriptionProperty.GetString() : null;
                var classId = await FindOrInsertClassAsync(connection, className, description, cancellationToken);
                if (!group.TryGetProperty("schedule", out var schedule)) continue;
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
            await using var profile = connection.CreateCommand(); profile.CommandText = "INSERT OR IGNORE INTO student_profiles(student_id, display_name, bio, favorite_technique) VALUES ($id, $name, 'Development-only imported demo profile.', $technique)"; profile.Parameters.AddWithValue("$id", studentId); profile.Parameters.AddWithValue("$name", item.TryGetProperty("displayName", out var profileName) ? profileName.GetString() ?? fullName : fullName); profile.Parameters.AddWithValue("$technique", item.TryGetProperty("funStats", out var stats) && stats.TryGetProperty("favoriteTechnique", out var technique) ? technique.GetString() ?? (object)DBNull.Value : DBNull.Value); await profile.ExecuteNonQueryAsync(cancellationToken);
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
        command.CommandText = @"SELECT a.student_id, a.username, a.pin_hash, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), s.birth_date FROM student_accounts a JOIN students s ON s.student_id = a.student_id LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE a.username = $username COLLATE NOCASE AND a.pin_hash IS NOT NULL AND a.active = 1 AND s.active = 1";
        command.Parameters.AddWithValue("$username", username.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var account = new StarterStudentAccount { StudentId = reader.GetInt32(0) };
        var result = _passwordHasher.VerifyHashedPassword(account, reader.GetString(2), pin);
        return result == PasswordVerificationResult.Failed ? null : new StudentAccount(reader.GetInt32(0), reader.GetString(1), reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4), IsBirthdayWeek(reader.IsDBNull(5) ? null : reader.GetString(5)));
    }

    public async Task<StudentAccount?> AuthenticateByStudentIdAsync(int studentId, string pin, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT a.student_id, a.username, a.pin_hash, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), s.birth_date FROM student_accounts a JOIN students s ON s.student_id = a.student_id LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE a.student_id = $studentId AND a.pin_hash IS NOT NULL AND a.active = 1 AND s.active = 1 AND COALESCE(p.profile_visible, 1) = 1";
        command.Parameters.AddWithValue("$studentId", studentId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var account = new StarterStudentAccount { StudentId = reader.GetInt32(0) };
        var result = _passwordHasher.VerifyHashedPassword(account, reader.GetString(2), pin);
        return result == PasswordVerificationResult.Failed ? null : new StudentAccount(reader.GetInt32(0), reader.GetString(1), reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4), IsBirthdayWeek(reader.IsDBNull(5) ? null : reader.GetString(5)));
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
        command.CommandText = @"SELECT s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)),
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
        while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4)));
        return list;
    }

    public async Task<bool> HasDisclaimerAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM student_disclaimer_acceptances WHERE student_id = $id AND disclaimer_version = 'student-profile-v1'"; command.Parameters.AddWithValue("$id", studentId);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) > 0;
    }

    public async Task AcceptDisclaimerAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO student_disclaimer_acceptances(student_id, disclaimer_version, accepted_at) VALUES ($id, 'student-profile-v1', $now)"; command.Parameters.AddWithValue("$id", studentId); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleItem>> GetScheduleAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "SELECT s.session_id, s.session_date, s.start_time, s.end_time, c.class_name, c.description, s.location_name, s.cancelled FROM class_sessions s JOIN classes c ON c.class_id = s.class_id WHERE date(s.session_date) >= date($from) AND date(s.session_date) < date($to) AND c.active = 1 ORDER BY date(s.session_date), COALESCE(s.start_time, '')"; command.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd")); command.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd"));
        var list = new List<ScheduleItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt32(0), DateTime.Parse(reader.GetString(1), CultureInfo.InvariantCulture), reader.IsDBNull(2) ? null : reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.GetInt32(7) != 0)); return list;
    }

    public async Task<IReadOnlyList<PortalAttendanceItem>> GetPortalAttendanceAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT a.attendance_id, a.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), a.check_in_time, a.notes
FROM attendance a JOIN students s ON s.student_id = a.student_id LEFT JOIN student_profiles p ON p.student_id = s.student_id
WHERE a.session_id = $session AND a.status IN ('present', 'helper') ORDER BY s.last_name, s.first_name";
        command.Parameters.AddWithValue("$session", sessionId);
        var list = new List<PortalAttendanceItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.IsDBNull(4) ? null : reader.GetString(4), reader.GetString(5)));
        return list;
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
		command.CommandText = @"SELECT s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), p.bio, p.favorite_technique, p.profile_image_path, (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), s.start_date, (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND a.status IN ('present', 'helper')), (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND a.status IN ('present', 'helper') AND strftime('%Y-%m', a.check_in_time) = strftime('%Y-%m', 'now')), (SELECT COUNT(1) FROM student_messages m WHERE m.recipient_id = s.student_id AND m.read_at IS NULL), (SELECT COUNT(1) FROM student_achievements a WHERE a.student_id = s.student_id), (SELECT h.awarded_date FROM student_rank_history h WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1) FROM students s LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE s.student_id = $id AND s.active = 1"; command.Parameters.AddWithValue("$id", studentId);
        int profileId; string displayName; string? bio; string? favoriteTechnique; string? image; string? rank; string? joinDate; int totalClasses; int classesThisMonth; int unread; int achievements; string? rankAwardedDate;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken)) return null;
            profileId = reader.GetInt32(0); displayName = reader.GetString(1); bio = reader.IsDBNull(2) ? null : reader.GetString(2); favoriteTechnique = reader.IsDBNull(3) ? null : reader.GetString(3); image = reader.IsDBNull(4) ? null : reader.GetString(4); rank = reader.IsDBNull(5) ? null : reader.GetString(5); joinDate = reader.IsDBNull(6) ? null : reader.GetString(6); totalClasses = reader.GetInt32(7); classesThisMonth = reader.GetInt32(8); unread = reader.GetInt32(9); achievements = reader.GetInt32(10); rankAwardedDate = reader.IsDBNull(11) ? null : reader.GetString(11);
        }
		var dates = new List<string>(); await using (var datesCommand = connection.CreateCommand()) { datesCommand.CommandText = "SELECT substr(check_in_time, 1, 10) FROM attendance WHERE student_id = $id AND status IN ('present', 'helper') ORDER BY check_in_time DESC LIMIT 30"; datesCommand.Parameters.AddWithValue("$id", studentId); await using var datesReader = await datesCommand.ExecuteReaderAsync(cancellationToken); while (await datesReader.ReadAsync(cancellationToken)) dates.Add(datesReader.GetString(0)); }
        var classesPerStripe = rank?.Contains("Black", StringComparison.OrdinalIgnoreCase) == true ? 0 : rank?.Contains("Brown", StringComparison.OrdinalIgnoreCase) == true ? 12 : 8;
        var classesSinceRank = totalClasses;
        if (!string.IsNullOrWhiteSpace(rankAwardedDate))
        {
			await using var rankClasses = connection.CreateCommand(); rankClasses.CommandText = "SELECT COUNT(1) FROM attendance WHERE student_id = $id AND status IN ('present', 'helper') AND date(check_in_time) >= date($awarded)"; rankClasses.Parameters.AddWithValue("$id", studentId); rankClasses.Parameters.AddWithValue("$awarded", rankAwardedDate); classesSinceRank = Convert.ToInt32(await rankClasses.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }
        var classesIntoStripe = classesPerStripe == 0 ? 0 : classesSinceRank % classesPerStripe;
        var classesToNextStripe = classesPerStripe == 0 ? 0 : classesPerStripe - classesIntoStripe;
        var nextMilestone = classesPerStripe == 0 ? "Next degree · instructor tracked" : "Next stripe";
        string? ageGroup; DateTime? birthDate; string? email; string? phone; string? uniformSize; string? beltSize;
        await using (var details = connection.CreateCommand()) { details.CommandText = "SELECT age_group, birth_date, email, phone, uniform_size, belt_size FROM students WHERE student_id = $id"; details.Parameters.AddWithValue("$id", studentId); await using var detailsReader = await details.ExecuteReaderAsync(cancellationToken); await detailsReader.ReadAsync(cancellationToken); ageGroup = detailsReader.IsDBNull(0) ? null : detailsReader.GetString(0); birthDate = detailsReader.IsDBNull(1) || !DateTime.TryParse(detailsReader.GetString(1), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedBirthDate) ? null : parsedBirthDate; email = detailsReader.IsDBNull(2) ? null : detailsReader.GetString(2); phone = detailsReader.IsDBNull(3) ? null : detailsReader.GetString(3); uniformSize = detailsReader.IsDBNull(4) ? null : detailsReader.GetString(4); beltSize = detailsReader.IsDBNull(5) ? null : detailsReader.GetString(5); }
        var guardians = new List<GuardianItem>(); await using (var guardianCommand = connection.CreateCommand()) { guardianCommand.CommandText = "SELECT DISTINCT g.first_name, g.last_name, sg.relationship, g.phone, g.email FROM student_guardians sg JOIN guardians g ON g.guardian_id = sg.guardian_id WHERE sg.student_id = $id AND g.active = 1 ORDER BY sg.is_primary DESC, g.last_name, g.first_name"; guardianCommand.Parameters.AddWithValue("$id", studentId); await using var guardianReader = await guardianCommand.ExecuteReaderAsync(cancellationToken); while (await guardianReader.ReadAsync(cancellationToken)) guardians.Add(new($"{guardianReader.GetString(0)} {guardianReader.GetString(1)}", guardianReader.GetString(2), guardianReader.IsDBNull(3) ? null : guardianReader.GetString(3), guardianReader.IsDBNull(4) ? null : guardianReader.GetString(4))); }
        var programs = new List<ProgramMembershipItem>(); await using (var programCommand = connection.CreateCommand()) { programCommand.CommandText = "SELECT program_name, program_code, progression_type, level_name, enrolled_date FROM student_program_memberships WHERE student_id = $id AND active = 1 ORDER BY program_name"; programCommand.Parameters.AddWithValue("$id", studentId); await using var programReader = await programCommand.ExecuteReaderAsync(cancellationToken); while (await programReader.ReadAsync(cancellationToken)) programs.Add(new(programReader.GetString(0), programReader.GetString(1), programReader.GetString(2), programReader.IsDBNull(3) ? null : programReader.GetString(3), programReader.IsDBNull(4) ? null : programReader.GetString(4))); }
        return new(profileId, displayName, bio, favoriteTechnique, image, rank, joinDate, totalClasses, classesThisMonth, unread, achievements, dates, classesIntoStripe, classesPerStripe, classesToNextStripe, nextMilestone, ageGroup, birthDate, email, phone, uniformSize, beltSize, guardians, programs);
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
            student.CommandText = @"UPDATE students SET email = $email, phone = $phone, uniform_size = $uniformSize, belt_size = $beltSize, updated_at = $now WHERE student_id = $id AND active = 1";
            student.Parameters.AddWithValue("$id", studentId);
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

    public Task<CheckInResult> CheckInAsync(int studentId, int sessionId, CancellationToken cancellationToken = default) => CheckInAsync(studentId, sessionId, false, cancellationToken);

    public async Task<CheckInResult> CheckInAsync(int studentId, int sessionId, bool helper, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var session = connection.CreateCommand();
        session.CommandText = @"SELECT c.class_name,
    (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = $student ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1)
FROM class_sessions s JOIN classes c ON c.class_id = s.class_id JOIN students st ON st.student_id = $student
WHERE s.session_id = $session AND date(s.session_date) = date('now', 'localtime') AND s.cancelled = 0 AND st.active = 1 AND COALESCE(NULLIF(st.status, ''), 'active') = 'active'";
        session.Parameters.AddWithValue("$student", studentId);
        session.Parameters.AddWithValue("$session", sessionId);
        await using var sessionReader = await session.ExecuteReaderAsync(cancellationToken);
        if (!await sessionReader.ReadAsync(cancellationToken)) return new(false, false, false, "Only an active class happening today can accept a check-in.");
        var className = sessionReader.GetString(0);
        var currentRank = sessionReader.IsDBNull(1) ? null : sessionReader.GetString(1);
        if (helper)
        {
            if (!TryGetBeltRankOrder(currentRank, out var studentOrder) || !TryGetHighestClassBeltOrder(className, out var requiredOrder, out var requiredRank))
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
        command.Parameters.AddWithValue("$status", helper ? "helper" : "present");
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
            await AwardAutomaticMilestonesAsync(connection, studentId, cancellationToken);
            return new(true, false, helper, null);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return new(false, true, false, "This student is already checked in for this class session.");
        }
    }

    private static bool TryGetHighestClassBeltOrder(string className, out int order, out string rank)
    {
        order = 0; rank = string.Empty;
        foreach (var (name, value) in BeltRankOrders)
        {
            if (className.Contains(name, StringComparison.OrdinalIgnoreCase) && value > order) { order = value; rank = $"{name} Belt"; }
        }
        return order > 0;
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
        await using var command = connection.CreateCommand(); command.CommandText = "SELECT m.message_id, m.sender_id, COALESCE(ps.display_name, TRIM(ss.first_name || ' ' || ss.last_name)), m.recipient_id, COALESCE(pr.display_name, TRIM(sr.first_name || ' ' || sr.last_name)), m.message_text, m.created_at, m.read_at IS NOT NULL FROM student_messages m JOIN students ss ON ss.student_id = m.sender_id JOIN students sr ON sr.student_id = m.recipient_id LEFT JOIN student_profiles ps ON ps.student_id = ss.student_id LEFT JOIN student_profiles pr ON pr.student_id = sr.student_id WHERE ((m.sender_id = $me AND m.recipient_id = $friend) OR (m.sender_id = $friend AND m.recipient_id = $me)) ORDER BY m.created_at LIMIT 200"; command.Parameters.AddWithValue("$me", studentId); command.Parameters.AddWithValue("$friend", friendId); var list = new List<MessageItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3), reader.GetString(4), reader.GetString(5), DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture), reader.GetBoolean(7))); return list;
    }

    public async Task<long> SendMessageAsync(int senderId, int recipientId, string text, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO student_messages(sender_id, recipient_id, message_text, created_at) VALUES ($sender, $recipient, $text, $now)"; command.Parameters.AddWithValue("$sender", senderId); command.Parameters.AddWithValue("$recipient", recipientId); command.Parameters.AddWithValue("$text", text.Trim()); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken); await using var idCommand = connection.CreateCommand(); idCommand.CommandText = "SELECT last_insert_rowid()"; return Convert.ToInt64(await idCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    private static async Task EnsureLegacyStudentColumnsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        foreach (var column in new[] { (Name: "age_group", Definition: "TEXT"), (Name: "uniform_size", Definition: "TEXT"), (Name: "belt_size", Definition: "TEXT"), (Name: "active", Definition: "INTEGER NOT NULL DEFAULT 1"), (Name: "status", Definition: "TEXT NOT NULL DEFAULT 'active'") })
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

    public async Task<IReadOnlyList<FeedItem>> GetFeedAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT p.post_id, COALESCE(p.student_id, 0), COALESCE(sp.display_name, TRIM(s.first_name || ' ' || s.last_name), 'Task Karate'), r.rank_name, p.post_text, p.post_type, p.created_at,
  (SELECT COUNT(1) FROM post_comments c WHERE c.post_id = p.post_id AND c.visible = 1 AND c.moderation_status = 'approved')
FROM posts p
LEFT JOIN students s ON s.student_id = p.student_id
LEFT JOIN student_profiles sp ON sp.student_id = p.student_id
LEFT JOIN student_rank_history h ON h.student_id = p.student_id AND h.student_rank_id = (SELECT MAX(h2.student_rank_id) FROM student_rank_history h2 WHERE h2.student_id = p.student_id)
LEFT JOIN ranks r ON r.rank_id = h.rank_id
WHERE p.visible = 1 AND p.moderation_status = 'approved' AND (p.post_type = 'news' OR (p.post_type = 'student' AND (p.student_id = $id OR EXISTS (SELECT 1 FROM student_friendships f WHERE f.status = 'accepted' AND ((f.student_id = $id AND f.friend_id = p.student_id) OR (f.friend_id = $id AND f.student_id = p.student_id))))))
ORDER BY p.created_at DESC LIMIT 50"; command.Parameters.AddWithValue("$id", studentId);
        var rows = new List<(long Id, int AuthorId, string AuthorName, string? RankName, string Text, string Type, DateTime CreatedAt, int CommentCount)>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken)) while (await reader.ReadAsync(cancellationToken)) rows.Add((reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.GetString(5), DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture), reader.GetInt32(7)));
        var list = new List<FeedItem>();
        foreach (var row in rows) list.Add(new(row.Id, row.AuthorId, row.AuthorName, row.RankName, row.Text, row.Type, row.CreatedAt, await GetReactionSummaryAsync(connection, row.Id, studentId, cancellationToken), row.CommentCount));
        return list;
    }

    public async Task<IReadOnlyList<StaffSocialPost>> GetStaffSocialFeedAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT p.post_id, COALESCE(p.student_id, 0), COALESCE(sp.display_name, TRIM(s.first_name || ' ' || s.last_name), 'Task Karate'), p.post_text, p.post_type, p.moderation_status, p.visible, p.created_at, p.updated_at,
  (SELECT COUNT(1) FROM post_comments c WHERE c.post_id = p.post_id)
FROM posts p
LEFT JOIN students s ON s.student_id = p.student_id
LEFT JOIN student_profiles sp ON sp.student_id = p.student_id
ORDER BY p.created_at DESC LIMIT 200";
        var list = new List<StaffSocialPost>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6) != 0, DateTime.Parse(reader.GetString(7), CultureInfo.InvariantCulture), DateTime.Parse(reader.GetString(8), CultureInfo.InvariantCulture), reader.GetInt32(9)));
        return list;
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
            var code = reader.GetString(0); var label = code switch { "fist_bump" => "Fist bump", "respect" => "Respect", "fire" => "On fire", _ => code };
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

    public async Task<long> CreatePostAsync(int studentId, string text, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO posts(student_id, post_text, post_type, moderation_status, visible, created_at, updated_at) VALUES ($student, $text, 'student', 'approved', 1, $now, $now)"; command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$text", text.Trim()); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken); await using var idCommand = connection.CreateCommand(); idCommand.CommandText = "SELECT last_insert_rowid()"; return Convert.ToInt64(await idCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
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
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT a.achievement_name, a.description, a.icon_name, sa.awarded_at FROM student_achievements sa JOIN achievements a ON a.achievement_id = sa.achievement_id WHERE sa.student_id = $id ORDER BY sa.awarded_at DESC"; command.Parameters.AddWithValue("$id", studentId); var list = new List<AchievementItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture))); return list;
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
		await using (var attendance = connection.CreateCommand()) { attendance.CommandText = "SELECT 'attendance', 'Checked into ' || c.class_name, COALESCE(c.description, 'Training attendance recorded.'), a.check_in_time, '/schedule' FROM attendance a JOIN class_sessions cs ON cs.session_id = a.session_id JOIN classes c ON c.class_id = cs.class_id WHERE a.student_id = $student AND a.status IN ('present', 'helper') ORDER BY a.check_in_time DESC LIMIT 10"; attendance.Parameters.AddWithValue("$student", studentId); await using var reader = await attendance.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.GetString(4))); }
        await using (var achievements = connection.CreateCommand()) { achievements.CommandText = "SELECT 'achievement', a.achievement_name, COALESCE(a.description, 'Milestone awarded by the dojo.'), sa.awarded_at, '/student/achievements' FROM student_achievements sa JOIN achievements a ON a.achievement_id = sa.achievement_id WHERE sa.student_id = $student ORDER BY sa.awarded_at DESC LIMIT 10"; achievements.Parameters.AddWithValue("$student", studentId); await using var reader = await achievements.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.GetString(4))); }
        await using (var stars = connection.CreateCommand()) { stars.CommandText = "SELECT 'gold_star', e.event_name, e.description, g.awarded_at, '/student/achievements/' || e.event_id FROM student_gold_stars g JOIN gold_star_events e ON e.event_id = g.event_id WHERE g.student_id = $student ORDER BY g.awarded_at DESC LIMIT 10"; stars.Parameters.AddWithValue("$student", studentId); await using var reader = await stars.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.GetString(4))); }
        await using (var posts = connection.CreateCommand()) { posts.CommandText = "SELECT 'post', 'Shared a dojo update', p.post_text, p.created_at, '/student/social' FROM posts p WHERE p.student_id = $student AND p.visible = 1 ORDER BY p.created_at DESC LIMIT 10"; posts.Parameters.AddWithValue("$student", studentId); await using var reader = await posts.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), reader.GetString(4))); }
        return list.OrderByDescending(item => item.OccurredAt).Take(20).ToList();
    }

    public async Task<IReadOnlyList<GoldStarEventItem>> GetGoldStarEventsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT e.event_id, e.event_name, e.description, e.event_date, 1, 0 FROM gold_star_events e JOIN student_gold_stars g ON g.event_id = e.event_id AND g.student_id = $student WHERE e.active = 1 ORDER BY g.awarded_at DESC, e.event_name"; command.Parameters.AddWithValue("$student", studentId); var list = new List<GoldStarEventItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture), true, false)); return list;
    }

    public async Task<GoldStarEventItem?> GetGoldStarEventAsync(int studentId, long eventId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = "SELECT e.event_id, e.event_name, e.description, e.event_date, 1, 0 FROM gold_star_events e JOIN student_gold_stars g ON g.event_id = e.event_id AND g.student_id = $student WHERE e.event_id = $event AND e.active = 1";
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

    private static NewsItem ToNewsItem(long id, string text, string createdAt)
    {
        var separator = text.IndexOf(" — ", StringComparison.Ordinal);
        return separator > 0
            ? new(id, text[..separator], text[(separator + 3)..], DateTime.Parse(createdAt, CultureInfo.InvariantCulture))
            : new(id, "Dojo news", text, DateTime.Parse(createdAt, CultureInfo.InvariantCulture));
    }

    public async Task<int> CreatePortalStudentAsync(PortalStudentWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO students(first_name, last_name, preferred_name, age_group, birth_date, start_date, uniform_size, belt_size, active, status) VALUES ($first, $last, $preferred, $age, $birth, $join, $uniform, $belt, 1, 'active'); SELECT last_insert_rowid();"; command.Parameters.AddWithValue("$first", request.FirstName.Trim()); command.Parameters.AddWithValue("$last", request.LastName.Trim()); command.Parameters.AddWithValue("$preferred", (object?)NullIfBlank(request.PreferredName) ?? DBNull.Value); command.Parameters.AddWithValue("$age", request.AgeGroup.Trim()); command.Parameters.AddWithValue("$birth", request.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? (object)DBNull.Value); command.Parameters.AddWithValue("$join", (request.JoinDate ?? DateTime.UtcNow.Date).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); command.Parameters.AddWithValue("$uniform", (object?)NullIfBlank(request.UniformSize) ?? DBNull.Value); command.Parameters.AddWithValue("$belt", (object?)NullIfBlank(request.BeltSize) ?? DBNull.Value); var id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture); await using var profile = connection.CreateCommand(); profile.CommandText = "INSERT INTO student_profiles(student_id, display_name, bio, favorite_technique, profile_visible) VALUES ($id, $display, $bio, $technique, 1)"; profile.Parameters.AddWithValue("$id", id); profile.Parameters.AddWithValue("$display", (object?)NullIfBlank(request.PreferredName) ?? $"{request.FirstName.Trim()} {request.LastName.Trim()}"); profile.Parameters.AddWithValue("$bio", (object?)NullIfBlank(request.Bio) ?? DBNull.Value); profile.Parameters.AddWithValue("$technique", (object?)NullIfBlank(request.FavoriteTechnique) ?? DBNull.Value); await profile.ExecuteNonQueryAsync(cancellationToken); return id;
    }

    public async Task<bool> UpdatePortalStudentAsync(int studentId, PortalStudentWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "UPDATE students SET first_name = $first, last_name = $last, preferred_name = $preferred, age_group = $age, birth_date = $birth, start_date = $join, uniform_size = $uniform, belt_size = $belt, updated_at = $now WHERE student_id = $id"; command.Parameters.AddWithValue("$first", request.FirstName.Trim()); command.Parameters.AddWithValue("$last", request.LastName.Trim()); command.Parameters.AddWithValue("$preferred", (object?)NullIfBlank(request.PreferredName) ?? DBNull.Value); command.Parameters.AddWithValue("$age", request.AgeGroup.Trim()); command.Parameters.AddWithValue("$birth", request.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? (object)DBNull.Value); command.Parameters.AddWithValue("$join", (request.JoinDate ?? DateTime.UtcNow.Date).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); command.Parameters.AddWithValue("$uniform", (object?)NullIfBlank(request.UniformSize) ?? DBNull.Value); command.Parameters.AddWithValue("$belt", (object?)NullIfBlank(request.BeltSize) ?? DBNull.Value); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); command.Parameters.AddWithValue("$id", studentId); if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) return false; await using var profile = connection.CreateCommand(); profile.CommandText = "INSERT INTO student_profiles(student_id, display_name, bio, favorite_technique, profile_visible) VALUES ($id, $display, $bio, $technique, 1) ON CONFLICT(student_id) DO UPDATE SET display_name = excluded.display_name, bio = excluded.bio, favorite_technique = excluded.favorite_technique, updated_at = excluded.updated_at"; profile.Parameters.AddWithValue("$id", studentId); profile.Parameters.AddWithValue("$display", (object?)NullIfBlank(request.PreferredName) ?? $"{request.FirstName.Trim()} {request.LastName.Trim()}"); profile.Parameters.AddWithValue("$bio", (object?)NullIfBlank(request.Bio) ?? DBNull.Value); profile.Parameters.AddWithValue("$technique", (object?)NullIfBlank(request.FavoriteTechnique) ?? DBNull.Value); profile.Parameters.AddWithValue("$updated_at", DateTime.UtcNow.ToString("O")); await profile.ExecuteNonQueryAsync(cancellationToken); return true;
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
        command.CommandText = "SELECT s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), s.age_group, s.birth_date, s.active, s.start_date, s.uniform_size, s.belt_size, p.bio, p.favorite_technique, (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND a.status IN ('present', 'helper')), (SELECT COUNT(1) FROM student_achievements a WHERE a.student_id = s.student_id), (SELECT COUNT(1) FROM student_gold_stars g WHERE g.student_id = s.student_id), COALESCE((SELECT group_concat(m.program_name, ', ') FROM student_program_memberships m WHERE m.student_id = s.student_id AND m.active = 1), ''), COALESCE(NULLIF(s.status, ''), CASE WHEN s.active = 1 THEN 'active' ELSE 'deactivated' END) FROM students s LEFT JOIN student_profiles p ON p.student_id = s.student_id" + (activeOnly ? " WHERE COALESCE(NULLIF(s.status, ''), CASE WHEN s.active = 1 THEN 'active' ELSE 'deactivated' END) = 'active'" : string.Empty) + " ORDER BY 2";
        var list = new List<PortalAdminStudent>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var studentId = reader.GetInt32(0);
            IReadOnlyList<string> programs = reader.IsDBNull(13) ? Array.Empty<string>() : reader.GetString(13).Split(", ", StringSplitOptions.RemoveEmptyEntries);
            list.Add(new(
                studentId,
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) || !DateTime.TryParse(reader.GetString(4), CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthDate) ? null : birthDate,
                reader.GetInt32(5) != 0,
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetString(9),
                reader.IsDBNull(10) ? null : reader.GetString(10),
                reader.GetInt32(11),
                await GetAttendanceStreakAsync(connection, studentId, cancellationToken),
                reader.GetInt32(12),
                reader.GetInt32(13),
                programs,
                reader.IsDBNull(15) ? "active" : reader.GetString(15)));
        }
        return list;
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
        command.CommandText = "SELECT g.guardian_id, g.first_name, g.last_name, g.email, g.phone, g.active, s.student_id, TRIM(s.first_name || ' ' || s.last_name), sg.relationship FROM guardians g LEFT JOIN student_guardians sg ON sg.guardian_id = g.guardian_id LEFT JOIN students s ON s.student_id = sg.student_id WHERE g.active = 1 ORDER BY g.last_name, g.first_name, s.last_name, s.first_name";
        var guardians = new Dictionary<int, (string FirstName, string LastName, string? Email, string? Phone, bool IsActive, List<PortalAdminGuardianStudent> Students)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var guardianId = reader.GetInt32(0);
            if (!guardians.TryGetValue(guardianId, out var guardian))
            {
                guardian = (reader.GetString(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4), reader.GetInt32(5) != 0, new List<PortalAdminGuardianStudent>());
                guardians.Add(guardianId, guardian);
            }

            if (!reader.IsDBNull(6) && !guardian.Students.Any(student => student.StudentId == reader.GetInt32(6)))
                guardian.Students.Add(new(reader.GetInt32(6), reader.GetString(7), reader.IsDBNull(8) ? "Guardian" : reader.GetString(8)));
        }

        return guardians.Select(pair => new PortalAdminGuardian(pair.Key, pair.Value.FirstName, pair.Value.LastName, pair.Value.Email, pair.Value.Phone, pair.Value.IsActive, pair.Value.Students)).ToList();
    }

    public async Task<IReadOnlyList<PortalAdminGuardianStudent>> GetPortalAdminGuardianStudentsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT student_id, TRIM(first_name || ' ' || last_name) FROM students WHERE active = 1 ORDER BY last_name, first_name";
        var students = new List<PortalAdminGuardianStudent>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) students.Add(new(reader.GetInt32(0), reader.GetString(1), "Guardian"));
        return students;
    }

    public async Task<int> CreatePortalGuardianAsync(PortalGuardianWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO guardians(first_name, last_name, email, phone, active, updated_at) VALUES ($first, $last, $email, $phone, 1, $now); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$first", request.FirstName.Trim()); command.Parameters.AddWithValue("$last", request.LastName.Trim()); command.Parameters.AddWithValue("$email", (object?)NullIfBlank(request.Email) ?? DBNull.Value); command.Parameters.AddWithValue("$phone", (object?)NullIfBlank(request.Phone) ?? DBNull.Value); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        var guardianId = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        await ReplaceGuardianLinksAsync(connection, guardianId, request.StudentIds, cancellationToken);
        return guardianId;
    }

    public async Task<bool> UpdatePortalGuardianAsync(int guardianId, PortalGuardianWriteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE guardians SET first_name = $first, last_name = $last, email = $email, phone = $phone, updated_at = $now WHERE guardian_id = $id";
        command.Parameters.AddWithValue("$first", request.FirstName.Trim()); command.Parameters.AddWithValue("$last", request.LastName.Trim()); command.Parameters.AddWithValue("$email", (object?)NullIfBlank(request.Email) ?? DBNull.Value); command.Parameters.AddWithValue("$phone", (object?)NullIfBlank(request.Phone) ?? DBNull.Value); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); command.Parameters.AddWithValue("$id", guardianId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) return false;
        await ReplaceGuardianLinksAsync(connection, guardianId, request.StudentIds, cancellationToken);
        return true;
    }

    private static async Task ReplaceGuardianLinksAsync(SqliteConnection connection, int guardianId, IReadOnlyList<int>? studentIds, CancellationToken cancellationToken)
    {
        await using var delete = connection.CreateCommand(); delete.CommandText = "DELETE FROM student_guardians WHERE guardian_id = $guardian"; delete.Parameters.AddWithValue("$guardian", guardianId); await delete.ExecuteNonQueryAsync(cancellationToken);
        foreach (var studentId in (studentIds ?? Array.Empty<int>()).Distinct())
        {
            await using var link = connection.CreateCommand(); link.CommandText = "INSERT OR IGNORE INTO student_guardians(student_id, guardian_id, relationship, is_primary) SELECT $student, $guardian, 'Guardian', CASE WHEN NOT EXISTS (SELECT 1 FROM student_guardians WHERE student_id = $student) THEN 1 ELSE 0 END FROM students WHERE student_id = $student AND active = 1"; link.Parameters.AddWithValue("$student", studentId); link.Parameters.AddWithValue("$guardian", guardianId); await link.ExecuteNonQueryAsync(cancellationToken);
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

    public async Task<bool> AwardGoldStarAsync(int studentId, long eventId, string? note, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var check = connection.CreateCommand(); check.CommandText = "SELECT COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), e.event_name FROM students s LEFT JOIN student_profiles p ON p.student_id = s.student_id CROSS JOIN gold_star_events e WHERE s.student_id = $student AND s.active = 1 AND e.event_id = $event"; check.Parameters.AddWithValue("$student", studentId); check.Parameters.AddWithValue("$event", eventId);
        await using var reader = await check.ExecuteReaderAsync(cancellationToken); if (!await reader.ReadAsync(cancellationToken)) return false; var studentName = reader.GetString(0); var eventName = reader.GetString(1);
        await reader.DisposeAsync();
        await using var award = connection.CreateCommand(); award.CommandText = "INSERT OR IGNORE INTO student_gold_stars(student_id, event_id, note, awarded_at) VALUES ($student, $event, $note, $now)"; award.Parameters.AddWithValue("$student", studentId); award.Parameters.AddWithValue("$event", eventId); award.Parameters.AddWithValue("$note", (object?)note?.Trim() ?? DBNull.Value); award.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); var changed = await award.ExecuteNonQueryAsync(cancellationToken) > 0;
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
		await using var countCommand = connection.CreateCommand(); countCommand.CommandText = "SELECT COUNT(1) FROM attendance WHERE student_id = $student AND status IN ('present', 'helper')"; countCommand.Parameters.AddWithValue("$student", studentId); var totalClasses = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
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
        var milestones = new List<(string Name, bool Qualifies)> { ("First Class!", totalClasses >= 1), ("3 Classes", totalClasses >= 3), ("5 Classes", totalClasses >= 5), ("10 Classes Strong", totalClasses >= 10), ("25 Classes", totalClasses >= 25), ("50 Classes", totalClasses >= 50), ("100 Classes", totalClasses >= 100), ("250 Classes", totalClasses >= 250), ("500 Classes", totalClasses >= 500), ("1,000 Classes", totalClasses >= 1000), ("2,500 Classes", totalClasses >= 2500), ("5,000 Classes", totalClasses >= 5000), ("10,000 Classes", totalClasses >= 10000), ("7-Day Rhythm", streak >= 7), ("14-Day Rhythm", streak >= 14), ("30-Day Rhythm", streak >= 30), ("60-Day Rhythm", streak >= 60), ("90-Day Rhythm", streak >= 90), ("180-Day Rhythm", streak >= 180), ("1-Year Dojo Anniversary", yearsAtDojo >= 1), ("3-Year Dojo Anniversary", yearsAtDojo >= 3), ("5-Year Dojo Anniversary", yearsAtDojo >= 5), ("10-Year Dojo Anniversary", yearsAtDojo >= 10) };
        var awarded = 0;
        foreach (var milestone in milestones.Where(x => x.Qualifies))
        {
            await using var command = connection.CreateCommand(); command.CommandText = "INSERT OR IGNORE INTO student_achievements(student_id, achievement_id, awarded_at) SELECT $student, achievement_id, $now FROM achievements WHERE achievement_name = $name"; command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$name", milestone.Name); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); if (await command.ExecuteNonQueryAsync(cancellationToken) > 0) { awarded++; await AddNewsPostAsync(connection, $"Milestone unlocked — {milestone.Name}", $"A training milestone was automatically awarded after attendance criteria were met.", cancellationToken); }
        }
        return awarded;
    }

    private static async Task<int> GetAttendanceStreakAsync(SqliteConnection connection, int studentId, CancellationToken cancellationToken)
    {
		await using var command = connection.CreateCommand(); command.CommandText = "SELECT DISTINCT date(check_in_time) FROM attendance WHERE student_id = $student AND status IN ('present', 'helper') ORDER BY date(check_in_time) DESC"; command.Parameters.AddWithValue("$student", studentId); var dates = new List<DateOnly>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) if (DateOnly.TryParse(reader.GetString(0), out var date)) dates.Add(date); if (dates.Count == 0) return 0; var streak = 1; for (var index = 1; index < dates.Count; index++) { if (dates[index - 1].DayNumber - dates[index].DayNumber != 1) break; streak++; } return streak;
    }

    private static async Task AddNewsPostAsync(SqliteConnection connection, string title, string body, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO posts(student_id, post_text, post_type, moderation_status, visible, created_at, updated_at) SELECT (SELECT student_id FROM students ORDER BY student_id LIMIT 1), $text, 'news', 'approved', 1, $now, $now WHERE EXISTS (SELECT 1 FROM students)"; command.Parameters.AddWithValue("$text", $"{title} — {body}"); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
