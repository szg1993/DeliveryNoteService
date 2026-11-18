# US-006: Agent review 02

- Review type: agent review
- Review date and time: 2026-11-18 17:13 +01:00
- Authoritative story: `docs/backlog/US-006-delivery-note-pages.md` (status In review)
- Plan and tasks: `docs/backlog/US-006-delivery-note-pages/README.md`, `tasks/T001`–`T007`
- `AgenticEngineering` commit: `80199f355a77c687df695044257172578b803225`
- Reviewed head: `feature/US-006-delivery-note-pages` at `a514fa507e0f2920cb511849bfdab3dead4e500a`
- Comparison base: `master` as published at `origin/master` = `25e44a13ad686e9f867af41035f73fd7c4f86480`
  (the local `master` ref is stale and was not used)
- Review coverage: complete (review 01 had complete coverage; this review covers the full follow-up range)
- Explicitly included uncommitted changes: none (working tree clean)
- Previous review report: [review-01.md](review-01.md) (agent review, committed in `b3b8bda`)
- Previous reviewed head: `5178e13dd62bbce0f444a4a834eb0af009fbe76f` (ancestor of the current head)
- Committed follow-up change range: `5178e13..a514fa5` (`b3b8bda`, `e724ea9`, `a514fa5`)
- Included working-tree diff: none
- Previous comparison-base commit: `25e44a1`; unchanged, so no base-movement effect
- Policy or evidence change: none
- Final verdict: **PASS**

## 1. Verdict and merge readiness

**PASS. Ready to merge after the human manual checks in section 9.** Both blocking findings from
review 01 are resolved by `e724ea9`. The follow-up changes introduce no blocking finding. One new
non-blocking finding (N004) is recorded.

Range inspection:

- `b3b8bda`: administrative; adds only `review-01.md`.
- `e724ea9`: correction attempt 1 for F001 and F002. Changes `web/src/app/app.scss`,
  `web/angular.json`, new `web/vitest-base.config.ts`, and progress notes in T002, T004 and T007.
  No test, assertion, requirement or story change.
- `a514fa5`: administrative; plan resume notes in `README.md` only.

| Item | Type | First reported in | Previous severity | Outcome | Evidence |
|---|---|---|---|---|---|
| F001 Main content layout removed | Finding | review-01 | Blocking | Resolved (attempt 1, `e724ea9`, succeeded) | `app.scss` rule is identical to base `25e44a1`; built bundle contains `.app-content[_ngcontent-%COMP%]{max-width:1200px;margin:0 auto;padding:24px 16px 48px}`; browser check: `main.app-content` computed `max-width: 1200px`, `padding: 24px 16px 48px`, `h1` at x = 16 at a 1024 px viewport |
| F002 Test 4 exceeds the default test timeout | Finding | review-01 | Blocking | Resolved (attempt 1, `e724ea9`, succeeded) | `testTimeout: 15000` in `web/vitest-base.config.ts`, loaded via `runnerConfig` (runner logs "Using Vitest configuration file"); helper wait in `src/testing/http.ts` stays at 5000 ms, below the test timeout; 8 of 8 full-suite runs passed 20/20; test 4 took 4081 and 3974 ms, the toolbar test 2331 and 2257 ms in the timed runs |
| N001 NG0956 identity `track` on note lines | Finding | review-01 | Non-blocking | Unresolved (not addressed; optional) | `create-delivery-note-page.html` unchanged in the range |
| N002 No-recipients hint not linked by `aria-describedby` | Finding | review-01 | Non-blocking | Unresolved (not addressed; optional) | Template unchanged in the range |
| N003 Invalid-date message depends on Material error keys | Finding | review-01 | Non-blocking | Unresolved (not addressed; optional) | `create-delivery-note-page.ts` unchanged in the range |

## 2. Blocking findings

None.

## 3. Non-blocking findings

- N001, N002, N003: carried forward unchanged from [review-01.md](review-01.md), section 3.
- N004 (new): `web/vitest-base.config.ts:1` uses ESM `import` syntax in a `.ts` file inside a package
  without `"type": "module"`. Every `npm test` run prints a Vite warning that this config uses
  features unsupported by `configLoader: 'native'`, "planned to become the default in a future major
  version of Vite". The config works today, and the timeout is applied. To keep the test command
  quiet and future-proof, consider renaming the file to `vitest-base.config.mts` and updating
  `runnerConfig` in `web/angular.json`. Optional.

## 4. Requirement traceability

