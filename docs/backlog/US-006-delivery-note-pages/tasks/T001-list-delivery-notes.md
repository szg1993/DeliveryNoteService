# T001: List delivery notes newest first

- Parent: [US-006](../README.md)
- Status: done
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

- [x] `/` redirects to `/delivery-notes`, which is lazy loaded and titled
      "Delivery notes".
- [x] The list request uses `sortDir=Desc` and a page size of 10.
- [x] The table shows the agreed columns, status text, the "Urgent" chip, and
      `yyyy-MM-dd` dates.
- [x] An empty response shows the empty-state message and no table.

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

2026-11-18:

- Started T001; moved US-006 from Ready to In progress.
- Red/green slices: updated `app.routes.spec.ts` (failed on the missing "Delivery
  notes" heading, then passed with the new `/delivery-notes` route and redirect);
  agreed test 1 (failed with no `/api/delivery-notes?page=1&limit=10&sortBy=Id&sortDir=Desc`
  request, then passed with `DeliveryNotesApi` and the table); agreed test 2 (the
  empty-state branch was written with the table, so the test was checked by
  temporarily removing that branch: it failed, then passed once restored).
- Added `delivery-notes/delivery-notes-api.ts` (`DeliveryNoteDetails` types only the
  listed fields; status as `'Pending' | 'Approved' | 'Rejected'`) and
  `delivery-notes/delivery-notes-page/` (generated with `ng generate`; `rxResource`
  for the first page, Material table, `mat-chip` "Urgent", "No delivery notes yet."
  empty state). Recipient cell shows "CODE – Name". The route is lazy loaded with
  title "Delivery notes"; the empty path redirects to it.
- Moved `Paging` and `PagedResponse<T>` from `recipients-api.ts` to
  `shared/paged-response.ts`; updated the recipient imports.
- Refactoring assessment (clean-code): the spec's `tableRowTexts` was identical to the
  one in `recipients-page.spec.ts`. Inspected both consumers and moved it to
  `src/testing/table.ts` as a stateless DOM query helper (no configuration,
  lifecycle, or shared state; each test still renders its own page and
  `HttpTestingController`). Excluded `src/testing/**` from `tsconfig.app.json` so test
  helpers stay out of the app compilation. Request-answering and render helpers stay
  local because their URLs and components differ. The page SCSS repeats three small
  rules from the recipients page (`.page-title`, `.message`, `.table-container`); kept
  them component-scoped rather than adding global styles for two small consumers. No
  other refactoring justified.
- Verification in `web/`: `npm run lint` all files pass; `npm test` 3 files, 15
  tests passed; `npm run build` succeeded (`delivery-notes-page` lazy chunk);
  `npm run format:check` all files formatted. No real backend was called.

Remaining: none for T001. Paging, loading, and errors are T003.
