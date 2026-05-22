using System.Text.Json;

namespace VoiceComputerAssistant.App.OpenAI;

public sealed record ResponsesApiResponse(
    string Id,
    string RawJson,
    JsonElement Root);

public sealed record ComputerCallOutput(
    string CallId,
    IReadOnlyList<ComputerAction> Actions);

public sealed record ComputerAction(
    string Type,
    int? X,
    int? Y,
    int? ScrollX,
    int? ScrollY,
    string? Button,
    string? Text,
    IReadOnlyList<string> Keys,
    IReadOnlyList<ComputerPoint> Path,
    JsonElement Raw);

public sealed record ComputerPoint(int X, int Y);
