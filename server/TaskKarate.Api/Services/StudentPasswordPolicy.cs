namespace TaskKarate.Api.Services;

public static class StudentPasswordPolicy
{
    public static IReadOnlyDictionary<string, string[]> Validate(string? password, string? confirmation)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(password))
        {
            errors["password"] = ["A password is required."];
            return errors;
        }

        var messages = new List<string>();
        if (password.Length < 12) messages.Add("Use at least 12 characters.");
        if (!password.Any(char.IsUpper)) messages.Add("Include at least one uppercase letter.");
        if (!password.Any(char.IsLower)) messages.Add("Include at least one lowercase letter.");
        if (!password.Any(char.IsDigit)) messages.Add("Include at least one number.");
        if (password != confirmation) messages.Add("Password confirmation does not match.");
        if (messages.Count > 0) errors["password"] = messages.ToArray();
        return errors;
    }
}
