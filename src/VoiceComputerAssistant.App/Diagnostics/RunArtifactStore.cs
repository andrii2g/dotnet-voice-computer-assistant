using System.Text.Json;

namespace VoiceComputerAssistant.App.Diagnostics;

public sealed class RunArtifactStore
{
    private readonly bool _saveScreenshots;
    private readonly string _runLogPath;

    private RunArtifactStore(string runDirectory, bool saveScreenshots)
    {
        RunDirectory = runDirectory;
        _saveScreenshots = saveScreenshots;
        _runLogPath = Path.Combine(runDirectory, "run-log.jsonl");
    }

    public string RunDirectory { get; }

    public static RunArtifactStore Create(string repoRoot, bool saveScreenshots)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var runDirectory = Path.Combine(repoRoot, "artifacts", "runs", timestamp);
        Directory.CreateDirectory(runDirectory);
        return new RunArtifactStore(runDirectory, saveScreenshots);
    }

    public Task SavePromptAsync(string prompt, CancellationToken cancellationToken) =>
        WriteAllTextAsync("prompt.txt", prompt, cancellationToken);

    public Task SaveValidationAsync(string validationJson, CancellationToken cancellationToken) =>
        WriteAllTextAsync("validation.json", validationJson, cancellationToken);

    public Task SaveResponseAsync(int turnIndex, string rawJson, CancellationToken cancellationToken) =>
        WriteAllTextAsync($"response-{turnIndex:000}.json", rawJson, cancellationToken);

    public Task SaveScreenshotAsync(int turnIndex, byte[] pngBytes, CancellationToken cancellationToken)
    {
        if (!_saveScreenshots)
        {
            return Task.CompletedTask;
        }

        var path = Path.Combine(RunDirectory, $"screenshot-{turnIndex:000}.png");
        return File.WriteAllBytesAsync(path, pngBytes, cancellationToken);
    }

    public Task SaveFinalTextAsync(string finalText, CancellationToken cancellationToken) =>
        WriteAllTextAsync("final.txt", finalText, cancellationToken);

    public Task AppendRunLogAsync(object entry, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(entry);
        return File.AppendAllTextAsync(_runLogPath, json + Environment.NewLine, cancellationToken);
    }

    private Task WriteAllTextAsync(string fileName, string content, CancellationToken cancellationToken)
    {
        var path = Path.Combine(RunDirectory, fileName);
        return File.WriteAllTextAsync(path, content, cancellationToken);
    }
}
