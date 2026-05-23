# Voice Computer Assistant Demo

Local-only .NET 10 Computer Use demo for a fake dashboard app.

## Safety

This repo is built as a constrained sandbox:

- browser traffic is restricted to `http://127.0.0.1:<port>/`
- external requests are aborted
- only read-only prompts are executed automatically
- edit, submit, credential, email, Git, and destructive prompts are blocked or deferred

Do not treat this project as a general-purpose desktop automation tool.

## Docs

- [Quickstart](docs/QUICKSTART.md)
- [Flow](docs/FLOW.md)
- [Safety](docs/SAFETY.md)
- [Future Voice Phase](docs/FUTURE_VOICE_PHASE.md)
