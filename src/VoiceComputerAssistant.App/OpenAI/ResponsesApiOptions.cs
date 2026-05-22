namespace VoiceComputerAssistant.App.OpenAI;

public sealed record ResponsesApiOptions(
    string ApiKey,
    string Model,
    Uri BaseUrl,
    int DisplayWidth,
    int DisplayHeight,
    string Environment)
{
    public static ResponsesApiOptions CreateDefault(
        string apiKey,
        string model,
        Uri baseUrl,
        int displayWidth,
        int displayHeight) =>
        new(
            apiKey,
            model,
            baseUrl,
            displayWidth,
            displayHeight,
            "browser");
}
