namespace VoiceComputerAssistant.App.Agent;

public sealed record AgentRunResult(
    bool Success,
    string FinalText,
    string? ErrorMessage);
