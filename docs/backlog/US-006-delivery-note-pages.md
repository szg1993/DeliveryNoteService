# US-006: Delivery note pages

- Status: Done
- Type: Frontend
- Area: Delivery notes
- Implementation repository: DeliveryNoteService
- Implementation workspace: `web/` Angular application
- Dependencies: `GET /api/delivery-notes` from
  [US-005](US-005-list-delivery-notes.md) (Done; tests mock it);
  `POST /api/delivery-notes` from [US-002](US-002-create-delivery-notes.md);
  `GET /api/recipients` from [US-001](US-001-create-and-view-recipients.md) for the
  recipient picker. No backend changes in this story.
- Refinement: Developer confirmed shared understanding after two grilling sessions on
  2026-11-18.
- Breakdown: [Implementation plan and tasks](US-006-delivery-note-pages/README.md).
- Branch: feature/US-006-delivery-note-pages

## User story

As a delivery-note user, I want to see the delivery notes already created and create
a complete new one in a web form, and to move between the application's pages, so
that I can manage delivery notes without calling the API directly.

## Scope

- Add a delivery-note list page showing a few key fields per note.
- Add a delivery-note creation page with a form covering every field of the US-002
  creation contract, including one or more note lines.
- Add application navigation between the delivery-note and recipient pages.
- Reuse the US-004 patterns: a hand-written typed `DeliveryNotesApi` client, signals,
  Angular Material, the shared error dialog, and snackbar confirmations.

## Acceptance criteria

### Navigation

1. Routes: `/delivery-notes` (list), `/delivery-notes/new` (creation form), and the
   existing `/recipients`. The empty path redirects to `/delivery-notes`. Each page is
   lazy loaded and sets its browser title.
2. The existing top toolbar shows "Delivery notes" and "Recipients" links; the link
   for the current page is visibly highlighted.

### List page

3. Opening the list loads `GET /api/delivery-notes` newest first (`SortDir=Desc`),
   10 per page, with previous and next controls driven by the backend's paging
   metadata, as on the recipient page.
4. The table shows note number, recipient (code and name), category, takeover place,
   ship date, estimated arrival date, status as plain text (for example "Pending",
   returned as a string in `noteStatusId`), and an "Urgent" column showing a plain
   "Urgent" text chip only for urgent notes (no icon font). Dates are shown as
   `yyyy-MM-dd`, as returned by the API.
5. With no notes, an empty-state message is shown instead of an empty table.
6. While the list is loading, a loading indicator is shown. A loading failure opens the
   error dialog; after it is closed the list area shows "Delivery notes could not be
   loaded." with a Retry button that reloads the list.
7. A "New delivery note" button opens the creation page. Rows are not clickable.

### Creation page

8. The form is one scrolling page with titled sections and these labels:
   - Header: Recipient, Category, Takeover place, EKAER number (optional),
     Comments (optional).
   - Flags (checkboxes, unchecked by default): Urgent, Crackable, Dangerous, Own cost.
   - Contact: Assigned to, Assignee phone, Contact person, Contact phone.
   - Package: Tare weight, Tare weight unit, Package quantity, Package scale,
     Length (X), Width (Y), Height (Z), Size unit.
   - Deadlines: Takeover date, Ship date, Estimated arrival date.
   - Lines: Part code, Description, Comment (optional), Quantity, Quantity unit,
     Weight, Weight unit.
9. The recipient is chosen from a `mat-select` listing all recipients
   (`GET /api/recipients?Limit=0`) as "CODE – Name". With no recipients, a hint under
   the select reads "No recipients yet. Create one on the Recipients page." and links
   to `/recipients`. If recipients cannot be loaded, the error dialog opens and the
   submit button stays disabled; there is no Retry on the form, and the user recovers
   by reloading or leaving the page.
10. Lines are rows of inputs. The form starts with one empty line; "Add line" appends
    an empty line; each line has a remove button that is disabled while only one line
    remains. Line order is preserved in the request.
11. Units and package scale are free-text inputs.
12. Client-side validation is shown inline and blocks submission without sending a
    request:
    - required fields and the US-002 maximum lengths;
    - quantities (package and line) greater than 0; weights and dimensions 0 or more;
    - date order: takeover date ≤ ship date ≤ estimated arrival date.
    The backend remains authoritative for all rules.
