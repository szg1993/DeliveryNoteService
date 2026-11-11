# US-001: Agent review 04

- Review type: agent review
- Review date and time: 2026-11-11 11:41 Europe/Budapest (UTC+01:00)
- Authoritative story: `docs/backlog/US-001-create-and-view-recipients.md` (AC9 revised 2026-11-11 in `8324535`)
- Plan: `docs/backlog/US-001-create-and-view-recipients/README.md`
- Harness repository and commit: `AgenticEngineering`, `d19a8d75f1a6e59c2c5953d32aa0741e4fc0195f`
- Reviewed branch: `feature/US-001-create-and-view-recipients`
- Reviewed head: `118d1b37be34275c1da2b52969442c1f3db564a4`
- Comparison base: original PR #1 base, `17fa518f621fbe6a6c4fac138dbf3cfb76ce0c3d`
- Review mode: follow-up review
- Previous review: [review-03.md](review-03.md), FAIL, coverage complete
- Previous reviewed head: `0ea32fd56da0a2a6991f0961b77ad84f32371d84` (ancestor of the reviewed head)
- Committed follow-up range: `0ea32fd56da0a2a6991f0961b77ad84f32371d84..118d1b37be34275c1da2b52969442c1f3db564a4`
- Included working-tree diff: None. The uncommitted `AGENTS.md` to `CLAUDE.md` renames at the repository root and in `service/test` are unrelated, do not overlap the story diff, and were excluded.
- Previous comparison base: `17fa518f621fbe6a6c4fac138dbf3cfb76ce0c3d`, unchanged. `master` is still `7720314`, which is the merge base with the reviewed head, so there is no base movement since review-03.
- Explicitly identified requirement change: the developer accepted the reduced generated OpenAPI contract on 2026-11-11 ("Accept reduced contract"), recorded as a revision of AC9 in `8324535`. The approval was given in chat and is taken from the story record and the review request; it was not observed directly by this reviewer.
- Review coverage: complete
- Explicitly included uncommitted changes: None
- Final verdict: FAIL

## Verdict and merge readiness

FAIL. The developer's explicit acceptance of the reduced OpenAPI contract
resolves the contract part of F007. Revised AC9 now requires only the three
operations, their status codes and the creation `Location` header, which
review-03 verified as present. The reduced schema detail is recorded below as an
acknowledged external-contract deviation and does not fail the review.

One part of F007 remains. `CreateRecipientDtoSchema`, which the story added in
`fa1a3ba`, is still unregistered dead code. Review-03's required correction for
the approval path was: "delete the dead filter and have the developer explicitly
approve and record the reduced contract". Only the approval and the record were
done. Neither the acceptance record nor the revised AC9 mentions keeping the
filter. F007 therefore stays blocking in narrowed form. Correcting it means
deleting one unused file, or the developer explicitly accepting that it stays.

Selected starting report: review-03 is the latest committed agent review. It has
complete coverage and is the baseline for unchanged code. No human acceptance
record exists.

Commits in the follow-up range:

- `35a75e4` US-001: Record review 03. Administrative. It adds only
  `docs/backlog/US-001-create-and-view-recipients/review-03.md`.
- `8324535` US-001: Record accepted reduced OpenAPI contract. A
  developer-approved requirement revision. It changes only the story file: it
  rewords AC9 and adds a progress note about review-03 and the acceptance. It
  makes no status or implementation change.
- `118d1b3` US-001: Record F007 contract acceptance in plan. Administrative. It
  adds one resume note to the plan, with no status, task or implementation
  change.

`git diff --stat 0ea32fd..HEAD -- service src` is empty, so the range contains
no implementation or test changes. Because there was no code correction,
review-story.md's no-correction rule was considered. It does not apply, because
the requirement revision is new evidence that changes F007's assessment.

