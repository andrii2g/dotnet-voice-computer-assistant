using VoiceComputerAssistant.App.Browser;

namespace VoiceComputerAssistant.Tests;

public sealed class KeyNormalizerTests
{
    [Theory]
    [InlineData("CTRL", "Control")]
    [InlineData("CONTROL", "Control")]
    [InlineData("CMD", "Meta")]
    [InlineData("META", "Meta")]
    [InlineData("ESC", "Escape")]
    [InlineData("ARROWLEFT", "ArrowLeft")]
    [InlineData("Enter", "Enter")]
    public void Normalizes_Expected_Key_Values(string input, string expected)
    {
        var actual = KeyNormalizer.Normalize(input);

        Assert.Equal(expected, actual);
    }
}
