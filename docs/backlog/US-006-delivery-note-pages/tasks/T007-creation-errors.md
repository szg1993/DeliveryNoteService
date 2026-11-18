# T007: Show creation errors in the dialog

- Parent: [US-006](../README.md)
- Status: todo
- Depends on: T004
- Increment: Edge case

## Outcome

Every failed creation request opens the shared error dialog with a readable
message, and the entered values stay in the form for correction.

## Scope

- Extend `readableErrorMessage` in `web/src/app/shared/error-message.ts`:
  - `Note.InvalidInput`: "Some delivery note values are invalid. Check quantities,
    weights, dimensions and date order."
  - `Note.NumberExhausted`: "No more delivery note numbers are available."
  - `Recipient.NotFound`: "The selected recipient no longer exists."
  - `error:DNC::FieldMissing` / `FieldTooLong` keep "The {field} field is invalid."
    with the backend's property name as sent (for example `PartQtyUm`).
  - Anything else, including network failures: the existing fallback.
- On the creation page, route every `HttpErrorResponse` through `openErrorDialog`
  without resetting the form.

## Acceptance criteria

- [ ] A 400 `Note.InvalidInput` response opens the dialog with its message and the
      entered values remain.
- [ ] The other agreed codes and network failures show their agreed messages.
- [ ] Existing recipient error messages are unchanged.

## Verification

TDD: agreed test 7, a mocked 400 `Note.InvalidInput` response opens the error
dialog with its message and keeps the entered values. Keep the existing recipient
tests green. Check the remaining mappings manually. Then run lint, test, build, and
format check in `web/`.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
