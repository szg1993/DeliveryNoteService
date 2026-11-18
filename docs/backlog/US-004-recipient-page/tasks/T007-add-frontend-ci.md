# T007: Add frontend CI workflow

- Parent: [US-004](../README.md)
- Status: cancelled
- Depends on: T001
- Increment: Prerequisite

## Outcome

Pull requests that change the frontend are checked automatically.

## Scope

- Add `.github/workflows/web.yml` (the repository has no workflow files yet).
- Trigger on `pull_request` with a `paths` filter for `web/**` and the workflow
  file itself.
- Use `actions/setup-node` with the Node major version used locally (24) and npm
  caching keyed on `web/package-lock.json`.
- In `web/`: `npm ci`, `npm run lint`, `npm run format:check`, `npm test` (single
  run, no watch), `npm run build`.

## Acceptance criteria

- [ ] The workflow runs only for pull requests that change `web/` or the workflow.
- [ ] It runs install, lint, format check, tests, and build, and fails on any
      failure.

## Verification

Validate the YAML and run each workflow command locally in `web/` to confirm it
succeeds non-interactively. The first real run happens on the story pull request;
record its result in this task's progress when available.

## Progress

- 2026-11-17: Added `.github/workflows/web.yml`: `pull_request` trigger with
  `paths` for `web/**` and the workflow file; one `ubuntu-latest` job with
  `working-directory: web`; `actions/setup-node` with Node 24 and npm caching on
  `web/package-lock.json`; steps `npm ci`, `npm run lint`,
  `npm run format:check`, `npm test` (script already runs `--watch=false`),
  `npm run build`. Each step fails the job on a non-zero exit.
- 2026-11-17: The YAML parses and passes `prettier --check` with the workspace
  Prettier.
- 2026-11-17: Local `npm ci` failed with `EBUSY` because two running
  `ng serve` processes (ports 4200 and 4300) lock
  `node_modules/sass-embedded-win32-x64`. The failed run removed part of
  `node_modules`, so the remaining commands could not run. With the user's
  approval, both `ng serve` processes were stopped, but `npm ci` still failed
  with `EBUSY` on the same empty folder. It is held by a third `node` → `dart`
  process tree (started 15:41) whose command line and owner are not visible
  without elevation. Stopping it (`Stop-Process -Force`, `taskkill /T /F`)
  failed with "Access is denied", so it runs elevated or as another user.
- 2026-11-17: Cancelled by the developer's decision not to implement T007. The
  workflow file was deleted and never committed. AC15 and the CI scope item were
  removed from the source story and recorded under excluded scope, so this task
  is no longer part of the completion contract. No remaining work.
