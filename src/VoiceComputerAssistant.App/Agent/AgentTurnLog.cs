namespace VoiceComputerAssistant.App.Agent;

public sealed record AgentTurnLog(
    int TurnIndex,
    string EventType,
    string Message);
