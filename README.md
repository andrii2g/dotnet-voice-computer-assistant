# Voice Computer Assistant Demo

`dotnet-voice-computer-assistant` is a .NET 10 prompt-driven Computer Use demo. Phase 1 is intentionally not voice-based yet: it proves the local browser control loop first, then voice capture and speech-to-text can be added on top of that stable foundation.

The app starts a safe local dashboard, opens Chromium with Playwright, validates the prompt, sends the task to the OpenAI Responses API with the Computer Use tool, executes returned actions locally, sends screenshots back, and prints the final text summary.

## Safety

This repository is designed as a local sandbox demo:

- the browser is restricted to `http://127.0.0.1:<port>/`
- external requests are aborted
- only read-only prompts are executed automatically
- edit, submit, credential, email, Git, and destructive requests are blocked or require future confirmation flows

Do not treat this Phase 1 app as a general-purpose desktop automation tool.

## Why Phase 1 Skips Speech

Speech-to-text is not the hard part of this project. The high-risk, high-uncertainty part is the Computer Use loop itself:

`prompt -> validation -> model action -> local execution -> screenshot -> follow-up response`

This repo validates that loop first so later voice work can reuse the same safety and execution path.

## Prerequisites

- .NET 10 SDK
- PowerShell for Playwright browser install
- OpenAI API key

## Setup

Restore and build:

```powershell
$env:DOTNET_CLI_HOME="$PWD\\.dotnet-home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT="1"
$env:NUGET_PACKAGES="$PWD\\.nuget-packages"
dotnet restore .\dotnet-voice-computer-assistant.slnx
dotnet build .\dotnet-voice-computer-assistant.slnx
```

Install the Playwright Chromium browser after build:

```powershell
pwsh .\src\VoiceComputerAssistant.App\bin\Debug\net10.0\playwright.ps1 install chromium
```

Linux users who need extra browser dependencies may need:

```powershell
pwsh .\src\VoiceComputerAssistant.App\bin\Debug\net10.0\playwright.ps1 install --with-deps chromium
```

## Configuration

The app reads configuration in this order:

1. `src/VoiceComputerAssistant.App/appsettings.json`
2. .NET user secrets via `AddUserSecrets<Program>()`
3. environment variables
4. CLI overrides such as `--port`, `--max-turns`, and `--headless`

`appsettings.json` is intended for safe defaults already checked into the repo. Secrets should go into user secrets or environment variables, not the repo.

### User Secrets

Initialize the secret values locally:

```powershell
dotnet user-secrets set "OpenAI:ApiKey" "sk-..." --project .\src\VoiceComputerAssistant.App
dotnet user-secrets set "OpenAI:Model" "gpt-5.5" --project .\src\VoiceComputerAssistant.App
```

You can inspect the local secret store with:

```powershell
dotnet user-secrets list --project .\src\VoiceComputerAssistant.App
```

### Environment Variables

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

See [.env.example](src/VoiceComputerAssistant.App/appsettings.json) for the full configuration shape.

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
  Run the `playwright.ps1 install chromium` command after build.
- Port already in use:
  Set `DEMO_SITE_PORT` or pass `--port`.
- Model unavailable:
  Override `OPENAI_MODEL` with a model/tool combination enabled for your account.
- API error:
  Check the saved raw response JSON in `artifacts/runs/<timestamp>/`.
- Browser blocked external navigation:
  This is expected in Phase 1. The browser is intentionally local-only.

## Tests

```powershell
$env:DOTNET_CLI_HOME="$PWD\\.dotnet-home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT="1"
$env:NUGET_PACKAGES="$PWD\\.nuget-packages"
dotnet test .\dotnet-voice-computer-assistant.slnx
```

## Future Phases

- Phase 2: push-to-talk audio capture and speech-to-text
- Phase 3: text-to-speech responses
- Phase 4: carefully allow-listed real targets
- Phase 5: human confirmation UI for edits and submissions
