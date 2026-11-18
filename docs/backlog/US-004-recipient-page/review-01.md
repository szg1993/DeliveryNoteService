# US-004: Review 01

- Review type: agent review
- Review date and time: 2026-11-18 10:53 +01:00
- Authoritative story: `docs/backlog/US-004-recipient-page.md`
- AgenticEngineering commit: `80199f355a77c687df695044257172578b803225` (branch `feature/add-angular-harness`)
- Reviewed head: `feature/US-004-recipient-page` at `41e16cc2fe587ea9ecbe7255ee3ceec1472df549`
- Comparison base: `master` at `bed5cf01a8df023895caaa11f1863d128b3b0b1d` (repository default branch; no pull request exists and the story names no other target)
- Review coverage: complete
- Explicitly included uncommitted changes: none (working tree clean at review time)
- Final verdict: **FAIL**

## 1. Verdict and merge readiness

**FAIL.** Not ready to merge.

The recipient page meets the story's listing, creation, and error-handling criteria. Lint, tests, and the production build pass, and extra review-only component checks with a mocked backend also pass. One blocking finding remains: `npm run format:check` fails on `web/angular.json`. The story branch's commit `b9b9c16` ("Modify for debug") introduced the problem. It violates the repository formatting rule and leaves AC14 unmet, because the README lists that command and the story requires it to succeed.

This is an initial review. No earlier `review-NN.md` report exists.

## 2. Blocking findings

### F001: `web/angular.json` fails the repository formatting check

- Location: `web/angular.json`, lines 6-8 (`cli.schematicCollections`).
- Violated requirement or rule: Story AC14 says the `web/` README commands succeed, and the README lists `npm run format:check`. The repository formatting rule (AgenticEngineering `agent-harness/AGENTS.md`, Angular `typescript-style.md` repository configuration) also requires implementation files to pass the configured Prettier check.
- Evidence: In a fresh clone of `41e16cc` with `npm ci`, `npm run format:check` exited with code 1 and reported `[warn] angular.json` and "Code style issues found in the above file." Prettier wants `"schematicCollections": ["angular-eslint"],` on one line. Commit `b9b9c16` ("Modify for debug") split the array across lines while adding `"analytics": false`. The file from before that commit (`b9b9c16~1:web/angular.json`) passes Prettier unchanged. T006 and T008 also record this failure.
- Required correction: Format `web/angular.json` with the workspace Prettier (`npm run format`, or `npx prettier --write angular.json` from `web/`). Keep or drop `"analytics": false` as the developer prefers. Then confirm that `npm run format:check` passes.

## 3. Non-blocking findings

### F002: The creation test cannot tell the normalized response code from the entered code

- Location: `web/src/app/recipients/recipients-page/recipients-page.spec.ts`, lines 103-116 and helper `answerCreateRequest` (lines 198-208).
- Rule: AC9 says the snackbar shows the normalized code from the response.
- Evidence: The test enters `BUDA01`, and the mocked 201 echoes the entered details, so it would also pass if the snackbar used the entered code. The implementation is correct: `create-recipient-form.ts` line 79 uses `created.code`. A review-only check that entered `buda01` and answered `BUDA01` showed "Recipient BUDA01 created".
- Suggested correction: In the existing creation test, enter a lower-case code and answer with the upper-case code, so the test pins AC9. This is optional.

## 4. Requirement traceability

