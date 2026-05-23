using System.Text.Json;
using VoiceComputerAssistant.App.Browser;
using VoiceComputerAssistant.App.Diagnostics;
using VoiceComputerAssistant.App.OpenAI;

namespace VoiceComputerAssistant.App.Agent;

public sealed class ResponsesComputerAgent
{
    private readonly ResponsesApiClient _apiClient;
    private readonly ResponseOutputParser _parser;
    private readonly ComputerActionExecutor _executor;
    private readonly int _maxTurns;
    private readonly string _demoUrl;

    public ResponsesComputerAgent(
        ResponsesApiClient apiClient,
        ResponseOutputParser parser,
        ComputerActionExecutor executor,
        int maxTurns,
        string demoUrl)
    {
        _apiClient = apiClient;
        _parser = parser;
        _executor = executor;
        _maxTurns = maxTurns;
        _demoUrl = demoUrl;
    }

    public async Task<AgentRunResult> RunAsync(
        string validatedPrompt,
        PlaywrightBrowserSession browser,
        RunArtifactStore artifacts,
        CancellationToken cancellationToken)
    {
        await artifacts.SavePromptAsync(validatedPrompt, cancellationToken);

        var instructions = BuildInstructions(_demoUrl);
        var response = await _apiClient.CreateInitialResponseAsync(
            validatedPrompt,
            instructions,
            cancellationToken);

        for (var turnIndex = 0; turnIndex < _maxTurns; turnIndex++)
        {
            await artifacts.SaveResponseAsync(turnIndex, response.RawJson, cancellationToken);

            if (HasPendingSafetyChecks(response.Root))
            {
                const string message = "Responses API returned pending safety checks and auto-acknowledgement is disabled.";
                await artifacts.AppendRunLogAsync(
                    new AgentTurnLog(turnIndex, "safety-check", message),
                    cancellationToken);
                return new AgentRunResult(false, string.Empty, message);
            }

            var computerCall = _parser.FindComputerCall(response);
            if (computerCall is null)
            {
                var finalText = _parser.ExtractFinalText(response);
                if (string.IsNullOrWhiteSpace(finalText))
                {
                    const string noTextMessage = "Final response did not include extractable text.";
                    await artifacts.AppendRunLogAsync(
                        new AgentTurnLog(turnIndex, "final-missing", noTextMessage),
                        cancellationToken);
                    return new AgentRunResult(false, string.Empty, noTextMessage);
                }

                await artifacts.SaveFinalTextAsync(finalText, cancellationToken);
                await artifacts.AppendRunLogAsync(
                    new AgentTurnLog(turnIndex, "final", "Final response received."),
                    cancellationToken);
                return new AgentRunResult(true, finalText, null);
            }

            await artifacts.AppendRunLogAsync(
                new AgentTurnLog(
                    turnIndex,
                    "computer_call",
                    $"call_id={computerCall.CallId}; actions={computerCall.Actions.Count}"),
                cancellationToken);

            await _executor.ExecuteAsync(browser.Page, computerCall.Actions, cancellationToken);
            await browser.EnsureStillOnAllowedOriginAsync(cancellationToken);

            var screenshotBytes = await browser.CaptureScreenshotAsync(cancellationToken);
            await artifacts.SaveScreenshotAsync(turnIndex, screenshotBytes, cancellationToken);

            var screenshotDataUrl = $"data:image/png;base64,{Convert.ToBase64String(screenshotBytes)}";
            response = await _apiClient.SendComputerCallOutputAsync(
                response.Id,
                computerCall.CallId,
                screenshotDataUrl,
                browser.Page.Url,
                Array.Empty<object>(),
                cancellationToken);
        }

        const string maxTurnMessage = "Maximum turn count reached before final response.";
        await artifacts.AppendRunLogAsync(
            new AgentTurnLog(_maxTurns, "max-turns", maxTurnMessage),
            cancellationToken);
        return new AgentRunResult(false, string.Empty, maxTurnMessage);
    }

    private static string BuildInstructions(string demoUrl) =>
        $"You are controlling a local browser dashboard at {demoUrl}. " +
        "Use the computer tool for UI interaction. " +
        "Do not navigate to external websites. " +
        "Do not perform destructive, purchasing, credential, email, or Git operations. " +
        "When finished, summarize only what is visible on the page.";

    private static bool HasPendingSafetyChecks(JsonElement root)
    {
        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("pending_safety_checks", out var checks) ||
                checks.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            if (checks.GetArrayLength() > 0)
            {
                return true;
            }
        }

        return false;
    }
}
