using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskKarate.Api.Data;
using TaskKarate.Api.Models;
using TaskKarate.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);
var connectionString = builder.Configuration.GetConnectionString("TaskKarate") ?? $"Data Source={Path.Combine(dataDirectory, "task-karate.db")}";

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<ApplicationDbContext>().AddSignInManager().AddDefaultTokenProviders();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDataProtection();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddAuthentication().AddCookie(StudentAuth.Scheme, options =>
{
    options.Cookie.Name = "task_karate_student";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "task_karate_staff";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
});
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173", "https://localhost:5173"];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddAuthorization(options => options.AddPolicy("Staff", policy => policy.RequireRole("Administrator", "Instructor", "Staff")));
builder.Services.AddScoped<AuditService>();
builder.Services.Configure<StarterDatabaseOptions>(options =>
{
    options.Path = builder.Configuration["StarterDatabase:Path"]
        ?? Environment.GetEnvironmentVariable("TASK_KARATE_STARTER_DB")
        ?? Path.Combine(dataDirectory, "TaskKarate_Starter.db");
    options.ContentRootPath = builder.Environment.ContentRootPath;
    options.ImportLegacySchedules = builder.Configuration.GetValue<bool>("StarterDatabase:ImportLegacySchedules") || Environment.GetEnvironmentVariable("TASK_KARATE_IMPORT_STARTER_SCHEDULES") is "1" or "true";
    options.ImportDemoStudents = builder.Configuration.GetValue<bool>("StarterDatabase:ImportDemoStudents") || Environment.GetEnvironmentVariable("TASK_KARATE_IMPORT_STARTER_STUDENTS") is "1" or "true";
});
builder.Services.AddSingleton<IPasswordHasher<StarterStudentAccount>, PasswordHasher<StarterStudentAccount>>();
builder.Services.AddSingleton<StudentExperienceService>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.Use(async (context, next) =>
{
    if (!context.Request.Cookies.ContainsKey("XSRF-TOKEN"))
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        context.Response.Cookies.Append("XSRF-TOKEN", token, new CookieOptions { HttpOnly = false, SameSite = SameSiteMode.Lax, Secure = context.Request.IsHttps, IsEssential = true, Path = "/" });
    }
    var isApiMutation = context.Request.Path.StartsWithSegments("/api") && (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) || HttpMethods.IsPatch(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method));
    if (isApiMutation && !context.Request.Path.StartsWithSegments("/api/health") && !context.Request.Path.Equals("/api/auth/login") && !ValidCsrf(context.Request))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { title = "CSRF validation failed", status = 400 });
        return;
    }
    await next();
});
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await BootstrapService.SeedAsync(scope.ServiceProvider, app.Configuration, app.Environment);
    await scope.ServiceProvider.GetRequiredService<StudentExperienceService>().EnsureReadyAsync();
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }));
var auth = app.MapGroup("/api/auth");
auth.MapGet("/csrf", (HttpContext context) =>
{
    var token = context.Request.Cookies["XSRF-TOKEN"];
    if (string.IsNullOrWhiteSpace(token))
    {
        token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        context.Response.Cookies.Append("XSRF-TOKEN", token, new CookieOptions { HttpOnly = false, SameSite = SameSiteMode.Lax, Secure = context.Request.IsHttps, IsEssential = true, Path = "/" });
    }
    return Results.Ok(new { token });
});
auth.MapGet("/me", (HttpContext context) => context.User.Identity?.IsAuthenticated == true ? Results.Ok(new { authenticated = true, email = context.User.Identity.Name, roles = context.User.Claims.Where(c => c.Type.EndsWith("role")).Select(c => c.Value) }) : Results.Unauthorized());
auth.MapPost("/login", async (LoginRequest request, SignInManager<AppUser> signIn, UserManager<AppUser> users) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["credentials"] = ["Email and password are required."] });
    var user = await users.FindByEmailAsync(request.Email.Trim());
    if (user is null || !user.IsActive || !(await signIn.CheckPasswordSignInAsync(user, request.Password, true)).Succeeded) return Results.Problem("Invalid credentials.", statusCode: 401);
    await signIn.SignInAsync(user, request.RememberMe);
    return Results.Ok(new { authenticated = true });
});
auth.MapPost("/logout", async (SignInManager<AppUser> signIn) => { await signIn.SignOutAsync(); return Results.NoContent(); }).RequireAuthorization("Staff");

