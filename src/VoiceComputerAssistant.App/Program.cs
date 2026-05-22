using System.Text.Json;
using VoiceComputerAssistant.App.Agent;
using VoiceComputerAssistant.App.App;
using VoiceComputerAssistant.App.Browser;
using VoiceComputerAssistant.App.DemoSite;
using VoiceComputerAssistant.App.Diagnostics;
using VoiceComputerAssistant.App.OpenAI;
using VoiceComputerAssistant.App.Safety;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
};

try
{
    var cliArgs = CliArgs.Parse(args);

    if (cliArgs.ShowHelp)
    {
        Console.WriteLine(CliArgs.HelpText);
        return 0;
    }

    var options = AppOptions.FromEnvironment(cliArgs);

    if (string.IsNullOrWhiteSpace(options.OpenAiApiKey))
    {
        Console.Error.WriteLine("OPENAI_API_KEY is required.");
        return 2;
    }

    var prompt = options.Prompt;
    if (string.IsNullOrWhiteSpace(prompt))
    {
        Console.WriteLine("Try: Filter the local dashboard by dotnet, open details for dotnet-voice-computer-assistant, and summarize what is visible.");
        Console.Write("Prompt> ");
        prompt = Console.ReadLine();
    }

    if (string.IsNullOrWhiteSpace(prompt))
    {
        Console.Error.WriteLine("A prompt is required.");
        return 2;
    }

    var validation = PromptValidator.Validate(prompt);
    var repoRoot = RepoPaths.FindRepoRoot();
    var artifacts = RunArtifactStore.Create(repoRoot, options.SaveScreenshots);
    await artifacts.SaveValidationAsync(
        JsonSerializer.Serialize(validation, new JsonSerializerOptions { WriteIndented = true }),
        cts.Token);

    if (!validation.CanExecuteAutomatically)
    {
        Console.Error.WriteLine($"Prompt was not executed: {validation.RiskLevel}. Reason: {validation.Reason}");
        return 3;
    }

    var siteDirectory = Path.Combine(repoRoot, "demo-site");

    Console.WriteLine("Voice Computer Assistant Demo - Phase 1 Prompt Mode");
    Console.WriteLine($"[config] model={options.OpenAiModel}");

    await using var server = await DemoSiteServer.StartAsync(siteDirectory, options.Port, cts.Token);
    Console.WriteLine($"[server] serving demo-site at {server.BaseUrl}/");

    await using var browser = await PlaywrightBrowserSession.StartAsync(server.BaseUrl, options.Headless, cts.Token);
    Console.WriteLine($"[browser] launched Chromium {(options.Headless ? "headless" : "headed")}, viewport={options.ViewportWidth}x{options.ViewportHeight}");
    Console.WriteLine($"[safety] Prompt allowed: {validation.RiskLevel}");

    var httpClient = new HttpClient();
    var apiClient = new ResponsesApiClient(options.ResponsesApiOptions, httpClient);
    var parser = new ResponseOutputParser();
    var executor = new ComputerActionExecutor(options.ViewportWidth, options.ViewportHeight);
    var agent = new ResponsesComputerAgent(apiClient, parser, executor, options.MaxTurns, server.BaseUrl);

    var result = await agent.RunAsync(validation.SanitizedPrompt, browser, artifacts, cts.Token);

    Console.WriteLine();
    Console.WriteLine("Final answer:");
    Console.WriteLine(result.FinalText);
    Console.WriteLine($"Artifacts: {artifacts.RunDirectory}");

    if (!result.Success && !string.IsNullOrWhiteSpace(result.ErrorMessage))
    {
        Console.Error.WriteLine(result.ErrorMessage);
    }

    return result.Success ? 0 : 1;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Operation cancelled.");
    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
