# US-006: Delivery note pages

- Source: DeliveryNoteService/docs/backlog/US-006-delivery-note-pages.md
- Refinement source: conversation: US-006 grilling sessions, 2026-11-18
- Lifecycle: active
- Created: 2026-11-18
- Updated: 2026-11-18
- Branch: feature/US-006-delivery-note-pages

## Approach

The Angular workspace, Material, the shared error dialog, and the recipient page
patterns exist from US-004, so no prerequisite task is needed.

T001 is the first working increment: the start route opens a lazy-loaded
`/delivery-notes` page that lists notes newest first from `GET /api/delivery-notes`,
with an empty state. Toolbar navigation (T002) and the list's paging, loading, and
error recovery (T003) extend the list. Creation is its own slice (T004): a complete
form that posts a valid note and returns to the list. Client-side numeric and
date-order checks (T005), recipient-loading edge cases (T006), and backend creation
errors in the dialog (T007) then harden the form.

T004 is the largest task. It stays one task because its single outcome, creating a
note, needs every form section, the lines, and date formatting before agreed test 4
can pass; splitting it would leave no demonstrable intermediate.

Run the tasks in order. T002 and T003 depend only on T001, and T005–T007 only on
T004, but each group edits the same files (`app`/list page, and the creation page),
so parallel work would conflict.

## Ordered tasks

| Order | Task | Status | Depends on | Increment |
| --- | --- | --- | --- | --- |
| 1 | [T001: List delivery notes newest first](tasks/T001-list-delivery-notes.md) | done | None | Skeleton |
| 2 | [T002: Navigate with toolbar links](tasks/T002-toolbar-navigation.md) | done | T001 | Extension |
| 3 | [T003: Page through notes and recover from load failures](tasks/T003-list-paging-and-errors.md) | done | T001 | Extension |
| 4 | [T004: Create a delivery note](tasks/T004-create-delivery-note.md) | done | T001 | Extension |
| 5 | [T005: Check numbers and date order before submitting](tasks/T005-numeric-and-date-checks.md) | done | T004 | Edge case |
| 6 | [T006: Handle missing or unavailable recipients](tasks/T006-recipient-edge-cases.md) | done | T004 | Edge case |
| 7 | [T007: Show creation errors in the dialog](tasks/T007-creation-errors.md) | done | T004 | Edge case |

The task file owns its status and progress. Keep the status above synchronized
with the task file.

## Acceptance coverage

| Source criterion | Tasks |
| --- | --- |
| AC1: routes, empty-path redirect, lazy loading, titles | T001 (`/delivery-notes`, redirect), T004 (`/delivery-notes/new`) |
| AC2: toolbar links with current-page highlight | T002 |
| AC3: newest first, 10 per page, previous/next from paging metadata | T001 (request), T003 (controls) |
| AC4: table columns, status text, "Urgent" chip, `yyyy-MM-dd` dates | T001 |
| AC5: empty-state message | T001 |
| AC6: loading indicator; error dialog then message with Retry | T003 |
| AC7: "New delivery note" button; rows not clickable | T004 |
| AC8: one page, titled sections, agreed labels | T004 |
| AC9: recipient select "CODE – Name" from `Limit=0` | T004 |
| AC9: no-recipients hint; load failure keeps submit disabled | T006 |
| AC10: lines start with one, add, remove disabled at one, order kept | T004 |
| AC11: free-text units and package scale | T004 |
| AC12: required fields and maximum lengths | T004 |
| AC12: quantities > 0, weights/dimensions ≥ 0, date order | T005 |
| AC13: number inputs as JSON numbers; native datepicker, `yyyy-MM-dd` | T004 |
| AC13: "Enter a valid date." for unparseable typed text | T005 |
| AC14: text untrimmed; empty optional text sent as `null` | T004 |
| AC15: one POST; submit disabled while in progress | T004 |
| AC16: snackbar with NoteNbr, navigate to list | T004 |
| AC17: Cancel returns to list without warning | T004 |
| AC18: every backend/network error opens the dialog; values kept | T003 (list), T006 (recipients), T007 (creation) |
| AC19: error-message mappings | T007 |
| Test boundary: updated redirect test, tests 1–2 | T001 |
| Test boundary: test 3 | T002 |
| Test boundary: tests 4–5 | T004 |
| Test boundary: test 6 | T005 |
| Test boundary: test 7 | T007 |

