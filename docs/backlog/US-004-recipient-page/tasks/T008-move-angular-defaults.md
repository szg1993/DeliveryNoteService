# T008: Move Angular defaults into repository instructions

- Parent: [US-004](../README.md)
- Status: todo
- Depends on: T001
- Increment: Prerequisite

## Outcome

This project's frontend choices live in this repository's instructions, and the
central harness guide keeps only general Angular setup rules.

## Scope

- Add `web/CLAUDE.md` that references the shared agent instructions and records
  the frontend decisions: Angular Material as the only component library, the
  `/api` dev proxy to the local backend, signals without NgRx, hand-written typed
  API clients, no end-to-end tests, and frontend tests that never call the real
  backend (mock HTTP with `HttpTestingController`, cover major behaviors only).
  Keep it short and consistent with the root `CLAUDE.md` simplicity preferences.
- In the separate AgenticEngineering repository, edit
  `agent-harness/technologies/angular/new-application.md` so that the
  project-specific defaults become general guidance or are removed, as agreed in
  the story. Commit that change in AgenticEngineering on its own; do not include it
  in this story branch.
- Run the harness validator and tool tests in AgenticEngineering after the edit.

## Acceptance criteria

- [ ] `web/CLAUDE.md` exists and states every decision listed above.
- [ ] The harness new-application guide no longer prescribes this project's
      specific choices, and its links still resolve.
- [ ] The harness validator and tool tests pass.

## Verification

Read both files against the story's agreed decisions. Run
`node tools/validate-harness.js` and the tool tests in AgenticEngineering.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
