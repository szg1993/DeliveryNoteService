# T004: Create a delivery note

- Parent: [US-006](../README.md)
- Status: todo
- Depends on: T001
- Increment: Extension

## Outcome

From the list, users open a creation form, enter a complete note with one or more
lines, and submit it; on success they return to the list with a confirmation.

## Scope

- Add `createDeliveryNote(details)` to `DeliveryNotesApi`, posting to
  `/api/delivery-notes` and returning the created note from the envelope's
  `content`. Type the request after `CreateDeliveryNoteDto` (see the plan's
  verified request shape); send no extra properties.
- Add `getAllRecipients()` to `RecipientsApi`, requesting `limit: 0`.
- Add a lazy-loaded `/delivery-notes/new` route titled "New delivery note" and a
  creation page in `web/src/app/delivery-notes/`. Add a "New delivery note" button
  on the list page linking to it.
- One scrolling page with titled sections and the story's AC8 labels: Header,
  Flags, Contact, Package, Deadlines, Lines. Use Signal Forms as in
  `CreateRecipientForm`.
- Recipient: `mat-select` of all recipients as "CODE – Name", sending
  `recipientId`.
- Flags: checkboxes, unchecked by default.
- Units and package scale: free-text inputs.
- Numbers: `type="number"` inputs allowing decimals, sent as JSON numbers.
- Dates: Material datepicker with the native date adapter defaults, starting empty,
  sent as the selected local calendar date in `yyyy-MM-dd` without UTC conversion.
- Lines: start with one empty line; "Add line" appends one; each line's remove
  button is disabled while only one remains; order is preserved.
- Validation: required fields and the US-002 maximum lengths, shown inline; invalid
  forms send no request.
- Text is sent as entered. Empty EKAER number, comments, and line comment are sent
  as `null`.
- Submit sends one POST and is disabled while in progress. On 201, show the snackbar
  "Delivery note {NoteNbr} created" and navigate to `/delivery-notes`.
- "Cancel" navigates to the list without saving or warning.

Numeric minimums and date order belong to T005, recipient edge cases to T006, and
backend error presentation to T007. Until T007, an `HttpErrorResponse` may open the
shared dialog with the existing generic mapping.

Verify and record in Progress how Signal Forms bind the datepicker input and how an
empty number input is represented; prefer normal framework binding.

## Acceptance criteria

- [ ] The list's "New delivery note" button opens `/delivery-notes/new`, lazy loaded
      and titled.
- [ ] The form has the agreed sections, labels, flags, free-text units, and lines
      behavior.
- [ ] A complete valid form sends one POST with lines in order, `yyyy-MM-dd`
      dates, JSON numbers, untrimmed text, and `null` for empty optional text.
- [ ] Submit is disabled while the request is in progress.
- [ ] On 201 the snackbar shows the returned note number and the app shows the list.
- [ ] A required field left empty shows an inline error and sends no request.
- [ ] Cancel returns to the list without a request or warning.

## Verification

TDD with `@testing-library/angular`, `HttpTestingController`, and a router so the
navigation to the list can be observed:

- Agreed test 4: a complete valid form with two lines sends the expected POST body;
  on 201 the snackbar shows and the app navigates to the list.
- Agreed test 5: submitting with a required field empty shows an inline error and
  sends no request.

Manually check adding and removing lines, maximum-length errors, the submit guard,
and Cancel in the dev server or browser pane without the real backend. Then run
lint, test, build, and format check in `web/`.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
