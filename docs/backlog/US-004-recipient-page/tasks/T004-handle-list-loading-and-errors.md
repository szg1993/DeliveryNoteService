# T004: Handle list loading and errors

- Parent: [US-004](../README.md)
- Status: done
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

- [x] A loading indicator is visible while the list request is pending.
- [x] A failed list request opens the error dialog with the mapped message and raw
      code.
- [x] After the dialog closes, the list area shows the failure text and Retry;
      Retry requests the same page again.
- [x] The mapping covers every case above.

## Verification

The agreed story test boundary adds no dedicated automated test here. Verify
manually with the dev server: stop the backend and load the page (network failure
→ fallback message and Retry), then start it and use Retry. `npm run lint`,
`npm test`, and `npm run format:check` pass, and earlier tests still pass.

## Progress

### 2026-11-17

- Implemented on `feature/US-004-recipient-page` (reservation attempt
  `b406b863-a795-40e7-8d49-0dff1954fca3` verified against the remote marker before
  starting).
- Added `src/app/shared/error-message.ts`: `readableErrorMessage(error, recipientCode?)`
  and `backendErrorCode(error)`. The backend sends no data with
  `Recipient.AlreadyExists`, so the caller passes the submitted code (T006);
  without it, that error falls back to the generic message. `Recipient.InvalidCode`
  uses the field name `Code`, matching the PascalCase names in `data.Field`.
  `FieldMissing`/`FieldTooLong` without `data.Field` fall back too.
- Added the shared `ErrorDialog` (`src/app/shared/error-dialog/`, generated with
  `ng generate`): title "Error", message, raw code in small grey text when present,
  OK button. `openErrorDialog(dialog, error, recipientCode?)` builds its data and
  returns `afterClosed()`, ready for T006.
- Recipient page: a `mat-progress-bar` ("Loading recipients") shows while the
  resource loads. The resource stream catches a failure, opens the dialog, and
  rethrows once it closes, so the resource enters its error state only after the
  dialog closes. The error state shows "Recipients could not be loaded." and
  Retry, which calls `reload()` for the current page. The template now reads the
  loaded page through one `loadedPage` computed (guarded by `hasValue()`, since
  `value()` throws in the error state) instead of separate `recipients`,
  `noRecipients` and `paging` computeds.
- Tests: per the agreed story test boundary, no permanent test was added. A
  throwaway Vitest spec (deleted afterwards) checked every mapping case:
  AlreadyExists with a code, FieldMissing/FieldTooLong with `data.Field`,
  InvalidCode, InvalidCountry, 409 UniqueKeyConflict, status 0 `ProgressEvent`,
  a text body and a null body. 9/9 passed.
- Manual verification (dev server on port 4300 proxied to an unused port 5999,
  because the developer's backend was already running and was not stopped):
  page load opened the dialog with "Something went wrong. Please try again." and
  no code while the progress bar showed behind it; after OK the list area showed
  "Recipients could not be loaded." and Retry. With a forwarder from 5999 to the
  backend started, Retry sent the same `page=1&limit=10&sortBy=Id&sortDir=Desc`
  request (200) and the table rendered. A stub returning 400
  `Recipient.InvalidCountry` showed "Unknown country code." with
  `Recipient.InvalidCountry` in small grey text.
- Refactoring assessment (`clean-code`): production code is small and single
  purpose; renamed the template alias to `page` so it no longer shadows the
  `loadedPage` member, and removed a duplicated parameter explanation from the
  `openErrorDialog` doc comment. The existing spec was not changed; its suite
  helpers already cover the list scenarios and no new test code was added, so no
  shared test helper was justified.
- Final checks after the last edit: `npm run format:check`, `npm run lint`,
  `npm test` (2 files, 8 tests) and `npm run build` pass.

Remaining work: none for T004. T006 reuses `openErrorDialog` for creation errors.
