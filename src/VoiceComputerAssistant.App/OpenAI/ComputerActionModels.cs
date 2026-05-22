using System.Text.Json;

namespace VoiceComputerAssistant.App.OpenAI;

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
