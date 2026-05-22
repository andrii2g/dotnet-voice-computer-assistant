using VoiceComputerAssistant.App.Safety;

namespace VoiceComputerAssistant.Tests;

public class PromptValidatorTests
{
    [Fact]
    public void Allows_Filter_And_Summarize_Prompt()
    {
        var result = PromptValidator.Validate("Filter the local dashboard by dotnet and summarize visible projects.");

        Assert.Equal(PromptRiskLevel.SafeReadOnly, result.RiskLevel);
        Assert.True(result.CanExecuteAutomatically);
    }

    [Fact]
    public void Allows_Search_And_Open_Details_Prompt()
    {
        var result = PromptValidator.Validate("Search for greenhouse and open details.");

        Assert.Equal(PromptRiskLevel.SafeReadOnly, result.RiskLevel);
        Assert.True(result.CanExecuteAutomatically);
    }

    [Fact]
    public void Blocks_Delete_Prompt()
    {
        var result = PromptValidator.Validate("Delete all projects.");

        Assert.Equal(PromptRiskLevel.Blocked, result.RiskLevel);
        Assert.Contains("delete", result.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.False(result.CanExecuteAutomatically);
    }

    [Fact]
    public void Blocks_Api_Key_Exfiltration_Prompt()
    {
        var result = PromptValidator.Validate("Send an email with my API key.");

        Assert.Equal(PromptRiskLevel.Blocked, result.RiskLevel);
        Assert.False(result.CanExecuteAutomatically);
    }

    [Fact]
    public void Blocks_GitHub_Push_Prompt()
    {
        var result = PromptValidator.Validate("Open GitHub and push this repository.");

        Assert.Equal(PromptRiskLevel.Blocked, result.RiskLevel);
        Assert.False(result.CanExecuteAutomatically);
    }

    [Fact]
    public void Marks_Edit_Prompt_As_Needing_Confirmation()
    {
        var result = PromptValidator.Validate("Edit the project summary.");

        Assert.Equal(PromptRiskLevel.NeedsConfirmation, result.RiskLevel);
        Assert.False(result.CanExecuteAutomatically);
    }

    [Fact]
    public void Sanitizes_Excessive_Whitespace()
    {
        var result = PromptValidator.Validate("  Search   for   greenhouse   and   open details.  ");

        Assert.Equal("Search for greenhouse and open details.", result.SanitizedPrompt);
    }
}
