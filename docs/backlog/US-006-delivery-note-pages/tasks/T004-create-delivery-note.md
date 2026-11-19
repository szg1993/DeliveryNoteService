# T004: Create a delivery note

- Parent: [US-006](../README.md)
- Status: done
- Depends on: T001
- Increment: Extension

## Outcome

From the list, users open a creation form, enter a complete note with one or more
lines, and submit it; on success they return to the list with a confirmation.

## Scope

- Add `createDeliveryNote(details)` to `DeliveryNotesApi`, posting to
  `/api/delivery-notes` and returning the created note from the envelope's
  `content`. Type the request after `CreateDeliveryNoteDto` (see the plan's
  verified request shape); send no extra properties.
- Add `getAllRecipients()` to `RecipientsApi`, requesting `limit: 0`.
- Add a lazy-loaded `/delivery-notes/new` route titled "New delivery note" and a
  creation page in `web/src/app/delivery-notes/`. Add a "New delivery note" button
  on the list page linking to it.
- One scrolling page with titled sections and the story's AC8 labels: Header,
  Flags, Contact, Package, Deadlines, Lines. Use Signal Forms as in
  `CreateRecipientForm`.
- Recipient: `mat-select` of all recipients as "CODE – Name", sending
  `recipientId`.
- Flags: checkboxes, unchecked by default.
- Units and package scale: free-text inputs.
- Numbers: `type="number"` inputs allowing decimals, sent as JSON numbers.
- Dates: Material datepicker with the native date adapter defaults, starting empty,
  sent as the selected local calendar date in `yyyy-MM-dd` without UTC conversion.
- Lines: start with one empty line; "Add line" appends one; each line's remove
  button is disabled while only one remains; order is preserved.
- Validation: required fields and the US-002 maximum lengths, shown inline; invalid
  forms send no request.
- Text is sent as entered. Empty EKAER number, comments, and line comment are sent
  as `null`.
- Submit sends one POST and is disabled while in progress. On 201, show the snackbar
  "Delivery note {NoteNbr} created" and navigate to `/delivery-notes`.
- "Cancel" navigates to the list without saving or warning.

Numeric minimums and date order belong to T005, recipient edge cases to T006, and
backend error presentation to T007. Until T007, an `HttpErrorResponse` may open the
shared dialog with the existing generic mapping.

Verify and record in Progress how Signal Forms bind the datepicker input and how an
empty number input is represented; prefer normal framework binding.

## Acceptance criteria

- [x] The list's "New delivery note" button opens `/delivery-notes/new`, lazy loaded
      and titled.
- [x] The form has the agreed sections, labels, flags, free-text units, and lines
      behavior.
- [x] A complete valid form sends one POST with lines in order, `yyyy-MM-dd`
      dates, JSON numbers, untrimmed text, and `null` for empty optional text.
- [x] Submit is disabled while the request is in progress.
- [x] On 201 the snackbar shows the returned note number and the app shows the list.
- [x] A required field left empty shows an inline error and sends no request.
- [x] Cancel returns to the list without a request or warning.

## Verification

TDD with `@testing-library/angular`, `HttpTestingController`, and a router so the
navigation to the list can be observed:

- Agreed test 4: a complete valid form with two lines sends the expected POST body;
  on 201 the snackbar shows and the app navigates to the list.
- Agreed test 5: submitting with a required field empty shows an inline error and
  sends no request.

Manually check adding and removing lines, maximum-length errors, the submit guard,
and Cancel in the dev server or browser pane without the real backend. Then run
lint, test, build, and format check in `web/`.

## Progress

2026-11-18: Implemented and verified.

- Added `DeliveryNotesApi.createDeliveryNote` (`NewDeliveryNote` typed after
  `CreateDeliveryNoteDto`; numbers and dates nullable like the DTO) and
  `RecipientsApi.getAllRecipients` (`limit=0`).
- Added the lazy `/delivery-notes/new` route titled "New delivery note", the
  `CreateDeliveryNotePage` (Signal Forms, sections Header, Flags, Contact, Package,
  Deadlines, Lines as labelled regions; lines as `fieldset`s "Line N"), and the list's
  "New delivery note" link button.
- Agreed tests 4 and 5 in `create-delivery-note-page.spec.ts` (RouterTestingHarness +
  HttpTestingController). Test 4 failed first (missing API and page), then passed.
  Test 5 was confirmed to fail when the Category rule was temporarily made optional.

