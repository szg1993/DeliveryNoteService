# US-004: Recipient page

- Status: Ready
- Type: Frontend
- Breakdown: [Implementation plan and tasks](US-004-recipient-page/README.md).
- Branch: feature/US-004-recipient-page
- Epic: [Recipient management](epics/recipient-management.md)
- Implementation repository: DeliveryNoteService
- Implementation workspace: `web/` Angular application
- Dependencies: `POST /api/recipients` and `GET /api/recipients` from
  [US-001](US-001-create-and-view-recipients.md). No backend changes.

## User story

As a delivery-note user, I want a web page where I can register recipients and see
the recipients already registered, so that I can maintain delivery destinations
without calling the API directly.

## Scope

- Create the first Angular application for DeliveryNoteService in `web/`, following
  the harness [Angular policy](../../../AgenticEngineering/agent-harness/technologies/angular/index.md).
- Move the project-specific Angular defaults (Angular Material, the `/api` dev
  proxy, no NgRx, and the testing approach agreed below) from the harness
  [new-application guide](../../../AgenticEngineering/agent-harness/technologies/angular/new-application.md)
  into this repository's instructions, leaving only general setup rules in the
  harness.
- Add one recipient page with a creation form above a recipient list.
- Create recipients through `POST /api/recipients` and list them through
  `GET /api/recipients`.
- Add a GitHub Actions workflow for the frontend.

## Acceptance criteria

### Listing

1. Opening the page loads recipients from `GET /api/recipients` and shows them in a
   table with code, name, country, ZIP, city, address, and foreign status.
2. The list shows 10 recipients per page, newest first (ID descending), with
   previous and next page controls driven by the backend's paging metadata.
3. With no recipients, the page shows an empty-state message instead of an empty
   table.
4. While the list is loading, the page shows a loading indicator. If loading fails,
   the error dialog (AC12) opens, and after it is closed the list area shows
   "Recipients could not be loaded." with a Retry button that reloads the list.

### Creation

5. The form has required inputs for code, name, country, ZIP, city, and address,
   labelled so they can be found by their visible labels. Country is a free-text
   two-letter code field.
6. The form enforces requiredness and the US-001 maximum lengths (code 10, name 200,
   country 2, ZIP 20, city 50, address 200). Violations are shown inline under the
   field and block submission without sending a request. The recipient-code
   character rule and country-code recognition stay in the backend.
7. The form does not trim or otherwise change name, ZIP, city, or address before
   sending them. Code and country normalization stays in the backend.
8. Submitting valid details sends one `POST /api/recipients` request. While it is in
   progress the submit button is disabled, so a double click does not send a
   second request.
9. On HTTP 201, a snackbar shows "Recipient {CODE} created" with the normalized code
   from the response, the form is cleared, and the list reloads on its first page so
   the new recipient is visible at the top.

### Backend errors

10. Every backend error response, from creation or listing, and every network
    failure opens a modal error dialog with a readable English message, the raw
    error code in small grey text when one exists, and an OK button. Entered form
    values are kept.
11. The dialog maps error codes to messages:
    - `Recipient.AlreadyExists`: "A recipient with code {CODE} already exists."
    - `error:DNC::FieldMissing`, `error:DNC::FieldTooLong`, and
      `Recipient.InvalidCode`: "The {field} field is invalid.", using the error's
      `Data.Field` when present and the code field for `Recipient.InvalidCode`.
    - `Recipient.InvalidCountry`: "Unknown country code."
    - Anything else, including network failures and the 409 `UniqueKeyConflict`
      race from US-001: "Something went wrong. Please try again."
12. The same dialog is used for list-loading failures (AC4).

### Application

13. The application calls the backend only through the relative `/api` path; local
    development forwards `/api` to the backend through the Angular dev-server proxy.
14. The `web/` README lists the commands to install, run, test, lint, and format the
    application, and these commands succeed.
15. A GitHub Actions workflow runs lint, tests, and build for pull requests that
    change files under `web/`.

## Agreed decisions

- Frontend and backend work are separate stories in one backlog, distinguished by
  their `Type`. This story changes no backend code.
- Harness new-application defaults apply: latest stable Angular CLI, SCSS, no SSR,
  zoneless standalone components, angular-eslint, Prettier, Vitest with
  `@testing-library/angular`, Angular Material, signals instead of a state library,
  and a hand-written typed `RecipientsApi` client.
- One route, `/recipients`, which is also the start route, lazy loaded from
  `src/app/recipients/`. No separate detail page or creation dialog.
- English UI text without internationalization support.
- No sorting controls, search, or page-size selector; newest first is fixed.
- Client-side validation covers requiredness and maximum lengths only, shown
  inline. Backend errors use the modal dialog; the success confirmation uses a
  snackbar.
- No CORS. The dev-server proxy avoids cross-origin calls locally; the same-origin
  or CORS decision belongs to a later deployment story.
- No authentication, matching the backend.

## Agreed test boundary

Frontend tests never call the real backend. Component tests render the recipient
page with `@testing-library/angular`, interact through labels, roles, and visible
text, and answer every HTTP request with mocked responses through Angular's
`HttpTestingController`. No real backend, end-to-end, or browser tests.

This is a hobby project: test the major behaviors only, with these six tests:

1. The list shows the recipients from a mocked response.
2. An empty mocked response shows the empty-state message.
3. Submitting valid details sends the expected POST body; on 201 the page shows the
   snackbar, clears the form, and requests the first page of the list again.
4. A mocked 400 `Recipient.AlreadyExists` response opens the error dialog with the
   readable message and keeps the entered values.
5. Submitting with a required field empty shows an inline error and sends no request.
6. The next-page control requests page 2.

The loading indicator, the double-submit guard, the Retry action, and the remaining
error mappings are verified manually rather than by dedicated tests.

## Open questions

None. The developer confirmed shared understanding on 2026-11-17.

## Excluded scope

- Viewing a single recipient's detail page, updating, or deleting recipients.
- Searching, sorting, and page-size selection.
- Delivery-note pages.
- End-to-end tests and tests against the real backend.
- Authentication, CORS, deployment, and production hosting.
- Backend changes.

## Progress

Drafted on 2026-11-17, replacing an earlier creation-only draft at the developer's
request to cover creation and listing. Refined on 2026-11-17 through a grilling
session; the developer confirmed the agreed specification ("Agree."). The
[task breakdown](US-004-recipient-page/README.md) was created on 2026-11-17 (T001–T008, all todo).
