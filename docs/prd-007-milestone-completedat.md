# PRD 007: Milestone `CompletedAt` — One-way check-in with server-owned timestamps

## Problem Statement

The Job Tracking app models milestone completion as a `bool IsComplete` field that can be toggled freely in both directions. The API already captures a `CompletedDate`, but the frontend never surfaces it and the UI allows unchecked by simply clicking a checked box again. There is no authoritative record of *when* a milestone was completed — the timestamp is computed server-side on every toggle, meaning a milestone completed last week and then accidentally unchecked/rechecked today would show today's date. Later, the shop wants a Kanban view of jobs organised by when each milestone was hit, which requires trustworthy timestamps.

## Solution

Collapse the redundant `IsComplete` + `CompletedDate` pair into a single nullable `CompletedAt` field. The checkbox toggles remain free (check to stamp, uncheck to clear), but the timestamp is now the authoritative signal of completion and is owned by the server clock. The frontend sends a simple `{ complete: true }` / `{ complete: false }` flag, and the server maps that to set or clear `CompletedAt`. The method is renamed from `ToggleMilestoneAsync` to `UpdateMilestoneAsync` to reflect that it is no longer a toggle.

## User Stories

1. As a worker in the shop, I want to check a milestone when I finish a step, so that the system records when it happened.
2. As a worker in the shop, I want to uncheck a milestone if I marked it by mistake, so that the incomplete state is restored.
3. As a job tracker user, I want the timestamp to reflect the server's clock, so that all completion times are consistent and not subject to client clock drift.
4. As a job tracker user, I want to see completed milestones visually distinguished (strikethrough), so that I can quickly tell which steps are done.
5. As a job tracker user, I want to see milestones in order, so that I can follow the job's progress sequentially.
6. As a developer, I want to query `CompletedAt` as the sole completion indicator, so that there is no ambiguity between `IsComplete == true` and `CompletedDate == null`.
7. As a planner, I want the system to record a stable `CompletedAt` per milestone, so that future Kanban and timeline views have accurate data.

## Implementation Decisions

### Schema: `CompletedAt` replaces `IsComplete` + `CompletedDate`
The API `Milestone` model drops `IsComplete` (bool) and `CompletedDate` (DateTime?) and gains `CompletedAt` (DateTime?). A non-null value means the milestone is complete; null means incomplete. This makes the data model enforce one canonical truth.

### Frontend model mirrors the change
The frontend `Milestone` model drops `IsComplete` and adds `CompletedAt`. All references to `milestone.IsComplete` in razor components, computed properties, and tests are updated to `milestone.CompletedAt != null`.

### Server-owned timestamp
The PATCH endpoint receives `{ complete: bool }`. When `true`, the server sets `CompletedAt = DateTime.UtcNow`. When `false`, the server sets `CompletedAt = null`. The server is the single source of truth for wall-clock time.

### DTO renamed and cleaned up
The DTO changes from:
```
{ IsComplete: bool, CompletedBy: string? }
```
to:
```
{ complete: bool }
```
`CompletedBy` is deferred until auth is added. The DTO is renamed from `MilestoneToggleDto` to a more descriptive name (e.g., `MilestoneUpdateDto`).

### Interface renamed
`IToggleMilestoneAsync(jobId, milestoneId, isComplete)` → `IUpdateMilestoneAsync(jobId, milestoneId, complete)`. This is not a toggle; the caller explicitly states the desired completion state.

### Method mapping
The controller action maps `dto.Complete` to `milestone.CompletedAt`:

```
if dto.Complete → milestone.CompletedAt = DateTime.UtcNow
if not dto.Complete → milestone.CompletedAt = null
```

### No auth, no undo, no identity
The `CompletedBy` field is left unused in the schema (remains nullable, never populated). Auth, admin undo, and worker identity are deferred.

### No new component
The existing `MilestoneChecklist.razor` checkbox binding updates from `milestone.IsComplete` to `milestone.CompletedAt != null`. The UI remains a free toggle (check/uncheck). The button-tap redesign is deferred.

## Testing Decisions

### What makes a good test
- Test external behavior, not implementation details
- For components: verify rendering output and callback invocation with the new `complete` flag
- For services: verify HTTP method, URL, request body contains `{ complete }`, and response deserialization maps `CompletedAt`
- For model logic: verify `Job.Status` uses `CompletedAt != null`
- For API: verify the PATCH endpoint sets/clears `CompletedAt` and returns the updated job

### Test modules and seams
All four existing test files are adapted — no new test files or infrastructure:

| Test file | Seam | What it tests | Prior art |
|---|---|---|---|
| `MilestoneChecklistTest.cs` | bUnit component | Checkbox renders as unchecked when `CompletedAt` is null; checked when non-null; clicking sends `Complete: true`/`false`; completed class applied | `MilestoneChecklistTest.cs:62` (checkbox assertions), `MilestoneChecklistTest.cs:70` (callback capture) |
| `JobStatusTest.cs` | Pure model unit | `Job.Status` computed from `CompletedAt != null` | `JobStatusTest.cs:7` (identical structure, just field change) |
| `ApiJobServiceTest.cs` | Service unit (MockHttpMessageHandler) | `UpdateMilestoneAsync` sends `PATCH` with `{ complete }` body | `ApiJobServiceTest.cs:58` (PATCH milestone test) |
| `JobsControllerTest.cs` | API integration (CustomWebApplicationFactory) | PATCH milestone with `{ complete: true }` sets `CompletedAt`; `{ complete: false }` clears it | `JobsControllerTest.cs:80` (toggle milestone integration test) |

### New seams
None. All existing seams are sufficient.

## Out of Scope

- **Button-tap UX** — replacing the checkbox with a tap button is noted for future work.
- **Admin undo** — a mechanism for supervisors to clear `CompletedAt` will be added later.
- **Auth / `CompletedBy`** — user login and identity capture is deferred.
- **Kanban view** — the downstream consumer of the timestamps is out of scope.
- **Custom milestone types** — user-defined milestone types are future work.
- **Change order milestones** — no change to the change order milestone sub-group behavior.

## Further Notes

- ADR 0001 (milestone checklist over states) is still valid — milestones remain independently checkable without order enforcement.
- The `CompletedBy` column on the milestones table remains in the schema but is not populated by this PRD. It will be wired once auth exists.
- This PRD should be published as a GitHub issue and labelled `ready-for-agent` once the `gh` CLI is available.
