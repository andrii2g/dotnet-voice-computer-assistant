namespace VoiceComputerAssistant.App.App;

public static class RepoPaths
{
    public static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var hasPlan = File.Exists(Path.Combine(current.FullName, "PLAN.md"));
            var hasSolution = File.Exists(Path.Combine(current.FullName, "dotnet-voice-computer-assistant.slnx"));

            if (hasPlan || hasSolution)
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
