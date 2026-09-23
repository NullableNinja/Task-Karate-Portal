using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskKarate.Api.Data;
using TaskKarate.Api.Models;

namespace TaskKarate.Api.Services;

public static class BootstrapService
{
    private static readonly string[] Roles = ["Administrator", "Instructor", "Staff", "Guardian", "Student"];
    private static readonly (string Name, int SortOrder)[] Ranks = [("White Belt", 10), ("Gold Belt", 20), ("Orange Belt", 30), ("Green Belt", 40), ("Purple Belt", 50), ("Blue Belt", 60), ("Red Belt", 70), ("Brown Belt", 80), ("Black Belt", 90)];

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in Roles)
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole<Guid>(role));

        var db = services.GetRequiredService<ApplicationDbContext>();
        foreach (var (name, order) in Ranks)
            if (!await db.BeltRanks.AnyAsync(x => x.Name == name)) db.BeltRanks.Add(new BeltRank { Name = name, SortOrder = order });
        await db.SaveChangesAsync();

        var configuredUsername = configuration["BootstrapAdmin:Username"] ?? Environment.GetEnvironmentVariable("TASK_KARATE_ADMIN_USERNAME");
        var email = configuration["BootstrapAdmin:Email"] ?? Environment.GetEnvironmentVariable("TASK_KARATE_ADMIN_EMAIL");
        var password = configuration["BootstrapAdmin:Password"] ?? Environment.GetEnvironmentVariable("TASK_KARATE_ADMIN_PASSWORD");
        var resetPassword = configuration["BootstrapAdmin:ResetPassword"] ?? Environment.GetEnvironmentVariable("TASK_KARATE_ADMIN_RESET_PASSWORD");
        var username = string.IsNullOrWhiteSpace(configuredUsername) ? email?.Trim() : configuredUsername.Trim();
        if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password) || !string.IsNullOrWhiteSpace(resetPassword)))
        {
            if (!environment.IsDevelopment() && !string.IsNullOrWhiteSpace(resetPassword)) throw new InvalidOperationException("BootstrapAdmin:ResetPassword is development-only.");
            var users = services.GetRequiredService<UserManager<AppUser>>();
            var loginEmail = string.IsNullOrWhiteSpace(email) ? $"{username}@local.taskkarate.invalid" : email.Trim().ToLowerInvariant();
            var user = await users.FindByNameAsync(username) ?? await users.FindByEmailAsync(loginEmail);
            if (user is null)
            {
                if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Bootstrap admin creation requires BootstrapAdmin:Password.");
                user = new AppUser { UserName = username.Trim(), Email = loginEmail, EmailConfirmed = true };
                var result = await users.CreateAsync(user, password);
                if (!result.Succeeded) throw new InvalidOperationException($"Bootstrap admin could not be created: {string.Join("; ", result.Errors.Select(x => x.Description))}");
            }
            else if (!string.Equals(user.UserName, username.Trim(), StringComparison.Ordinal))
            {
                user.UserName = username.Trim();
                var result = await users.UpdateAsync(user);
                if (!result.Succeeded) throw new InvalidOperationException($"Bootstrap admin username could not be updated: {string.Join("; ", result.Errors.Select(x => x.Description))}");
            }
            if (!string.IsNullOrWhiteSpace(resetPassword))
            {
                var token = await users.GeneratePasswordResetTokenAsync(user);
                var result = await users.ResetPasswordAsync(user, token, resetPassword);
                if (!result.Succeeded) throw new InvalidOperationException($"Bootstrap admin password reset failed: {string.Join("; ", result.Errors.Select(x => x.Description))}");
            }
            if (!await users.IsInRoleAsync(user, "Administrator")) await users.AddToRoleAsync(user, "Administrator");
        }

        if (environment.IsDevelopment() && IsEnabled(configuration, "Seed:ImportLegacySchedules"))
            await ImportLegacySchedulesAsync(db, environment.ContentRootPath);
        if (environment.IsDevelopment() && IsEnabled(configuration, "Seed:ImportDemoStudents"))
            await ImportDemoStudentsAsync(db, environment.ContentRootPath);
    }

    private static bool IsEnabled(IConfiguration configuration, string key)
    {
        var value = configuration[key] ?? Environment.GetEnvironmentVariable(key.Replace(':', '_')) ?? Environment.GetEnvironmentVariable(key.Replace(":", "__", StringComparison.Ordinal));
        return value is not null && (value.Equals("1", StringComparison.OrdinalIgnoreCase) || value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("yes", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task ImportLegacySchedulesAsync(ApplicationDbContext db, string root)
    {
        var path = Path.GetFullPath(Path.Combine(root, "..", "..", "..", "..", "data", "schedules.json"));
        if (!File.Exists(path)) path = Path.GetFullPath(Path.Combine(root, "..", "..", "data", "schedules.json"));
        if (!File.Exists(path)) return;
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(path));
        foreach (var programProperty in document.RootElement.EnumerateObject())
        {
            var programName = programProperty.Value.GetProperty("title").GetString() ?? programProperty.Name;
            var program = await db.Programs.FirstOrDefaultAsync(x => x.Name == programName);
            if (program is null) { program = new ProgramArea { Name = programName, Description = "Imported from the legacy schedule archive; verify before publishing." }; db.Programs.Add(program); await db.SaveChangesAsync(); }
            foreach (var group in programProperty.Value.GetProperty("groups").EnumerateArray())
            {
                var groupName = group.GetProperty("name").GetString() ?? "Class";
                foreach (var day in group.GetProperty("schedule").EnumerateObject())
                    foreach (var occurrence in day.Value.EnumerateArray())
                    {
                        var time = occurrence.GetProperty("time").GetString() ?? "";
                        var duration = occurrence.TryGetProperty("isHour", out var hour) && hour.GetBoolean() ? 60 : 45;
                        var name = $"{groupName} — {day.Name} {time}";
                        if (!await db.ClassTemplates.AnyAsync(x => x.ProgramAreaId == program.Id && x.Name == name)) db.ClassTemplates.Add(new ClassTemplate { ProgramAreaId = program.Id, Name = name, DayOfWeek = day.Name, StartTime = time, DurationMinutes = duration, BeltScope = string.Join(", ", group.GetProperty("belts").EnumerateArray().Select(x => x.GetString())) });
                    }
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task ImportDemoStudentsAsync(ApplicationDbContext db, string root)
    {
        var path = Path.GetFullPath(Path.Combine(root, "..", "..", "..", "..", "data", "portal-students.json"));
        if (!File.Exists(path)) return;
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(path));
        foreach (var item in document.RootElement.GetProperty("students").EnumerateArray())
        {
            var name = item.GetProperty("name").GetString() ?? "Demo Student";
            var parts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 || await db.Students.AnyAsync(x => x.FirstName == parts[0] && x.LastName == parts[1])) continue;
            var rank = item.TryGetProperty("rank", out var rankProperty) ? rankProperty.GetString() : null;
            var beltRank = await db.BeltRanks.FirstOrDefaultAsync(x => x.Name == rank);
            db.Students.Add(new Student { FirstName = parts[0], LastName = parts[1], AgeGroup = item.GetProperty("ageGroup").GetString() == "kids" ? "Kids" : "TeensAdults", JoinDate = item.TryGetProperty("joinDate", out var joined) && DateTime.TryParse(joined.GetString(), out var joinDate) ? joinDate : DateTime.UtcNow.Date, UniformSize = item.TryGetProperty("uniformSize", out var uniform) ? uniform.GetString() : null, BeltSize = item.TryGetProperty("beltSize", out var beltSize) ? beltSize.GetString() : null, BeltRankId = beltRank?.Id });
        }
        await db.SaveChangesAsync();
    }
}
