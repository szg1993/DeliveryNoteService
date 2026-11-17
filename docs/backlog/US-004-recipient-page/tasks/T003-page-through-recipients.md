# T003: Page through recipients

- Parent: [US-004](../README.md)
- Status: done
- Depends on: T002
- Increment: Extension

## Outcome

Users can move between pages of recipients with previous and next controls.

## Scope

- Add previous and next controls below the table, driven by the response's
  `paging.page` and `paging.totalPages`. Previous is disabled on page 1; next is
  disabled on the last page.
- Selecting a control requests that page with the same limit and sort.
- Show the current page and total pages, such as "Page 1 of 3".
- Hide or disable the controls when the list is empty.

## Acceptance criteria

- [x] Next requests the following page; previous requests the preceding one.
- [x] Controls are disabled at the first and last page.
- [x] The current page and total pages are visible.

## Verification

Component test with `HttpTestingController` (story test boundary test 6): with a
first-page response reporting several pages, clicking next sends a request for
page 2 and renders its rows. `npm run lint`, `npm test`, and `npm run format:check`
pass. Check the disabled states manually.

## Progress

### 2026-11-17

- `RecipientsPage` shows a `Recipient pages` navigation below the table with
  Material "Previous page" and "Next page" buttons and a "Page X of Y" label taken
  from the response's `paging`. Previous is disabled when `paging.page <= 1`; next
  is disabled when `paging.page >= paging.totalPages`. The controls render only
  after a response arrives and stay inside the non-empty branch, so an empty list
  shows no controls.
- Selecting a control updates the page signal by one; `rxResource` then requests
  that page through `RecipientsApi.getRecipients(page)`, which keeps limit 10 and
  `sortBy=Id&sortDir=Desc`.
- TDD in vertical slices, each test seen failing first: next (no button found),
  previous (no button found), page label (text not found), and both disabled-state
  tests (`expected false to be true`).
- Component tests in `recipients-page.spec.ts` (story test boundary test 6 and the
  rest of the acceptance criteria): next requests page 2 and renders its rows;
  previous from page 2 requests page 1 again; "Page 1 of 3" is shown; previous is
  disabled and next enabled on page 1 of 2; next is disabled and previous enabled
  on page 2 of 2. The empty-state test also asserts that no page navigation is
  shown; that assertion guards existing behavior and was not seen failing. Every
  request is matched by exact URL and query through `HttpTestingController`.
- Refactoring assessment (`clean-code`): production code is small, and the two
  handlers and template conditions read directly, so no production refactoring was
  justified. In the tests, `answerRecipientsRequest` gained an optional
  `totalPages` so the paging metadata stays visible in each scenario. Added
  `recipientWithCode` for rows identified only by code, `listedCodes` instead of
  indexing raw row text, and `pagingButton` used for every control lookup.
- Test-infrastructure scope: helpers stay local to `recipients-page.spec.ts`. The
  only other spec, `app.routes.spec.ts`, tests routing and does not page, so no
  shared module adds reuse. Each test renders a fresh TestBed in `render`; TestBed
  owns teardown and `afterEach` verifies no unexpected requests.
- Verification: `npm test` passed (8 tests in 2 files), `npm run lint` passed,
  `npm run format:check` passed, and `npm run build` succeeded.

Remaining work: the manual check of the disabled states against a running backend
was not done in this session; component tests cover them. Do it during human
review of the story.