| Item | Type | First reported in | Previous severity | Outcome | Evidence |
| --- | --- | --- | --- | --- | --- |
| F007: Generated OpenAPI contract regressed and does not match the implementation | Finding | review-03 | Blocking | Partly resolved by approved alternative; unresolved for the dead `CreateRecipientDtoSchema` (carried, blocking, narrowed). No code correction was attempted. | Revised AC9 (`8324535`) and the recorded developer acceptance cover the reduced schema detail, the numeric sort enums, the missing parameter descriptions and the assembly-name title. `service/src/WebApp/RestApi/CreateRecipientDtoSchema.cs:7` is unchanged in the range. A search of `service` and `src` finds only its declaration, and `service/src/WebApp/Configuration/WebAppInit.cs:9` registers only `NoteContractSchema`. |
| N001: Snapshot retains `JobExecutions` | Finding | review-03 | Non-blocking | Unresolved (carried) | `service/src/Persistence/Migrations/AppDbContextModelSnapshot.cs` is unchanged in the range. |
| R001: Default-parallel suite stall | Risk | review-01 | Non-blocking risk | Unresolved (carried) | No test change; not rerun. |
| R002: PostgreSQL timestamp precision | Risk | review-01 | Non-blocking risk | Unresolved inherent limit (carried) | No test or persistence change; not rerun. |

## Blocking findings

### F007: Unregistered `CreateRecipientDtoSchema` remains as dead code (narrowed)

- First reported in: review-03
- Correction attempts: none. The range contains only records and a requirement revision.
- Location: `service/src/WebApp/RestApi/CreateRecipientDtoSchema.cs:7`; registration point `service/src/WebApp/Configuration/WebAppInit.cs:9`
- Violated rule: clean code (dead code), as cited in review-03. The class
  suggests that the create-recipient schema is marked required, non-nullable and
  `minLength: 1`, but the accepted generated contract does not do this. The
  class is a story addition from `fa1a3ba`, not pre-existing code.
- Evidence: the file is unchanged since review-03. The search
  `grep -rn "CreateRecipientDtoSchema" --include=*.cs service src` returns only
  the class declaration. `WebAppInit` calls
  `services.AddSwaggerGen(options => options.SchemaFilter<NoteContractSchema>())`.
- Required correction: delete `CreateRecipientDtoSchema.cs` and confirm that the
  solution still builds. Registering it instead is also acceptable, because it
  only adds detail to the accepted reduced contract. If the developer prefers to
  keep the file, record an explicit acceptance of that so this finding can be
  closed as an approved alternative.

## Non-blocking findings

### N001: Migration snapshot retains `JobExecutions`, which the runtime model no longer maps

- First reported in: review-03
- Location: `service/src/Persistence/Migrations/AppDbContextModelSnapshot.cs:26`
- Status: unchanged. The next generated migration will include a
  `DropTable JobExecutions`. The developer should decide whether to drop the
  table on purpose or keep it. This does not affect the verdict.

## Requirement traceability

Every status except AC9 is inherited from review-03 because no implementation,
test or other requirement text changed in the range. Inherited evidence was not
rerun in this review.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
| --- | --- | --- | --- | --- |
| AC1: 201, envelope, Location | Unchanged since review-03 | Inherited from review-03; not rerun | Met (inherited) | None |
| AC2: Read back normalized code/country, supplied text, ID, UTC time, foreign status | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC3: Immediately listed | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC4: Invalid creation rejected, nothing created | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC5: Deterministic list order | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC6: Empty list succeeds | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC7: Existing detail returns 200 | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC8: Unknown ID returns 404 | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC9 (revised `8324535`): three operations, their status codes and the creation Location header in the generated OpenAPI; detailed schema annotations not required | Endpoint metadata in `RecipientEndpoints.cs`, unchanged | Re-evaluated against the revised text using review-03's generated-document evidence: all three operations, status codes (201/400, 200/400, 200/400/404) and `Location` were present. Generation was not rerun because the code is unchanged. | Met | None for AC9; the dead code in F007 is a separate code-guideline issue |
| AC10: Duplicate names allowed | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC11: Normalized-code uniqueness; 400 `Recipient.AlreadyExists` sequentially and concurrently | Unchanged | Inherited (SQLite and the approved PostgreSQL run in review-03); not rerun | Met (inherited) | None |
| AC12: Paging defaults and ID/name sorting | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC13: Recognized country, uppercased, HU domestic | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC14: Required fields, normalization, lengths | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC15: Demo paging format, validation, limit=0 | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC16: 400/404 error behavior and stable codes | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC17: Normalized code character set | Unchanged | Inherited; not rerun | Met (inherited) | None |
| AC18: No postal or address verification | Unchanged | Inherited; not rerun | Met (inherited) | None |
| Revised agreed test boundary | Unchanged | Inherited; not rerun | Met (inherited) | None |
| Repository conventions (logging, DTO mapping, error codes, formatting) | Unchanged | Inherited formatter verification; not rerun | Met (inherited) | None |
| Technology OpenAPI/ASP.NET Core metadata consistency | `WebAppInit` Swagger registration, unchanged | Review-03 generated document | Acknowledged deviation (approved reduced contract); not a failing item | F007 (dead code only) |
| Historical TDD execution | Unchanged | Inherited; not recreated | Met (inherited) | None |

