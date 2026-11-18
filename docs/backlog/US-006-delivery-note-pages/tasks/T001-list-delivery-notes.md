# T001: List delivery notes newest first

- Parent: [US-006](../README.md)
- Status: todo
- Depends on: None (US-005 endpoint is merged; tests mock it)
- Increment: Skeleton

## Outcome

Opening the app lands on a "Delivery notes" page that lists the newest notes from
`GET /api/delivery-notes`, or an empty-state message when there are none.

## Scope

- Add `web/src/app/delivery-notes/delivery-notes-api.ts`: a hand-written
  `DeliveryNotesApi` following `RecipientsApi`, with typed note details (only the
  fields the pages use need precise typing) and `getDeliveryNotes(page)` requesting
  `page`, `limit: 10`, `sortBy: 'Id'`, `sortDir: 'Desc'`. Reuse `PagedResponse<T>`;
  move it with `Paging` to `shared/` since two features now use it.
- Add a lazy-loaded `/delivery-notes` route with title "Delivery notes" and a
  `DeliveryNotesPage` in `web/src/app/delivery-notes/`. Redirect the empty path to
  `/delivery-notes`.
- The page shows a "Delivery notes" heading and a Material table with note number,
  recipient (code and name), category, takeover place, ship date, estimated arrival
  date, status text from `noteStatusId`, and an "Urgent" column with a plain
  "Urgent" chip only on urgent notes. Dates are shown as returned.
- With no notes, show an empty-state message instead of the table.

Paging controls, loading indicator, and error handling belong to T003. Rows are not
clickable.

## Acceptance criteria

- [ ] `/` redirects to `/delivery-notes`, which is lazy loaded and titled
      "Delivery notes".
- [ ] The list request uses `sortDir=Desc` and a page size of 10.
- [ ] The table shows the agreed columns, status text, the "Urgent" chip, and
      `yyyy-MM-dd` dates.
- [ ] An empty response shows the empty-state message and no table.

## Verification

TDD with `@testing-library/angular` and `HttpTestingController`:

- Update `app.routes.spec.ts` so the start route expects the "Delivery notes"
  heading at `/delivery-notes`.
- Agreed test 1: the list shows notes from a mocked response requested with
  `SortDir=Desc`.
- Agreed test 2: an empty mocked list shows the empty-state message.

Then run `npm run lint`, `npm test`, `npm run build`, and `npm run format:check` in
`web/`. Never call the real backend.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
