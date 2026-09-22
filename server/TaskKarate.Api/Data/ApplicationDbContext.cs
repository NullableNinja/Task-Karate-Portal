using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskKarate.Api.Models;

namespace TaskKarate.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AppUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<GuardianStudent> GuardianStudents => Set<GuardianStudent>();
    public DbSet<ProgramArea> Programs => Set<ProgramArea>();
    public DbSet<ClassTemplate> ClassTemplates => Set<ClassTemplate>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<BeltRank> BeltRanks => Set<BeltRank>();
    public DbSet<StudentRankHistory> StudentRankHistory => Set<StudentRankHistory>();
    public DbSet<RankRequirement> RankRequirements => Set<RankRequirement>();
    public DbSet<StudentRequirementProgress> StudentRequirementProgress => Set<StudentRequirementProgress>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<NewsPost> NewsPosts => Set<NewsPost>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<ConsentDocument> ConsentDocuments => Set<ConsentDocument>();
    public DbSet<ConsentAcceptance> ConsentAcceptances => Set<ConsentAcceptance>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<AppUser>().ToTable("AspNetUsers");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>().ToTable("AspNetRoles");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>().ToTable("AspNetUserRoles");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("AspNetUserTokens");

        b.Entity<Student>().HasIndex(x => new { x.IsActive, x.LastName, x.FirstName });
        b.Entity<Student>().HasIndex(x => x.BeltRankId);
        b.Entity<Guardian>().HasIndex(x => new { x.IsActive, x.LastName });
        b.Entity<Guardian>().HasIndex(x => x.Email).IsUnique();
        b.Entity<GuardianStudent>().HasKey(x => new { x.GuardianId, x.StudentId });
        b.Entity<GuardianStudent>().HasOne(x => x.Guardian).WithMany(x => x.Students).HasForeignKey(x => x.GuardianId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<GuardianStudent>().HasOne(x => x.Student).WithMany(x => x.Guardians).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ProgramArea>().HasIndex(x => new { x.IsActive, x.Name }).IsUnique();
        b.Entity<ClassTemplate>().HasIndex(x => new { x.IsActive, x.DayOfWeek, x.StartTime });
        b.Entity<ClassSession>().HasIndex(x => x.SessionDateUtc);
        b.Entity<ClassSession>().HasIndex(x => new { x.ClassTemplateId, x.SessionDateUtc }).IsUnique();
        b.Entity<Enrollment>().HasIndex(x => new { x.StudentId, x.ClassTemplateId }).IsUnique();
        b.Entity<AttendanceRecord>().HasIndex(x => new { x.ClassSessionId, x.StudentId }).IsUnique();
        b.Entity<AttendanceRecord>().HasIndex(x => x.StudentId);
        b.Entity<BeltRank>().HasIndex(x => new { x.SortOrder, x.Name }).IsUnique();
        b.Entity<StudentRankHistory>().HasIndex(x => new { x.StudentId, x.AwardedAtUtc });
        b.Entity<RankRequirement>().HasIndex(x => new { x.BeltRankId, x.SortOrder }).IsUnique();
        b.Entity<StudentRequirementProgress>().HasIndex(x => new { x.StudentId, x.RankRequirementId }).IsUnique();
        b.Entity<Announcement>().HasIndex(x => new { x.Status, x.PublishedAtUtc });
        b.Entity<NewsPost>().HasIndex(x => new { x.Status, x.PublishedAtUtc });
        b.Entity<NewsPost>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<ConsentDocument>().HasIndex(x => new { x.Name, x.Version }).IsUnique();
        b.Entity<AuditEvent>().HasIndex(x => new { x.Entity, x.EntityId, x.OccurredAtUtc });
        b.Entity<ConsentAcceptance>().ToTable("ConsentAcceptances", table => table.HasCheckConstraint("CK_ConsentAcceptance_Subject", "StudentId IS NOT NULL OR GuardianId IS NOT NULL"));

        foreach (var entity in b.Model.GetEntityTypes().Where(x => typeof(TrackedEntity).IsAssignableFrom(x.ClrType)))
        {
            entity.FindProperty(nameof(TrackedEntity.CreatedAtUtc))?.SetDefaultValueSql("CURRENT_TIMESTAMP");
            entity.FindProperty(nameof(TrackedEntity.UpdatedAtUtc))?.SetDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
