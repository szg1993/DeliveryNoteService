# US-006: Agent review 01

- Review type: agent review
- Review date and time: 2026-11-18 16:59 +01:00
- Authoritative story: `docs/backlog/US-006-delivery-note-pages.md` (status In review)
- Plan and tasks: `docs/backlog/US-006-delivery-note-pages/README.md`, `tasks/T001`–`T007`
- `AgenticEngineering` commit: `80199f355a77c687df695044257172578b803225`
- Reviewed head: `feature/US-006-delivery-note-pages` at `5178e13dd62bbce0f444a4a834eb0af009fbe76f`
- Comparison base: `master` as published at `origin/master` = `25e44a13ad686e9f867af41035f73fd7c4f86480`
  (the story branch's base). The local `master` ref is stale at `75c82ee` and lacks US-005, so it was
  not used; diffing against it would wrongly include the US-005 backend changes.
- Review coverage: complete
- Explicitly included uncommitted changes: none (working tree clean)
- Final verdict: **FAIL**

## 1. Verdict and merge readiness

**FAIL. Not ready to merge.** The story's functionality is implemented, and the code follows the
repository and Angular conventions closely. Two blocking findings remain:

- F001: T002 removed the app's main content layout (`.app-content` max width and padding), so every
  page, including the existing Recipients page, now renders flush against the window edges.
- F002: agreed test 4 takes about 5 s even when its spec file runs alone. That is the Vitest default
  timeout, so `npm test` failed in every one of four full-suite runs during this review.

Previous reports: none. This is an initial review. Correction history: none.

## 2. Blocking findings

### F001: Main content layout removed (regression)

- Location: `web/src/app/app.scss` (commit `b74f537`, T002); `web/src/app/app.html:13` still uses
  `<main class="app-content">`.
- Rule: no regressions to existing behaviour (review-story "Classify findings"). The story asks only
  to add toolbar links (AC2), and T002's scope says to keep the existing toolbar.
- Evidence: at base `25e44a1`, `app.scss` defines
  `.app-content { max-width: 1200px; margin: 0 auto; padding: 24px 16px 48px; }`. T002 replaced that
  rule with the nav styles, and no other stylesheet defines `.app-content`
  (`grep -rn app-content web/src` only matches `app.html`). The deviation is not mentioned in T002's
  progress notes. In a browser check (dev server with an empty proxy config), the computed style of
  `main.app-content` was `max-width: none; padding: 0px; margin: 0px`, and the page `h1` started at
  x = 0. This affects the new pages and the existing Recipients page.
- Required correction: restore the `.app-content` rule alongside the new toolbar and nav styles.

### F002: Agreed test 4 exceeds the default test timeout, so the suite fails

- Location: `web/src/app/delivery-notes/create-delivery-note-page/create-delivery-note-page.spec.ts:16`
  ("creates a note with two lines as entered and returns to the list"). Related:
  `web/src/testing/http.ts` (`waitFor` timeout of 5000 ms).
- Rule: required tests must pass, and required checks must be reliable (review-story "Review the
  implementation"; `web/CLAUDE.md` tests; the story's agreed test boundary, test 4).
- Evidence:
  - `npm test` (full suite) was run four times, and test 4 timed out every time ("Test timed out in
    5000ms", at 5264, 5314, 5234 and 5545 ms). In one of those runs the T002 toolbar test
    (`app.spec.ts`) also timed out, at 5411 ms.
  - Running only the creation spec (`ng test --include …create-delivery-note-page.spec.ts`), test 4
    passed twice and then timed out once at 5098 ms. Its run time sits right at the 5 s limit.
  - T007 raised the `answerEmptyListRequest` `waitFor` timeout to 5 s to fix the flaky toolbar test.
    That cannot make the test reliable, because the per-test timeout is also 5 s. The implementation
    notes report 20/20 in three runs; this could not be reproduced on the review machine.
- Required correction: make test 4 and the toolbar test pass reliably in full-suite runs. For
  example, reduce the per-test work, or give these slower routed tests an explicit, justified test
  timeout above the helper's wait. Keep the helper's wait below the test timeout. Then confirm with
  repeated full-suite runs.

## 3. Non-blocking findings

- N001: Running the creation spec logs Angular dev warning NG0956 ("track by identity caused
  re-creation of the entire collection of size 1"). The only identity `track` in the changed code is
  `@for (line of noteForm.noteLines; track line …)` in `create-delivery-note-page.html:232`. Check
  whether model updates re-create every line's DOM, which loses focus and costs work. If they do,
  track a stable per-line key. Values are kept, because they live in the model.
- N002: The no-recipients hint (`create-delivery-note-page.html:24-29`) is a plain paragraph rather
  than `mat-hint`, which T006 records and justifies. It meets AC9 ("a hint under the select"). It is
  not linked to the select with `aria-describedby`, so screen-reader users who focus the select
  will not hear it. Linking it is optional.
- N003: `dateErrorMessages` (`create-delivery-note-page.ts:300-308`) depends on Material's
  `matDatepickerParse` error key and on the extra message-less `matDatepickerMin` error. This is
  documented framework behaviour, and T005 explains it in a comment. Only manual checks cover it
  (per the agreed boundary), so a Material upgrade could change it silently. Re-check the
  invalid-date message after upgrades.

## 4. Requirement traceability

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
|---|---|---|---|---|
| AC1 Routes `/delivery-notes`, `/delivery-notes/new`, `/recipients`; empty path redirects; lazy loaded; titles | `app.routes.ts` (`loadComponent`, `title`, `redirectTo: 'delivery-notes'`) | `app.routes.spec.ts` (updated); build shows lazy chunks; browser: title "New delivery note" | Met | None |
| AC2 Toolbar "Delivery notes" and "Recipients" links, current one highlighted | `app.html` (`routerLinkActive="active-link"`, `ariaCurrentWhenActive`), `app.scss` | `app.spec.ts` (test 3); highlight checked manually per T002 | Met | F001 (side effect of this change) |
| AC3 `GET /api/delivery-notes` newest first, 10 per page, previous/next from paging metadata | `delivery-notes-api.ts` `getDeliveryNotes`; `delivery-notes-page.html` paging nav | Test 1 (`sortDir=Desc`, `limit=10`); paging verified manually (T003) | Met | None |
| AC4 Columns, status text, "Urgent" text chip only for urgent, `yyyy-MM-dd` dates as returned | `delivery-notes-page.html` columns, `mat-chip` | Test 1 asserts every cell | Met | None |
| AC5 Empty-state message | `delivery-notes-page.html` "No delivery notes yet." | Test 2 | Met | None |
| AC6 Loading indicator; failure opens dialog, then "Delivery notes could not be loaded." with Retry | `delivery-notes-page.ts` `loadDeliveryNotes`, `retry()`; template | Manual (T003), per the agreed boundary | Met | None |
| AC7 "New delivery note" button; rows not clickable | `delivery-notes-page.html:3`; rows have no handlers | Code inspection; manual (T004) | Met | None |
| AC8 One scrolling page, titled sections, agreed labels | `create-delivery-note-page.html` (Header, Flags, Contact, Package, Deadlines, Lines; labels match) | Tests 4–7 find fields by these labels and regions | Met | None |
| AC9 Recipient `mat-select` from `GET /api/recipients?Limit=0` as "CODE – Name"; no-recipients hint with link; load failure → dialog, submit disabled, no Retry | `recipients-api.ts` `getAllRecipients`; `create-delivery-note-page.ts` `loadRecipients`, `noRecipients`, `recipientsLoadFailed` | Tests 4–7 (request `limit=0`, option text); review browser check: failed load opened the dialog and submit stayed disabled; hint checked manually (T006) | Met | N002 |
| AC10 Lines start with one; Add line; remove disabled at one; order kept | `emptyNote()`, `addLine`, `removeLine`, `[disabled]="noteForm.noteLines.length === 1"` | Test 4 (two lines in order); remove checked manually (T004) | Met | N001 |
| AC11 Units and package scale are free text | Plain `matInput` text inputs | Test 4 | Met | None |
| AC12 Inline validation blocks submission: required fields, maximum lengths, quantities > 0, weights/dimensions ≥ 0, date order | Schema in `create-delivery-note-page.ts:159-207`; limits match `NoteLimits` | Test 5 (required), test 6 (date order); numeric minimums and max lengths verified manually | Met | None |
| AC13 `type="number"`, decimals, JSON numbers; native datepicker defaults; "Enter a valid date."; local `yyyy-MM-dd` without UTC conversion | Template number inputs; `provideNativeDateAdapter()`; `dateErrorMessages`; `calendarDate` | Test 4 (decimals as numbers, `yyyy-MM-dd`); invalid-date message verified manually (T005) | Met | N003 |
| AC14 Text untrimmed; empty optional text sent as `null` | `toNewDeliveryNote`, `emptyAsNull` | Test 4 (`' Győr '`, `null` EKAER, comments and line comment) | Met | None |
| AC15 One POST; submit disabled while in progress | Submission action; `[disabled]="noteForm().submitting() …"` | Test 4 (`expectOne`); guard verified manually (T004) | Met | None |
| AC16 On 201 snackbar "Delivery note {NoteNbr} created", navigate to the list | `confirmCreated` | Test 4 | Met | F002 (test unreliable) |
| AC17 Cancel returns to the list without saving or a warning | `<a mat-button routerLink="/delivery-notes">Cancel</a>` | Code inspection; manual (T004) | Met | None |
| AC18 Every backend or network error from creation or loading opens the shared dialog; values kept | `openErrorDialog` in both pages and recipient loading; form not reset | Test 7; review browser check (failed recipient load) | Met | None |
| AC19 Error-message mappings | `shared/error-message.ts` (new `Recipient.NotFound`, `Note.InvalidInput`, `Note.NumberExhausted`; existing field and fallback cases) | Test 7 (`Note.InvalidInput`); the rest verified manually or by a throwaway spec (T007) | Met | None |
| Test boundary: exactly seven new tests plus the updated redirect test | `app.spec.ts` (1), `delivery-notes-page.spec.ts` (2), `create-delivery-note-page.spec.ts` (4); `app.routes.spec.ts` updated | Counted in the specs | Met | F002 |
| Tests never call the real backend | `HttpTestingController` everywhere; `verify()` in `afterEach` | Spec inspection | Met | None |
| No backend changes | `git diff 25e44a1 5178e13 -- service` is empty | Diff | Met | None |

## 5. Verification performed

All commands ran in `web/` on the reviewed head with a clean working tree.

| Command | Result |
|---|---|
| `npm run lint` | Pass ("All files pass linting.") |
| `npm run format:check` | Pass ("All matched files use Prettier code style!") |
| `npm run build` | Pass; lazy chunks `delivery-notes-page` and `create-delivery-note-page` |
| `npm test` (×4) | **Fail** in every run: 1, 1, 2 and 1 failures out of 20. Test 4 timed out each time ("Test timed out in 5000ms"); the toolbar test also timed out once (5411 ms) |
| `npx ng test --watch=false --include src/app/delivery-notes/create-delivery-note-page/create-delivery-note-page.spec.ts` (×3) | Passed twice (4/4); one run failed with test 4 at 5098 ms. That run also logged NG0956 |
| Browser check: `npx ng serve --port 4321 --proxy-config <temporary-directory>/empty-proxy.json` (no `/api` target, so the real backend was unreachable), page `/delivery-notes/new` | Recipient load failed, and the dialog showed "Something went wrong. Please try again."; after OK, "Create delivery note" was disabled and no hint showed; title "New delivery note". Computed style of `main.app-content`: `max-width: none`, `padding: 0px` (F001). The server was stopped afterwards |

The real backend API was never run or called.

## 6. Code-guideline assessment

Applied: repository `CLAUDE.md` and `web/CLAUDE.md` (hobby-scale proportionality, Angular Material
only, hand-written API clients, signals, `HttpTestingController`). Also applied: the Angular
technology policy (TypeScript style, components, reactivity, dependency injection, routing, HTTP,
forms, testing) and `clean-code`.

- Formatting, lint and build pass. Strict TypeScript holds: no `any` and no non-null assertions in
  the changed code.
- The change follows the US-004 patterns: `@Service()` API clients with typed request and response
  interfaces, `rxResource` with explicit loading, error and empty states, Signal Forms with built-in
  `required`, `maxLength`, `min` and `minDate` (a custom `validate` only for the exclusive minimum),
  `[formField]` bindings, built-in control flow, and lazy `loadComponent` routes with titles.
- The request is mapped explicitly (`toNewDeliveryNote`, `toNewNoteLine`), as the forms policy
  requires, and the fields match `CreateDeliveryNoteDto` (which disallows unknown members). Client
  maximum lengths match `NoteLimits`, and numeric and date rules match the backend DTO checks.
- Duplication: the error-dialog `catchError` and the paging nav are repeated between the two list
  pages. T003 records and justifies this as acceptable at two consumers, which this review accepts
  under the repository's simplicity rule.
- Test infrastructure: `src/testing/table.ts` (moved from the recipients spec, two consumers) and
  `src/testing/http.ts` (`answerEmptyListRequest`, two consumers) are stateless, configured per call
  and use each test's own TestBed. Both are adopted by every relevant consumer, and
  `tsconfig.app.json` keeps them out of the app build. T007 changed the `http.ts` timeout outside
  its stated scope, with a recorded reason. That change is acceptable in itself but does not fix the
  flakiness (F002).
- Tests meet the Angular testing policy: routed pages are rendered with the real router and forms,
  elements are found by role and label, only the HTTP boundary is substituted, and the assertions
  check visible text and request bodies. Their reliability fails (F002).

## 7. Architecture assessment

- The new feature lives in `web/src/app/delivery-notes/`. `PagedResponse` and `Paging` moved to
  `shared/` because two features now use them, and recipient imports were updated with no behaviour
  change.
- Components use API client services, never `HttpClient` directly, and keep the relative `/api`
  base, as `web/CLAUDE.md` requires.
- `provideNativeDateAdapter()` is provided at the creation page component, the only consumer. This
  is a narrow, justified scope.
- No external contract changed: there are no backend changes, and the client matches the existing
  US-001, US-002 and US-005 endpoints.
- No parallel design. The only architecture-level defect is the lost layout container (F001).

## 8. Recorded deviations

- Unacknowledged: removal of the `.app-content` layout rule in T002 (F001).
- Acknowledged in T007: `web/src/testing/http.ts` wait timeout raised to 5 s, outside the task's
  stated scope, to address the flaky T002 test. Recorded here; see F002 for why it is insufficient.
- Acknowledged in T006: the no-recipients hint is a paragraph, not `mat-hint`. It is consistent with
  AC9.
- Acknowledged in T005: the invalid-date message relies on the datepicker's `matDatepickerParse`
  error key, and the datepicker's `matDatepickerMin` error is filtered out. This is consistent with
  AC13's "native date adapter and its defaults".
- Wording choices within the story's freedom: the list column header is "Estimated arrival", the
  paging buttons are "Previous page" and "Next page" (as on the recipient page), and the empty state
  reads "No delivery notes yet."
- Process note: this report's commit subject follows the reviewer prompt ("US-006: Add agent review
  01") rather than the workflow's "US-006: Record review 01". Explicit user instructions take
  precedence.

## 9. Unverified items and remaining risks

The story's agreed boundary leaves these behaviours to human manual checks. The implementation notes
record checks against local stubs, but this review did not repeat them in the browser, except where
section 5 says so:

- list paging, loading bar, failure dialog then Retry;
- the active toolbar link highlight;
- adding and removing lines, maximum-length messages, numeric minimums, the double-submit guard,
  and Cancel;
- the no-recipients hint and its link;
- the "Enter a valid date." message, and date display in the browser locale;
- the error mappings other than `Note.InvalidInput` (`Note.NumberExhausted`, `Recipient.NotFound`,
  `FieldMissing`/`FieldTooLong` with `data.Field`, network failure, unknown codes);
- end-to-end behaviour against the real backend, including the created note appearing at the top of
  the list. This is out of scope for automated tests per the story.

Remaining risk: test timing depends on the machine (F002). Faster machines may pass where the review
machine did not.
