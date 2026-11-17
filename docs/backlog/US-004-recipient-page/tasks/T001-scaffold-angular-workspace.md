# T001: Scaffold the Angular workspace

- Parent: [US-004](../README.md)
- Status: todo
- Depends on: None
- Increment: Prerequisite

## Outcome

`web/` contains a working Angular application whose start route shows an empty
"Recipients" page, with linting, formatting, component tests, Angular Material, and
the `/api` dev proxy in place. Unlocks every later task.

## Scope

- Create the workspace in `DeliveryNoteService/web/` with the latest stable Angular
  CLI, following the harness
  [new-application guide](../../../../../AgenticEngineering/agent-harness/technologies/angular/new-application.md):
  SCSS, no SSR, CLI defaults for zoneless standalone components. Commit the lock
  file.
- Add angular-eslint (`ng add angular-eslint`), Prettier with `.prettierrc` and
  `format` / `format:check` scripts, `@testing-library/angular` on the CLI's Vitest
  runner, and Angular Material (`ng add @angular/material`) with one theme.
- Add `proxy.conf.json` forwarding `/api` to `http://localhost:5211`, referenced
  from the `serve` target.
- Add `app.routes.ts` with a lazy-loaded `/recipients` route from
  `src/app/recipients/`, redirecting the empty path to it. The page shows only a
  "Recipients" heading.
- Provide `HttpClient` in `app.config.ts`.
- Add `web/README.md` listing the install, run (with the backend URL the proxy
  expects), test, lint, format, and format-check commands.

No recipient API calls yet.

## Acceptance criteria

- [ ] `npm ci`, `npm run lint`, `npm test` (single run), `npm run build`, and
      `npm run format:check` succeed in `web/`.
- [ ] Navigating to `/` shows the "Recipients" heading at `/recipients`.
- [ ] One component test, using Testing Library and `RouterTestingHarness`,
      proves the start route renders the heading.
- [ ] `/api` requests from the dev server are proxied to `http://localhost:5211`.
- [ ] The README commands match the `package.json` scripts.

## Verification

Run the README commands in a clean `web/` checkout. Start the dev server and
confirm the page loads and that `/api/recipients` is proxied, assuming the backend
is running; otherwise confirm the proxy configuration is referenced by `ng serve`.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
