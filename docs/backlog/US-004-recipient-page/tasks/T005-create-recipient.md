# T005: Create a recipient

- Parent: [US-004](../README.md)
- Status: done
- Depends on: T002
- Increment: Extension

## Outcome

Users can register a recipient with the form above the list, see a confirmation,
and find the new recipient at the top of the list.

## Scope

- Add `createRecipient(details)` to `RecipientsApi`, calling
  `POST /api/recipients` and returning the created `RecipientDetails`.
- Add the creation form above the list using the
  [Angular forms guide](../../../../../AgenticEngineering/agent-harness/technologies/angular/forms.md)
  and Angular Material inputs: code, name, country (free-text two-letter code),
  ZIP, city, and address, each with a visible label.
- Validate requiredness and maximum lengths inline (code 10, name 200, country 2,
  ZIP 20, city 50, address 200). Invalid input blocks submission and sends no
  request.
- Send the values exactly as entered; do not trim or change case.
- Disable the submit button while the request is in progress.
- On 201, show a snackbar "Recipient {CODE} created" with the code from the
  response, reset the form, and reload the list on page 1.

Backend error presentation for creation belongs to T006.

## Acceptance criteria

- [x] The six inputs are findable by their labels.
- [x] A missing or too-long value shows an inline error and no request is sent.
- [x] Valid input sends one POST with the values unchanged.
- [x] The submit button is disabled while the request is pending.
- [x] On 201 the snackbar appears, the form is empty, and page 1 of the list is
      requested again.

## Verification

Component tests with `HttpTestingController` (story test boundary tests 3 and 5):

3. Filling valid details and submitting sends the expected POST body, including a
   value with surrounding spaces kept as entered; answering 201 shows the
   snackbar, clears the form, and sends a new page-1 list request.
5. Submitting with a required field empty shows its inline error and sends no
   POST.

Check the double-submit guard manually. `npm run lint`, `npm test`, and
`npm run format:check` pass.

## Progress

### 2026-11-17

Reservation: resumed attempt `b406b863-a795-40e7-8d49-0dff1954fca3` on
`feature/US-004-recipient-page` after explicit developer direction; the remote
ownership record matched the confirmed local record.

Implementation:

- `RecipientsApi.createRecipient(details)` posts `NewRecipient` to
  `POST /api/recipients` and returns the `content` of the 201 response.
- New `CreateRecipientForm` component (`recipients/create-recipient-form/`) with
  Signal Forms (`form`, `FormRoot`, `FormField`) and Material inputs. One field
  list drives the six labelled inputs, the `required` and `maxLength` rules
  (code 10, name 200, country 2, ZIP 20, city 50, address 200) and their inline
  `mat-error` messages. Values are sent exactly as entered.
- The submit button is disabled while `submitting()`; Signal Forms also ignores a
  second `submit()` while one is in progress.
- On 201 it shows the snackbar "Recipient {CODE} created", resets the form and
  emits `created`. `RecipientsPage` places the form above the list and reloads
  page 1 (`reload()` when already on page 1, otherwise switches to page 1).
- Creation failures are not presented yet; T006 owns that.

Verification (component tests through `RecipientsPage` with
`HttpTestingController`):

- Test boundary 3: valid details with a name `' Buda Logistics '` send one POST
  with the unchanged body; 201 shows the snackbar, empties all six inputs (found
  by label) and answers a new page-1 list request.
- Test boundary 5: an empty code shows "Code is required." and sends no POST.
- Too-long value: country `HUN` shows "Country must be at most 2 characters." and
  sends no POST.
- Pending state: two immediate clicks send exactly one POST and the button is
  disabled until the response. A mutation check (removing the `[disabled]`
  binding) made this test fail.
- Manual check against the real backend (`dotnet run --launch-profile http`,
  `npm start`): two programmatic clicks in one frame produced one
  `POST /api/recipients` (201) followed by one page-1 reload; the button was
  disabled after the first click; the response kept `" Manual Check Kft "`; the
  snackbar and the new top row appeared. Submitting an empty form showed the
  required errors, linked to their inputs through `aria-describedby`, and sent no
  POST.
- `npm test` (12 passed), `npm run lint`, `npm run format:check` and
  `npm run build` pass.

Observation: Signal Forms' `maxLength` also sets the native `maxlength`
attribute, so a browser stops typing at the limit; the inline too-long error
remains as the client-side guard for values set otherwise.

Refactoring assessment (`clean-code`):

- Production: the form is a separate component so the page stays a list
  container and the form owns creation, snackbar and reset. A single
  `recipientFields` list replaces six repeated template blocks and six schema
  rule pairs. Removed the empty generated stylesheet. No further refactoring
  justified.
- Tests: `recipientWithCode` now builds from the shared `validRecipient` instead
  of repeating the same literal values. `answerRecipientsRequest` waits for the
  list request with Testing Library `waitFor` because the reload starts after the
  asynchronous submit completes, and `whenStable()` cannot settle while that
  request is pending. Removed a trailing assertion so the pending-state test
  checks one outcome.
- Test-helper scope: the only Angular spec exercising recipients is
  `recipients-page.spec.ts`; `app.routes.spec.ts` shares no HTTP or form
  mechanics. The new helpers (`fillRecipientForm`, `recipientFormValues`,
  `answerCreateRequest`, `expectNoCreateRequest`, `createButton`) stay local to
  that file. Each test renders a fresh page with its own `HttpTestingController`
  (TestBed resets per test); no shared mutable state or lifecycle owner is
  needed. Shared infrastructure would add no reuse yet.

Remaining work: none for T005. Backend creation errors are T006.
