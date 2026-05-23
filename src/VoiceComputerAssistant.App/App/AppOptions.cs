using System.Globalization;
using Microsoft.Extensions.Configuration;
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
            OpenAiBaseUrl);

    public static AppOptions FromConfiguration(IConfiguration config, CliArgs cliArgs)
    {
        var prompt = cliArgs.Prompt;
        var apiKey = config["OpenAI:ApiKey"] ?? config["OPENAI_API_KEY"] ?? string.Empty;
        var model = config["OpenAI:Model"] ?? config["OPENAI_MODEL"] ?? "gpt-5.5";
        var baseUrl = new Uri(
            config["OpenAI:BaseUrl"] ??
            config["OPENAI_BASE_URL"] ??
            "https://api.openai.com/v1",
            UriKind.Absolute);
        var port = cliArgs.Port ?? ReadInt(config, "Demo:SitePort", "DEMO_SITE_PORT") ?? 5050;
        var headless = cliArgs.Headless || ReadBool(config, "Demo:BrowserHeadless", "DEMO_BROWSER_HEADLESS") == true;
        var saveScreenshots = !cliArgs.NoScreenshots &&
            ReadBool(config, "Demo:SaveScreenshots", "DEMO_SAVE_SCREENSHOTS") != false;
        var maxTurns = cliArgs.MaxTurns ?? ReadInt(config, "Demo:MaxTurns", "DEMO_MAX_TURNS") ?? 20;
        var viewportWidth = ReadInt(config, "Demo:ViewportWidth") ?? 1280;
        var viewportHeight = ReadInt(config, "Demo:ViewportHeight") ?? 720;

        return new AppOptions(
            prompt,
            apiKey,
            model,
            baseUrl,
            port,
            headless,
            saveScreenshots,
            maxTurns,
            viewportWidth,
            viewportHeight);
    }

    private static int? ReadInt(IConfiguration config, string key, string? fallbackKey = null)
    {
        var value = config[key] ?? (fallbackKey is null ? null : config[fallbackKey]);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.Parse(value, CultureInfo.InvariantCulture);
    }

    private static bool? ReadBool(IConfiguration config, string key, string? fallbackKey = null)
    {
        var value = config[key] ?? (fallbackKey is null ? null : config[fallbackKey]);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return bool.Parse(value);
    }
}
