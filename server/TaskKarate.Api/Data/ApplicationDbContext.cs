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
        // The portal service still reads the imported legacy schema directly. Keep
        // the EF platform tables namespaced so both schemas can share one file
        // during the production migration without table-name collisions.
        b.Entity<AppUser>().ToTable("platform_AspNetUsers");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>().ToTable("platform_AspNetRoles");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>().ToTable("platform_AspNetUserRoles");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("platform_AspNetUserClaims");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("platform_AspNetUserLogins");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("platform_AspNetRoleClaims");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("platform_AspNetUserTokens");
        b.Entity<Student>().ToTable("platform_Students");
        b.Entity<Guardian>().ToTable("platform_Guardians");
        b.Entity<GuardianStudent>().ToTable("platform_GuardianStudents");
        b.Entity<ProgramArea>().ToTable("platform_Programs");
        b.Entity<ClassTemplate>().ToTable("platform_ClassTemplates");
        b.Entity<ClassSession>().ToTable("platform_ClassSessions");
        b.Entity<Enrollment>().ToTable("platform_Enrollments");
        b.Entity<AttendanceRecord>().ToTable("platform_AttendanceRecords");
        b.Entity<BeltRank>().ToTable("platform_BeltRanks");
        b.Entity<StudentRankHistory>().ToTable("platform_StudentRankHistory");
        b.Entity<RankRequirement>().ToTable("platform_RankRequirements");
        b.Entity<StudentRequirementProgress>().ToTable("platform_StudentRequirementProgress");
        b.Entity<Announcement>().ToTable("platform_Announcements");
        b.Entity<NewsPost>().ToTable("platform_NewsPosts");
        b.Entity<MediaAsset>().ToTable("platform_MediaAssets");
        b.Entity<ConsentDocument>().ToTable("platform_ConsentDocuments");
        b.Entity<ConsentAcceptance>().ToTable("platform_ConsentAcceptances");
        b.Entity<AuditEvent>().ToTable("platform_AuditEvents");

        b.Entity<Student>().HasIndex(x => new { x.IsActive, x.LastName, x.FirstName });
        b.Entity<Student>().HasIndex(x => x.BeltRankId);
        b.Entity<Guardian>().HasIndex(x => new { x.IsActive, x.LastName });
        b.Entity<Guardian>().HasIndex(x => x.Email).IsUnique();
        b.Entity<GuardianStudent>().HasKey(x => new { x.GuardianId, x.StudentId });
        b.Entity<GuardianStudent>().HasOne(x => x.Guardian).WithMany(x => x.Students).HasForeignKey(x => x.GuardianId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<GuardianStudent>().HasOne(x => x.Student).WithMany(x => x.Guardians).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ProgramArea>().HasIndex(x => new { x.IsActive, x.Name }).IsUnique();
        b.Entity<ClassTemplate>().HasIndex(x => new { x.IsActive, x.DayOfWeek, x.StartTime });
        b.Entity<ClassTemplate>().Property(x => x.ClassType).HasMaxLength(40);
        b.Entity<ClassSession>().HasOne(x => x.AssignedStudent).WithMany().HasForeignKey(x => x.AssignedStudentId).OnDelete(DeleteBehavior.SetNull);
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
