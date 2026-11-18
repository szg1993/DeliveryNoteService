# US-004: Review 03

- Review type: agent review
- Review date and time: 2026-11-18 11:45 +01:00
- Authoritative story: `docs/backlog/US-004-recipient-page.md`
- AgenticEngineering commit: `80199f355a77c687df695044257172578b803225` (branch `feature/add-angular-harness`)
- Reviewed head: `feature/US-004-recipient-page` at `36c5d4b1e636ae60691b40f51f7274c5fff8bdae`
- Comparison base: `master` at `bed5cf01a8df023895caaa11f1863d128b3b0b1d` (repository default branch; no pull request exists and the story names no other target)
- Review coverage: complete
- Explicitly included uncommitted changes: none. The working tree had two uncommitted edits at review time (section 9); they were not identified as part of the review and are excluded
- Final verdict: **PASS**

Follow-up metadata:

- Previous review report: [review-02.md](review-02.md) (agent review, committed in `72130d1`, verdict PASS, coverage complete)
- Previous reviewed head: `a53451f58ec94e74bccec4d08f802a5c0f903a6e`. It is an ancestor of the current reviewed head
- Committed follow-up change range: `a53451f..36c5d4b` (`72130d1`, `36c5d4b`)
- Included working-tree diff: none
- Previous comparison-base commit: `master` at `bed5cf0`. The base has not moved, so base movement has no effect
- Policy or evidence change: none

## 1. Verdict and merge readiness

**PASS.** Ready for human review and manual verification, subject to the uncommitted working-tree edits noted in section 9.

This is a follow-up review. The baseline is review 02, the latest agent review, which has complete coverage. Items carried from review 01 through review 02 were rechecked. Conclusions for unchanged code are inherited.

The only implementation change in the range is `36c5d4b`, a layout change made at the developer's request. It adds a Material toolbar and a centered, padded content area, places the form and the list in outlined Material cards with `h2` titles, lays the form fields out in a responsive grid, right-aligns the submit button and paging, and wraps the table in a horizontal scroll container. Field labels, the form's accessible name, button names, the `h1` "Recipients" heading, the empty-state and failure texts, the paging text, and all component logic are unchanged. Format check, lint, the 13 tests, and the production build pass on the reviewed head. The change introduces one non-blocking finding (F003). F002 stays open as non-blocking.

Commit review:

- `72130d1` "US-004: Record review 02": administrative. Adds only `review-02.md`.
- `36c5d4b` "US-004: Give the recipient page a padded card layout": implementation change. Changes `web/src/app/app.html`, `app.ts`, adds `app.scss`; changes `create-recipient-form.html` and `.ts`, adds `create-recipient-form.scss`; changes `recipients-page.html` and `.ts`, adds `recipients-page.scss`; appends review-02 and layout-change entries to the plan `README.md` resume notes. No story, plan, or task status change.

| Item | Type | First reported in | Previous severity | Outcome | Evidence |
|---|---|---|---|---|---|
| F001: `web/angular.json` fails `npm run format:check` (AC14) | Finding | review-01 | Blocking | Resolved (in review 02; still resolved) | `npm run format:check` passes on `36c5d4b`; `angular.json` unchanged in the range |
| F002: creation test cannot tell the normalized response code from the entered code | Finding | review-01 | Non-blocking | Unresolved | No correction attempted; `recipients-page.spec.ts` unchanged in the range. Developer decided not to correct it |

Correction history: no item received a correction attempt in this range. F001's correction 1 (`1a5986d`) succeeded, as recorded in review 02.

## 2. Blocking findings

None

## 3. Non-blocking findings

### F002: The creation test cannot tell the normalized response code from the entered code (carried from review 01)

- First reported in: [review-01.md](review-01.md).
- Location: `web/src/app/recipients/recipients-page/recipients-page.spec.ts`, creation test and helper `answerCreateRequest` (unchanged).
- Rule: AC9 says the snackbar shows the normalized code from the response.
- Evidence: the test enters `BUDA01` and the mocked 201 echoes it, so it would also pass if the snackbar used the entered code. The implementation is correct.
- Suggested correction: enter a lower-case code and answer with the upper-case code. Optional; the developer decided not to correct it.

### F003: The layout change records "text unchanged" but adds visible text