var staff = app.MapGroup("/api").RequireAuthorization("Staff");
staff.MapGet("/students", async (ApplicationDbContext db, bool? includeInactive) => Results.Ok(await db.Students.AsNoTracking().Include(x => x.BeltRank).Where(x => includeInactive == true || x.IsActive).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).Select(x => new StudentDto(x.Id, x.FirstName, x.LastName, x.PreferredName, x.BirthDate, x.AgeGroup, x.JoinDate, x.UniformSize, x.BeltSize, x.IsActive, x.BeltRank == null ? null : x.BeltRank.Name)).ToListAsync()));
staff.MapPost("/students", async (StudentRequest request, ApplicationDbContext db, HttpContext context, AuditService audit) =>
{
    if (!ValidAgeGroup(request.AgeGroup) || string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["student"] = ["First name, last name, and a valid age group are required."] });
    var student = new Student { FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), PreferredName = request.PreferredName?.Trim(), BirthDate = request.BirthDate, AgeGroup = request.AgeGroup, JoinDate = request.JoinDate?.Date ?? DateTime.UtcNow.Date, UniformSize = request.UniformSize?.Trim(), BeltSize = request.BeltSize?.Trim(), BeltRankId = request.BeltRankId };
    db.Students.Add(student); await db.SaveChangesAsync(); await audit.RecordAsync(context, "Create", "Student", student.Id, new { student.AgeGroup }); return Results.Created($"/api/students/{student.Id}", new { student.Id });
});
staff.MapPut("/students/{id:guid}", async (Guid id, StudentRequest request, ApplicationDbContext db, HttpContext context, AuditService audit) =>
{
    var student = await db.Students.FindAsync(id); if (student is null) return Results.NotFound();
    if (!ValidAgeGroup(request.AgeGroup) || string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["student"] = ["Student details are invalid."] });
    student.FirstName = request.FirstName.Trim(); student.LastName = request.LastName.Trim(); student.PreferredName = request.PreferredName?.Trim(); student.BirthDate = request.BirthDate; student.AgeGroup = request.AgeGroup; student.JoinDate = request.JoinDate?.Date ?? student.JoinDate; student.UniformSize = request.UniformSize?.Trim(); student.BeltSize = request.BeltSize?.Trim(); student.BeltRankId = request.BeltRankId; student.UpdatedAtUtc = DateTime.UtcNow;
    await db.SaveChangesAsync(); await audit.RecordAsync(context, "Update", "Student", id, new { student.AgeGroup }); return Results.NoContent();
});
staff.MapPost("/students/{id:guid}/deactivate", async (Guid id, ApplicationDbContext db, HttpContext context, AuditService audit) => { var student = await db.Students.FindAsync(id); if (student is null) return Results.NotFound(); student.IsActive = false; student.UpdatedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync(); await audit.RecordAsync(context, "Deactivate", "Student", id); return Results.NoContent(); });

staff.MapGet("/guardians", async (ApplicationDbContext db) => Results.Ok(await db.Guardians.AsNoTracking().Include(x => x.Students).ThenInclude(x => x.Student).Where(x => x.IsActive).OrderBy(x => x.LastName).Select(x => new GuardianDto(x.Id, x.FirstName, x.LastName, x.Email, x.Phone, x.Students.Select(s => new LinkedStudentDto(s.StudentId, s.Student.FirstName + " " + s.Student.LastName, s.Relationship)).ToList())).ToListAsync()));
staff.MapPost("/guardians", async (GuardianRequest request, ApplicationDbContext db, HttpContext context, AuditService audit) =>
{
    if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["guardian"] = ["A name and valid email are required."] });
    if (await db.Guardians.AnyAsync(x => x.Email == request.Email.Trim().ToLowerInvariant())) return Results.Conflict(new { title = "Guardian already exists" });
    var guardian = new Guardian { FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = request.Email.Trim().ToLowerInvariant(), Phone = request.Phone?.Trim() }; db.Guardians.Add(guardian); await LinkGuardianStudents(db, guardian.Id, request.StudentIds); await db.SaveChangesAsync(); await audit.RecordAsync(context, "Create", "Guardian", guardian.Id); return Results.Created($"/api/guardians/{guardian.Id}", new { guardian.Id });
});
staff.MapPut("/guardians/{id:guid}", async (Guid id, GuardianRequest request, ApplicationDbContext db, HttpContext context, AuditService audit) =>
{
    var guardian = await db.Guardians.Include(x => x.Students).FirstOrDefaultAsync(x => x.Id == id); if (guardian is null) return Results.NotFound();
    if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["guardian"] = ["Guardian details are invalid."] });
    guardian.FirstName = request.FirstName.Trim(); guardian.LastName = request.LastName.Trim(); guardian.Email = request.Email.Trim().ToLowerInvariant(); guardian.Phone = request.Phone?.Trim(); guardian.UpdatedAtUtc = DateTime.UtcNow; db.GuardianStudents.RemoveRange(guardian.Students); await LinkGuardianStudents(db, id, request.StudentIds); await db.SaveChangesAsync(); await audit.RecordAsync(context, "Update", "Guardian", id); return Results.NoContent();
});

