# US-004: Recipient page

- Source: DeliveryNoteService/docs/backlog/US-004-recipient-page.md
- Refinement source: conversation: US-004 grilling session, 2026-11-17
- Lifecycle: active
- Created: 2026-11-17
- Updated: 2026-11-18
- Branch: feature/US-004-recipient-page

## Approach

No Angular code exists yet, so T001 is a prerequisite: it creates the `web/`
workspace with the agreed toolchain (angular-eslint, Prettier, Vitest with
Testing Library, Angular Material, `/api` proxy) and an empty `/recipients` start
page. Bundling the toolchain with the first feature would put several new
foundations into one prompt, so it is split out; it unlocks every later task.

T002 is the first working increment: the page lists recipients from
`GET /api/recipients`, newest first, with an empty state. Paging controls (T003)
and loading/error handling with the shared error dialog (T004) extend the list.
Creation follows as its own slice (T005), with backend creation errors routed
through the dialog afterwards (T006). Moving the project-specific Angular
defaults into repository instructions (T008) is independent once the workspace
exists. CI (T007) was cancelled on 2026-11-17 by the developer's decision.

## Ordered tasks

| Order | Task | Status | Depends on | Increment |
| --- | --- | --- | --- | --- |
| 1 | [T001: Scaffold the Angular workspace](tasks/T001-scaffold-angular-workspace.md) | done | None | Prerequisite |
| 2 | [T002: List recipients newest first](tasks/T002-list-recipients.md) | done | T001 | Skeleton |
| 3 | [T003: Page through recipients](tasks/T003-page-through-recipients.md) | done | T002 | Extension |
| 4 | [T004: Handle list loading and errors](tasks/T004-handle-list-loading-and-errors.md) | done | T002 | Extension |
| 5 | [T005: Create a recipient](tasks/T005-create-recipient.md) | done | T002 | Extension |
| 6 | [T006: Show creation errors in the dialog](tasks/T006-show-creation-errors.md) | done | T004, T005 | Edge case |
| 7 | [T007: Add frontend CI workflow](tasks/T007-add-frontend-ci.md) | cancelled | T001 | Prerequisite |
| 8 | [T008: Move Angular defaults into repository instructions](tasks/T008-move-angular-defaults.md) | done | T001 | Prerequisite |

The task file owns its status and progress. Keep the status above synchronized
with the task file.

T008 can proceed in parallel with T002–T006. T003, T004, and T005 depend
only on T002, but all three change the recipient page component, so run them one
after another to avoid conflicting edits.

## Acceptance coverage

| Source criterion | Tasks |
| --- | --- |
| AC1: list table with all recipient columns | T002 |
| AC2: 10 per page, newest first, previous/next from paging metadata | T002 (order), T003 (controls) |
| AC3: empty-state message | T002 |
| AC4: loading indicator; load failure dialog, message and Retry | T004 |
| AC5: six labelled required inputs, free-text country | T005 |
| AC6: inline requiredness and max-length validation blocks submit | T005 |
| AC7: name, ZIP, city, address sent unchanged | T005 |
| AC8: one POST; submit disabled while in progress | T005 |
| AC9: snackbar, form cleared, list reloaded on page 1 | T005 |
| AC10: backend errors in modal dialog, raw code shown, values kept | T004 (dialog, list), T006 (creation) |
| AC11: error-code message mapping | T004 (mapping), T006 (creation cases) |
| AC12: same dialog for list-loading failures | T004 |
| AC13: relative `/api` path and dev-server proxy | T001, T002 |
| AC14: `web/` README commands succeed | T001 |
| Scope: Angular defaults moved into repository instructions | T008 |
| Test boundary tests 1–2 | T002 |
| Test boundary test 3, 5 | T005 |
| Test boundary test 4 | T006 |
| Test boundary test 6 | T003 |

## External prerequisites

- US-001 `POST /api/recipients` and `GET /api/recipients`: Done and merged in
  master. Verified in `service/src/WebApp/RestApi/RecipientEndpoints.cs`.
- Node.js 24.21.0 and npm 11.19.0 are installed locally (checked 2026-11-17).

## Assumptions and open questions

No open questions. Agreed decisions are recorded in the source story.

Verified backend contract (camelCase JSON):

- List: `GET /api/recipients?page={n}&limit=10&sortBy=Id&sortDir=Desc` returns
  `{ content: RecipientDetails[], paging: { page, limit, totalCount, totalPages } }`.
- Create: `POST /api/recipients` with `{ code, name, country, zip, city, address }`
  returns 201 with `{ content: RecipientDetails }`.
- `RecipientDetails`: `id, code, name, country, zip, city, address, createdAt,
  isForeign`.
- Errors: `{ errorCode, data, correlationId }`; field errors put the field name in
  `data.Field`.
- Local backend HTTP URL: `http://localhost:5211` (`WebApp` launch settings), the
  proxy target.

T008 changes the harness guide in the separate AgenticEngineering repository. That
change is committed there on its own and is not part of this story branch.

## Resume notes

- Reservation: attempt `b406b863-a795-40e7-8d49-0dff1954fca3`, remote `origin`,
  branch `feature/US-004-recipient-page`, marker
  `6ebc678753b5fbfbebdaa5effdf3bb092411e6de`.
- Review history: [review 01](review-01.md) (`85d3209`, 2026-11-18) returned FAIL
  with complete coverage. F001 (blocking, first reported in review 01):
  `web/angular.json` failed `npm run format:check`, so AC14 was not met. F002
  (non-blocking) is a test-strength note and was not corrected.
- Correction 1 (2026-11-18): attempted F001 in `1a5986d`. Prettier reformatted
  `web/angular.json`, keeping `"analytics": false`; recorded in T001.
  `npm run format:check`, `npm run lint`, `npm test` (13 tests) and
  `npm run build` pass. Unsuccessful fixes for F001 so far: none. A follow-up
  review is next.
