# T003: Page through notes and recover from load failures

- Parent: [US-006](../README.md)
- Status: done
- Depends on: T001
- Increment: Extension

## Outcome

Users page through all notes with previous and next controls, see a loading
indicator while the list loads, and can retry after a failed load.

## Scope

Follow `RecipientsPage` in `DeliveryNotesPage`:

- Previous and next controls driven by the response's paging metadata, with
  "Page X of Y"; disabled at the first and last page.
- An indeterminate progress bar while loading.
- On failure, open the shared error dialog; after it closes, show "Delivery notes
  could not be loaded." with a Retry button that reloads the current page.

## Acceptance criteria

- [x] Previous/next request the adjacent page and are disabled at the bounds.
- [x] A loading indicator shows while the list request is pending.
- [x] A failed load opens the error dialog, then shows the message and Retry;
      Retry reloads the list.

## Verification

No new automated tests (the agreed boundary verifies paging, loading, and Retry
manually). Keep the T001 tests green. Manually check in the dev server or browser
pane, using a mocked or stubbed `/api` response rather than the real backend:
paging with more than 10 notes, the loading bar, and a failing response followed by
Retry. Then run lint, test, build, and format check in `web/`.

## Progress

2026-11-18:

- `DeliveryNotesPage` now follows `RecipientsPage`: a `page` signal drives the
  `rxResource` params; an indeterminate `mat-progress-bar` ("Loading delivery
  notes") shows while loading; a failed load opens `openErrorDialog`, and after it
  closes the page shows "Delivery notes could not be loaded." with Retry, which
  reloads the current page; a "Delivery note pages" nav shows "Page X of Y" with
  Previous/Next disabled at the first and last page.
- Automated checks (`web/`): `npm run lint` passed; `npm test` 4 files, 16 tests
  passed (no new tests, per the agreed boundary); `npm run build` passed;
  `npx prettier --check` on the changed files passed.
- Manual check: `ng serve` with a scratchpad proxy to a local Node stub on port
  5299 (25 notes, 1.5 s delay, page 3 fails with 500 on its first request); the real
  backend was not reachable. Observed: loading bar on initial load, paging, and
  Retry; page 1 showed DN-000025..16, "Page 1 of 3", Previous disabled; Next loaded
  page 2 (both enabled); Next to page 3 opened the error dialog (message appeared
  only after OK); Retry reloaded page 3 (5 rows, Next disabled); Previous loaded
  page 2. Stub log showed requests for pages 1, 2, 3, 3, 2.
- Refactoring assessment (clean-code): the error-dialog `catchError` pipe and the
  paging nav markup now appear in two pages. Kept local: two straightforward
  copies of an established pattern do not yet justify a shared operator or paging
  component under the repository's simplicity rule; revisit if a third list page
  needs them. Names, responsibilities and template structure match
  `RecipientsPage`; no other refactoring justified. No test code changed, so no
  test-helper reuse assessment was needed.

Remaining work: none.
