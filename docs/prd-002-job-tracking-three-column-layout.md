# PRD 002: Job Tracking — Three-Column Job List

## Problem Statement

The current prototype displays a single hardcoded job ("Smithers Residence") with its 12-milestone checklist. There is no way to see all jobs at a glance, switch between them, or get a quick status read on what's in the pipeline. The user (the shop's institutional knowledge) still has to keep the full mental model of every job's state, which is exactly the problem the Job Tracking module was created to solve.

Without a job list, the tool cannot replace Task3 scheduling or serve as a centralized view of shop progress.

## Solution

Replace the single-job page with a persistent three-column layout:

```
┌──────────┬───────────────────┬────────────────────┐
│          │                   │                    │
│ NavMenu  │   Job List        │   Job Detail       │
│  (left)  │   (middle)        │   (right)          │
│          │                   │                    │
│ • Jobs   │ ┌─────────────┐   │ Smithers Residence │
│          │ │ Smithers    │   │ ☐ Designed         │
│          │ │   Res.  ◉   │   │ ☐ Sent for appr.   │
│          │ │             │   │ ☐ Approved to build│
│          │ │ Johnson     │   │ ☐ Production start │
│          │ │   Kit.  ○   │   │ ☐ ...              │
│          │ │ Lakewood    │   │                    │
│          │ │   Ren.  ○   │   │                    │
│          │ └─────────────┘   │                    │
└──────────┴───────────────────┴────────────────────┘
```

- **Left column**: Navigation sidebar — unchanged from the current prototype. Only "Jobs" for now.
- **Middle column**: Scrollable list of all jobs. Each row shows the job name and a computed status badge.
- **Right column**: Job detail view with the milestone checklist. Shows a placeholder ("Select a job to view details") when nothing is selected.

The job data comes from an in-memory mock service (`IJobService` interface with a hardcoded implementation returning 3–5 jobs), keeping the prototype lightweight and swappable for a real backend later.

## User Stories

1. As a shop user, I want to see all jobs listed in a single column, so that I can see what's in the pipeline at a glance.
2. As a shop user, I want each job row to show a status badge, so that I can identify bottlenecks and stalled jobs without opening each one.
3. As a shop user, I want to click a job in the list to see its detail, so that I can view its milestone checklist and other information.
4. As a shop user, I want the currently selected job to be visually highlighted in the list, so that I always know which job I'm viewing.
5. As a shop user, I want the milestone checklist to remain fully functional when viewing a job in the right column, so that I can toggle completion status as before.
6. As a shop user, I want to see an empty state in the right column when no job is selected, so that the layout never looks broken or blank.
7. As a shop user, I want the three-column layout to be visible without page navigation, so that switching between jobs feels immediate.
8. As a developer, I want the job data to come from an injectable service interface, so that the mock can be replaced with a real backend later.
9. As a developer, I want the status badge to be computed from the existing milestone model, so that no separate status field needs to be maintained.

## Implementation Decisions

- **Three-column layout**: Always-visible split — left (sidebar navigation), middle (job list), right (job detail). No routing changes; single page manages state.
- **Data service**: `IJobService` interface with a `GetJobsAsync()` method returning `Task<List<Job>>`. `MockJobService` registers as a singleton providing 3–5 hardcoded jobs.
- **Job model extension**: Add an `Id` property (string/GUID) for stable selection. Add a computed `Status` property derived from milestone completion:

  | Last Completed Milestone | Status |
  |---|---|
  | None | New |
  | 1. Designed | In Design |
  | 2. Sent for approval | Awaiting Approval |
  | 3. Approved to build | Approved |
  | 4. Production started | In Production |
  | 5. Components machined and assembled | Machined |
  | 6. Components finished | Finished |
  | 7. Final assembly done | Final Assembly |
  | 8. Loaded | Loaded |
  | 9. Delivered | Delivered |
  | 10. Billed | Billing |
  | 11. Paid | Paid |
  | 12. Closed | Closed |

  Status is computed — no new database column, no persistence.

- **Job selection**: Page-level state variable (`Job? SelectedJob`) in the Home page. The middle column (`JobList`) communicates via `EventCallback<Job>`; the right column (`JobDetail`) receives a `Job?` parameter. No URL routing, no shared state service.
- **Empty state**: Right column shows a centered "Select a job to view details" message when `SelectedJob` is null. The right column is a new `JobDetail` component that owns this logic and delegates to `MilestoneChecklist` when a job is selected.
- **Component tree**:
  - `Home.razor` — orchestrates the three-column layout; owns job list state; injects `IJobService`
  - `JobList.razor` — new component: renders job rows, highlights selected, emits `EventCallback<Job>`
  - `JobDetail.razor` — new component: empty state or delegates to `MilestoneChecklist`
  - `MilestoneChecklist.razor` — unchanged; still takes a `Job` parameter
- **No job creation UI**: The mock service returns hardcoded jobs only. Creation comes with a real backend.
- **No CSS framework**: Continue with functional CSS in component-scoped stylesheets, matching PRD-001's approach. No Bootstrap, Tailwind, or other framework.
- **Existing ADRs respected**: Milestone checklist model (ADR 0001) is unchanged. The three-column layout is additive — it works with any number of milestones.

## Testing Decisions

- **What makes a good test**: Same philosophy as PRD-001 — test external behavior (what the user sees and interacts with), not internal state management or component implementation details.
- **Testing seams** (highest first):
  1. **Model-level** (new seam): Unit test `Job.Status` computation directly. Given a Job with milestones in various completion states, assert the correct status label. Fast, no rendering, covers the badge logic in isolation.
  2. **Component-level** (existing pattern): bUnit tests for `JobList` and `JobDetail`, following the `MilestoneChecklistTest.cs` pattern:
     - `JobList` renders all job names in the mock service; clicking a job row invokes the callback with the correct job
     - `JobDetail` shows the empty state when no job is provided; renders the checklist with job name when a job is provided
- **Prior art**: `MilestoneChecklistTest.cs` (7 tests, bUnit + xUnit, `BunitContext` base class) establishes the component-testing pattern.
- **Modules under test**: Job Tracking Blazor components only.

## Out of Scope

- Database or persistence of any kind
- Job creation or deletion UI
- Job editing (name, milestones, customer info)
- Role-based access / authentication
- Document upload or document buckets
- Change order sub-milestones (the existing change order model is unaffected)
- Sorting, filtering, or searching the job list
- Routing-based navigation (single-page state only)
- CSS framework or visual polish beyond layout readability
- Any connection to the Design Component or Data Ingestion modules

## Further Notes

This PRD was not published to GitHub Issues because `gh` CLI is not available in this environment. The PRD is filed in the repo as `docs/prd-002-job-tracking-three-column-layout.md` and should be reviewed before implementation begins.
