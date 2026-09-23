namespace TaskKarate.Api.Services;

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
