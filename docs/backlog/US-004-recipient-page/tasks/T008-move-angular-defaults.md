# T008: Move Angular defaults into repository instructions

- Parent: [US-004](../README.md)
- Status: done
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

- [x] `web/CLAUDE.md` exists and states every decision listed above.
- [x] The harness new-application guide no longer prescribes this project's
      specific choices, and its links still resolve.
- [x] The harness validator and tool tests pass.

## Verification

Read both files against the story's agreed decisions. Run
`node tools/validate-harness.js` and the tool tests in AgenticEngineering.

## Progress

- 2026-11-18: Added `web/CLAUDE.md`. It links the shared agent instructions and
  the repository `CLAUDE.md`, and records the frontend decisions: Angular
  Material only, `/api` through `proxy.conf.json` to `http://localhost:5211`,
  signals without NgRx, hand-written typed API clients, no end-to-end tests, and
  tests that never call the real backend (`HttpTestingController`, major
  behaviors only).
- 2026-11-18: In AgenticEngineering, `new-application.md` now tells each
  application to record its component library, state approach, development
  backend connection and test scope in its own repository instructions. The
  Material, NgRx, `/api` proxy and end-to-end prescriptions became general
  choices, and the feature-folder examples no longer use this project's names.
  Committed on its own as `80199f3` on `feature/add-angular-harness`; not part
  of this story branch.
- 2026-11-18: Verification: `node tools/validate-harness.js` reports "Harness
  policy is valid." `node --test tools/harness-policy.test.js
  tools/skills.test.js` passes 25 of 25. Read both files against the story's
  scope and decisions. `npx prettier --write CLAUDE.md` in `web/` left the file
  unchanged. `npm run format:check` flags only `web/angular.json`, from the
  developer's earlier commit `b9b9c16`; it was not touched by this task.
- 2026-11-18: Refactoring assessment: the task changes instructions only, with
  no production or test code, so no refactoring applies.
- Remaining work: none.
