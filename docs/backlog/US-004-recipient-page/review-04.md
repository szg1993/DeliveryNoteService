# US-004: Review 04

- Review type: agent review
- Review date and time: 2026-11-18 11:50 +01:00
- Authoritative story: `docs/backlog/US-004-recipient-page.md`
- AgenticEngineering commit: `80199f355a77c687df695044257172578b803225` (branch `feature/add-angular-harness`)
- Reviewed head: `feature/US-004-recipient-page` at `50bcef2ad27b973a0e0df2190524676693c05bfd`
- Comparison base: `master` at `bed5cf01a8df023895caaa11f1863d128b3b0b1d` (repository default branch; no pull request exists and the story names no other target)
- Review coverage: complete
- Explicitly included uncommitted changes: none. The working tree was clean at review time
- Final verdict: **PASS**

Follow-up metadata:

- Previous review report: [review-03.md](review-03.md) (agent review, committed in `5f9f41b`, verdict PASS, coverage complete)
- Previous reviewed head: `36c5d4b1e636ae60691b40f51f7274c5fff8bdae`. It is an ancestor of the current reviewed head
- Committed follow-up change range: `36c5d4b..50bcef2` (`5f9f41b`, `50bcef2`)
- Included working-tree diff: none
- Previous comparison-base commit: `master` at `bed5cf0`. The base has not moved, so base movement has no effect
- Policy or evidence change: none

## 1. Verdict and merge readiness

**PASS.** Ready for human review and manual verification.

This is a follow-up review. The baseline is review 03, the latest agent review, which has complete coverage. Items carried from review 01 through review 03 were rechecked. Conclusions for unchanged code are inherited.

The only implementation change in the range renames the toolbar text and the document title from "Delivery notes" to "Delivery Note Service". These are the two edits that review 03 recorded as uncommitted and excluded; the developer has now committed them. No acceptance criterion prescribes the application title, and no test refers to it. The same commit corrects the plan resume note for F003. Format check, lint, the 13 tests, and the production build pass on the reviewed head. The follow-up changes introduce no new finding.

Commit review:

- `5f9f41b` "US-004: Record review 03": administrative. Adds only `review-03.md`.
- `50bcef2` "US-004: Rename the app title and correct review 03 F003": changes `web/src/app/app.html` line 1 (toolbar text) and `web/src/index.html` line 5 (`<title>`); updates the plan `README.md` resume notes (corrects the layout note, adds review 03 and rename entries). No story, plan, or task status change.

| Item | Type | First reported in | Previous severity | Outcome | Evidence |
|---|---|---|---|---|---|
| F001: `web/angular.json` fails `npm run format:check` (AC14) | Finding | review-01 | Blocking | Resolved (in review 02; still resolved) | `npm run format:check` passes on `50bcef2`; `angular.json` unchanged in the range |
| F002: creation test cannot tell the normalized response code from the entered code | Finding | review-01 | Non-blocking | Unresolved | No correction attempted; `recipients-page.spec.ts` unchanged in the range. Developer decided not to correct it |
| F003: the layout change records "text unchanged" but adds visible text | Finding | review-03 | Non-blocking | Resolved | `50bcef2` corrects the plan resume note: it now says the layout change added the toolbar title and the card headings "New recipient" and "Recipient list" and made Retry an outlined button. The `36c5d4b` commit message is deliberately not rewritten (no history rewriting); the plan note is the record of acknowledgement, so the correction is sufficient |

Correction history: F003 received correction 1 in `50bcef2`, which succeeded. F002 received no correction attempt. F001's correction 1 (`1a5986d`) succeeded, as recorded in review 02.

## 2. Blocking findings

None

## 3. Non-blocking findings

### F002: The creation test cannot tell the normalized response code from the entered code (carried from review 01)

- First reported in: [review-01.md](review-01.md).
- Location: `web/src/app/recipients/recipients-page/recipients-page.spec.ts`, creation test and helper `answerCreateRequest` (unchanged).
- Rule: AC9 says the snackbar shows the normalized code from the response.
- Evidence: the test enters `BUDA01` and the mocked 201 echoes it, so it would also pass if the snackbar used the entered code. The implementation is correct.
- Suggested correction: enter a lower-case code and answer with the upper-case code. Optional; the developer decided not to correct it.

## 4. Requirement traceability

