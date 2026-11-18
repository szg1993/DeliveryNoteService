# US-004: Review 02

- Review type: agent review
- Review date and time: 2026-11-18 11:11 +01:00
- Authoritative story: `docs/backlog/US-004-recipient-page.md`
- AgenticEngineering commit: `80199f355a77c687df695044257172578b803225` (branch `feature/add-angular-harness`)
- Reviewed head: `feature/US-004-recipient-page` at `a53451f58ec94e74bccec4d08f802a5c0f903a6e`
- Comparison base: `master` at `bed5cf01a8df023895caaa11f1863d128b3b0b1d` (repository default branch; no pull request exists and the story names no other target)
- Review coverage: complete
- Explicitly included uncommitted changes: none (working tree clean at review time)
- Final verdict: **PASS**

Follow-up metadata:

- Previous review report: [review-01.md](review-01.md) (agent review, committed in `85d3209`, verdict FAIL, coverage complete)
- Previous reviewed head: `41e16cc2fe587ea9ecbe7255ee3ceec1472df549`. It is an ancestor of the current reviewed head
- Committed follow-up change range: `41e16cc..a53451f` (`85d3209`, `1a5986d`, `a53451f`)
- Included working-tree diff: none
- Previous comparison-base commit: `master` at `bed5cf0`. The base has not moved, so base movement has no effect
- Policy or evidence change: none

## 1. Verdict and merge readiness

**PASS.** Ready for human review and manual verification.

This is a follow-up review. The baseline is review 01, the only earlier report. It is an agent review with complete coverage, so its conclusions for unchanged code are inherited and not repeated.

The only implementation change in the range is commit `1a5986d`, which reformats `web/angular.json` with the workspace Prettier. The file is semantically identical to the previous head, and `"analytics": false` is kept. `npm run format:check` now passes, so F001 is resolved and AC14 is met. Lint, tests, and the production build still pass. The follow-up changes introduce no new findings. F002 remains open as a non-blocking finding by the developer's decision.

Commit review:

- `85d3209` "US-004: Record review 01": administrative. Adds only `review-01.md`.
- `1a5986d` "US-004: Correct review 01 F001 formatting": correction 1 for F001. Changes `web/angular.json` (one array collapsed to one line) and appends a dated progress entry to `tasks/T001-scaffold-angular-workspace.md`. No requirement or status change.
- `a53451f` "US-004: Record correction 1 for review 01": administrative. Appends review history and correction notes to the plan `README.md` resume notes. No status, task outcome, or requirement change.

| Item | Type | First reported in | Previous severity | Outcome | Evidence |
|---|---|---|---|---|---|
| F001: `web/angular.json` fails `npm run format:check` (AC14) | Finding | review-01 | Blocking | Resolved | Correction 1 in `1a5986d` succeeded. `npm run format:check` in a fresh clone of `a53451f` reports "All matched files use Prettier code style!" (exit 0). The parsed JSON equals the `41e16cc` version |
| F002: creation test cannot tell the normalized response code from the entered code | Finding | review-01 | Non-blocking | Unresolved | No correction attempted; the developer chose not to correct it (plan resume notes in `a53451f`). `recipients-page.spec.ts` is unchanged in the range |

Correction history: F001 received one correction attempt (correction 1, `1a5986d`), which succeeded. No other item received a correction attempt.

## 2. Blocking findings

None

## 3. Non-blocking findings

### F002: The creation test cannot tell the normalized response code from the entered code (carried from review 01)

- First reported in: [review-01.md](review-01.md).
- Location: `web/src/app/recipients/recipients-page/recipients-page.spec.ts`, creation test and helper `answerCreateRequest` (unchanged since review 01).
- Rule: AC9 says the snackbar shows the normalized code from the response.
- Evidence: the test enters `BUDA01` and the mocked 201 echoes it, so the test would also pass if the snackbar used the entered code. The implementation is correct (review 01 check). The file is unchanged in the follow-up range.
- Suggested correction: enter a lower-case code and answer with the upper-case code in the existing creation test. Optional; the developer decided not to correct it.

## 4. Requirement traceability

