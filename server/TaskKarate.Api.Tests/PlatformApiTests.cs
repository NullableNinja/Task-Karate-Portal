using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskKarate.Api.Data;
using TaskKarate.Api.Models;
using TaskKarate.Api.Services;

namespace TaskKarate.Api.Tests;

public sealed class PlatformApiTests : IClassFixture<PlatformFactory>
{
    private readonly PlatformFactory factory;
    public PlatformApiTests(PlatformFactory factory) => this.factory = factory;

    [Fact]
    public async Task Anonymous_staff_roster_is_rejected_but_public_content_is_available()
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/students")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/public/announcements")).StatusCode);
    }

    [Fact]
    public async Task Staff_can_sign_in_and_duplicate_attendance_is_rejected()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await CsrfAsync(client);
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = PlatformFactory.Email, password = PlatformFactory.Password, rememberMe = true, disclaimerAccepted = true });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var student = new Student { FirstName = "Test", LastName = "Student", AgeGroup = "Kids", JoinDate = DateTime.UtcNow.Date };
        var program = new ProgramArea { Name = "Test Program" };
        db.AddRange(student, program); await db.SaveChangesAsync();
        var template = new ClassTemplate { ProgramAreaId = program.Id, Name = "Test Class", DayOfWeek = "Monday", StartTime = "05:00 PM", DurationMinutes = 60 };
        db.Add(template); await db.SaveChangesAsync();
        var session = new ClassSession { ClassTemplateId = template.Id, SessionDateUtc = DateTime.UtcNow.Date };
        db.Add(session); await db.SaveChangesAsync();

        await CsrfAsync(client);
        var first = await client.PostAsJsonAsync("/api/attendance", new { studentId = student.Id, classSessionId = session.Id });
        var duplicate = await client.PostAsJsonAsync("/api/attendance", new { studentId = student.Id, classSessionId = session.Id });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    private static async Task CsrfAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        var token = (await response.Content.ReadFromJsonAsync<CsrfResponse>())!.Token!;
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token);
    }
    private sealed record CsrfResponse(string? Token);
}

public sealed class PlatformFactory : WebApplicationFactory<Program>
{
    public const string Email = "test-admin@example.test";
    public const string Password = "Local-test-password-123!";
    private readonly string dbPath = Path.Combine(Path.GetTempPath(), $"task-karate-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:TaskKarate", $"Data Source={dbPath}");
        builder.UseSetting("BootstrapAdmin:Email", Email);
        builder.UseSetting("BootstrapAdmin:Password", Password);
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:TaskKarate"] = $"Data Source={dbPath}",
            ["BootstrapAdmin:Email"] = Email,
            ["BootstrapAdmin:Password"] = Password,
            ["AllowedOrigins:0"] = "http://localhost:5173"
        }));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try { File.Delete(dbPath); } catch { }
    }
}

public sealed class StudentPasswordPolicyTests
{
    [Fact]
    public void Rejects_short_or_weak_student_passwords_and_mismatched_confirmation()
    {
        var errors = StudentPasswordPolicy.Validate("weak", "different");
        Assert.True(errors.ContainsKey("password"));
        Assert.Contains("Use at least 12 characters.", errors["password"]);
        Assert.Contains("Include at least one uppercase letter.", errors["password"]);
        Assert.Contains("Include at least one number.", errors["password"]);
        Assert.Contains("Password confirmation does not match.", errors["password"]);
    }

    [Fact]
    public void Accepts_a_strong_matching_password()
    {
        Assert.Empty(StudentPasswordPolicy.Validate("Better-password-123", "Better-password-123"));
    }

    [Fact]
    public void Accepts_a_four_digit_check_in_pin()
    {
        Assert.Empty(StudentPinPolicy.Validate("4826", "4826"));
    }
}