The follow-up change touches only the application shell's toolbar text and the HTML document title. No acceptance criterion names either. All rows are inherited from review 03 (and through it from reviews 01 and 02) and were not re-evaluated beyond the regression checks in section 5, except the rows listed as re-evaluated. Paths are relative to `web/` unless they say otherwise.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
|---|---|---|---|---|
| Scope: create the first Angular application in `web/` following the harness Angular policy | Rename is plain template and HTML text | Re-evaluated: fresh clone of `50bcef2`, format check, lint, tests, build pass | Met | None |
| Scope: move project-specific Angular defaults into repository instructions | Unchanged | Inherited (not rerun) | Met (inherited) | None |
| Scope: one recipient page with the creation form above a recipient list | Unchanged | Inherited (not rerun) | Met (inherited) | None |
| Scope: create through `POST /api/recipients`, list through `GET /api/recipients` | Unchanged | Inherited; existing tests rerun and pass | Met (inherited) | None |
| AC1: table with code, name, country, ZIP, city, address, foreign status | Unchanged | Inherited; test reruns and passes | Met (inherited) | None |
| AC2: 10 per page, newest first, previous/next from paging metadata | Unchanged | Inherited; test reruns and passes | Met (inherited) | None |
| AC3: empty-state message | Unchanged | Inherited; test reruns and passes | Met (inherited) | None |
| AC4: loading indicator, error dialog, failure text and Retry | Unchanged | Inherited; manual check remains (section 9) | Met (inherited) | None |
| AC5: required labelled inputs, free-text country | Unchanged | Inherited; tests rerun and pass | Met (inherited) | None |
| AC6: requiredness and maximum lengths inline, no request on violation | Unchanged | Inherited; test reruns and passes | Met (inherited) | None |
| AC7: values sent unchanged, normalization in the backend | Unchanged | Inherited; test reruns and passes | Met (inherited) | None |
| AC8: one POST, submit disabled while in progress | Unchanged | Inherited; manual double-submit check remains | Met (inherited) | None |
| AC9: snackbar with normalized code, form cleared, list reloads page 1 | Unchanged | Inherited; test reruns and passes | Met (inherited) | F002 (test strength only) |
| AC10: modal error dialog with readable message, raw code, OK, values kept | Unchanged | Inherited; test reruns and passes | Met (inherited) | None |
| AC11: error-code to message mapping | Unchanged | Inherited (not rerun) | Met (inherited) | None |
| AC12: same dialog for list-loading failures | Unchanged | Inherited (not rerun) | Met (inherited) | None |
| AC13: backend through relative `/api`, dev-server proxy | Unchanged | Inherited; proxy not exercised | Met (inherited) | None |
| AC14: README commands succeed | README unchanged | Re-evaluated: `npm ci`, `npm run format:check`, `npm run lint`, `npm test`, `npm run build` pass. `npm start` and `npm run format` not run (section 9) | Met | None |
| Agreed decision: English UI text without internationalization | "Delivery Note Service" is plain English text | Re-evaluated by reading the diff | Met | None |
| Agreed test boundary: component tests, `HttpTestingController`, no real backend or e2e | Unchanged; no test refers to the toolbar text or document title (searched `web/src` spec files for "Delivery") | The existing 13 tests rerun and pass | Met (inherited) | None |
| Agreed test boundary tests 1-6 | Unchanged | Rerun as part of `npm test` | Met (inherited) | None |

## 5. Verification performed

The npm commands ran from `web/` in a fresh clone of the reviewed head `50bcef2` (`git clone --branch feature/US-004-recipient-page <repository-root> <temporary-directory>`), which left the developer's `node_modules`, build output, and running dev server untouched. The reviewer did not start, call, or write to the real backend API and did no visual checks.

| Command | Result |
|---|---|
| `git status` (implementation repository) | Clean working tree. Branch `feature/US-004-recipient-page`, HEAD `50bcef2`. Stash `dsa` present and left untouched |
| `git merge-base --is-ancestor 36c5d4b HEAD` | Exit 0 (previous reviewed head is an ancestor) |
| `git rev-parse master` | `bed5cf0` (base unchanged since review 01) |
| `git log 36c5d4b..HEAD`, `git show 50bcef2` | Two commits, as listed in section 1 |
| Search of `web/src` and `web/README.md` for "Delivery" | Only `app.html` line 1 and `index.html` line 5; no spec or README reference |
| `npm ci` | Passed (exit 0) |
| `npm run format:check` | Passed (exit 0): "All matched files use Prettier code style!" |
| `npm run lint` | Passed (exit 0): "All files pass linting." |
| `npm test` | Passed (exit 0): 2 files, 13 tests |
| `npm run build` | Passed (exit 0): no budget warnings; lazy `recipients-page` chunk |

## 6. Code-guideline assessment

Applied policy: repository `CLAUDE.md` (simplicity preferences), `web/CLAUDE.md` (Angular Material only), the AgenticEngineering global policy, and the Angular policy at `80199f3` (the change touches templates only). The `clean-code` skill was applied to the follow-up change.

- **Formatting and lint:** pass.
- **Change content:** two one-line text edits; no logic, structure, accessibility, or style impact. The toolbar is not a heading, so the page's heading order (`h1` "Recipients", two `h2` card titles) is unchanged.
- **Documentation:** the corrected resume note is accurate against `36c5d4b`. One edited line in the plan `README.md` is longer than the surrounding wrapped lines; this is cosmetic, the file is outside the Prettier scope, and it is not recorded as a finding.
- **Tests:** the change adds no behavior, so no new test is required.

## 7. Architecture assessment

The change is limited to presentation text in the root `App` shell and the HTML document head. Routing, the API client, state, error handling, and the `/api` proxy are unchanged. No contract consumed outside the web application changes.

No architecture defects found.

## 8. Recorded deviations

The deviations recorded in reviews 01 to 03 still apply:

1. Tests beyond the agreed six (acknowledged in T003 and T005).
2. Manual checks replaced by throwaway specs (acknowledged in T004 and T006).
3. Angular version ranges kept as generated, with the lock file pinning exact versions (acknowledged in T001).
4. T007 (frontend CI) cancelled by the developer.
5. T008 harness change in AgenticEngineering `80199f3` on `feature/add-angular-harness`, merging separately.
6. `"analytics": false` in `web/angular.json` (recorded in T001).
7. Layout additions not described in the story: application toolbar, padded content area, outlined cards with the headings "New recipient" and "Recipient list", responsive form grid, right-aligned submit and paging, horizontal table scroll, and an outlined Retry button. Now fully acknowledged in the plan resume notes (F003 resolved).

New in this range:

8. Application title "Delivery Note Service" in the toolbar and the document title. The story does not name a title. Acknowledged in the plan resume notes (`50bcef2`) as the developer's edit.

## 9. Unverified items and remaining risks

- No visual check of the renamed toolbar or browser tab title was done; the change is plain text.
- `npm start` and the `/api` proxy were not exercised, because the review must not start or call the real backend.
- `npm run format` was not run, because it would rewrite files. The format check passes.
- The story's manual checks (loading indicator, Retry, double-submit guard, remaining error mappings) remain for human review.