- First reported in: this report.
- Location: `web/src/app/app.html` line 1 (toolbar text "Delivery notes"); `web/src/app/recipients/recipients-page/recipients-page.html` lines 5 and 14 (`h2` "New recipient" and "Recipient list"); line 24 (Retry changed from `mat-button` to `mat-stroked-button`); `docs/backlog/US-004-recipient-page/README.md` layout-change resume note; commit message of `36c5d4b`.
- Rule: review-story.md requires implementation deviations to be recorded so the developer can carry them elsewhere; the plan notes are the acknowledgement for this change.
- Evidence: the resume note and commit message state "Text, labels, and behavior are unchanged", but the change adds three visible strings and two headings to the page structure. The additions do not conflict with any acceptance criterion: the story prescribes English UI text, the creation form above the list, and labels, names, and messages that remain unchanged. The `h1` "Recipients" heading the route test relies on is still the only heading with that exact name.
- Suggested correction: amend the plan resume note to say that the layout change adds the toolbar title and the two card headings and restyles Retry as a stroked button. Documentation only; no code change needed.

## 4. Requirement traceability

The follow-up change touches the templates and styles behind AC1–AC6, AC8–AC10, AC12, and the route-level `Recipients` heading. Those rows were re-evaluated by reading the changed templates and rerunning the existing tests. Rows marked inherited rely on review 01 and review 02 and were not rerun beyond the regression checks in section 5. Paths are relative to `web/` unless they say otherwise.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
|---|---|---|---|---|
| Scope: create the first Angular application in `web/` following the harness Angular policy | Layout uses Angular Material only (`MatToolbarModule`, `MatCardModule`), standalone imports, external `.scss` style files with default encapsulation, Material system tokens | Re-evaluated: fresh clone of `36c5d4b`, format check, lint, tests, build pass | Met | None |
| Scope: move project-specific Angular defaults into repository instructions | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| Scope: one recipient page with the creation form above a recipient list | `recipients-page.html`: form card precedes list card | Re-evaluated by template reading | Met | F003 (documentation only) |
| Scope: create through `POST /api/recipients`, list through `GET /api/recipients` | Unchanged | Inherited; existing tests rerun and pass | Met (inherited) | None |
| AC1: table with code, name, country, ZIP, city, address, foreign status | Same column definitions, now inside `.table-container` (horizontal scroll) | Test 1 reruns and passes | Met | None |
| AC2: 10 per page, newest first, previous/next from paging metadata | Paging `nav` unchanged except `class="paging"`; `aria-label` and button names kept | Paging test reruns and passes ("Page 1 of 3", next page requests page 2) | Met | None |
| AC3: empty-state message | "No recipients yet." kept, now `p.message` | Empty-state test reruns and passes | Met | None |
| AC4: loading indicator, error dialog, failure text and Retry | Progress bar with same `aria-label`; failure text and Retry button kept inside `div.message`; Retry restyled as stroked button | Inherited from review 01 review checks; manual check remains (section 9) | Met | F003 |
| AC5: required labelled inputs, free-text country | Form fields unchanged, wrapped in `div.fields` grid; `mat-label` and form `aria-label` kept | Tests find inputs by label and pass | Met | None |
| AC6: requiredness and maximum lengths inline, no request on violation | `mat-error` blocks unchanged inside each field | Required-field test reruns and passes | Met | None |
| AC7: values sent unchanged, normalization in the backend | Unchanged | Inherited; creation test reruns and passes | Met (inherited) | None |
| AC8: one POST, submit disabled while in progress | Submit button unchanged, wrapped in `div.actions` | Inherited; manual double-submit check remains | Met (inherited) | None |
| AC9: snackbar with normalized code, form cleared, list reloads page 1 | `(created)="showFirstPage()"` binding kept | Creation test reruns and passes | Met | F002 (test strength only) |
| AC10: modal error dialog with readable message, raw code, OK, values kept | Dialog code unchanged | Already-exists test reruns and passes | Met (inherited) | None |
| AC11: error-code to message mapping | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC12: same dialog for list-loading failures | Unchanged | Inherited from review 01 (not rerun) | Met (inherited) | None |
| AC13: backend through relative `/api`, dev-server proxy | Unchanged | Inherited; proxy not exercised | Met (inherited) | None |
| AC14: README commands succeed | README unchanged | Re-evaluated: `npm ci`, `npm run format:check`, `npm run lint`, `npm test`, `npm run build` pass. `npm start` and `npm run format` not run (section 9) | Met | None |
| Agreed test boundary: component tests, `HttpTestingController`, no real backend or e2e | Unchanged; layout needs no new behavior test | The existing 13 tests rerun and pass; `app.routes.spec.ts` still finds the `Recipients` heading by exact name | Met (inherited) | None |
| Agreed test boundary tests 1-6 | Unchanged | Rerun as part of `npm test` | Met (inherited) | None |

## 5. Verification performed

The npm commands ran from `web/` in a fresh clone of the reviewed head `36c5d4b` (`git clone --branch feature/US-004-recipient-page <repository-root> <temporary-directory>`), which left the developer's `node_modules`, build output, and running dev server untouched. The reviewer did not start, call, or write to the real backend API and did no visual checks.

