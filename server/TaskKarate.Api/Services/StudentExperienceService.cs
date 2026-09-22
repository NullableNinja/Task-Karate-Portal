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

public sealed record StudentAccount(int StudentId, string Username, string DisplayName, string? RankName);
public sealed record StudentSummary(int StudentId, string DisplayName, string? RankName, string? ProfileImagePath);
public sealed record ScheduleItem(int SessionId, DateTime SessionDate, string? StartTime, string? EndTime, string ClassName, string? Description, string? Location, bool Cancelled);
public sealed record StudentProfile(int StudentId, string DisplayName, string? Bio, string? FavoriteTechnique, string? ProfileImagePath, string? RankName, string? JoinDate, int TotalClasses, int ClassesThisMonth, int UnreadMessages, int AchievementCount, IReadOnlyList<string> AttendanceDates, int ClassesIntoStripe, int ClassesPerStripe, int ClassesToNextStripe, string NextMilestone);
public sealed record FriendshipItem(int StudentId, string DisplayName, string? RankName, string Status, bool Incoming);
public sealed record MessageItem(long MessageId, int SenderId, string SenderName, int RecipientId, string RecipientName, string MessageText, DateTime CreatedAt, bool IsRead);
public sealed record AchievementItem(string Name, string? Description, string? IconName, DateTime? AwardedAt);
public sealed record NewsItem(long Id, string Title, string Body, DateTime PublishedAt);
public sealed record FeedItem(long PostId, int AuthorId, string AuthorName, string? RankName, string Text, string PostType, DateTime CreatedAt);

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
CREATE TABLE IF NOT EXISTS student_profiles (student_id INTEGER PRIMARY KEY, display_name TEXT, bio TEXT, profile_image_path TEXT, favorite_technique TEXT, profile_visible INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_rank_history (student_rank_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, rank_id INTEGER NOT NULL, awarded_date TEXT NOT NULL, awarded_by_user_id INTEGER, notes TEXT, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(rank_id) REFERENCES ranks(rank_id));
CREATE TABLE IF NOT EXISTS classes (class_id INTEGER PRIMARY KEY AUTOINCREMENT, class_name TEXT NOT NULL, description TEXT, active INTEGER NOT NULL DEFAULT 1);
CREATE TABLE IF NOT EXISTS class_schedule (schedule_id INTEGER PRIMARY KEY AUTOINCREMENT, class_id INTEGER NOT NULL, day_of_week INTEGER NOT NULL, start_time TEXT NOT NULL, end_time TEXT, instructor_id INTEGER, location_name TEXT, active INTEGER NOT NULL DEFAULT 1, FOREIGN KEY(class_id) REFERENCES classes(class_id));
CREATE TABLE IF NOT EXISTS class_sessions (session_id INTEGER PRIMARY KEY AUTOINCREMENT, class_id INTEGER NOT NULL, schedule_id INTEGER, session_date TEXT NOT NULL, start_time TEXT, end_time TEXT, instructor_id INTEGER, location_name TEXT, cancelled INTEGER NOT NULL DEFAULT 0, cancellation_reason TEXT, notes TEXT, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(class_id) REFERENCES classes(class_id), UNIQUE(class_id, session_date, start_time));
CREATE TABLE IF NOT EXISTS attendance (attendance_id INTEGER PRIMARY KEY AUTOINCREMENT, session_id INTEGER NOT NULL, student_id INTEGER NOT NULL, check_in_time TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, check_out_time TEXT, status TEXT NOT NULL DEFAULT 'present', checked_in_by_user_id INTEGER, notes TEXT, FOREIGN KEY(session_id) REFERENCES class_sessions(session_id) ON DELETE CASCADE, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, UNIQUE(session_id, student_id));
CREATE TABLE IF NOT EXISTS achievements (achievement_id INTEGER PRIMARY KEY AUTOINCREMENT, achievement_name TEXT NOT NULL UNIQUE, description TEXT, icon_name TEXT, active INTEGER NOT NULL DEFAULT 1);
CREATE TABLE IF NOT EXISTS student_achievements (student_id INTEGER NOT NULL, achievement_id INTEGER NOT NULL, awarded_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY(student_id, achievement_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(achievement_id) REFERENCES achievements(achievement_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS posts (post_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER, user_id INTEGER, post_text TEXT NOT NULL, post_type TEXT NOT NULL DEFAULT 'student', moderation_status TEXT NOT NULL DEFAULT 'approved', visible INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE TABLE IF NOT EXISTS student_accounts (student_id INTEGER PRIMARY KEY, username TEXT NOT NULL UNIQUE COLLATE NOCASE, password_hash TEXT NOT NULL, active INTEGER NOT NULL DEFAULT 1, created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_disclaimer_acceptances (acceptance_id INTEGER PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, disclaimer_version TEXT NOT NULL, accepted_at TEXT NOT NULL, FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_friendships (student_id INTEGER NOT NULL, friend_id INTEGER NOT NULL, status TEXT NOT NULL, requested_by INTEGER NOT NULL, created_at TEXT NOT NULL, updated_at TEXT NOT NULL, PRIMARY KEY(student_id, friend_id), CHECK(student_id <> friend_id), FOREIGN KEY(student_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(friend_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS student_messages (message_id INTEGER PRIMARY KEY AUTOINCREMENT, sender_id INTEGER NOT NULL, recipient_id INTEGER NOT NULL, message_text TEXT NOT NULL, created_at TEXT NOT NULL, read_at TEXT, FOREIGN KEY(sender_id) REFERENCES students(student_id) ON DELETE CASCADE, FOREIGN KEY(recipient_id) REFERENCES students(student_id) ON DELETE CASCADE);
CREATE INDEX IF NOT EXISTS ix_student_accounts_username ON student_accounts(username);
CREATE INDEX IF NOT EXISTS ix_class_sessions_date ON class_sessions(session_date, cancelled);
CREATE INDEX IF NOT EXISTS ix_attendance_student ON attendance(student_id, session_id);
CREATE INDEX IF NOT EXISTS ix_messages_thread ON student_messages(sender_id, recipient_id, created_at);
CREATE INDEX IF NOT EXISTS ix_posts_news ON posts(post_type, visible, moderation_status, created_at);
";
        await command.ExecuteNonQueryAsync(cancellationToken);
        await ImportDevelopmentDataAsync(connection, cancellationToken);
        await MaterializeUpcomingSessionsAsync(connection, cancellationToken);
        await SeedDemoDataAsync(connection, cancellationToken);
        await EnsureBootstrapAccountAsync(connection, cancellationToken);
        Interlocked.Exchange(ref _ready, 1);
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
            await using var profile = connection.CreateCommand(); profile.CommandText = "INSERT OR IGNORE INTO student_profiles(student_id, display_name, bio, favorite_technique) VALUES ($id, $name, 'Development-only imported demo profile.', $technique)"; profile.Parameters.AddWithValue("$id", studentId); profile.Parameters.AddWithValue("$name", item.TryGetProperty("displayName", out var profileName) ? profileName.GetString() ?? fullName : fullName); profile.Parameters.AddWithValue("$technique", item.TryGetProperty("funStats", out var stats) && stats.TryGetProperty("favoriteTechnique", out var technique) ? technique.GetString() ?? (object)DBNull.Value : DBNull.Value); await profile.ExecuteNonQueryAsync(cancellationToken);
            if (item.TryGetProperty("rank", out var rankProperty)) await AddRankHistoryAsync(connection, studentId, rankProperty.GetString(), cancellationToken);
        }
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
        var password = Environment.GetEnvironmentVariable("TASK_KARATE_STUDENT_PASSWORD");
        var idText = Environment.GetEnvironmentVariable("TASK_KARATE_STUDENT_ID");
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || !int.TryParse(idText, out var studentId)) return;
        await using var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(1) FROM students WHERE student_id = $id AND active = 1";
        check.Parameters.AddWithValue("$id", studentId);
        if (Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) != 1) return;
        await using var exists = connection.CreateCommand();
        exists.CommandText = "SELECT COUNT(1) FROM student_accounts WHERE student_id = $id";
        exists.Parameters.AddWithValue("$id", studentId);
        if (Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 1) return;
        var hash = _passwordHasher.HashPassword(new StarterStudentAccount { StudentId = studentId }, password);
        await using var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO student_accounts(student_id, username, password_hash) VALUES ($id, $username, $hash)";
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

    public async Task<StudentAccount?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT a.student_id, a.username, a.password_hash, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1) FROM student_accounts a JOIN students s ON s.student_id = a.student_id LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE a.username = $username COLLATE NOCASE AND a.active = 1 AND s.active = 1";
        command.Parameters.AddWithValue("$username", username.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var account = new StarterStudentAccount { StudentId = reader.GetInt32(0) };
        var result = _passwordHasher.VerifyHashedPassword(account, reader.GetString(2), password);
        return result == PasswordVerificationResult.Failed ? null : new StudentAccount(reader.GetInt32(0), reader.GetString(1), reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4));
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

    public async Task<StudentProfile?> GetProfileAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT s.student_id, COALESCE(p.display_name, TRIM(s.first_name || ' ' || s.last_name)), p.bio, p.favorite_technique, p.profile_image_path, (SELECT r.rank_name FROM student_rank_history h JOIN ranks r ON r.rank_id = h.rank_id WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1), s.start_date, (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND a.status = 'present'), (SELECT COUNT(1) FROM attendance a WHERE a.student_id = s.student_id AND strftime('%Y-%m', a.check_in_time) = strftime('%Y-%m', 'now')), (SELECT COUNT(1) FROM student_messages m WHERE m.recipient_id = s.student_id AND m.read_at IS NULL), (SELECT COUNT(1) FROM student_achievements a WHERE a.student_id = s.student_id), (SELECT h.awarded_date FROM student_rank_history h WHERE h.student_id = s.student_id ORDER BY h.awarded_date DESC, h.student_rank_id DESC LIMIT 1) FROM students s LEFT JOIN student_profiles p ON p.student_id = s.student_id WHERE s.student_id = $id AND s.active = 1"; command.Parameters.AddWithValue("$id", studentId);
        int profileId; string displayName; string? bio; string? favoriteTechnique; string? image; string? rank; string? joinDate; int totalClasses; int classesThisMonth; int unread; int achievements; string? rankAwardedDate;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken)) return null;
            profileId = reader.GetInt32(0); displayName = reader.GetString(1); bio = reader.IsDBNull(2) ? null : reader.GetString(2); favoriteTechnique = reader.IsDBNull(3) ? null : reader.GetString(3); image = reader.IsDBNull(4) ? null : reader.GetString(4); rank = reader.IsDBNull(5) ? null : reader.GetString(5); joinDate = reader.IsDBNull(6) ? null : reader.GetString(6); totalClasses = reader.GetInt32(7); classesThisMonth = reader.GetInt32(8); unread = reader.GetInt32(9); achievements = reader.GetInt32(10); rankAwardedDate = reader.IsDBNull(11) ? null : reader.GetString(11);
        }
        var dates = new List<string>(); await using (var datesCommand = connection.CreateCommand()) { datesCommand.CommandText = "SELECT substr(check_in_time, 1, 10) FROM attendance WHERE student_id = $id AND status = 'present' ORDER BY check_in_time DESC LIMIT 30"; datesCommand.Parameters.AddWithValue("$id", studentId); await using var datesReader = await datesCommand.ExecuteReaderAsync(cancellationToken); while (await datesReader.ReadAsync(cancellationToken)) dates.Add(datesReader.GetString(0)); }
        var classesPerStripe = rank?.Contains("Black", StringComparison.OrdinalIgnoreCase) == true ? 0 : rank?.Contains("Brown", StringComparison.OrdinalIgnoreCase) == true ? 12 : 8;
        var classesSinceRank = totalClasses;
        if (!string.IsNullOrWhiteSpace(rankAwardedDate))
        {
            await using var rankClasses = connection.CreateCommand(); rankClasses.CommandText = "SELECT COUNT(1) FROM attendance WHERE student_id = $id AND status = 'present' AND date(check_in_time) >= date($awarded)"; rankClasses.Parameters.AddWithValue("$id", studentId); rankClasses.Parameters.AddWithValue("$awarded", rankAwardedDate); classesSinceRank = Convert.ToInt32(await rankClasses.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }
        var classesIntoStripe = classesPerStripe == 0 ? 0 : classesSinceRank % classesPerStripe;
        var classesToNextStripe = classesPerStripe == 0 ? 0 : classesPerStripe - classesIntoStripe;
        var nextMilestone = classesPerStripe == 0 ? "Next degree · instructor tracked" : "Next stripe";
        return new(profileId, displayName, bio, favoriteTechnique, image, rank, joinDate, totalClasses, classesThisMonth, unread, achievements, dates, classesIntoStripe, classesPerStripe, classesToNextStripe, nextMilestone);
    }

    public async Task<(bool Success, bool Duplicate)> CheckInAsync(int studentId, int sessionId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var exists = connection.CreateCommand(); exists.CommandText = "SELECT COUNT(1) FROM class_sessions s JOIN students st ON st.student_id = $student WHERE s.session_id = $session AND s.cancelled = 0 AND st.active = 1"; exists.Parameters.AddWithValue("$student", studentId); exists.Parameters.AddWithValue("$session", sessionId); if (Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) == 0) return (false, false);
        await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO attendance(session_id, student_id, check_in_time, status) VALUES ($session, $student, $now, 'present')"; command.Parameters.AddWithValue("$session", sessionId); command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); try { await command.ExecuteNonQueryAsync(cancellationToken); return (true, false); } catch (SqliteException ex) when (ex.SqliteErrorCode == 19) { return (false, true); }
    }

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
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT m.message_id, m.sender_id, COALESCE(ps.display_name, TRIM(ss.first_name || ' ' || ss.last_name)), m.recipient_id, COALESCE(pr.display_name, TRIM(sr.first_name || ' ' || sr.last_name)), m.message_text, m.created_at, m.read_at IS NOT NULL FROM student_messages m JOIN students ss ON ss.student_id = m.sender_id JOIN students sr ON sr.student_id = m.recipient_id LEFT JOIN student_profiles ps ON ps.student_id = ss.student_id LEFT JOIN student_profiles pr ON pr.student_id = sr.student_id WHERE ((m.sender_id = $me AND m.recipient_id = $friend) OR (m.sender_id = $friend AND m.recipient_id = $me)) ORDER BY m.created_at LIMIT 200"; command.Parameters.AddWithValue("$me", studentId); command.Parameters.AddWithValue("$friend", friendId); var list = new List<MessageItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3), reader.GetString(4), reader.GetString(5), DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture), reader.GetBoolean(7))); return list;
    }

    public async Task<long> SendMessageAsync(int senderId, int recipientId, string text, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO student_messages(sender_id, recipient_id, message_text, created_at) VALUES ($sender, $recipient, $text, $now)"; command.Parameters.AddWithValue("$sender", senderId); command.Parameters.AddWithValue("$recipient", recipientId); command.Parameters.AddWithValue("$text", text.Trim()); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken); await using var idCommand = connection.CreateCommand(); idCommand.CommandText = "SELECT last_insert_rowid()"; return Convert.ToInt64(await idCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public async Task<IReadOnlyList<FeedItem>> GetFeedAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT p.post_id, COALESCE(p.student_id, 0), COALESCE(sp.display_name, TRIM(s.first_name || ' ' || s.last_name), 'Task Karate'), r.rank_name, p.post_text, p.post_type, p.created_at
FROM posts p
LEFT JOIN students s ON s.student_id = p.student_id
LEFT JOIN student_profiles sp ON sp.student_id = p.student_id
LEFT JOIN student_rank_history h ON h.student_id = p.student_id AND h.student_rank_id = (SELECT MAX(h2.student_rank_id) FROM student_rank_history h2 WHERE h2.student_id = p.student_id)
LEFT JOIN ranks r ON r.rank_id = h.rank_id
WHERE p.visible = 1 AND p.moderation_status = 'approved' AND (p.post_type = 'news' OR p.student_id = $id OR EXISTS (SELECT 1 FROM student_friendships f WHERE f.status = 'accepted' AND ((f.student_id = $id AND f.friend_id = p.student_id) OR (f.friend_id = $id AND f.student_id = p.student_id))))
ORDER BY p.created_at DESC LIMIT 50"; command.Parameters.AddWithValue("$id", studentId);
        var list = new List<FeedItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetString(4), reader.GetString(5), DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture))); return list;
    }

    public async Task<long> CreatePostAsync(int studentId, string text, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO posts(student_id, post_text, post_type, moderation_status, visible, created_at, updated_at) VALUES ($student, $text, 'student', 'approved', 1, $now, $now)"; command.Parameters.AddWithValue("$student", studentId); command.Parameters.AddWithValue("$text", text.Trim()); command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await command.ExecuteNonQueryAsync(cancellationToken); await using var idCommand = connection.CreateCommand(); idCommand.CommandText = "SELECT last_insert_rowid()"; return Convert.ToInt64(await idCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    public async Task<IReadOnlyList<AchievementItem>> GetAchievementsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT a.achievement_name, a.description, a.icon_name, sa.awarded_at FROM student_achievements sa JOIN achievements a ON a.achievement_id = sa.achievement_id WHERE sa.student_id = $id ORDER BY sa.awarded_at DESC"; command.Parameters.AddWithValue("$id", studentId); var list = new List<AchievementItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture))); return list;
    }

    public async Task<IReadOnlyList<NewsItem>> GetNewsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken); await using var command = connection.CreateCommand(); command.CommandText = "SELECT post_id, CASE WHEN post_type = 'news' THEN 'Dojo news' ELSE 'Community update' END, post_text, created_at FROM posts WHERE visible = 1 AND moderation_status = 'approved' AND post_type = 'news' ORDER BY created_at DESC LIMIT 50"; var list = new List<NewsItem>(); await using var reader = await command.ExecuteReaderAsync(cancellationToken); while (await reader.ReadAsync(cancellationToken)) list.Add(new(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture))); return list;
    }
}
