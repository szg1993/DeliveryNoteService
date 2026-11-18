# T006: Handle missing or unavailable recipients

- Parent: [US-006](../README.md)
- Status: done
- Depends on: T004
- Increment: Edge case

## Outcome

The creation form explains what to do when there are no recipients, and cannot be
submitted when recipients failed to load.

## Scope

On the creation page:

- When the recipient list loads empty, show under the select: "No recipients yet.
  Create one on the Recipients page." with a link to `/recipients`.
- When loading recipients fails, open the shared error dialog and keep the submit
  button disabled. No Retry; the user reloads or leaves the page.

## Acceptance criteria

- [x] An empty recipient list shows the hint with a working link to `/recipients`.
- [x] A recipient loading failure opens the error dialog and leaves submit
      disabled.

## Verification

No new automated tests (the agreed boundary verifies these manually). Keep tests
4–6 green. Check both cases manually with stubbed `/api/recipients` responses, not
the real backend. Then run lint, test, build, and format check in `web/`.

## Progress

2026-11-18: Implemented and verified.

- `CreateDeliveryNotePage` loads recipients through `loadRecipients()`: on an
  `HttpErrorResponse` it opens the shared error dialog (`openErrorDialog`) and rethrows,
  so the `rxResource` ends in `error` at once. `recipientsLoadFailed` (status `error`)
  joins `submitting()` in the submit button's `disabled` binding. No Retry.
- `noRecipients` (loaded and empty) shows "No recipients yet. Create one on the
  Recipients page." under the select, with "Recipients page" as a `routerLink` to
  `/recipients`. The select and the hint share a `recipient-field` grid cell; the hint
  is a paragraph rather than `mat-hint` so the select's "Recipient is required." error
  does not replace it after a submit attempt.
- No new automated tests, per the agreed boundary.

Manual check (2026-11-18, browser pane, `ng serve --port 4310 --proxy-config` pointing
`/api` at a local Node stub on port 5299; the real backend was never contacted): with
an empty recipient list the hint showed under the select and its link opened
`/recipients` (title "Recipients"); with the stub answering `GET /api/recipients` with
500 the error dialog opened ("Something went wrong. Please try again." with the code),
no hint showed, and after closing it the "Create delivery note" button stayed disabled;
an Enter keypress in a field sent no POST (the stub logged only GETs). Port 4300 was
held by another process, so the check used 4310.

Verification in `web/`: `npm run lint` passed; `npm test` 19/19 passed (5 files,
tests 4–6 included); `npm run build` succeeded; `npm run format:check` passed after
`prettier --write` on the changed page files.

Refactoring assessment (clean-code): `loadRecipients` resembles the list page's
`loadDeliveryNotes` but differs on purpose: the list fails only after the dialog
closes because it then offers Retry, while the creation page fails at once so submit
is disabled immediately. Two consumers with different timing do not justify a shared
helper. The derived states are named `computed` signals rather than template
expressions. No test code changed, so no test infrastructure was reassessed.

Remaining: none for T006.
