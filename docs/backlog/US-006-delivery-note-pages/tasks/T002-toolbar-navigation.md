# T002: Navigate with toolbar links

- Parent: [US-006](../README.md)
- Status: todo
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

- [ ] The toolbar shows "Delivery notes" and "Recipients" links.
- [ ] Clicking each link routes to its page.
- [ ] The current page's link is visibly highlighted (for example via
      `aria-current="page"` plus an active class).

## Verification

TDD: agreed test 3 renders the app shell with the real routes, clicks the
"Recipients" and "Delivery notes" links by role and name, answers the resulting
list requests through `HttpTestingController`, and checks each page heading.
Check the highlight manually in the dev server or browser pane with mocked or no
backend data. Then run lint, test, build, and format check in `web/`.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
