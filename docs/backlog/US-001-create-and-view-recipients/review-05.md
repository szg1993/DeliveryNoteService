# US-001: Agent review 05

- Review type: agent review
- Review date and time: 2026-11-11 11:49 Europe/Budapest (UTC+01:00)
- Authoritative story: `docs/backlog/US-001-create-and-view-recipients.md` (AC9 revised 2026-11-11 in `8324535`)
- Plan: `docs/backlog/US-001-create-and-view-recipients/README.md`
- Harness repository and commit: `agentic-engineering`, `d19a8d75f1a6e59c2c5953d32aa0741e4fc0195f`
- Reviewed branch: `feature/US-001-create-and-view-recipients`
- Reviewed head: `bbc5c93bf6ca5812d275c084d44f017fdd95c364`
- Comparison base: original PR #1 base, `17fa518f621fbe6a6c4fac138dbf3cfb76ce0c3d`
- Review mode: follow-up review
- Previous review: [review-04.md](review-04.md), FAIL, coverage complete
- Previous reviewed head: `118d1b37be34275c1da2b52969442c1f3db564a4` (ancestor of the reviewed head)
- Committed follow-up range: `118d1b37be34275c1da2b52969442c1f3db564a4..bbc5c93bf6ca5812d275c084d44f017fdd95c364`
- Included working-tree diff: None. The uncommitted `AGENTS.md` to `CLAUDE.md` renames at the repository root and in `service/test` are unrelated, do not overlap the story diff, and were excluded.
- Previous comparison base: `17fa518f621fbe6a6c4fac138dbf3cfb76ce0c3d`, unchanged. `master` is still `7720314`, which is the merge base with the reviewed head, so there is no base movement since review-04.
- Explicitly identified policy or evidence change: None
- Review coverage: complete
- Explicitly included uncommitted changes: None
- Final verdict: PASS

## Verdict and merge readiness

PASS. The only blocking item from review-04, the narrowed F007, is resolved.
Commit `1cb679d` deletes the unregistered
`service/src/WebApp/RestApi/CreateRecipientDtoSchema.cs`, which was review-04's
first-listed required correction. No source reference to the class remains, the
solution builds with no warnings, and the non-PostgreSQL test suite passes. The
follow-up range contains no other implementation change. Every requirement is
met, either inherited unchanged from review-04/review-03 or re-confirmed here.
The story is ready for human review and acceptance. Non-blocking items N001,
R001 and R002 remain and do not prevent the verdict.

Selected starting report: review-04 is the latest committed agent review. It has
complete coverage and is the baseline for unchanged code. No human acceptance
record exists.

Commits in the follow-up range:

- `db3ace0` US-001: Record review 04. Administrative. It adds only
  `docs/backlog/US-001-create-and-view-recipients/review-04.md`.
- `1cb679d` US-001: Remove unused CreateRecipientDtoSchema (F007 correction 1).
  Implementation correction. It deletes only
  `service/src/WebApp/RestApi/CreateRecipientDtoSchema.cs` (21 lines).
- `bbc5c93` US-001: Record review-04 F007 correction attempt 1. Administrative.
  It adds a resume note to the plan `README.md` and a progress entry to
  `tasks/T002-create-and-retrieve-recipient.md`. It changes no status, task
  outcome or requirement text.

| Item | Type | First reported in | Previous severity | Outcome | Evidence |
| --- | --- | --- | --- | --- | --- |
| F007: Unregistered `CreateRecipientDtoSchema` remains as dead code (narrowed; originally "Generated OpenAPI contract regressed and does not match the implementation") | Finding | review-03 | Blocking | Resolved. Correction attempt 1 (`1cb679d`) succeeded. | The file is deleted. `git grep -n CreateRecipientDtoSchema` finds matches only in backlog records, not in `service` or `src`. `service/src/WebApp/WebApp.csproj` is SDK-style with no explicit `Compile` items, so nothing references the removed path. The remaining `ISchemaFilter` implementations (`NoteContractSchema`, `EnumSchemaFilter`, `DtoPropertyDescriptionFilter`) are unaffected. The build and tests pass. The contract part of F007 was already resolved by the approved revised AC9 in review-04. |
| N001: Snapshot retains `JobExecutions` | Finding | review-03 | Non-blocking | Unresolved (carried) | `service/src/Persistence/Migrations/AppDbContextModelSnapshot.cs` is unchanged in the range. |
| R001: Default-parallel suite stall | Risk | review-01 | Non-blocking risk | Unresolved (carried) | The suite was run only with `xUnit.ParallelizeTestCollections=false`; the default-parallel behavior was not rechecked. |
| R002: PostgreSQL timestamp precision | Risk | review-01 | Non-blocking risk | Unresolved inherent limit (carried) | No test or persistence change; PostgreSQL tests were not run. |

## Blocking findings

None

## Non-blocking findings

### N001: Migration snapshot retains `JobExecutions`, which the runtime model no longer maps

- First reported in: review-03
- Location: `service/src/Persistence/Migrations/AppDbContextModelSnapshot.cs:26`
- Status: unchanged and carried. The next generated migration will include a
  `DropTable JobExecutions`. The developer should decide whether to drop the
  table on purpose or keep it. This does not affect the verdict.

## Requirement traceability

