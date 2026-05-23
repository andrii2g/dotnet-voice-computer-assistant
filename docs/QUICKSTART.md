# Quickstart

## Prerequisites

- .NET 10 SDK
- PowerShell for Playwright browser install
- OpenAI API key

## Restore and Build

```powershell
$env:DOTNET_CLI_HOME="$PWD\\.dotnet-home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT="1"
$env:NUGET_PACKAGES="$PWD\\.nuget-packages"
dotnet restore .\dotnet-voice-computer-assistant.slnx
dotnet build .\dotnet-voice-computer-assistant.slnx /p:UseSharedCompilation=false
```

## Install Playwright Chromium

```powershell
pwsh .\src\VoiceComputerAssistant.App\bin\Debug\net10.0\playwright.ps1 install chromium
```

Linux users who need system browser dependencies may need:

```powershell
pwsh .\src\VoiceComputerAssistant.App\bin\Debug\net10.0\playwright.ps1 install --with-deps chromium
```

## Configuration

Configuration is read in this order:

1. `src/VoiceComputerAssistant.App/appsettings.json`
2. `src/VoiceComputerAssistant.App/appsettings.Local.json` if present
3. .NET user secrets via `AddUserSecrets<Program>()`
4. environment variables
5. CLI overrides such as `--port`, `--max-turns`, and `--headless`

Safe defaults belong in:

- [appsettings.json](../src/VoiceComputerAssistant.App/appsettings.json)
- [.env.example](../.env.example)

Secrets should not be committed.

## User Secrets

Set the API key locally:

```powershell
dotnet user-secrets set "OpenAI:ApiKey" "sk-..." --project .\src\VoiceComputerAssistant.App
dotnet user-secrets set "OpenAI:Model" "gpt-5.5" --project .\src\VoiceComputerAssistant.App
```

Inspect the local secret store:

```powershell
dotnet user-secrets list --project .\src\VoiceComputerAssistant.App
```

## Environment Variables

PowerShell:

```powershell
$env:OPENAI_API_KEY="sk-..."
$env:OPENAI_MODEL="gpt-5.5"
$env:OPENAI_BASE_URL="https://api.openai.com/v1"
```

Optional:

```powershell
$env:DEMO_SITE_PORT="5050"
$env:DEMO_BROWSER_HEADLESS="false"
$env:DEMO_SAVE_SCREENSHOTS="true"
$env:DEMO_MAX_TURNS="20"
```

## Run

Prompt mode:

```powershell
dotnet run --project .\src\VoiceComputerAssistant.App -- --prompt "Filter the local dashboard by dotnet, open details for dotnet-voice-computer-assistant, and summarize what is visible."
```

Interactive mode:

```powershell
dotnet run --project .\src\VoiceComputerAssistant.App
```

Example prompts:

- `Filter the local dashboard by dotnet, open details for dotnet-voice-computer-assistant, and summarize what is visible.`
- `Search for greenhouse, open the matching project details, and summarize the idea.`
- `Sort projects by most recently updated and tell me the first three visible project names.`

## Troubleshooting

- Missing `OPENAI_API_KEY`:
  The app exits before starting the browser.
- Playwright browsers not installed:
  Run the Chromium install command after build.
- Port already in use:
  Set `DEMO_SITE_PORT` or pass `--port`.
- Model unavailable:
  Override `OpenAI:Model` or `OPENAI_MODEL`.
- API error:
  Inspect `artifacts/runs/<timestamp>/`.
- Browser blocked external navigation:
  This is expected in Phase 1.

## Tests

```powershell
$env:DOTNET_CLI_HOME="$PWD\\.dotnet-home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT="1"
$env:NUGET_PACKAGES="$PWD\\.nuget-packages"
dotnet test .\tests\VoiceComputerAssistant.Tests\VoiceComputerAssistant.Tests.csproj --no-restore /p:UseSharedCompilation=false
```
