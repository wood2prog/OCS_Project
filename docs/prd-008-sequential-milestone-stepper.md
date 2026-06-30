# PRD 008: Sequential Milestone Stepper

## Problem Statement

The milestone checklist currently treats all 12 milestones as independent checkboxes with no ordering constraint. A worker can mark "Loaded" before "Designed" or "Final assembly done" before "Components machined and assembled." While this freedom was deliberately chosen for simplicity, it doesn't match how work actually flows through the shop. A job cannot be loaded before it is designed. Components cannot be assembled before they are machined. The UI should guide the worker through the correct sequence, surfacing the single next action while making it clear what has been done and what remains.

## Solution

Replace the flat checkbox list with a vertical stepper. Each milestone is a step node in a timeline. Only the current (next incomplete) milestone is clickable. Completing it stamps the server-owned `CompletedAt` timestamp and advances the active step to the next locked milestone, making it clickable. The sequence is enforced by the UI; the API remains permissive for backward compatibility.

## User Stories

1. As a worker in the shop, I want to see the full job lifecycle as a vertical timeline, so that I can quickly understand what has been done and what comes next.
2. As a worker in the shop, I want only the current step to be clickable, so that I cannot accidentally skip ahead to a future milestone.
3. As a worker in the shop, I want completed steps to have a distinct visual (checked/complete), so that I can see progress at a glance.
4. As a worker in the shop, I want the current step to be visually highlighted, so that I know what action is expected next.
5. As a worker in the shop, I want future steps to be visually dimmed and non-clickable, so that it's obvious they aren't available yet.
6. As a worker in the shop, I want to click the active step once to mark it complete, so that recording progress takes minimal effort.
7. As a job tracker user, I want the completion timestamp to be recorded server-side when the step is completed, so that the record is authoritative.
8. As a job tracker user, I want completed milestones to still show their order and label in the timeline, so that the full lifecycle remains visible after completion.
9. As a developer, I want the API to remain permissive (accept completion requests for any milestone in any order), so that the change is reversible and future admin undo is easy to add.
10. As a developer, I want the existing component callback signature kept unchanged, so that no orchestration code (Home.razor, JobDetail.razor, ApiJobService, JobsController) needs modification.

## Implementation Decisions

### Frontend-only enforcement
The API accepts PATCH requests for any milestone in any order, exactly as it does today. The sequential constraint lives entirely in the `MilestoneChecklist.razor` component. This keeps the change reversible and avoids touching any backend code, service layer, or tests outside the component.

### Vertical stepper replaces checkbox list
The current `<ul>` of checkboxes is replaced with a vertical step indicator — each milestone rendered as a node with a circle indicator, connector lines between steps, and three visual states:

- **Completed**: green circle with checkmark icon, faded label text, not clickable
- **Active**: blue circle with step number, bold label text, clickable — cursor is pointer
- **Locked**: gray circle with step number, dimmed label text, not clickable — cursor is default

CSS classes: `.step-completed`, `.step-active`, `.step-locked`.

### Active step determination
The active step is the milestone with the lowest `Order` where `CompletedAt == null`. If all milestones have a non-null `CompletedAt`, all steps show the completed state. This is computed as a property on the component — no model changes needed.

### Click behavior
Clicking the active step fires `OnMilestoneToggled.InvokeAsync((Job.Id, milestone.Id, true))`. Always passes `Complete = true`. Locked and completed steps ignore clicks. The component does not mutate `CompletedAt` directly; it fires the same callback the parent (`Home.razor`) already handles.

### Change order integration
Change orders are deferred from this PRD. The existing ADR 0001 model (change orders create sub-milestones that are excluded from main status computation) is preserved unchanged. When a change order exists on a job, the main timeline pauses at its current position — the stepper does not advance past the step where the change order was raised until the change order sub-milestones are resolved. The UI for the pause/resume interaction is out of scope here; this PRD covers only the main 12-milestone stepper.

