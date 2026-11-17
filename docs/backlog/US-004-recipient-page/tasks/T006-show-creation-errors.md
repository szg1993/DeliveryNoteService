# T006: Show creation errors in the dialog

- Parent: [US-004](../README.md)
- Status: todo
- Depends on: T004, T005
- Increment: Edge case

## Outcome

When the backend rejects a new recipient or the request fails, the user sees the
readable error dialog and can correct the form without retyping it.

## Scope

- On any creation failure, open the shared error dialog from T004 with the mapped
  message and raw code.
- Keep the entered form values and re-enable the submit button.
- Do not show the success snackbar or reload the list.

## Acceptance criteria

- [ ] A 400 `Recipient.AlreadyExists` response opens the dialog with "A recipient
      with code {CODE} already exists." and the raw code.
- [ ] Other 400 responses, unexpected statuses, and network failures open the
      dialog with their mapped message.
- [ ] Entered values remain in the form and submission is possible again.

## Verification

Component test with `HttpTestingController` (story test boundary test 4):
submitting valid details and answering 400 with
`{ errorCode: "Recipient.AlreadyExists" }` shows the dialog with the readable
message and keeps every entered value. Check the other mappings manually.
`npm run lint`, `npm test`, and `npm run format:check` pass.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
