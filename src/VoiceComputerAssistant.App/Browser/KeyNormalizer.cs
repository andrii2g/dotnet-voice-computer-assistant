namespace VoiceComputerAssistant.App.Browser;

public static class KeyNormalizer
{
    private static readonly IReadOnlyDictionary<string, string> Mappings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CTRL"] = "Control",
            ["CONTROL"] = "Control",
            ["CMD"] = "Meta",
            ["META"] = "Meta",
            ["ALT"] = "Alt",
            ["OPTION"] = "Alt",
            ["SHIFT"] = "Shift",
            ["ENTER"] = "Enter",
            ["RETURN"] = "Enter",
            ["ESC"] = "Escape",
            ["ESCAPE"] = "Escape",
            ["BACKSPACE"] = "Backspace",
            ["TAB"] = "Tab",
            ["SPACE"] = "Space",
            ["ARROWUP"] = "ArrowUp",
            ["ARROWDOWN"] = "ArrowDown",
            ["ARROWLEFT"] = "ArrowLeft",
            ["ARROWRIGHT"] = "ArrowRight"
        };

    public static string Normalize(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key value is required.", nameof(key));
        }

        var trimmed = key.Trim();
        return Mappings.TryGetValue(trimmed, out var normalized)
            ? normalized
            : trimmed;
    }
}
