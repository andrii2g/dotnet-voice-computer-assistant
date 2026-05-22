using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace VoiceComputerAssistant.App.OpenAI;

public sealed class ResponsesApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ResponsesApiOptions _options;

    public ResponsesApiClient(ResponsesApiOptions options, HttpClient? httpClient = null)
    {
        _options = options;
        _httpClient = httpClient ?? new HttpClient();
    }

    public Task<ResponsesApiResponse> CreateInitialResponseAsync(
        string prompt,
        string instructions,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = _options.Model,
            tools = new[]
            {
                CreateComputerTool()
            },
            instructions,
            input = prompt
        };

        return SendAsync(payload, cancellationToken);
    }

    public Task<ResponsesApiResponse> SendComputerCallOutputAsync(
        string previousResponseId,
        string callId,
        string screenshotDataUrl,
        IReadOnlyList<object>? acknowledgedSafetyChecks,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = _options.Model,
            tools = new[]
            {
                CreateComputerTool()
            },
            previous_response_id = previousResponseId,
            input = new object[]
            {
                new
                {
                    type = "computer_call_output",
                    call_id = callId,
                    acknowledged_safety_checks = acknowledgedSafetyChecks ?? Array.Empty<object>(),
                    output = new
                    {
                        type = "computer_screenshot",
                        image_url = screenshotDataUrl
                    }
                }
            }
        };

        return SendAsync(payload, cancellationToken);
    }

    private async Task<ResponsesApiResponse> SendAsync(object payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildResponsesEndpoint());
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        return ParseResponse(rawJson);
    }

    private Uri BuildResponsesEndpoint() =>
        new(_options.BaseUrl, "responses");

    private object CreateComputerTool() =>
        new
        {
            type = "computer_use_preview",
            display_width = _options.DisplayWidth,
            display_height = _options.DisplayHeight,
            environment = _options.Environment
        };

    private static ResponsesApiResponse ParseResponse(string rawJson)
    {
        using var document = JsonDocument.Parse(rawJson);
        var root = document.RootElement.Clone();

        if (!TryGetRequiredString(root, "id", out var id))
        {
            throw new InvalidOperationException("Responses API response is missing the required id field.");
        }

        return new ResponsesApiResponse(id!, rawJson, root);
    }

    private static bool TryGetRequiredString(JsonElement element, string propertyName, out string? value)
    {
        value = null;

        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();
        return !string.IsNullOrWhiteSpace(value);
    }
}