Statuses for requirements not touched by the follow-up range are inherited from review 01, which
completed those checks. Inherited evidence was not rerun except where stated.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
|---|---|---|---|---|
| AC1 Routes, redirect, lazy loading, titles | `app.routes.ts` | Inherited from review 01; redirect test passed in this review's runs; build shows lazy chunks | Met | None |
| AC2 Toolbar links, current one highlighted | `app.html`, `app.scss` (nav styles unchanged; `.app-content` restored) | Toolbar test passed 8/8 runs; browser check: "Recipients" link had `active-link` on `/recipients` | Met | F001 resolved |
| AC3 List request, newest first, 10 per page, paging | Unchanged | Inherited (test 1 passed in this review's runs) | Met | None |
| AC4 Columns, status, Urgent chip, dates | Unchanged | Inherited (test 1 passed) | Met | None |
| AC5 Empty-state message | Unchanged | Inherited (test 2 passed) | Met | None |
| AC6 Loading indicator, failure dialog, Retry | Unchanged | Inherited (manual, per agreed boundary) | Met | None |
| AC7 New delivery note button; rows not clickable | Unchanged | Inherited | Met | None |
| AC8 One page, titled sections, labels | Unchanged | Inherited (tests 4–7 passed) | Met | None |
| AC9 Recipient select, hint, load failure | Unchanged | Inherited; browser check here showed the error dialog on failed load with the backend unreachable | Met | N002 |
| AC10 Lines | Unchanged | Inherited (test 4 passed) | Met | N001 |
| AC11 Free-text units and scale | Unchanged | Inherited | Met | None |
| AC12 Inline validation | Unchanged | Inherited (tests 5, 6 passed) | Met | None |
| AC13 Number inputs, datepicker, date format | Unchanged | Inherited | Met | N003 |
| AC14 Untrimmed text, empty optional as `null` | Unchanged | Inherited (test 4 passed) | Met | None |
| AC15 One POST, submit disabled while pending | Unchanged | Inherited (test 4 passed) | Met | None |
| AC16 Snackbar and navigation on 201 | Unchanged | Test 4 passed reliably (8/8 runs) | Met | F002 resolved |
| AC17 Cancel | Unchanged | Inherited | Met | None |
| AC18 Errors open the shared dialog; values kept | Unchanged | Inherited (test 7 passed) | Met | None |
| AC19 Error-message mappings | Unchanged | Inherited (test 7 passed) | Met | None |
| Test boundary: seven new tests plus updated redirect test | No test files changed in the range | 20 tests in 5 files, as in review 01 | Met | None |
| Tests never call the real backend | Unchanged; config change only sets a timeout | Spec inspection inherited | Met | None |
| No backend changes | `git diff 25e44a1 a514fa5 -- service` empty (range touches only `web/` and `docs/`) | Diff | Met | None |

## 5. Verification performed

All commands ran in `web/` on the reviewed head with a clean working tree.

| Command | Result |
|---|---|
| `npm test` (×6) | Pass in every run: 5 files, 20/20 tests. Each run logged "Using Vitest configuration file: `<repository-root>/web/vitest-base.config.ts`" and the Vite config-loader warning (N004) |
| `npx ng test --watch=false --reporters=verbose` (×2, full suite) | Pass 20/20 both times. Test 4: 4081 and 3974 ms; toolbar test: 2331 and 2257 ms; redirect test: 1863 and 1671 ms |
| `npm run lint` | Pass ("All files pass linting.") |
| `npm run format:check` | Pass ("All matched files use Prettier code style!") |
| `npm run build` | Pass; lazy chunks `create-delivery-note-page`, `delivery-notes-page`, `recipients-page`; main bundle contains the scoped `.app-content` rule |
| Browser check: `npx ng serve --port 4321 --proxy-config <temporary-directory>/empty-proxy.json` (no `/api` target, so the real backend was unreachable), page `/recipients` at 1024 px | `main.app-content` computed `max-width: 1200px`, `padding: 24px 16px 48px`; `h1` at x = 16; "Recipients" link active; failed load opened the error dialog. The server was stopped afterwards |

The real backend API was never run or called. Full-suite runs here took about 4 s for test 4,
below the 5 s default as well, so the margin added by the 15 s timeout was not exercised on this
machine in these runs; the implementation notes record runs up to 8154 ms that the new timeout
covers.

## 6. Code-guideline assessment

Applied: repository `CLAUDE.md` and `web/CLAUDE.md` (hobby-scale proportionality), the Angular
technology policy (testing: keep the repository's runner, Vitest), and `clean-code` to the
follow-up changes.

- `app.scss`: the restored rule matches the base exactly and sits after the unchanged nav styles.
- `vitest-base.config.ts` and `angular.json`: one configuration value with a comment explaining why
  it exists and how it relates to the helper wait. This is the simplest correction, keeps the
  existing runner, and changes no test or seam. A global timeout also relaxes the limit for fast
  tests, which is acceptable at this project's scale. See N004 for the loader warning.
- Formatting, lint and build pass.

## 7. Architecture assessment

No architecture change in the range. The test-runner configuration is local to `web/` and is not
an external contract. Review 01's assessment stands.

## 8. Recorded deviations

- Carried from review 01 (acknowledged): `web/src/testing/http.ts` wait raised to 5 s in T007;
  no-recipients hint as a paragraph (T006); datepicker error-key handling (T005); wording choices.
- Resolved: the unacknowledged `.app-content` removal (F001) is reverted.
- New (acknowledged in T004 and T007 progress notes): a project-wide 15 s Vitest test timeout via
  `web/vitest-base.config.ts`.
- Process note: the commit subject follows the reviewer prompt ("US-006: Add agent review 02")
  rather than the workflow's "US-006: Record review 02". Explicit user instructions take precedence.

## 9. Unverified items and remaining risks

The human manual checks listed in review 01, section 9, remain open. This review did not repeat
them:

- list paging, loading bar, failure dialog then Retry;
- the active toolbar link highlight (spot-checked here for "Recipients" only);
- adding and removing lines, maximum-length messages, numeric minimums, the double-submit guard,
  and Cancel;
- the no-recipients hint and its link;
- the "Enter a valid date." message, and date display in the browser locale;
- the error mappings other than `Note.InvalidInput`;
- end-to-end behaviour against the real backend, including the created note appearing at the top
  of the list.

Also worth a glance: the restored layout on a wide window (centred at 1200 px), which the
implementation notes record but this review checked only at 1024 px.

Remaining risk: test timing still depends on the machine, but the 15 s limit leaves roughly a
twofold margin over the slowest recorded run (8154 ms).
