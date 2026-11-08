# T001: Establish the versioned demo baseline

- Parent: [US-001](../README.md)
- Status: todo
- Depends on: None
- Increment: Prerequisite
- Execution: Human gate

## Outcome

A checkout of the agreed repository baseline contains the existing target solution
and shared dependencies required for reproducible story implementation.

## Scope

The developer reviews the existing untracked `service/`, `src/`, `.editorconfig`,
and OpenAPI generation script and decides which files constitute the demo
baseline. Commit those intended inputs separately, excluding local settings,
generated build outputs, credentials, and unrelated artifacts. This task does not
authorize an agent to bulk-stage untracked files.

## Acceptance criteria

- [ ] The developer establishes and records the reviewed baseline commit.
- [ ] Git tracks `service/DeliveryNoteService.sln`, its required projects, and all
      referenced shared source dependencies and runtime/test assets.
- [ ] Existing unrelated local work is preserved.

## Verification

Inspect the baseline commit and tracked project references. Record the commit ID
and any explicit exclusions. No new product behavior or automated test is required
for this human gate; build and test verification belongs to the consuming tasks.

## Progress

No baseline decision or commit has been recorded. At planning time only
`.gitignore` was tracked; the existing solution and shared sources were untracked.