The only code change in the range deletes an unregistered schema filter that no
code referenced, so it cannot change runtime or generated-OpenAPI behavior.
Every status is inherited from review-04 (which inherited from review-03 except
AC9). Inherited evidence was not rerun in this review, apart from the
non-PostgreSQL regression suite listed under verification, which passed.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
| --- | --- | --- | --- | --- |
| AC1: 201, envelope, Location | Unchanged since review-04 | Inherited from review-04/03; not rerun individually | Met (inherited) | None |
| AC2: Read back normalized code/country, supplied text, ID, UTC time, foreign status | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC3: Immediately listed | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC4: Invalid creation rejected, nothing created | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC5: Deterministic list order | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC6: Empty list succeeds | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC7: Existing detail returns 200 | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC8: Unknown ID returns 404 | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC9 (revised `8324535`): three operations, their status codes and the creation Location header in the generated OpenAPI; detailed schema annotations not required | Endpoint metadata in `RecipientEndpoints.cs`, unchanged. The deleted filter was never registered in `WebAppInit`, so generation is unaffected. | Inherited from review-04 (review-03 generated-document evidence); OpenAPI generation not rerun | Met (inherited) | None |
| AC10: Duplicate names allowed | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC11: Normalized-code uniqueness; 400 `Recipient.AlreadyExists` sequentially and concurrently | Unchanged | Inherited (SQLite, and the approved PostgreSQL run in review-03); PostgreSQL not rerun | Met (inherited) | None |
| AC12: Paging defaults and ID/name sorting | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC13: Recognized country, uppercased, HU domestic | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC14: Required fields, normalization, lengths | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC15: Demo paging format, validation, limit=0 | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC16: 400/404 error behavior and stable codes | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC17: Normalized code character set | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| AC18: No postal or address verification | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| Revised agreed test boundary | Unchanged | Inherited; not rerun individually | Met (inherited) | None |
| Repository conventions (logging, DTO mapping, error codes, formatting) | Unchanged except the deletion | Formatter verification for `service/src/WebApp/` rerun and passed; other evidence inherited | Met | None |
| Technology OpenAPI/ASP.NET Core metadata consistency | `WebAppInit` Swagger registration, unchanged | Inherited review-03 generated document | Acknowledged deviation (approved reduced contract); not a failing item | None (F007 resolved) |
| Historical TDD execution | Unchanged | Inherited; not recreated | Met (inherited) | None |

## Verification performed

All commands ran from the repository root. No source, test, story, plan or task
file was changed.

```text
git merge-base --is-ancestor 118d1b3 HEAD
```

Result: exit 0, so the previous reviewed head is an ancestor of the reviewed head.

```text
git log --oneline 118d1b3..HEAD
git diff --name-status 118d1b3..HEAD
git show --stat db3ace0 1cb679d bbc5c93
```

Result: three commits. Changed files: `README.md` (M), `review-04.md` (A),
`tasks/T002-create-and-retrieve-recipient.md` (M), and
`service/src/WebApp/RestApi/CreateRecipientDtoSchema.cs` (D). The only
implementation change is the deletion.

```text
git rev-parse master
git merge-base master HEAD
```

Result: both are `7720314`. The base has not moved since review-04.

```text
git grep -n CreateRecipientDtoSchema
git grep -n "AddSchemaFilter\|ISchemaFilter" -- service src
git grep -n "Compile Include\|Compile Remove" -- '*.csproj'
```

Result: `CreateRecipientDtoSchema` appears only in backlog documents (story,
plan, task file and earlier review reports). No matches in `service` or `src`.
The remaining schema filters are `NoteContractSchema`, `EnumSchemaFilter` and
`DtoPropertyDescriptionFilter`. No project file lists compile items explicitly.

```text
dotnet build service/DeliveryNoteService.sln
```

Result: Build succeeded, 0 warnings, 0 errors.

```text
dotnet test service/DeliveryNoteService.sln --filter "Database!=PostgreSQL" -- xUnit.ParallelizeTestCollections=false
```

Result: exit 0. `ApiTests.dll`: 74 passed, 0 failed, 0 skipped.
`Medusa.Enap.Common.UnitTests.dll`: 182 passed, 0 failed, 0 skipped.

```text
dotnet format service/DeliveryNoteService.sln --verify-no-changes --include service/src/WebApp/
```

Result: exit 0, no formatting changes required.

PostgreSQL and Testcontainers tests were not run. OpenAPI generation was not
rerun, because the deleted filter was never registered.

## Code-guideline assessment

Applied central guidance: the .NET and Web API technology policies, the
repository `CLAUDE.md` and `service/test/CLAUDE.md`, and `clean-code`. The
correction is a pure dead-code removal that matches review-04's required
correction. It leaves no orphaned usings or references, and the Swashbuckle
dependency is still used by other filters. The record commits accurately
describe the correction and its evidence, which this review reproduced. No new
code-guideline issue was introduced.

## Architecture assessment

No architecture-relevant change in the range. Deleting an unregistered filter
does not change the generated OpenAPI contract, module dependencies or
configuration. The architecture assessment of review-03 and review-04 stands.

## Recorded deviations

- Acknowledged external-contract deviation (approved 2026-11-11, recorded in
  `8324535`), carried from review-04. Compared with the review-01 contract
  (`service/openapi.json` at `5f5b513`), the generated OpenAPI document has
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
  carried from review-01. Tests ran with collection parallelization disabled.
- R002: PostgreSQL timestamp precision is an inherent provider limit. It is
  carried from review-01.
- N001: the `JobExecutions` snapshot drift needs a deliberate decision before
  the next migration.
- The developer's chat approval of the reduced contract is taken from the story
  record. It was not observed directly.
- The formatter failures in the US-002 files that review-03 noted remain outside
  US-001 scope.
- Inherited per-requirement evidence, PostgreSQL tests and OpenAPI generation
  were not rerun.
