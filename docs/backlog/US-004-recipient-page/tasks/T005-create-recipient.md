# T005: Create a recipient

- Parent: [US-004](../README.md)
- Status: todo
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

- [ ] The six inputs are findable by their labels.
- [ ] A missing or too-long value shows an inline error and no request is sent.
- [ ] Valid input sends one POST with the values unchanged.
- [ ] The submit button is disabled while the request is pending.
- [ ] On 201 the snackbar appears, the form is empty, and page 1 of the list is
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

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
