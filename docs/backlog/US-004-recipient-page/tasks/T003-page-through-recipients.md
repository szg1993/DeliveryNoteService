# T003: Page through recipients

- Parent: [US-004](../README.md)
- Status: todo
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

- [ ] Next requests the following page; previous requests the preceding one.
- [ ] Controls are disabled at the first and last page.
- [ ] The current page and total pages are visible.

## Verification

Component test with `HttpTestingController` (story test boundary test 6): with a
first-page response reporting several pages, clicking next sends a request for
page 2 and renders its rows. `npm run lint`, `npm test`, and `npm run format:check`
pass. Check the disabled states manually.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
