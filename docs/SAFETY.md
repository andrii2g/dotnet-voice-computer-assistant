# Safety

Phase 1 is intentionally local-only.

## Why

- Computer Use is still the highest-risk part of the workflow.
- Real sites, credentials, and side effects should not be trusted until the loop is stable.
- A local dashboard provides deterministic UI, bounded risk, and debuggable artifacts.

## Current Constraints

- only `http://127.0.0.1:<port>/` is allowed
- external browser requests are aborted
- safe read-only prompts are executed automatically
- edit, submit, email, credential, Git, and destructive prompts are blocked or deferred
- pending API safety checks stop the run by default

## Before Real Targets

The following must change before this project can safely control anything real:

- target allow-lists beyond the local demo page
- explicit human confirmation before edits or submissions
- richer policy around safety checks
- stronger auditing and run review
- better handling for authenticated contexts