### Status derivation unchanged
The `Job.Status` computed property continues to derive from the highest-`Order` milestone with a non-null `CompletedAt`. No changes to the status map, the 13 status values, or the status badge logic in `JobList.razor`.

### Callback signature kept
`OnMilestoneToggled` remains `EventCallback<(int JobId, int MilestoneId, bool Complete)>`. The component always passes `Complete = true`. The parent orchestration (`Home.razor`, `JobDetail.razor`) and the service layer (`ApiJobService`) are untouched.

## Testing Decisions

### What makes a good test
- Test external behavior: what the user sees (CSS classes, text content) and what happens when they interact (callbacks fired or not fired)
- Do not test internal state computation in isolation — the stepper states are derived from `CompletedAt` on the `Milestone` model, which is already tested by `JobStatusTest.cs`
- A single test covers one scenario (e.g. "step 1 active, rest locked") by constructing a `Job` with the appropriate `CompletedAt` values and asserting the rendered CSS classes

### Test modules and seams

| Test file | Seam | What it tests | Prior art |
|---|---|---|---|
| `MilestoneChecklistTest.cs` | bUnit component | Renders job name; renders 12 milestones in order; initial state has step 1 active, 2–12 locked; partially-complete state (first 3 done → step 4 active, 5–12 locked); all-complete state; clicking active step fires callback with `Complete = true`; clicking locked/completed step fires nothing | `MilestoneChecklistTest.cs:33` (existing bUnit pattern for rendering, DOM query, callback capture) |
| `JobStatusTest.cs` | Pure model unit | Unchanged — Status computation is the same logic | — |
| `ApiJobServiceTest.cs` | Service unit (MockHttpMessageHandler) | Unchanged — no service-level changes | — |
| `JobsControllerTest.cs` | API integration (CustomWebApplicationFactory) | Unchanged — no API changes | — |
| `JobDetailTest.cs` | bUnit component | Unchanged — pass-through wrapper; optionally add a test that the stepper receives the correct job | — |

### New seams
None. All existing seams are sufficient. The component test is the highest practical seam — it validates the full UI behavior without a browser or backend.

### Tests to adapt (MilestoneChecklistTest.cs)
All 7 existing tests are rewritten. The new test file covers:

| New test | What it asserts |
|---|---|
| `Renders_job_name` | Stepper shows the job name |
| `Renders_all_12_milestones_in_order` | 12 step nodes in correct order with correct labels |
| `All_steps_locked_when_none_complete` | Step 1 has class `step-active`; steps 2–12 have class `step-locked` |
| `Active_step_advances_as_milestones_complete` | First 3 done → step 4 active, 1–3 completed, 5–12 locked |
| `All_steps_completed_when_all_done` | All 12 have class `step-completed` |
| `Clicking_active_step_fires_callback_with_Complete_true` | Click on active step → callback invoked with `Complete = true` |
| `Clicking_locked_step_does_not_fire_callback` | Click on any locked step → callback not invoked |

## Out of Scope

- **Admin undo** — a mechanism for supervisors or admins to clear `CompletedAt` and move the stepper backward is deferred. The permissive API means this can be added later without a data migration.
- **Change order stepper UI** — the sub-milestone stepper for change order loops is deferred. Change orders continue to use their own flat checklist as before.
- **Parallel phases** — running multiple milestone tracks in parallel (e.g. kitchen + bathroom) is deferred.
- **Graphical milestone designer** — drag-and-drop template editing is deferred.
- **Skipped milestones** — all 12 milestones remain mandatory for every job.
- **Auth / identity** — the `CompletedBy` field is left unused, same as today.
- **Animation** — transition animations between stepper states are not included.
- **CSS framework** — no Bootstrap, Tailwind, or third-party design system. Custom CSS only.

## Further Notes

- ADR 0001's "milestones are checked off independently without enforcing order" clause is superseded by ADR 0003 (sequential milestone stepper). The remainder of ADR 0001 (checklist model over formal state machine, change order sub-milestones, parallel phase flexibility) remains valid.
