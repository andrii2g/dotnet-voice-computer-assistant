using System.Globalization;
using VoiceComputerAssistant.App.OpenAI;

namespace VoiceComputerAssistant.App.App;

public sealed record AppOptions(
    string? Prompt,
    string OpenAiApiKey,
    string OpenAiModel,
    Uri OpenAiBaseUrl,
    int Port,
    bool Headless,
    bool SaveScreenshots,
    int MaxTurns,
    int ViewportWidth,
    int ViewportHeight)
{
    public ResponsesApiOptions ResponsesApiOptions =>
        ResponsesApiOptions.CreateDefault(
            OpenAiApiKey,
            OpenAiModel,
            OpenAiBaseUrl,
            ViewportWidth,
            ViewportHeight);

    public static AppOptions FromEnvironment(CliArgs cliArgs)
    {
        var prompt = cliArgs.Prompt;
        var apiKey = ReadString("OPENAI_API_KEY") ?? string.Empty;
        var model = ReadString("OPENAI_MODEL") ?? "computer-use-preview";
        var baseUrl = new Uri(ReadString("OPENAI_BASE_URL") ?? "https://api.openai.com/v1", UriKind.Absolute);
        var port = cliArgs.Port ?? ReadInt("DEMO_SITE_PORT") ?? 5050;
        var headless = cliArgs.Headless || ReadBool("DEMO_BROWSER_HEADLESS") == true;
        var saveScreenshots = !cliArgs.NoScreenshots && ReadBool("DEMO_SAVE_SCREENSHOTS") != false;
        var maxTurns = cliArgs.MaxTurns ?? ReadInt("DEMO_MAX_TURNS") ?? 20;

        return new AppOptions(
            prompt,
            apiKey,
            model,
            baseUrl,
            port,
            headless,
            saveScreenshots,
            maxTurns,
            1280,
            720);
    }

    private static string? ReadString(string variableName) =>
        Environment.GetEnvironmentVariable(variableName);

    private static int? ReadInt(string variableName)
    {
        var value = ReadString(variableName);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.Parse(value, CultureInfo.InvariantCulture);
    }

    private static bool? ReadBool(string variableName)
    {
        var value = ReadString(variableName);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return bool.Parse(value);
    }
}
