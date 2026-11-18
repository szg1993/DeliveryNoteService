# T006: Handle missing or unavailable recipients

- Parent: [US-006](../README.md)
- Status: todo
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

- [ ] An empty recipient list shows the hint with a working link to `/recipients`.
- [ ] A recipient loading failure opens the error dialog and leaves submit
      disabled.

## Verification

No new automated tests (the agreed boundary verifies these manually). Keep tests
4–6 green. Check both cases manually with stubbed `/api/recipients` responses, not
the real backend. Then run lint, test, build, and format check in `web/`.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
