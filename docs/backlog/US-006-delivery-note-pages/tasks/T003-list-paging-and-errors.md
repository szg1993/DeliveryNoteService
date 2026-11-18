# T003: Page through notes and recover from load failures

- Parent: [US-006](../README.md)
- Status: todo
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

- [ ] Previous/next request the adjacent page and are disabled at the bounds.
- [ ] A loading indicator shows while the list request is pending.
- [ ] A failed load opens the error dialog, then shows the message and Retry;
      Retry reloads the list.

## Verification

No new automated tests (the agreed boundary verifies paging, loading, and Retry
manually). Keep the T001 tests green. Manually check in the dev server or browser
pane, using a mocked or stubbed `/api` response rather than the real backend:
paging with more than 10 notes, the loading bar, and a failing response followed by
Retry. Then run lint, test, build, and format check in `web/`.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
