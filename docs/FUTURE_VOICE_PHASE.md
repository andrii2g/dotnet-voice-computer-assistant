# Future Voice Phase

Phase 2 should only add the front end of the pipeline:

```text
push-to-talk
  -> record wav
  -> speech-to-text
  -> existing PromptValidator
  -> existing ResponsesComputerAgent
```

Later phases:

- Phase 3: spoken summaries with TTS
- Phase 4: carefully allow-listed real targets such as GitHub issues or local docs
- Phase 5: confirmation UI for edits and submissions

Speech input must be treated exactly like typed input: validate it before sending it into the agent loop.
