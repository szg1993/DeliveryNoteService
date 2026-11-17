# T002: List recipients newest first

- Parent: [US-004](../README.md)
- Status: todo
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

- [ ] Opening the page sends one request for page 1, limit 10, sorted by ID
      descending, through the relative `/api` path.
- [ ] The table shows every column for each returned recipient.
- [ ] An empty response shows the empty-state message and no table rows.

## Verification

Component tests with `HttpTestingController` (story test boundary tests 1 and 2):

1. A mocked response with two recipients renders both rows with their details.
2. An empty mocked response shows the empty-state message.

Each test asserts the request URL and query. `npm run lint`, `npm test`, and
`npm run format:check` pass.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
