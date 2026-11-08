# Backend migration backlog

This directory is the authoritative backlog for the DeliveryNoteService backend
redesign. Stories and their progress are maintained in repository Markdown files.

## Project scope

- Use `CustomDeliveryNote/Backend` as the reference for existing functionality and
  business rules.
- Implement the redesigned backend in `DeliveryNoteService/service`.
- Use Minimal APIs following `service/src/WebApp/RestApi/DemoEndpoints.cs` and the
  target solution's application and persistence patterns.
- Existing client compatibility is not required. Routes, DTOs, responses, and
  internal structure can be redesigned.
- Use TDD for behavior changes and bug fixes.

## Backlog index

| Epic | Story | Status | Next step |
| --- | --- | --- | --- |
| [Recipient management](epics/recipient-management.md) | [US-001: Create and view recipients](US-001-create-and-view-recipients.md) | In progress | Implement T002 in the [plan](US-001-create-and-view-recipients/README.md). |

Recipient management is the proposed first migration area. The remaining backend
capabilities still need an inventory; this index is not a complete migration plan.
IDs identify stories and do not establish delivery priority.

## Story workflow

1. Draft the user outcome, acceptance criteria, scope, dependencies, and open
   questions. Keep unresolved stories in Draft.
2. Refine the story with the developer and agree on its public test boundaries.
3. Explicitly request task breakdown after the story is agreed. Keep its plan and
   task records in a directory with the same stem as the story file, following the
   workspace harness workflow.
4. Implement one observable behavior at a time using TDD: write a failing test,
   observe the expected failure, and implement enough behavior to pass.
5. Record task verification and agent review, followed by explicit human
   acceptance. A passing agent review alone does not mark a story Done.

Story files hold authoritative requirements and story status. When task plans
exist, task files hold authoritative task progress. Keep this index synchronized
with the story files. Follow the workspace harness reservation rules before task
breakdown or implementation; drafting a story does not start either activity.

## Testing agreement

Tests should verify observable behavior through agreed public interfaces. Before
writing tests for each story, record the proposed boundaries in that story and
confirm them with the developer. HTTP endpoints are the agreed boundary for the
initial Recipient story, following the demo's API testing approach. Additional
boundaries require an explicit agreement. Test names and expected results should
express the acceptance criteria rather than internal implementation choices.
