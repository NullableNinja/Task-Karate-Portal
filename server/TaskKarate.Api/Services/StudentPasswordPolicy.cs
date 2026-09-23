namespace TaskKarate.Api.Services;

public static class StudentPasswordPolicy
{
    public const string TemporaryPassword = "Black Belt";

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

public static class StudentPinPolicy
{
    public static IReadOnlyDictionary<string, string[]> Validate(string? pin, string? confirmation)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(pin))
        {
            errors["pin"] = ["A check-in PIN is required."];
            return errors;
        }

        var messages = new List<string>();
        if (pin.Length is < 4 or > 6 || pin.Any(character => !char.IsDigit(character))) messages.Add("PINs must be 4 to 6 digits.");
        if (pin != confirmation) messages.Add("PIN confirmation does not match.");
        if (messages.Count > 0) errors["pin"] = messages.ToArray();
        return errors;
    }
}