Paths are relative to `web/` unless they say otherwise. "Review check" means a temporary component spec that the reviewer ran through `RecipientsPage` with `HttpTestingController` in a scratch clone and then discarded. It did not change the repository (see section 5).

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
|---|---|---|---|---|
| Scope: create the first Angular application in `web/` following the harness Angular policy | Angular 22.2 workspace: SCSS, no SSR, zoneless standalone defaults, angular-eslint, Prettier, Vitest + `@testing-library/angular`, Angular Material, strict `tsconfig.json` | `npm ci`, lint, test, and build pass in a fresh clone. The format check fails (F001) | Met (formatting tracked separately under F001/AC14) | F001 |
| Scope: move project-specific Angular defaults (Material, `/api` proxy, no NgRx, testing approach) into repository instructions, leaving general rules in the harness | `web/CLAUDE.md`. AgenticEngineering `80199f3` changes `agent-harness/technologies/angular/new-application.md` | Read both files against the story. In AgenticEngineering, `node tools/validate-harness.js` reports "Harness policy is valid." and `node --test tools/harness-policy.test.js tools/skills.test.js` passes 25/25. Links in the edited guide resolve | Met | None |
| Scope: one recipient page with the creation form above the list | `recipients-page.html` lines 1-3 (`app-create-recipient-form` above the list) | `recipients-page.spec.ts` renders both | Met | None |
| Scope: create through `POST /api/recipients`, list through `GET /api/recipients` | `recipients-api.ts` lines 46-56 | Spec helpers `answerRecipientsRequest` and `answerCreateRequest` match the exact URLs | Met | None |
| AC1: page loads `GET /api/recipients` and shows a table with code, name, country, ZIP, city, address, foreign status | `recipients-page.ts` lines 22-26. `recipients-page.html` lines 16-48 (Foreign as Yes/No) | Spec "lists the newest recipients from the backend" (header and every cell) | Met | None |
| AC2: 10 per page, newest first (ID descending), previous/next controls driven by backend paging metadata | `recipients-api.ts` line 48 (`limit: 10, sortBy: 'Id', sortDir: 'Desc'`). `recipients-page.html` lines 50-68 (disabled states from `paging.page` and `paging.totalPages`) | Spec next, previous, "Page 1 of 3", and both disabled-state tests. Every request is matched by exact query | Met | None |
| AC3: empty-state message instead of an empty table | `recipients-page.html` lines 13-14 | Spec "shows an empty-state message when there are no recipients" | Met | None |
| AC4: loading indicator. On failure the error dialog opens. After close, "Recipients could not be loaded." with Retry that reloads | `recipients-page.html` lines 5-11. `recipients-page.ts` lines 28-29, 51-64 (rethrow after `afterClosed()`) | No permanent test (agreed manual item). Review check: the progress bar is labelled "Loading recipients" while pending. A network failure opens the "Error" dialog, and the failure text is hidden until OK. After OK, the text and Retry show. Retry sends the same page-1 request and renders the rows. T004 records the manual dev-server check | Met | None |
| AC5: required inputs for code, name, country, ZIP, city, address, findable by visible labels. Country is free text | `create-recipient-form.ts` lines 20-27. `create-recipient-form.html` lines 2-10 (`mat-label`, plain `input`) | Spec `recipientInput` finds every input by label | Met | None |
| AC6: requiredness and US-001 maximum lengths (10/200/2/20/50/200) inline. Violations block submission without a request. Code and country rules stay in the backend | `create-recipient-form.ts` lines 52-63 (`required`, `maxLength`). `create-recipient-form.html` lines 6-8 (`mat-error`) | Spec "shows an inline error and sends nothing when a required value is missing" and "... when a value is too long" (`expectNone` POST) | Met | None |
| AC7: name, ZIP, city, address not trimmed or changed. Code and country normalization stays in the backend | `create-recipient-form.ts` lines 66-69 send the model signal unchanged | Spec creation test asserts the body with `' Buda Logistics '`. Review check: the POST body kept a lower-case code and `' N '` unchanged | Met | None |
| AC8: one POST. The submit button is disabled while in progress | `create-recipient-form.html` line 12 (`[disabled]="recipientForm().submitting()"`) | Spec "disables create and ignores another submit while the request is pending" (`expectOne` after two clicks) | Met | None |
| AC9: on 201, snackbar "Recipient {CODE} created" with the normalized code from the response, form cleared, list reloads on page 1 | `create-recipient-form.ts` lines 78-82. `recipients-page.ts` lines 35-41 | Spec creation test (snackbar, empty inputs, page-1 request). Review check: entered `buda01`, the response `BUDA01` showed "Recipient BUDA01 created" | Met | F002 (test strength only) |
| AC10: every backend error and network failure, from creation or listing, opens a modal dialog with a readable message, the raw code in small grey text when present, and OK. Entered values kept | `shared/error-dialog/*` (`.error-code` uses `body-small` and the `outline` color). `create-recipient-form.ts` lines 70-75. `recipients-page.ts` lines 56-64 | Spec duplicate-code test (dialog, raw code, values kept, button re-enabled). Review checks: list 400 shows the raw code. Creation 400s, 409, 500 without a body, and a network failure each open the dialog, keep the entered values, and show no snackbar | Met | None |
| AC11: error-code to message mapping (AlreadyExists, FieldMissing/FieldTooLong/InvalidCode, InvalidCountry, everything else) | `shared/error-message.ts` lines 20-37. Codes and `data.Field` match `service/src/Common.Core/Exceptions/*` and `service/src/Core/Errors/ErrorCodes.cs` | Spec covers AlreadyExists. Review checks covered InvalidCountry, FieldTooLong with `Field: "Name"`, FieldMissing with `Field: "City"`, InvalidCode, 409 UniqueKeyConflict, 500, and a network failure. T004 and T006 record equivalent throwaway checks | Met | None |
| AC12: same dialog for list-loading failures | `recipients-page.ts` line 61 reuses `openErrorDialog` | Review check (AC4 row) | Met | None |
| AC13: backend only through the relative `/api` path. Dev server proxies `/api` | `recipients-api.ts` lines 47, 54. `proxy.conf.json`. `angular.json` `serve.options.proxyConfig` | Spec URL matching. Configuration read. The proxy was not exercised by the reviewer (no backend use permitted). T001 records a stub-server proxy check | Met | None |
| AC14: the `web/` README lists install, run, test, lint, and format commands, and these commands succeed | `README.md` lists `npm ci`, `npm start`, `npm test`, `npm run lint`, `npm run format`, `npm run format:check`, `npm run build` | Fresh clone: `npm ci`, `npm test`, `npm run lint`, and `npm run build` pass. **`npm run format:check` fails.** `npm start` and `npm run format` were not run by the reviewer (section 9) | Not met | F001 |
| Agreed test boundary: component tests with Testing Library, labels/roles/text, every request mocked through `HttpTestingController`, no real backend or end-to-end tests | `recipients-page.spec.ts`, `app.routes.spec.ts` | Both specs provide `provideHttpClientTesting()`. `afterEach` verifies no unexpected requests. There are no e2e or browser tests | Met | None |
| Agreed test boundary tests 1-6 | `recipients-page.spec.ts`: test 1 lines 12-41, test 2 lines 43-51, test 3 lines 103-116, test 4 lines 131-144, test 5 lines 146-155, test 6 lines 53-61 | All pass (13/13 tests in 2 files) | Met | None (extra tests recorded in section 8) |

