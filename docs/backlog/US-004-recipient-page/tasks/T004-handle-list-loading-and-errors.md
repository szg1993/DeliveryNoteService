# T004: Handle list loading and errors

- Parent: [US-004](../README.md)
- Status: todo
- Depends on: T002
- Increment: Extension

## Outcome

While the list loads, a loading indicator is visible. If loading fails, a modal
error dialog explains the problem in readable English, and the user can retry.
The dialog and its message mapping are reusable for creation errors (T006).

## Scope

- Show a loading indicator while a list request is in progress.
- Add a shared error dialog in `src/app/shared/` using Angular Material dialog:
  a title, the readable message, the raw `errorCode` in small grey text when one
  exists, and an OK button.
- Add a function that maps an `HttpErrorResponse` to the readable message, per
  story AC11:
  - `Recipient.AlreadyExists` → "A recipient with code {CODE} already exists."
  - `error:DNC::FieldMissing`, `error:DNC::FieldTooLong`, `Recipient.InvalidCode`
    → "The {field} field is invalid.", using `data.Field`, or the code field for
    `Recipient.InvalidCode`.
  - `Recipient.InvalidCountry` → "Unknown country code."
  - Anything else, including network failures (status 0) and 409
    `error:DNC::UniqueKeyConflict` → "Something went wrong. Please try again."
- On a list-loading failure, open the dialog; after it closes, show "Recipients
  could not be loaded." with a Retry button that reloads the current page.

## Acceptance criteria

- [ ] A loading indicator is visible while the list request is pending.
- [ ] A failed list request opens the error dialog with the mapped message and raw
      code.
- [ ] After the dialog closes, the list area shows the failure text and Retry;
      Retry requests the same page again.
- [ ] The mapping covers every case above.

## Verification

The agreed story test boundary adds no dedicated automated test here. Verify
manually with the dev server: stop the backend and load the page (network failure
→ fallback message and Retry), then start it and use Retry. `npm run lint`,
`npm test`, and `npm run format:check` pass, and earlier tests still pass.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
