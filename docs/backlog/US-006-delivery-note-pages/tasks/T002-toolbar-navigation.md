# T002: Navigate with toolbar links

- Parent: [US-006](../README.md)
- Status: done
- Depends on: T001
- Increment: Extension

## Outcome

Users move between the delivery-note and recipient pages from the top toolbar, and
the link for the current page is visibly highlighted.

## Scope

- In `web/src/app/app.html` and `app.ts`, add "Delivery notes" and "Recipients"
  links to the existing toolbar using `routerLink` and `routerLinkActive` with a
  visible active style. Keep the "Delivery Note Service" title.
- No side drawer or tabs.

## Acceptance criteria

- [x] The toolbar shows "Delivery notes" and "Recipients" links.
- [x] Clicking each link routes to its page.
- [x] The current page's link is visibly highlighted (for example via
      `aria-current="page"` plus an active class).

## Verification

TDD: agreed test 3 renders the app shell with the real routes, clicks the
"Recipients" and "Delivery notes" links by role and name, answers the resulting
list requests through `HttpTestingController`, and checks each page heading.
Check the highlight manually in the dev server or browser pane with mocked or no
backend data. Then run lint, test, build, and format check in `web/`.

## Progress

- 2026-11-18: Wrote agreed test 3 in `web/src/app/app.spec.ts` first. It renders
  `App` with the real `routes`, clicks "Recipients" then "Delivery notes" by role and
  name, answers each lazily loaded page's first list request through
  `HttpTestingController`, and checks the page's level-1 heading. It failed (no
  "Recipients" link), then passed after the change.
- 2026-11-18: `app.html`/`app.ts` add a `<nav>` with "Delivery notes" and
  "Recipients" links (`routerLink`, `routerLinkActive="active-link"`,
  `ariaCurrentWhenActive="page"`); the "Delivery Note Service" title stays.
  `app.scss` shows the active link as an inverted pill (on-primary background,
  primary text). No drawer or tabs.
- 2026-11-18: Test note: the click helper does not await
  `ApplicationRef.whenStable()`, because Angular counts a pending HTTP request as a
  pending task, so stability would wait for the request the test has not answered
  yet (the first attempt timed out). The answer helper uses `waitFor` for the lazy
  route load instead.
- 2026-11-18: Manual highlight check: `ng serve` on port 4317 with an empty proxy
  config, so no request could reach the real backend (list requests 404ed and the
  error dialog was closed). In the browser pane, at `/delivery-notes` the
  "Delivery notes" link had `aria-current="page"`, class `active-link`, white
  background and primary text; after clicking "Recipients" the URL was
  `/recipients` and the highlight moved to "Recipients".
- 2026-11-18: Refactoring assessment (clean-code): the production change is small
  template and style edits with nothing worth extracting. The new test helper
  `answerEmptyListRequest` resembles the page specs' `answerDeliveryNotesRequest`
  and `answerRecipientsRequest` (waitFor, expectOne, flush, whenStable). All three
  consumers were inspected: the page helpers own scenario-specific URLs, paging
  values, and item builders, so a shared helper would move only three lines of
  mechanics while adding parameters. Local helpers suffice; no shared
  `web/src/testing` helper was added. Each test gets a fresh TestBed and
  `HttpTestingController`, verified in `afterEach`. No refactoring applied.
- 2026-11-18: Verification in `web/`: `npx prettier --write` on the four changed
  files (unchanged), `npm run lint` (all files pass), `npm test` (4 files, 16 tests
  passed), `npm run build` (success), `npm run format:check` (pass).

Remaining work: none.
