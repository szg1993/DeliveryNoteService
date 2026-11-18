# T007: Show creation errors in the dialog

- Parent: [US-006](../README.md)
- Status: done
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

- [x] A 400 `Note.InvalidInput` response opens the dialog with its message and the
      entered values remain.
- [x] The other agreed codes and network failures show their agreed messages.
- [x] Existing recipient error messages are unchanged.

## Verification

TDD: agreed test 7, a mocked 400 `Note.InvalidInput` response opens the error
dialog with its message and keeps the entered values. Keep the existing recipient
tests green. Check the remaining mappings manually. Then run lint, test, build, and
format check in `web/`.

## Progress

2026-11-18: Implemented and verified.

- `readableErrorMessage` maps `Recipient.NotFound`, `Note.InvalidInput` and
  `Note.NumberExhausted` to the agreed messages. `FieldMissing`/`FieldTooLong` and the
  fallback were already in place and are unchanged.
- The creation page already routed every `HttpErrorResponse` from the POST through
  `openErrorDialog` without resetting the form (T004), so no page change was needed.
- Agreed test 7 added to `create-delivery-note-page.spec.ts` (red first: the dialog
  showed the fallback message; green after the mapping). It checks the dialog message
  and that Category, Tare weight and the line's Part code keep their values.
- Mapping evidence for the other codes: a throwaway Vitest spec (deleted, not
  committed, since only test 7 is agreed) confirmed `Note.NumberExhausted` (409),
  `Recipient.NotFound` (404), `FieldMissing` with `PartQtyUm`, `FieldTooLong` with
  `Category`, a network failure (status 0, `ProgressEvent`), an unknown code (500),
  and the unchanged `Recipient.AlreadyExists`, `Recipient.InvalidCode` and
  `Recipient.InvalidCountry` messages. No browser check was run; the in-app check of
  these messages remains a human manual check per the story.
- Regression fix outside the task's stated scope: the T002 toolbar test
  (`app.spec.ts`) failed in about three of four full-suite runs, also on the
  committed T006 state, because the first lazy route load took over 1 s under parallel
  load and `answerEmptyListRequest` used `waitFor`'s 1 s default. The helper now waits
  up to 5 s and still returns as soon as the request appears. Waiting for
  `whenStable()` first was tried and rejected: the pending mocked request keeps the
  app unstable.

Verification in `web/`: `npm run lint` passed; `npm test` 20/20 passed in three
consecutive full runs (tests 4–7 and recipient tests included); `npm run build`
succeeded; `npm run format:check` passed after `prettier --write` on the changed files.

Refactoring assessment (clean-code): the new `switch` cases keep the existing
one-code-one-message shape, with the recipient code next to the other recipient codes;
no extraction is justified. In the spec, `rejectCreateRequest` mirrors the local
`answerCreateRequest` and the recipients page's `rejectCreateRequest`. A shared
`testing/http.ts` helper would need method and URL parameters for two one-line uses,
so local helpers suffice; the scenario's values stay visible through the existing
`fillValidNote()`, `input()`, `section()` and `line()` helpers. The shared
`answerEmptyListRequest` (consumers: `app.spec.ts` and this spec) keeps its responsibility, configuration and per-test isolation; only its
wait timeout changed. No other refactoring was justified.

Remaining: human manual checks of the remaining error mappings in the app.
