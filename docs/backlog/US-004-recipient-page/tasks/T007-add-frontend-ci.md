# T007: Add frontend CI workflow

- Parent: [US-004](../README.md)
- Status: todo
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

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
