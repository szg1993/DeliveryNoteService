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

2026-11-08 implementation preflight: the backlog is now tracked in commit
8140af9, but the solution and required shared sources remain untracked. The
developer removed `service/demo` manually. Implementation is waiting for this
human gate; no baseline files were staged or committed by the agent. There are
344 candidate baseline files outside the 158 duplicate shared-source files under
`service/src/Common.*`. Review and commit the intended baseline, or explicitly
authorize the agent to perform that separate baseline work before resuming T002.