## External prerequisites

- US-005 `GET /api/delivery-notes`: Done and merged in master (`25e44a1`). Tests mock
  it; the real backend is needed only for manual checks.
- US-002 `POST /api/delivery-notes` and US-001 `GET /api/recipients`: Done and merged.
- No backend changes in this story.

## Assumptions and open questions

No open questions. Agreed decisions are recorded in the source story.

Verified facts (2026-11-18):

- `web/src/app/` has the US-004 patterns to reuse: `RecipientsApi` (`@Service()`,
  `PagedResponse<T>`, `Paging`, lowercase query names such as `sortDir`, which the
  backend binds case-insensitively), `RecipientsPage` (`rxResource`, loading bar,
  error dialog then Retry, paging nav), `CreateRecipientForm` (Signal Forms from
  `@angular/forms/signals` with `form`, `required`, `maxLength`, `submission`),
  `openErrorDialog`, and `readableErrorMessage` in `shared/error-message.ts`.
- `app.routes.spec.ts` currently expects `/` to redirect to `/recipients`; T001
  updates it.
- The backend returns camelCase JSON. The status is a string in `noteStatusId`
  (`JsonStringEnumConverter`); the recipient is nested in `recipient`; dates in
  `deadline` are `yyyy-MM-dd`.
- Creation request shape (`CreateDeliveryNoteDto`): header fields `ekaerNbr`,
  `takeoverPlace`, `category`, `comments`, flags `isUrgent`, `isCrackable`,
  `isDangerous`, `isOwnCost`, `recipientId`, and nested `contactInfo`
  (`assignTo`, `assignToPhone`, `contact`, `contactPhone`), `packageDetail`
  (`tareWgt`, `tareWgtUm`, `pkgQty`, `pkgScale`, `pkgSizeX/Y/Z`, `pkgSizeUm`),
  `deadline` (`takeoverDate`, `shipDate`, `estimatedArrivalDate`), and `noteLines`
  (`partCode`, `partDesc`, `partCmt`, `partQty`, `partQtyUm`, `partWgt`,
  `partWgtUm`). Unknown properties are rejected, so send no extra fields.
- Maximum lengths (`NoteLimits`): EKAER 10, takeover place 30, category 20,
  comments 200, assigned to 200, assignee phone 30, contact person 200, contact
  phone 30, tare weight unit 10, package scale 10, size unit 10, part code 100,
  description 200, line comment 200, quantity unit 10, weight unit 10.
- Error codes: `Note.InvalidInput`, `Note.NumberExhausted`, `Recipient.NotFound`
  (404); `FieldMissing`/`FieldTooLong` carry the unqualified property name in
  `data.Field`.

To verify during T004: how Signal Forms bind the Material datepicker input
(a `ControlValueAccessor`) and how an empty `type="number"` input is represented in
the model. Prefer normal framework binding; record the outcome in T004's progress.

## Resume notes

- Reservation: attempt `7c3bc02c-0a5d-43a0-b8fd-23e919f5f67b`, remote `origin`,
  branch `feature/US-006-delivery-note-pages`, marker
  `ecf5276e9b26ba9f888df2cfc08505f171ff7f52`, confirmed 2026-11-18.
- 2026-11-18: T001–T007 are done and the story is In review. Remaining: the human
  manual checks listed in the story's test boundary, then story review.
- 2026-11-18: [Review 01](review-01.md) (head `5178e13`, commit `b3b8bda`) returned
  `FAIL` with blocking F001 (lost `.app-content` layout rule) and F002 (test 4 and the
  toolbar test exceed Vitest's 5 s timeout). Non-blocking N001–N003 not addressed.
- 2026-11-18: Correction attempt 1 for F001 and F002 in `e724ea9`: restored
  `.app-content` in `web/src/app/app.scss`; set `testTimeout: 15000` in
  `web/vitest-base.config.ts`. Evidence: 5 consecutive `npm test` runs passed 20/20;
  lint, build, format check pass; layout checked against an empty-proxy dev server.
  Finding history: F001 1 attempt, F002 1 attempt. Next: follow-up review 02.
