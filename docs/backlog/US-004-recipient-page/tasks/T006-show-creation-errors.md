# T006: Show creation errors in the dialog

- Parent: [US-004](../README.md)
- Status: done
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

- [x] A 400 `Recipient.AlreadyExists` response opens the dialog with "A recipient
      with code {CODE} already exists." and the raw code.
- [x] Other 400 responses, unexpected statuses, and network failures open the
      dialog with their mapped message.
- [x] Entered values remain in the form and submission is possible again.

## Verification

Component test with `HttpTestingController` (story test boundary test 4):
submitting valid details and answering 400 with
`{ errorCode: "Recipient.AlreadyExists" }` shows the dialog with the readable
message and keeps every entered value. Check the other mappings manually.
`npm run lint`, `npm test`, and `npm run format:check` pass.

## Progress

### 2026-11-17

Reservation: continued attempt `b406b863-a795-40e7-8d49-0dff1954fca3` on
`feature/US-004-recipient-page` at the developer's request; the remote branch tip
was the reservation marker, it matched the confirmed local record, and no other
US-004 branch existed.

Implementation:

- `CreateRecipientForm.createRecipient()` catches an `HttpErrorResponse` from the
  POST and opens the shared dialog with `openErrorDialog(dialog, error, code)`,
  passing the submitted code for the `Recipient.AlreadyExists` message. Other
  errors are rethrown. The success steps (snackbar, reset, `created`) moved to
  `confirmCreated()`, so a failure skips them: the values stay, the list is not
  reloaded, and Signal Forms ends `submitting()`, which re-enables the button.

Verification (substituted backend only; the real API was not started):

- Red first: the new test failed because no dialog appeared and the 400 surfaced
  as an unhandled rejection.
- Test boundary 4 (`recipients-page.spec.ts`): valid details answered with 400
  `{ errorCode: "Recipient.AlreadyExists" }` show the "Error" dialog with
  "A recipient with code BUDA01 already exists." and `Recipient.AlreadyExists`,
  keep all six values, and leave Create recipient enabled.
- Instead of the manual check, a throwaway component spec through
  `RecipientsPage` with `HttpTestingController` (deleted afterwards) covered:
  400 `Recipient.InvalidCountry`, 400 `error:DNC::FieldTooLong` with
  `data.Field: "Name"`, 400 `Recipient.InvalidCode`, 400 with an unknown code,
  500 without a body, and a network failure (`ProgressEvent`). Each case showed its
  mapped message, showed the raw code only when the backend sent one, kept every
  value, showed no snackbar, sent no list reload (`verify()`), and sent a new POST
  when Create recipient was clicked again after OK. 6/6 passed.
- `npm test` (2 files, 13 tests), `npm run lint` and `npm run build` pass.
  `npm run format:check` passes for every T006 file. It reports only
  `web/angular.json`, which carries an unrelated uncommitted Angular CLI edit
  (`analytics: false`) that this task left untouched.

Refactoring assessment (`clean-code`):

- Production: splitting the success steps into `confirmCreated()` keeps the
  submit action a short try/catch. The error mapping and dialog are reused from
  T004 unchanged. No further refactoring is justified.
- Tests: `rejectCreateRequest(errorBody)` mirrors `answerCreateRequest` and keeps
  the error body visible at the call site. The other helpers are reused. As in
  T005, `recipients-page.spec.ts` is the only recipient spec, so the helpers stay
  local; each test renders a fresh page with its own `HttpTestingController`, so
  no shared state or lifecycle owner is needed.

Remaining work: none for T006.