| Command | Result |
|---|---|
| `git status --short` (implementation repository) | Two modified, uncommitted files: `web/src/app/app.html`, `web/src/index.html` (section 9). Branch `feature/US-004-recipient-page`, HEAD `36c5d4b` |
| `git merge-base --is-ancestor a53451f HEAD` | Exit 0 (previous reviewed head is an ancestor) |
| `git merge-base master HEAD` and `git rev-parse master` | Both `bed5cf0` (base unchanged since review 01) |
| `git log a53451f..HEAD`, `git show 36c5d4b` | Two commits, as listed in section 1 |
| `npm ci` | Passed (exit 0). The same install-script approval warnings as earlier reviews |
| `npm run format:check` | Passed (exit 0): "All matched files use Prettier code style!" |
| `npm run lint` | Passed (exit 0): "All files pass linting." |
| `npm test` | Passed (exit 0): 2 files, 13 tests |
| `npm run build` | Passed (exit 0): no budget warnings; lazy `recipients-page` chunk |

## 6. Code-guideline assessment

Applied policy: repository `CLAUDE.md` (simplicity preferences), `web/CLAUDE.md` (Angular Material only), the AgenticEngineering global policy, and the Angular policy at `80199f3`: `components.md` (structure, templates, accessibility, styles) and `typescript-style.md` for the changed `.ts` files. The `clean-code` skill was applied to the follow-up change.

- **Formatting and lint:** pass.
- **Structure:** standalone components keep importing only the Material modules their templates use. Angular is 22.x, so the omitted `changeDetection` follows the OnPush default. Each component gains its own external `.scss` file through `styleUrl`.
- **Styles (`components.md`):** styles live in the component style files with default emulated encapsulation. No `ViewEncapsulation.None` or `::ng-deep`. Colors and typography use Material system tokens (`--mat-sys-*`) from the theme in `styles.scss`, and no new UI library is added. Element selectors such as `.section mat-card-content` target elements in the component's own template, so emulated encapsulation scopes them correctly.
- **Accessibility:** heading order is `h1` "Recipients" then two `h2` card titles. Labels, form `aria-label`, progress-bar `aria-label`, paging `nav` `aria-label`, and native buttons are kept. The table scroll container keeps the table usable at narrow widths.
- **Readability and proportionality:** the change is small, declarative, and matches the hobby-project simplicity preference. Spacing and breakpoint values are plain literals in style files, which is proportionate here.
- **Tests:** the change adds no behavior, so no new test is required. The existing tests confirm that the accessible names they rely on still resolve.

## 7. Architecture assessment

The change is limited to presentation: templates, component styles, and Material module imports. Routing, the API client, state, error handling, and the `/api` proxy are unchanged. The toolbar and content wrapper sit in the root `App` shell, which is the appropriate place for application-wide chrome. No contract consumed outside the web application changes.

No architecture defects found.

## 8. Recorded deviations

The deviations recorded in reviews 01 and 02 still apply and are unchanged:

1. Tests beyond the agreed six (acknowledged in T003 and T005).
2. Manual checks replaced by throwaway specs (acknowledged in T004 and T006).
3. Angular version ranges kept as generated, with the lock file pinning exact versions (acknowledged in T001).
4. T007 (frontend CI) cancelled by the developer.
5. T008 harness change in AgenticEngineering `80199f3` on `feature/add-angular-harness`, merging separately.
6. `"analytics": false` in `web/angular.json` (recorded in T001).

New in this range:

7. Layout additions not described in the story: an application toolbar, a padded centered content area, outlined cards, a responsive form grid, right-aligned submit and paging, and horizontal table scroll. Acknowledged in the plan resume notes (`36c5d4b`) as a developer request after review 02. The added toolbar title, the two card headings, and the stroked Retry style are not described there (F003).

## 9. Unverified items and remaining risks

- **Uncommitted working-tree edits:** at review time `web/src/app/app.html` changed the toolbar text from "Delivery notes" to "Delivery Note Service", and `web/src/index.html` changed the document title the same way. Neither is part of the reviewed head or this verdict. They touch the toolbar line added in `36c5d4b` but no acceptance criterion. If they are committed, they need a follow-up review before human acceptance.
- No visual check was done; the developer's notes record visual checks at 1280×900 and 375×812 against a fake backend.
- `npm start` and the `/api` proxy were not exercised, because the review must not start or call the real backend.
- `npm run format` was not run, because it would rewrite files. The format check passes.
- The story's manual checks (loading indicator, Retry, double-submit guard, remaining error mappings) remain for human review.
- `npm ci` still warns that some packages' install scripts are not approved. Lint, test, and build work without them.