13. Numbers use `type="number"` inputs, allow decimals, and are sent as JSON numbers.
    Dates use a Material datepicker with the native date adapter and its defaults: they
    start empty, display in the browser locale, and accept typing; typed text that
    cannot be parsed shows "Enter a valid date." inline. They are sent as the selected
    local calendar date in `yyyy-MM-dd`, without UTC conversion.
14. Text is sent as entered, without trimming. Optional text fields left empty (EKAER
    number, comments, line comment) are sent as `null`.
15. Submitting valid details sends one `POST /api/delivery-notes`; the submit button is
    disabled while it is in progress.
16. On HTTP 201, a snackbar shows "Delivery note {NoteNbr} created" with the number from
    the response, and the app navigates to the list, where the new note is at the top.
17. "Cancel" returns to the list without saving and without an unsaved-changes warning.

### Backend errors

18. Every backend error and network failure, from creation or from loading notes or
    recipients, opens the shared modal error dialog; entered form values are kept.
19. The shared error-message mapping adds:
    - `error:DNC::FieldMissing` and `error:DNC::FieldTooLong`: the existing
      "The {field} field is invalid.", where `{field}` is the backend's unqualified
      property name as sent in `data.Field` (for example `PartQtyUm`), without a line
      index or translation to form labels.
    - `Note.InvalidInput`: "Some delivery note values are invalid. Check quantities,
      weights, dimensions and date order."
    - `Note.NumberExhausted`: "No more delivery note numbers are available."
    - `Recipient.NotFound`: "The selected recipient no longer exists."
    - Anything else, including network failures: "Something went wrong. Please try
      again."

## Agreed decisions

- Start on `/delivery-notes`; delivery notes are the main business object.
- Toolbar links rather than a side drawer or tabs.
- No note detail page; it belongs to a later story.
- One page with sections rather than a stepper.
- A plain select rather than an autocomplete for recipients.
- Free-text units, matching the backend; no fixed unit lists.
- Client-side numeric and date-order checks, because the backend reports them only as
  the field-less `Note.InvalidInput`.
- English UI text without internationalization support.
- Second grilling (2026-11-18): show backend field names as-is in field errors; add a
  no-recipients hint with a link; no Retry for a failed recipient load; keep the native
  date adapter defaults with an inline invalid-date message; show urgency as a text
  chip rather than an icon.

## Agreed test boundary

Frontend tests never call the real backend. Component tests use
`@testing-library/angular`, interact through labels, roles, and visible text, and
answer every HTTP request through `HttpTestingController`. Exactly these seven new
tests, plus the existing `app.routes.spec.ts` test updated to expect the empty path to
redirect to `/delivery-notes`:

1. The list shows notes from a mocked response, requested with `SortDir=Desc`.
2. An empty mocked list shows the empty-state message.
3. The toolbar's "Recipients" and "Delivery notes" links route to each page.
4. Submitting a complete valid form with two lines sends the expected POST body
   (lines in order, `yyyy-MM-dd` dates, `null` for empty optional fields); on 201 the
   snackbar shows and the app navigates to the list.
5. Submitting with a required field empty shows an inline error and sends no request.
6. A ship date earlier than the takeover date shows the date-order error and sends no
   request.
7. A mocked 400 `Note.InvalidInput` response opens the error dialog with its message
   and keeps the entered values.

Paging, loading indicators, Retry, adding and removing lines, the double-submit guard,
numeric minimums, the no-recipients hint, the recipient loading failure, the
invalid-date message, and the remaining error mappings are verified manually.

## Open questions

None. The developer confirmed shared understanding on 2026-11-18, after both grilling
sessions.

## Excluded scope

- Note detail, editing, deletion, and status changes.
- Filtering, searching, sorting controls, and page-size selection.
- Creating a recipient from within the note form.
- An unsaved-changes warning.
- Backend changes (the list endpoint is US-005).
- End-to-end tests, tests against the real backend, authentication, CORS, and
  deployment.

## Progress

Drafted on 2026-11-18 at the developer's request. Refined on 2026-11-18 through a
grilling session; the developer confirmed the agreed specification ("Yes"). A second
grilling session on 2026-11-18 settled the redirect test, field-error names, recipient
edge cases, date input behavior, and the urgent display; the developer confirmed it
("Yes"). The [task breakdown](US-006-delivery-note-pages/README.md) was created on
2026-11-18 (T001–T007, all todo).

Implemented on 2026-11-18 (T001–T007); agent review 02 returned `PASS`. On 2026-11-19
the developer approved human review and manual testing; see
[human acceptance 03](US-006-delivery-note-pages/review-03.md). Status `Done`.
