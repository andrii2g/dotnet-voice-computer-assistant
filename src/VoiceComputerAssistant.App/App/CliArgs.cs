namespace VoiceComputerAssistant.App.App;

public sealed record CliArgs(
    string? Prompt,
    bool Headless,
    int? Port,
    int? MaxTurns,
    bool NoScreenshots,
    bool ShowHelp)
{
    public const string HelpText =
        """
        Usage:
          --prompt "..."          Prompt to execute.
          --headless              Run browser headless.
          --port 5050             Local demo site port.
          --max-turns 20          Maximum Computer Use loop turns.
          --no-screenshots        Do not save screenshots to artifacts.
          --help                  Print help.
        """;

    public static CliArgs Parse(string[] args)
    {
        string? prompt = null;
        var headless = false;
        int? port = null;
        int? maxTurns = null;
        var noScreenshots = false;
        var showHelp = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];

            switch (arg)
            {
                case "--prompt":
                    prompt = ReadValue(args, ref index, arg);
                    break;
                case "--port":
                    port = int.Parse(ReadValue(args, ref index, arg));
                    break;
                case "--max-turns":
                    maxTurns = int.Parse(ReadValue(args, ref index, arg));
                    break;
                case "--headless":
                    headless = true;
                    break;
                case "--no-screenshots":
                    noScreenshots = true;
                    break;
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {arg}");
            }
        }

        return new CliArgs(prompt, headless, port, maxTurns, noScreenshots, showHelp);
    }

    private static string ReadValue(IReadOnlyList<string> args, ref int index, string optionName)
    {
        if (index + 1 >= args.Count)
        {
            throw new ArgumentException($"Missing value for {optionName}");
        }

        index++;
        return args[index];
    }
}