## 5. Verification performed

All commands except the AgenticEngineering ones ran from `web/` in a fresh clone of the reviewed head `41e16cc` (`git clone --branch feature/US-004-recipient-page <repository-root> <temporary-directory>`). That kept the developer's `node_modules` and build output untouched. The reviewer did not start, call, or write to the real backend API.

| Command | Result |
|---|---|
| `git status --short` (implementation repository) | Clean. HEAD `41e16cc` |
| `npm ci` | Passed (exit 0). npm 11 warned that the install scripts of `@parcel/watcher`, `esbuild`, `lmdb`, and `msgpackr-extract` are not approved. Later commands work without them |
| `npm run format:check` | **Failed** (exit 1): `[warn] angular.json` / "Code style issues found in the above file." |
| `npx prettier --stdin-filepath angular.json` against `b9b9c16~1:web/angular.json` and the current file | The earlier file is unchanged by Prettier. The current file differs only in `schematicCollections` wrapping |
| `npm run lint` | Passed: "All files pass linting." |
| `npm test` | Passed: 2 files, 13 tests |
| `npm run build` | Passed. Initial total 298.72 kB raw (81.90 kB transfer) within budgets. Lazy `recipients-page` chunk |
| Review-only spec `src/app/recipients/recipients-page/review-check.spec.ts` (scratch clone only, not committed), run with `npx ng test --watch=false` | Passed: 3 files, 24 tests (13 existing + 11 review checks for AC4, AC9, AC10, AC11, and AC12) |
| AgenticEngineering: `node tools/validate-harness.js` | "Harness policy is valid." (exit 0) |
| AgenticEngineering: `node --test tools/harness-policy.test.js tools/skills.test.js` | 25 passed, 0 failed |
| Backend contract read: `service/src/Common.AspNetCore/ErrorHandling/*`, `service/src/Common.Core/Exceptions/*`, `service/src/Core/Errors/ErrorCodes.cs` | The error codes and the `data.Field` key match the frontend mapping. No `service/` files change in the story diff |

## 6. Code-guideline assessment

Applied policy: repository `CLAUDE.md` (simplicity preferences), `web/CLAUDE.md`, and the AgenticEngineering Angular policy at `80199f3`: new application, TypeScript style, components, reactivity, dependency injection, routing, HTTP, forms, and testing. The `clean-code` and `component-testing` skills were applied. No .NET or Web API policy applies because the story changes no backend code.

- **Formatting:** fails on `web/angular.json` (F001). All other files pass Prettier.
- **TypeScript:** strict compiler and template options are set in `tsconfig.json`. There is no `any` and no non-null assertion. Members are `private`/`protected` where possible, and exports are named. The `as BackendError` cast in `error-message.ts` narrows `HttpErrorResponse.error` after an object check, which is acceptable.
- **Components:** standalone with the default OnPush (Angular 22). Built-in control flow with `track`. `input`/`output` functions. External templates. CLI file-name convention. Material only. The page is a list container and the form owns creation, which keeps responsibilities clear.
- **Reactivity:** signals and `computed`. `rxResource` handles loading, error, and value states explicitly. There are no manual subscriptions and no effects.
- **Dependency injection and routing:** `inject()`, `@Service()` for the API client, `app.config.ts` as composition root, lazy `loadComponent`, and a route `title`.
- **Forms:** Signal Forms with schema rules and `[formField]`. `mat-error` links errors to inputs. Submit is disabled while pending, and invalid submissions are rejected visibly.
- **HTTP:** a dedicated typed client, and components never use `HttpClient` directly. Two HTTP-guide rules conflict with explicit higher-precedence sources. Precedence settles both, so neither is a finding:
  - "Inject the API base URL through a typed `InjectionToken`": the code uses the literal relative path `/api/recipients`. Story AC13 and the agreed decisions fix a relative `/api` path with no host. The repository `CLAUDE.md` adds infrastructure only for a concrete current problem. Resolution: repository instructions and the story win, and no environment-specific host is hard-coded.
  - "Keep cross-cutting error handling in interceptors": the two callers open the shared dialog. The list must react after the dialog closes, and the creation message quotes the submitted code. Both need caller context. The shared `openErrorDialog` and `readableErrorMessage` avoid duplicated logic. This is proportionate to the repository's simplicity preference.
