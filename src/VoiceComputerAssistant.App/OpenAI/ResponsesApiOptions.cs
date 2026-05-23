namespace VoiceComputerAssistant.App.OpenAI;

public sealed record ResponsesApiOptions(
    string ApiKey,
    string Model,
    Uri BaseUrl)
{
    public static ResponsesApiOptions CreateDefault(
        string apiKey,
        string model,
        Uri baseUrl) =>
        new(
            apiKey,
            model,
            baseUrl);
}
