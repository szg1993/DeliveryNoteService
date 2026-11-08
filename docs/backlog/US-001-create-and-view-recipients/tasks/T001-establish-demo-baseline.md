# T001: Establish the versioned demo baseline

- Parent: [US-001](../README.md)
- Status: done
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

- [x] The developer establishes and records the reviewed baseline commit.
- [x] Git tracks `service/DeliveryNoteService.sln`, its required projects, and all
      referenced shared source dependencies and runtime/test assets.
- [x] Existing unrelated local work is preserved.

## Verification

Inspect the baseline commit and tracked project references. Record the commit ID
and any explicit exclusions. No new product behavior or automated test is required
for this human gate; build and test verification belongs to the consuming tasks.

## Progress

2026-11-08 completed: baseline commit
`96c28175a57702594123080c992b423b4f8fc2b3` (`T001: Capture reviewed demo baseline`)
tracks exactly the reviewed 344 existing files. It contains no backlog edits or
changes to imported source contents. All 22 ProjectReference paths resolve within
the selected baseline. The manually removed nested `service/demo` is absent, and
the 158 duplicate files in `service/src/Common.*` remain untracked and untouched.

Scoped clean-code assessment: this task records an existing baseline rather than
changing behavior, so no code refactoring is justified. Imported source formatting
is preserved under the explicit snapshot instruction; no broad formatter was run.
No Markdown formatter is configured; tracking-file links and whitespace were
verified instead. No tests or build were required for this gate. .NET SDK
10.0.302 and Docker CLI 29.6.1 were found; root separately verified the Docker
engine and .NET 8 runtimes. Application build/test results remain for T002.

Skills: `clean-code` used for the scoped baseline/refactoring assessment. No test
skills were loaded for this behavior-preserving import; no tests were written.

2026-11-08: The developer explicitly authorized the agent to "review and commit
those baseline files separately, then continue US-001", resolving the human-only
ownership of this gate. Review selected 344 existing files and confirmed all 22
project references remain inside that selection. The 158 unused copies under
`service/src/Common.*` are excluded and remain untouched. No local-secret
filenames, private-key material, or token signatures were found; development
settings contain local demo defaults. No production behavior is being changed.

Historical planning state: no baseline decision or commit had been recorded, and only
`.gitignore` was tracked; the existing solution and shared sources were untracked.

2026-11-08 implementation preflight: the backlog is now tracked in commit
8140af9, but the solution and required shared sources remain untracked. The
developer removed `service/demo` manually. Implementation is waiting for this
human gate; no baseline files were staged or committed by the agent. There are
344 candidate baseline files outside the 158 duplicate shared-source files under
`service/src/Common.*`. Review and commit the intended baseline, or explicitly
authorize the agent to perform that separate baseline work before resuming T002.
