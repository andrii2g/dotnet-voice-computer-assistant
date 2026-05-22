using VoiceComputerAssistant.App.App;

var cliArgs = CliArgs.Parse(args);

if (cliArgs.ShowHelp)
{
    Console.WriteLine(CliArgs.HelpText);
    return 0;
}

var options = AppOptions.FromEnvironment(cliArgs);

Console.WriteLine("Voice Computer Assistant Demo");
Console.WriteLine($"Model: {options.OpenAiModel}");
Console.WriteLine($"Port: {options.Port}");
Console.WriteLine($"Headless: {options.Headless}");
Console.WriteLine($"Save screenshots: {options.SaveScreenshots}");
Console.WriteLine($"Max turns: {options.MaxTurns}");

if (!string.IsNullOrWhiteSpace(options.Prompt))
{
    Console.WriteLine("Prompt captured from CLI.");
}
else
{
    Console.WriteLine("No prompt provided yet. Execution flow will be added in a later step.");
}

return 0;