Only AC14 and the workspace-scaffolding scope row are affected by the follow-up changes and were re-evaluated. Every other status is inherited from review 01, which completed those checks. Inherited evidence was not rerun, apart from the regression checks in section 5 (lint, the existing 13 tests, and the build). Paths are relative to `web/` unless they say otherwise.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
|---|---|---|---|---|
| Scope: create the first Angular application in `web/` following the harness Angular policy | Unchanged from review 01, except `angular.json` formatting (`1a5986d`) | Re-evaluated: fresh clone of `a53451f`, `npm ci`, `npm run format:check`, `npm run lint`, `npm test`, `npm run build` all pass | Met | None |
| Scope: move project-specific Angular defaults into repository instructions | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| Scope: one recipient page with the creation form above the list | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| Scope: create through `POST /api/recipients`, list through `GET /api/recipients` | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC1: list table with code, name, country, ZIP, city, address, foreign status | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC2: 10 per page, newest first, previous/next from paging metadata | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC3: empty-state message | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC4: loading indicator, error dialog, failure text and Retry | Unchanged | Inherited from review 01 review checks (not rerun) | Met (inherited) | None |
| AC5: required labelled inputs, free-text country | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC6: requiredness and maximum lengths inline, no request on violation | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC7: values sent unchanged, normalization in the backend | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC8: one POST, submit disabled while in progress | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC9: snackbar with normalized code, form cleared, list reloads page 1 | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | F002 (test strength only) |
| AC10: modal error dialog with readable message, raw code, OK, values kept | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC11: error-code to message mapping | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC12: same dialog for list-loading failures | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC13: backend through relative `/api`, dev-server proxy | Unchanged. `serve.options.proxyConfig` in `angular.json` is unaffected by the reformat (parsed JSON identical) | Inherited from review 01 (not rerun); proxy not exercised | Met (inherited) | None |
| AC14: the `web/` README lists install, run, test, lint, and format commands, and these commands succeed | `README.md` unchanged. `angular.json` reformatted in `1a5986d` | Re-evaluated: `npm ci`, `npm run format:check`, `npm run lint`, `npm test`, `npm run build` pass in a fresh clone. `npm start` and `npm run format` not run (section 9) | Met | F001 resolved |
| Agreed test boundary: component tests, `HttpTestingController`, no real backend or e2e | Unchanged | Inherited from review 01; the existing 13 tests rerun and pass | Met (inherited) | None |
| Agreed test boundary tests 1-6 | Unchanged | Inherited from review 01; rerun as part of `npm test` | Met (inherited) | None |

## 5. Verification performed

The npm commands ran from `web/` in a fresh clone of the reviewed head `a53451f` (`git clone --branch feature/US-004-recipient-page <repository-root> <temporary-directory>`), which kept the developer's `node_modules` and build output untouched. The reviewer did not start, call, or write to the real backend API.

| Command | Result |
|---|---|
| `git status` (implementation repository) | Clean. Branch `feature/US-004-recipient-page`, HEAD `a53451f` |
| `git merge-base --is-ancestor 41e16cc HEAD` | Exit 0 (previous reviewed head is an ancestor) |
| `git merge-base master HEAD` and `git rev-parse master` | Both `bed5cf0` (base unchanged since review 01) |
| `git log --stat 41e16cc..HEAD` and `git show` of each commit | Three commits, as listed in section 1 |
| `npm ci` | Passed (exit 0). The same four install-script approval warnings as review 01 |
| `npm run format:check` | Passed (exit 0): "All matched files use Prettier code style!" |
| `npm run lint` | Passed (exit 0): "All files pass linting." |
| `npm test` | Passed (exit 0): 2 files, 13 tests |
| `npm run build` | Passed (exit 0): application bundle generation complete, lazy `recipients-page` chunk |
| `node -e` comparison of parsed `web/angular.json` at `41e16cc` and `a53451f` | "semantically equal: true" |

## 6. Code-guideline assessment

Applied policy: repository `CLAUDE.md` (simplicity preferences), `web/CLAUDE.md`, the AgenticEngineering global policy, and the Angular policy index at `80199f3` (TypeScript style and new-application formatting rules for the changed workspace file). The `clean-code` skill was applied to the follow-up change.

- **Formatting:** all files now pass the configured Prettier check. The change is the minimal formatter output for `web/angular.json`.
- **Readability and design:** the correction is formatting-only. No code, test, or behavior changed, so no refactoring is warranted. The T001 progress entry records the correction, its refactoring assessment, and its verification.
- **Tests:** no test change was needed for a formatting correction. F002 stays open as a non-blocking item.

## 7. Architecture assessment

The follow-up range changes no source, contract, configuration value, or dependency. `web/angular.json` is semantically unchanged, so the proxy, build, and test configuration are unaffected. The review 01 architecture assessment (no defects) is inherited.

No architecture defects found.

## 8. Recorded deviations

The deviations recorded in review 01 still apply and are unchanged:

1. Tests beyond the agreed six (acknowledged in T003 and T005).
2. Manual checks replaced by throwaway specs (acknowledged in T004 and T006).
3. Angular version ranges kept as generated, with the lock file pinning exact versions (acknowledged in T001).
4. T007 (frontend CI) cancelled by the developer.
5. T008 harness change in AgenticEngineering `80199f3` on `feature/add-angular-harness`, merging separately.
6. `"analytics": false` in `web/angular.json` from the developer's commit `b9b9c16`. Correction 1 keeps it, and T001 now records it.

No new deviations.

## 9. Unverified items and remaining risks

- `npm start` and the `/api` proxy were not exercised, because the review must not start or call the real backend. The proxy configuration is semantically unchanged since review 01.
- `npm run format` was not run, because it would rewrite files. The format check passes.
- The story's manual checks against a running backend (loading indicator, Retry, double-submit guard, remaining error mappings) remain for human review, as in review 01.
- `npm ci` still warns that four packages' install scripts are not approved. Lint, test, and build work without them.
