using System.Text.Json;

namespace VoiceComputerAssistant.App.OpenAI;

public sealed class ResponseOutputParser
{
    public ComputerCallOutput? FindComputerCall(ResponsesApiResponse response)
    {
        if (!response.Root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var item in output.EnumerateArray())
        {
            if (!PropertyEquals(item, "type", "computer_call"))
            {
                continue;
            }

            if (!TryGetString(item, "call_id", out var callId) &&
                !TryGetString(item, "callId", out callId))
            {
                throw new InvalidOperationException("computer_call item is missing call_id.");
            }

            var actions = ParseActions(item);
            return new ComputerCallOutput(callId!, actions);
        }

        return null;
    }

    public string ExtractFinalText(ResponsesApiResponse response)
    {
        if (TryGetOutputText(response.Root, out var outputText))
        {
            return outputText!;
        }

        if (!response.Root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        foreach (var item in output.EnumerateArray())
        {
            if (!PropertyEquals(item, "type", "message"))
            {
                continue;
            }

            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (TryGetString(contentItem, "text", out var text))
                {
                    return text!;
                }
            }
        }

        return string.Empty;
    }

    private static IReadOnlyList<ComputerAction> ParseActions(JsonElement computerCall)
    {
        if (computerCall.TryGetProperty("actions", out var actions) && actions.ValueKind == JsonValueKind.Array)
        {
            return actions.EnumerateArray().Select(ParseAction).ToArray();
        }

        if (computerCall.TryGetProperty("action", out var singleAction) && singleAction.ValueKind == JsonValueKind.Object)
        {
            return [ParseAction(singleAction)];
        }

        return Array.Empty<ComputerAction>();
    }

    private static ComputerAction ParseAction(JsonElement action)
    {
        var type = GetRequiredString(action, "type");

        return new ComputerAction(
            type,
            TryGetInt(action, "x"),
            TryGetInt(action, "y"),
            TryGetInt(action, "scrollX") ?? TryGetInt(action, "scroll_x"),
            TryGetInt(action, "scrollY") ?? TryGetInt(action, "scroll_y"),
            TryGetStringValue(action, "button"),
            TryGetStringValue(action, "text"),
            ParseKeys(action),
            ParsePath(action),
            action.Clone());
    }

    private static IReadOnlyList<string> ParseKeys(JsonElement action)
    {
        if (!action.TryGetProperty("keys", out var keys) || keys.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return keys
            .EnumerateArray()
            .Where(key => key.ValueKind == JsonValueKind.String)
            .Select(key => key.GetString()!)
            .ToArray();
    }

    private static IReadOnlyList<ComputerPoint> ParsePath(JsonElement action)
    {
        if (!action.TryGetProperty("path", out var path) || path.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ComputerPoint>();
        }

        var points = new List<ComputerPoint>();
        foreach (var point in path.EnumerateArray())
        {
            points.Add(new ComputerPoint(
                TryGetInt(point, "x") ?? 0,
                TryGetInt(point, "y") ?? 0));
        }

        return points;
    }

    private static bool TryGetOutputText(JsonElement root, out string? text)
    {
        text = null;

        if (!root.TryGetProperty("output_text", out var outputText))
        {
            return false;
        }

        switch (outputText.ValueKind)
        {
            case JsonValueKind.String:
                text = outputText.GetString();
                return !string.IsNullOrWhiteSpace(text);
            case JsonValueKind.Array:
            {
                var builder = outputText
                    .EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.String)
                    .Select(item => item.GetString())
                    .Where(item => !string.IsNullOrWhiteSpace(item));
                text = string.Join(Environment.NewLine, builder!);
                return !string.IsNullOrWhiteSpace(text);
            }
            default:
                return false;
        }
    }

    private static bool PropertyEquals(JsonElement element, string propertyName, string expectedValue) =>
        TryGetString(element, propertyName, out var actualValue) &&
        string.Equals(actualValue, expectedValue, StringComparison.Ordinal);

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        if (!TryGetString(element, propertyName, out var value))
        {
            throw new InvalidOperationException($"Missing required string property: {propertyName}.");
        }

        return value!;
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string? value)
    {
        value = null;

        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string? TryGetStringValue(JsonElement element, string propertyName) =>
        TryGetString(element, propertyName, out var value) ? value : null;

    private static int? TryGetInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return property.GetInt32();
    }
}