- **Tests:** they enter through the rendered page, keep the application real, and substitute only the backend. They find elements by label, role, and text and use Testing Library waits with no fixed delays. Arrange-Act-Assert sections are separated by blank lines. The helpers form a readable local test language and keep scenario values visible. Shared test infrastructure was assessed: `recipients-page.spec.ts` is the only recipient spec, and `app.routes.spec.ts` shares only the HTTP providers. Local helpers are enough, as each task's progress also records. F002 is a minor test-strength gap.
- **Commits:** the developer's own commit `b9b9c16` ("Modify for debug") has no story or task ID. The ID convention in `implement-task.md` governs agent task commits, so this is noted, not a finding.

## 7. Architecture assessment

- The change stays inside `web/` plus backlog documentation. There are no backend, API-contract, persisted-data, or deployment-configuration changes.
- Feature folder `src/app/recipients/` and shared `src/app/shared/` follow the new-application structure guidance. The single lazy route `/recipients` is the start route, as agreed.
- The frontend consumes the existing US-001 contract unchanged. The `RecipientDetails`, paging, and error-body types match the contract recorded in the plan and the backend sources.
- Failure handling is explicit for listing (dialog, then failure state and Retry) and creation (dialog, values kept, button re-enabled). Non-HTTP errors in creation are rethrown rather than swallowed.
- No parallel design, no extra state library, and no new external contract. CORS and production hosting are deferred to a later story as agreed.
- The backlog `README.md` gains `Type: Backend`/`Type: Frontend` and a frontend test-boundary note, and US-001 to US-003 gain `Type: Backend`. Both follow the story's agreed decision that frontend and backend stories are distinguished by `Type`.

No architecture defects found.

## 8. Recorded deviations

1. **Tests beyond the agreed six.** The story's test boundary specifies "these six tests" and verifies the double-submit guard manually. The implementation adds six more component tests: previous page, page label, both disabled states, the pending/double-submit guard, and a too-long value. They are acknowledged in T003 and T005 progress. The developer may keep or trim them.
2. **Manual checks replaced by throwaway specs.** T004 and T006 checked the remaining error mappings with deleted throwaway specs. T006 used no manual dev-server check. Both are acknowledged in the task progress. The reviewer's own review checks confirm those mappings.
3. **Angular version pinning.** The harness new-application guide says to pin the exact Angular version through `package.json`. `web/package.json` keeps the CLI-generated caret ranges, and `package-lock.json` pins exact versions. This is acknowledged in T001 ("Version ranges kept as generated; the lock file pins exact versions"), as the guide allows when a default is replaced.
4. **T007 (frontend CI) cancelled.** The developer removed it from scope on 2026-11-17. It is recorded under the story's excluded scope and in T007.
5. **T008 harness change outside the story branch.** The `new-application.md` change lives in AgenticEngineering commit `80199f3` on `feature/add-angular-harness`, as the task directs. It merges separately from this story.
6. **`analytics: false` in `web/angular.json`.** The developer's commit `b9b9c16` adds this local CLI setting, which no task records. It has no story impact apart from the formatting failure (F001).

## 9. Unverified items and remaining risks

- `npm start` and the `/api` proxy were not exercised by the reviewer, because the review must not start or call the real backend. The configuration was read, and T001 records a stub-server proxy check. Human review should confirm the dev server against the backend.
- `npm run format` was not run, because it would rewrite files. Its correction is part of F001.
- The story calls for manual verification of the loading indicator, Retry, the double-submit guard, and the remaining error mappings against a running backend. The reviewer covered them only with a mocked backend. They remain for human review.
- The duplicate-code message quotes the code as entered (for example `buda01`), because the backend's `Recipient.AlreadyExists` response carries no data. AC11 does not require the normalized code, so this is not a finding.
- `npm ci` warns that four packages' install scripts are not approved. Lint, test, and build work without them. The developer may approve them with `npm install-scripts approve`.
