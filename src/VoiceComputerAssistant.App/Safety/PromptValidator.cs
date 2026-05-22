using System.Text.RegularExpressions;

namespace VoiceComputerAssistant.App.Safety;

public static partial class PromptValidator
{
    private static readonly string[] SafeIndicators =
    [
        "filter",
        "search",
        "sort",
        "open details",
        "summarize",
        "inspect",
        "navigate inside local dashboard",
        "tell me what is visible"
    ];

    private static readonly string[] NeedsConfirmationIndicators =
    [
        "edit",
        "create",
        "submit",
        "save",
        "move",
        "update",
        "rename",
        "add",
        "remove"
    ];

    private static readonly string[] BlockedIndicators =
    [
        "delete",
        "destroy",
        "wipe",
        "purchase",
        "buy",
        "pay",
        "send email",
        "email",
        "login",
        "log in",
        "password",
        "token",
        "api key",
        "secret",
        "push to github",
        "commit",
        "force push",
        "change credentials",
        "open external website",
        "browse web"
    ];

    public static PromptValidationResult Validate(string? prompt)
    {
        var sanitizedPrompt = Sanitize(prompt);

        if (string.IsNullOrWhiteSpace(sanitizedPrompt))
        {
            return new PromptValidationResult(
                PromptRiskLevel.Blocked,
                "Prompt is empty.",
                sanitizedPrompt);
        }

        var normalizedPrompt = sanitizedPrompt.ToLowerInvariant();

        if (ReferencesGitHubAutomation(normalizedPrompt))
        {
            return new PromptValidationResult(
                PromptRiskLevel.Blocked,
                "Prompt contains blocked GitHub automation.",
                sanitizedPrompt);
        }

        var blockedMatch = FindFirstMatch(normalizedPrompt, BlockedIndicators);
        if (blockedMatch is not null)
        {
            return new PromptValidationResult(
                PromptRiskLevel.Blocked,
                $"Prompt contains blocked operation: {blockedMatch}.",
                sanitizedPrompt);
        }

        var needsConfirmationMatch = FindFirstMatch(normalizedPrompt, NeedsConfirmationIndicators);
        if (needsConfirmationMatch is not null)
        {
            return new PromptValidationResult(
                PromptRiskLevel.NeedsConfirmation,
                $"Prompt contains state-changing operation: {needsConfirmationMatch}.",
                sanitizedPrompt);
        }

        var safeMatch = FindFirstMatch(normalizedPrompt, SafeIndicators);
        if (safeMatch is not null)
        {
            return new PromptValidationResult(
                PromptRiskLevel.SafeReadOnly,
                $"Prompt is read-only and matches safe operation: {safeMatch}.",
                sanitizedPrompt);
        }

        return new PromptValidationResult(
            PromptRiskLevel.NeedsConfirmation,
            "Prompt does not match the allow-listed read-only patterns.",
            sanitizedPrompt);
    }

    private static string Sanitize(string? prompt) =>
        WhitespaceRegex().Replace(prompt?.Trim() ?? string.Empty, " ");

    private static string? FindFirstMatch(string prompt, IEnumerable<string> indicators)
    {
        foreach (var indicator in indicators)
        {
            if (prompt.Contains(indicator, StringComparison.Ordinal))
            {
                return indicator;
            }
        }

        return null;
    }

    private static bool ReferencesGitHubAutomation(string prompt) =>
        prompt.Contains("github", StringComparison.Ordinal) &&
        (prompt.Contains("push", StringComparison.Ordinal) ||
         prompt.Contains("commit", StringComparison.Ordinal) ||
         prompt.Contains("login", StringComparison.Ordinal) ||
         prompt.Contains("log in", StringComparison.Ordinal));

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