staff.MapGet("/programs", async (ApplicationDbContext db) => Results.Ok(await db.Programs.AsNoTracking().Include(x => x.ClassTemplates).Where(x => x.IsActive).OrderBy(x => x.Name).Select(x => new ProgramDto(x.Id, x.Name, x.Description, x.ClassTemplates.Where(t => t.IsActive).Select(t => new TemplateDto(t.Id, t.Name, t.DayOfWeek, t.StartTime, t.DurationMinutes, t.BeltScope)).ToList())).ToListAsync()));
staff.MapPost("/programs", async (ProgramRequest request, ApplicationDbContext db, HttpContext context, AuditService audit) => { if (string.IsNullOrWhiteSpace(request.Name)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["name"] = ["Program name is required."] }); var program = new ProgramArea { Name = request.Name.Trim(), Description = request.Description?.Trim() }; db.Programs.Add(program); await db.SaveChangesAsync(); await audit.RecordAsync(context, "Create", "Program", program.Id); return Results.Created($"/api/programs/{program.Id}", new { program.Id }); });
staff.MapPost("/class-templates", async (TemplateRequest request, ApplicationDbContext db, HttpContext context, AuditService audit) => { if (request.ProgramId == Guid.Empty || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.DayOfWeek) || request.DurationMinutes is < 1 or > 480) return Results.ValidationProblem(new Dictionary<string, string[]> { ["template"] = ["Program, name, day, and duration are required."] }); if (!await db.Programs.AnyAsync(x => x.Id == request.ProgramId && x.IsActive)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["programId"] = ["Program does not exist."] }); var template = new ClassTemplate { ProgramAreaId = request.ProgramId, Name = request.Name.Trim(), DayOfWeek = request.DayOfWeek.Trim(), StartTime = request.StartTime.Trim(), DurationMinutes = request.DurationMinutes, BeltScope = request.BeltScope?.Trim() }; db.ClassTemplates.Add(template); await db.SaveChangesAsync(); await audit.RecordAsync(context, "Create", "ClassTemplate", template.Id); return Results.Created($"/api/class-templates/{template.Id}", new { template.Id }); });
staff.MapGet("/sessions", async (ApplicationDbContext db, DateTime? date) => { var day = (date ?? DateTime.UtcNow).Date; return Results.Ok(await db.ClassSessions.AsNoTracking().Include(x => x.ClassTemplate).Where(x => x.SessionDateUtc >= day && x.SessionDateUtc < day.AddDays(1)).OrderBy(x => x.ClassTemplate.StartTime).Select(x => new SessionDto(x.Id, x.ClassTemplateId, x.ClassTemplate.Name, x.ClassTemplate.StartTime, x.ClassTemplate.DurationMinutes, x.SessionDateUtc, x.IsCancelled)).ToListAsync()); });
staff.MapPost("/sessions", async (SessionRequest request, ApplicationDbContext db, HttpContext context, AuditService audit) => { var template = await db.ClassTemplates.FirstOrDefaultAsync(x => x.Id == request.ClassTemplateId && x.IsActive); if (template is null) return Results.ValidationProblem(new Dictionary<string, string[]> { ["classTemplateId"] = ["Class template does not exist."] }); var date = request.SessionDateUtc.Date; if (await db.ClassSessions.AnyAsync(x => x.ClassTemplateId == template.Id && x.SessionDateUtc == date)) return Results.Conflict(new { title = "Class session already exists" }); var session = new ClassSession { ClassTemplateId = template.Id, SessionDateUtc = DateTime.SpecifyKind(date, DateTimeKind.Utc), Notes = request.Notes?.Trim() }; db.ClassSessions.Add(session); await db.SaveChangesAsync(); await audit.RecordAsync(context, "Create", "ClassSession", session.Id); return Results.Created($"/api/sessions/{session.Id}", new { session.Id }); });
staff.MapGet("/attendance", async (Guid sessionId, ApplicationDbContext db) => Results.Ok(await db.AttendanceRecords.AsNoTracking().Where(x => x.ClassSessionId == sessionId).Include(x => x.Student).OrderBy(x => x.Student.LastName).Select(x => new AttendanceDto(x.Id, x.StudentId, x.Student.FirstName + " " + x.Student.LastName, x.CheckedInAtUtc, x.Notes)).ToListAsync()));
staff.MapPost("/attendance", async (AttendanceRequest request, ApplicationDbContext db, HttpContext context, AuditService audit) => { if (!await db.Students.AnyAsync(x => x.Id == request.StudentId && x.IsActive) || !await db.ClassSessions.AnyAsync(x => x.Id == request.ClassSessionId && !x.IsCancelled)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["attendance"] = ["A real active student and non-cancelled class session are required."] }); if (await db.AttendanceRecords.AnyAsync(x => x.StudentId == request.StudentId && x.ClassSessionId == request.ClassSessionId)) return Results.Conflict(new { title = "Attendance already recorded" }); var record = new AttendanceRecord { StudentId = request.StudentId, ClassSessionId = request.ClassSessionId, Notes = request.Notes?.Trim() }; db.AttendanceRecords.Add(record); await db.SaveChangesAsync(); await audit.RecordAsync(context, "CheckIn", "AttendanceRecord", record.Id, new { request.StudentId, request.ClassSessionId }); return Results.Created($"/api/attendance/{record.Id}", new { record.Id }); });