## Verification performed

All commands ran from the repository root. No source, test, story, plan or task
file was changed.

```text
git merge-base --is-ancestor 0ea32fd HEAD
```

Result: exit 0, so the previous reviewed head is an ancestor of the reviewed head.

```text
git log --oneline 0ea32fd..HEAD
git diff --name-status 0ea32fd..HEAD
git show 8324535 118d1b3
git show --stat 35a75e4
```

Result: three commits. The changed files are only the story file, the plan
`README.md` and the added `review-03.md`. The story diff rewords AC9 and adds a
progress note. The plan diff adds one resume note. `35a75e4` adds only
`review-03.md`.

```text
git diff --stat 0ea32fd..HEAD -- service src
```

Result: empty. There are no implementation or test changes.

```text
git rev-parse master
git merge-base master HEAD
```

Result: both are `7720314`. The base has not moved since review-03.

```text
grep -rn "CreateRecipientDtoSchema" --include=*.cs service src
```

Result: one match, the class declaration at
`service/src/WebApp/RestApi/CreateRecipientDtoSchema.cs:7`. It has no
registration.

Builds, tests, formatter verification and OpenAPI generation were not rerun.
The implementation is unchanged since review-03, which ran them. No PostgreSQL
or Testcontainers tests were run.

## Code-guideline assessment

Applied central guidance: .NET and Web API (OpenAPI) technology policies, the
repository `CLAUDE.md` and `service/test/CLAUDE.md`, and `clean-code`. The range
changes no code, so the code assessment of review-03 stands. The only open
code-guideline item is F007's dead `CreateRecipientDtoSchema`. The repository's
proportionality guidance does not excuse it, because removing an unused file is
a direct fix and not a speculative refactor. The requirement and record commits
are consistent with each other, and the story text accurately reflects the
review-03 finding.

## Architecture assessment

No architecture-relevant change in the range. Review-03's architecture
assessment stands. The reduced generated OpenAPI document is an external
contract. It is now an acknowledged and approved deviation, recorded below, so
it no longer fails the review on its own.

## Recorded deviations

- Acknowledged external-contract deviation (approved 2026-11-11, recorded in
  `8324535`, review-03 F007 context). Compared with the review-01 contract
  (`service/openapi.json` at `5f5b513`), the generated OpenAPI document now has
  these reductions:
  - `CreateRecipientDto` has no `required` list. Its properties are shown as
    nullable, with no `minLength` and no descriptions.
  - `RecipientsSortBy` and `SortDirection` appear as integer enums `[0, 1]`.
  - The page and limit parameters have no descriptions.
  - `info` shows the assembly full name as the title, with version `1.0`.

  Record this in any external tracker or with API consumers.
- Developer-approved requirement revisions of 2026-11-11 (`d518e71`: AC11,
  AC14, AC16, AC17 and the test boundary) remain as recorded in review-03.
- The scope deviations recorded in review-02 and review-03 remain: the Core
  layer replaces the planned split, demo removal, and removal of
  `service/openapi.json` and `OpenApiSpecTest`. The plan should be reconciled
  when the story is completed.

## Unverified items and remaining risks

- R001: the cause of the default-parallel stall is still not established. It is
  carried from review-01.
- R002: PostgreSQL timestamp precision is an inherent provider limit. It is
  carried from review-01.
- N001: the `JobExecutions` snapshot drift needs a deliberate decision before
  the next migration.
- The developer's chat approval ("Accept reduced contract") is taken from the
  story record and the review request. It was not observed directly.
- The formatter failures in the US-002 files that review-03 noted remain outside
  US-001 scope.
- Inherited test, formatter and OpenAPI-generation evidence was not rerun,
  because the implementation is unchanged.
