using Microsoft.AspNetCore.Identity;

namespace TaskKarate.Api.Models;

public sealed class AppUser : IdentityUser<Guid>
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

public abstract class TrackedEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class Student : TrackedEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? PreferredName { get; set; }
    public DateTime? BirthDate { get; set; }
    public required string AgeGroup { get; set; }
    public DateTime JoinDate { get; set; }
    public string? UniformSize { get; set; }
    public string? BeltSize { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? BeltRankId { get; set; }
    public BeltRank? BeltRank { get; set; }
    public ICollection<GuardianStudent> Guardians { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
}

public sealed class Guardian : TrackedEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<GuardianStudent> Students { get; set; } = [];
}

public sealed class GuardianStudent
{
    public Guid GuardianId { get; set; }
    public Guardian Guardian { get; set; } = null!;
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public string Relationship { get; set; } = "Guardian";
    public bool IsPrimary { get; set; }
    public DateTime LinkedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class ProgramArea : TrackedEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ClassTemplate> ClassTemplates { get; set; } = [];
}

public sealed class ClassTemplate : TrackedEntity
{
    public required string Name { get; set; }
    public required string DayOfWeek { get; set; }
    public required string StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? BeltScope { get; set; }
    public required Guid ProgramAreaId { get; set; }
    public ProgramArea ProgramArea { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public ICollection<ClassSession> Sessions { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}

public sealed class ClassSession : TrackedEntity
{
    public required Guid ClassTemplateId { get; set; }
    public ClassTemplate ClassTemplate { get; set; } = null!;
    public DateTime SessionDateUtc { get; set; }
    public string? Notes { get; set; }
    public bool IsCancelled { get; set; }
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
}

public sealed class Enrollment : TrackedEntity
{
    public required Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public required Guid ClassTemplateId { get; set; }
    public ClassTemplate ClassTemplate { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}

public sealed class AttendanceRecord : TrackedEntity
{
    public required Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public required Guid ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;
    public DateTime CheckedInAtUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

public sealed class BeltRank : TrackedEntity
{
    public required string Name { get; set; }
    public int SortOrder { get; set; }
    public ICollection<Student> Students { get; set; } = [];
    public ICollection<StudentRankHistory> StudentRankHistory { get; set; } = [];
    public ICollection<RankRequirement> Requirements { get; set; } = [];
}

public sealed class StudentRankHistory : TrackedEntity
{
    public required Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public required Guid BeltRankId { get; set; }
    public BeltRank BeltRank { get; set; } = null!;
    public DateTime AwardedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public string? Notes { get; set; }
}

public sealed class RankRequirement : TrackedEntity
{
    public required Guid BeltRankId { get; set; }
    public BeltRank BeltRank { get; set; } = null!;
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public ICollection<StudentRequirementProgress> Progress { get; set; } = [];
}

public sealed class StudentRequirementProgress : TrackedEntity
{
    public required Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public required Guid RankRequirementId { get; set; }
    public RankRequirement RankRequirement { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? Notes { get; set; }
}

public abstract class PublishedContent : TrackedEntity
{
    public required string Title { get; set; }
    public required string Body { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? PublishedAtUtc { get; set; }
    public Guid? PublishedByUserId { get; set; }
}

public sealed class Announcement : PublishedContent
{
    public DateTime? ExpiresAtUtc { get; set; }
}

public sealed class NewsPost : PublishedContent
{
    public string? Slug { get; set; }
}

public sealed class MediaAsset : TrackedEntity
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required string StorageKey { get; set; }
    public long SizeBytes { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ConsentDocument : TrackedEntity
{
    public required string Name { get; set; }
    public required string Version { get; set; }
    public required string StorageKey { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ConsentAcceptance : TrackedEntity
{
    public required Guid ConsentDocumentId { get; set; }
    public ConsentDocument ConsentDocument { get; set; } = null!;
    public Guid? StudentId { get; set; }
    public Student? Student { get; set; }
    public Guid? GuardianId { get; set; }
    public Guardian? Guardian { get; set; }
    public required Guid AcceptedByUserId { get; set; }
    public DateTime AcceptedAtUtc { get; set; } = DateTime.UtcNow;
    public string AcceptanceMethod { get; set; } = "Account";
}

public sealed class AuditEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ActorUserId { get; set; }
    public required string Action { get; set; }
    public required string Entity { get; set; }
    public required string EntityId { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public string? MetadataJson { get; set; }
}