staff.MapGet("/announcements", async (ApplicationDbContext db) => Results.Ok(await db.Announcements.AsNoTracking().OrderByDescending(x => x.UpdatedAtUtc).Select(x => new ContentDto(x.Id, x.Title, x.Body, x.Status, x.PublishedAtUtc)).ToListAsync()));
staff.MapPost("/announcements", async (ContentRequest request, ApplicationDbContext db, HttpContext context, AuditService audit) => await CreateContent(db, context, audit, request, false));
staff.MapPost("/announcements/{id:guid}/publish", async (Guid id, ApplicationDbContext db, HttpContext context, AuditService audit) => await PublishAnnouncement(db, context, audit, id));
staff.MapGet("/news", async (ApplicationDbContext db) => Results.Ok(await db.NewsPosts.AsNoTracking().OrderByDescending(x => x.UpdatedAtUtc).Select(x => new ContentDto(x.Id, x.Title, x.Body, x.Status, x.PublishedAtUtc)).ToListAsync()));
staff.MapPost("/news", async (ContentRequest request, ApplicationDbContext db, HttpContext context, AuditService audit) => await CreateContent(db, context, audit, request, true));
staff.MapPost("/news/{id:guid}/publish", async (Guid id, ApplicationDbContext db, HttpContext context, AuditService audit) => await PublishNews(db, context, audit, id));

var publicApi = app.MapGroup("/api/public");
publicApi.MapGet("/schedule", async (ApplicationDbContext db, DateTime? from, DateTime? to) => { var start = (from ?? DateTime.UtcNow.Date).Date; var end = (to ?? start.AddDays(14)).Date.AddDays(1); return Results.Ok(await db.ClassSessions.AsNoTracking().Include(x => x.ClassTemplate).ThenInclude(x => x.ProgramArea).Where(x => !x.IsCancelled && x.SessionDateUtc >= start && x.SessionDateUtc < end).OrderBy(x => x.SessionDateUtc).ThenBy(x => x.ClassTemplate.StartTime).Select(x => new { x.Id, date = x.SessionDateUtc, className = x.ClassTemplate.Name, program = x.ClassTemplate.ProgramArea.Name, x.ClassTemplate.StartTime, x.ClassTemplate.DurationMinutes }).ToListAsync()); });
publicApi.MapGet("/announcements", async (ApplicationDbContext db) => Results.Ok(await db.Announcements.AsNoTracking().Where(x => x.Status == "Published" && (x.ExpiresAtUtc == null || x.ExpiresAtUtc > DateTime.UtcNow)).OrderByDescending(x => x.PublishedAtUtc).Select(x => new ContentDto(x.Id, x.Title, x.Body, x.Status, x.PublishedAtUtc)).ToListAsync()));
publicApi.MapGet("/news", async (ApplicationDbContext db) => Results.Ok(await db.NewsPosts.AsNoTracking().Where(x => x.Status == "Published").OrderByDescending(x => x.PublishedAtUtc).Select(x => new ContentDto(x.Id, x.Title, x.Body, x.Status, x.PublishedAtUtc)).ToListAsync()));
app.MapStudentExperience();
app.Run();

