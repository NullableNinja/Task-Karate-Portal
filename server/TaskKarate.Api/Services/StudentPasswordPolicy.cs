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
        if (password.Length is < 4 or > 6 || password.Any(character => !char.IsDigit(character))) messages.Add("Student PINs must be 4 to 6 digits.");
        if (password != confirmation) messages.Add("Password confirmation does not match.");
        if (messages.Count > 0) errors["password"] = messages.ToArray();
        return errors;
    }
}
