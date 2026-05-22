namespace VoiceComputerAssistant.App.Safety;

public sealed record PromptValidationResult(
    PromptRiskLevel RiskLevel,
    string Reason,
    string SanitizedPrompt)
{
    public bool CanExecuteAutomatically => RiskLevel == PromptRiskLevel.SafeReadOnly;
}
