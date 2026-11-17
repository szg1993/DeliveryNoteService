# T002: List recipients newest first

- Parent: [US-004](../README.md)
- Status: done
- Depends on: T001
- Increment: Skeleton

## Outcome

Opening the recipient page shows the first 10 recipients from the backend, newest
first, or an empty-state message when there are none. This is the first working
increment.

## Scope

- Add a hand-written typed `RecipientsApi` service in `src/app/recipients/` with
  `getRecipients(page)` calling
  `GET /api/recipients?page={page}&limit=10&sortBy=Id&sortDir=Desc`, and the
  `RecipientDetails` and paged-response types matching the contract in the
  [plan](../README.md#assumptions-and-open-questions).
- Show the recipients in an Angular Material table with code, name, country, ZIP,
  city, address, and foreign status (shown as Yes/No).
- Show an empty-state message when `content` is empty.
- Keep page state in signals.

Paging controls, loading indicator, and error handling are later tasks.

## Acceptance criteria

- [x] Opening the page sends one request for page 1, limit 10, sorted by ID
      descending, through the relative `/api` path.
- [x] The table shows every column for each returned recipient.
- [x] An empty response shows the empty-state message and no table rows.

## Verification

Component tests with `HttpTestingController` (story test boundary tests 1 and 2):

1. A mocked response with two recipients renders both rows with their details.
2. An empty mocked response shows the empty-state message.

Each test asserts the request URL and query. `npm run lint`, `npm test`, and
`npm run format:check` pass.

## Progress

### 2026-11-17

- Added `RecipientsApi` (`src/app/recipients/recipients-api.ts`), an Angular 22
  `@Service()` with `getRecipients(page)` calling
  `GET /api/recipients?page={page}&limit=10&sortBy=Id&sortDir=Desc` through the
  relative `/api` path, plus the `RecipientDetails`, `Paging`, and `PagedResponse`
  contract types.
- `RecipientsPage` keeps the page number in a signal and loads it with `rxResource`
  (stable in Angular 22). It renders an Angular Material table (Code, Name, Country,
  ZIP, City, Address, Foreign as Yes/No), or "No recipients yet." when `content` is
  empty. Before the first response the table shows only its header; the loading
  indicator is T004.
- TDD: each test was seen failing first (no request sent; empty-state text missing),
  then passing. Mutation check: changing `sortDir` to `Asc` fails both page tests.
- Component tests in `recipients-page.spec.ts` (story test boundary tests 1 and 2):
  two recipients render as rows with every column; an empty response shows the
  empty-state message and no rows. Each matches the exact request URL and query
  through `HttpTestingController.expectOne`, and `afterEach` verifies no
  unexpected requests.
- `app.routes.spec.ts` now provides `provideHttpClient()` and
  `provideHttpClientTesting()`, because the routed page now sends a request. This
  keeps that test away from the network.
- Refactoring assessment (`clean-code`): production code is small and has a single
  responsibility. The table columns are kept explicit in the template rather than
  generated, so they stay readable. In the tests, the repeated render providers,
  request matching, response envelope, stabilization, and row-text extraction
  moved into helpers: `renderRecipientsPage`, `answerRecipientsRequest`,
  `recipient`, and `tableRowTexts`. Recipient field values, the requested page, and
  the expected rows stay visible in each test.
- Test-infrastructure scope: the helpers are local to `recipients-page.spec.ts`.
  The only other spec, `app.routes.spec.ts`, exercises routing and shares only the
  HTTP providers, so a shared suite module would add no useful reuse yet.
  Configuration happens in `render` per test, through a fresh TestBed. Each test is
  isolated, and TestBed owns teardown. T003–T006 extend the same spec file and can
  reuse these helpers; promote them to a shared module only if another spec needs
  them.
- Verification: `npm run lint` passed, `npm test` passed (3 tests in 2 files),
  `npm run format:check` passed, and `npm run build` succeeded.

Remaining work: none for T002. Paging controls (T003) and loading/error handling
(T004) are separate tasks.
