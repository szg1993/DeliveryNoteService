# T001: Scaffold the Angular workspace

- Parent: [US-004](../README.md)
- Status: done
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

- [x] `npm ci`, `npm run lint`, `npm test` (single run), `npm run build`, and
      `npm run format:check` succeed in `web/`.
- [x] Navigating to `/` shows the "Recipients" heading at `/recipients`.
- [x] One component test, using Testing Library and `RouterTestingHarness`,
      proves the start route renders the heading.
- [x] `/api` requests from the dev server are proxied to `http://localhost:5211`.
- [x] The README commands match the `package.json` scripts.

## Verification

Run the README commands in a clean `web/` checkout. Start the dev server and
confirm the page loads and that `/api/recipients` is proxied, assuming the backend
is running; otherwise confirm the proxy configuration is referenced by `ng serve`.

## Progress

### 2026-11-17

- Resumed reservation attempt `b406b863-a795-40e7-8d49-0dff1954fca3` on
  `feature/US-004-recipient-page` after explicit user direction; remote marker
  `6ebc678` matched the local record.
- Created `web/` with `npx @angular/cli@latest new web --style=scss --ssr=false
  --skip-git`: Angular 22.2 (CLI 22.2.7), TypeScript 6.0, zoneless standalone
  defaults, Vitest runner, lock file committed.
- New-application defaults applied: `ng add angular-eslint` (22.7.0),
  CLI-generated Prettier with `.prettierrc` plus `format` / `format:check`
  scripts, `@testing-library/angular` and `@testing-library/dom`,
  `ng add @angular/material` (22.2.7) with one azure/blue Material 3 theme in
  `src/styles.scss`. Added `strict` and `strictTemplates` explicitly to
  `tsconfig.json` per the TypeScript style guide (the CLI relies on defaults).
  Replaced: `test` script runs once (`ng test --watch=false`) so `npm test`
  matches the agreed single-run command. Version ranges kept as generated; the
  lock file pins exact versions.
- Added `proxy.conf.json` (`/api` → `http://localhost:5211`) referenced from the
  `serve` target options, `provideHttpClient()` in `app.config.ts`, and
  `app.routes.ts` redirecting `` to the lazy-loaded `recipients` route
  (`loadComponent`, title "Recipients"). The page shows only the heading.
  Replaced the CLI welcome page with a bare `<router-outlet />` shell.
- TDD: `src/app/app.routes.spec.ts` (RouterTestingHarness + Testing Library
  `findByRole`) failed first (no heading), then passed after adding the routes.
- Refactoring assessment (clean-code): removed the unused `title` signal, the
  CLI welcome template, the generated default `app.spec.ts`, and empty
  component stylesheets. No further refactoring justified for this scaffold.
  Test infrastructure: one component test; local setup suffices. A shared
  harness (HTTP substitute via `HttpTestingController`) is deferred to T002, its
  first consumer.
- Verification after deleting `node_modules`, `dist`, `.angular`: `npm ci`
  succeeded; `npm run lint` all files pass; `npm test` 1 passed; `npm run
  build` complete (lazy `recipients-page` chunk); `npm run format:check` all
  files formatted.
- Dev server: `npm start` served `/`, which redirected to `/recipients` with
  heading and title "Recipients" (checked in the browser pane). With a stub HTTP
  server on port 5211 (backend not running), `GET
  http://localhost:4200/api/recipients?page=1` returned the stub response for
  path `/api/recipients?page=1`, proving the proxy forwards to 5211.
- npm 11 reported unapproved install scripts (esbuild, lmdb, msgpackr-extract);
  lint, test, build and serve work without them.
- A fresh clone with `core.autocrlf=true` checked `web/` out with CRLF, and
  `npm run format:check` failed on 10 files. Added `web/.gitattributes`
  (`* text=auto eol=lf`). Re-verified in a fresh clone of the branch: `npm ci`,
  `npm run format:check`, `npm run lint`, `npm test` (1 passed) and
  `npm run build` all succeed.

### 2026-11-18

- Review correction for [review-01](../review-01.md) F001 (AC14,
  `npm run format:check` failed on `web/angular.json`). Ran
  `npx prettier --write angular.json` from `web/`: `schematicCollections` is back
  on one line. Kept the developer's `"analytics": false` from `b9b9c16`.
- Refactoring assessment (clean-code): formatting-only change, no refactoring
  justified.
- Verification from `web/`: `npm run format:check` all files formatted;
  `npm run lint` all files pass; `npm test` 2 files, 13 tests passed;
  `npm run build` complete. The real backend was not started or called.

Remaining work: none.