Binding outcome (the plan's "To verify during T004"):

- Material datepicker: `MatDatepickerInput` provides `NG_VALUE_ACCESSOR`, so
  `[formField]` binds it through Signal Forms' CVA interop with no custom code. With
  `provideNativeDateAdapter()` the model holds `Date | null`; typed text (`11/20/2026`)
  and calendar picks both set a local-midnight `Date`. The page formats it from
  local year/month/day as `yyyy-MM-dd`, so no UTC shift.
- `type="number"` inputs: with a `number | null` model, Signal Forms' native binding
  stores `valueAsNumber` and an empty input as `null` (cleared input showed
  "Quantity is required."). Decimals arrive as JSON numbers.
- Signal Forms tags array items with an identity `Symbol` property, so line requests
  are copied field by field (`toNewNoteLine`) rather than spread; JSON would drop the
  symbol anyway, but explicit mapping keeps the request exact.

Manual check (2026-11-18, browser pane, `ng serve --proxy-config` pointing `/api` at a
local Node stub on port 5299; the real backend was never contacted): the list button
opens `/delivery-notes/new` with the title; the recipient select lists "CODE – Name";
add line appends, remove is disabled at one line and removing line 1 keeps line 2's
values; an empty submit shows inline required errors and sends no POST; an 11-character
weight unit shows "Weight unit must be at most 10 characters."; with a 3 s stub delay
the submit button was disabled and a second click sent no second POST (stub logged one
POST with the expected body); on 201 the snackbar "Delivery note DN-000001 created"
showed over the list with the new note; Cancel returned to the list without a request
or warning.

Verification in `web/`: `npm run lint` passed; `npm test` 18/18 passed (5 files);
`npm run build` succeeded (creation page is its own lazy chunk); `npm run format:check`
passed after `prettier --write` on the changed files.

Refactoring assessment (clean-code): production code keeps the `CreateRecipientForm`
pattern (schema, `submission` action, `openErrorDialog`); repeated rule calls became
small `requiredText`/`optionalText`/`requiredValue` helpers, and request mapping lives
in plain functions (`toNewDeliveryNote`, `toNewNoteLine`, `emptyAsNull`,
`calendarDate`). The template's per-field `mat-error` loops stay explicit because
`mat-error` must be direct content of `mat-form-field`. In tests, agreed test 4 now
builds on the same `fillValidNote()` arrange helper as test 5 and keeps only its
distinguishing values visible. Test reuse: inspected `app.spec.ts`,
`delivery-notes-page.spec.ts`, and `recipients-page.spec.ts`; the "wait for a page's
list request and answer it empty" mechanic was duplicated in `app.spec.ts` and the new
spec, so it moved to `web/src/testing/http.ts` (`answerEmptyListRequest`), used by both.
It is stateless, configured per call, uses each test's own TestBed
`HttpTestingController`, and owns no lifecycle. The page specs' own answer helpers
flush non-empty, scenario-specific pages, so they stay local. Form-filling helpers stay
local to the creation spec; T005 and T007 will extend the same file.

Remaining: none for T004. Until T007, creation errors use the shared generic dialog
mapping; recipient load failures and the no-recipients hint belong to T006.

- 2026-11-18 (review 01 correction, attempt 1, F002): the slow tests were the first
  routed test in each spec file (agreed test 4, the toolbar test 3, and the redirect test
  in `app.routes.spec.ts`). The time goes to the first lazy page load and render while
  the full suite runs in parallel; the form is already filled with `fireEvent`, not
  per-keystroke typing. Before the fix, one `npm test` run here failed 3 of 20 tests at
  the 5 s default. Fix: `web/vitest-base.config.ts` sets `testTimeout: 15000`, loaded
  through `runnerConfig` in the `test` target of `web/angular.json`. The
  `answerEmptyListRequest` wait in `web/src/testing/http.ts` stays at 5 s, below the
  per-test timeout. No test, assertion or test seam changed. Evidence: five consecutive
  full `npm test` runs passed 20/20; test 4 took 8141, 8154, 6845, 4479 and 3369 ms, the
  toolbar test 5796, 4441, 4314, 2861 and 2014 ms, and the redirect test up to 4350 ms.
  `npm run lint`, `npm run build` and `npm run format:check` passed. Refactoring
  assessment: one configuration value with its reason in a comment; nothing to refactor.