static bool ValidCsrf(HttpRequest request)
{
    var cookie = request.Cookies["XSRF-TOKEN"]; var header = request.Headers["X-XSRF-TOKEN"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(cookie) || string.IsNullOrWhiteSpace(header)) return false;
    try { return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(cookie), Convert.FromBase64String(header)); } catch (FormatException) { return false; }
}
static bool ValidAgeGroup(string value) => value is "Kids" or "TeensAdults";
static async Task LinkGuardianStudents(ApplicationDbContext db, Guid guardianId, IEnumerable<Guid> studentIds)
{
    var ids = studentIds.Distinct().ToList(); var existing = await db.Students.Where(x => ids.Contains(x.Id) && x.IsActive).Select(x => x.Id).ToListAsync(); db.GuardianStudents.AddRange(existing.Select((id, index) => new GuardianStudent { GuardianId = guardianId, StudentId = id, IsPrimary = index == 0 }));
}
static async Task<IResult> CreateContent(ApplicationDbContext db, HttpContext context, AuditService audit, ContentRequest request, bool news)
{
    if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["content"] = ["Title and body are required."] });
    PublishedContent content = news ? new NewsPost { Title = request.Title.Trim(), Body = request.Body.Trim(), Slug = Slugify(request.Title) } : new Announcement { Title = request.Title.Trim(), Body = request.Body.Trim(), ExpiresAtUtc = request.ExpiresAtUtc };
    db.Add(content); await db.SaveChangesAsync(); await audit.RecordAsync(context, "Create", news ? "NewsPost" : "Announcement", content.Id); return Results.Created($"/api/{(news ? "news" : "announcements")}/{content.Id}", new { content.Id });
}
static async Task<IResult> PublishAnnouncement(ApplicationDbContext db, HttpContext context, AuditService audit, Guid id)
{
    var item = await db.Announcements.FindAsync(id); if (item is null) return Results.NotFound(); item.Status = "Published"; item.PublishedAtUtc = DateTime.UtcNow; item.PublishedByUserId = CurrentUserId(context); item.UpdatedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync(); await audit.RecordAsync(context, "Publish", "Announcement", id); return Results.NoContent();
}
static async Task<IResult> PublishNews(ApplicationDbContext db, HttpContext context, AuditService audit, Guid id)
{
    var item = await db.NewsPosts.FindAsync(id); if (item is null) return Results.NotFound(); item.Status = "Published"; item.PublishedAtUtc = DateTime.UtcNow; item.PublishedByUserId = CurrentUserId(context); item.UpdatedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync(); await audit.RecordAsync(context, "Publish", "NewsPost", id); return Results.NoContent();
}
static Guid? CurrentUserId(HttpContext context) => Guid.TryParse(context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
static string Slugify(string value) => string.Join('-', value.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

public sealed record LoginRequest(string Email, string Password, bool RememberMe = false);
public sealed record StudentRequest(string FirstName, string LastName, string? PreferredName, DateTime? BirthDate, string AgeGroup, DateTime? JoinDate, string? UniformSize, string? BeltSize, Guid? BeltRankId);
public sealed record GuardianRequest(string FirstName, string LastName, string Email, string? Phone, IReadOnlyCollection<Guid> StudentIds);
public sealed record ProgramRequest(string Name, string? Description);
public sealed record TemplateRequest(Guid ProgramId, string Name, string DayOfWeek, string StartTime, int DurationMinutes, string? BeltScope);
public sealed record SessionRequest(Guid ClassTemplateId, DateTime SessionDateUtc, string? Notes);
public sealed record AttendanceRequest(Guid StudentId, Guid ClassSessionId, string? Notes);
public sealed record ContentRequest(string Title, string Body, DateTime? ExpiresAtUtc = null);
public sealed record StudentDto(Guid Id, string FirstName, string LastName, string? PreferredName, DateTime? BirthDate, string AgeGroup, DateTime JoinDate, string? UniformSize, string? BeltSize, bool IsActive, string? BeltRank);
public sealed record LinkedStudentDto(Guid Id, string Name, string Relationship);
public sealed record GuardianDto(Guid Id, string FirstName, string LastName, string Email, string? Phone, IReadOnlyCollection<LinkedStudentDto> Students);
public sealed record ProgramDto(Guid Id, string Name, string? Description, IReadOnlyCollection<TemplateDto> Templates);
public sealed record TemplateDto(Guid Id, string Name, string DayOfWeek, string StartTime, int DurationMinutes, string? BeltScope);
public sealed record SessionDto(Guid Id, Guid ClassTemplateId, string Name, string StartTime, int DurationMinutes, DateTime SessionDateUtc, bool IsCancelled);
public sealed record AttendanceDto(Guid Id, Guid StudentId, string StudentName, DateTime CheckedInAtUtc, string? Notes);
public sealed record ContentDto(Guid Id, string Title, string Body, string Status, DateTime? PublishedAtUtc);

public partial class Program { }
