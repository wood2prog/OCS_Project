# PRD 001: Job Tracking — Minimal Slice

## Problem Statement

The shop has no centralized view of where a job is in its lifecycle. Progress is tracked on paper Clipboards, in the owner's head, and via ad-hoc conversations. The user (who currently carries the full mental model of every job's status) needs a lightweight digital tool to replace Task3 scheduling and provide a single place to see job progress at a glance. Without this, the shop cannot function without the user's constant presence.

The first step is validating that the milestone model is the right abstraction before investing in a full database-backed system.

## Solution

A single Blazor web page that displays one hardcoded job ("Smithers Residence") with its 12-milestone checklist. Each milestone is a clickable checkbox. Clicking toggles the checked state. The milestone labels are drawn from the default template defined in Session 5. No database, no authentication, no job list — just a tangible prototype the user can open in a browser and interact with.

If the user can look at this page and say "yes, this is how I want to track job progress," the milestone model is validated and we proceed to a full implementation. If they say "I need dates visible," or "the order is wrong," or "I need sub-items," we adjust before any database schema is locked in.

## User Stories

1. As a user, I want to see a single page with a job name displayed, so that I know which job's progress I'm looking at.
2. As a user, I want to see all 12 milestones listed in order, so that I can see the full job lifecycle at a glance.
3. As a user, I want to see which milestones are already completed, so that I know what's been done.
4. As a user, I want to click an unchecked milestone to mark it complete, so that I can record progress.
5. As a user, I want to click a checked milestone to uncheck it, so that I can correct mistakes.
6. As a user, I want the milestone labels to match the shop's actual process language ("Designed", "Sent for approval", etc.), so that the tool speaks the same language I do.
7. As a user, I want completed milestones to be visually distinct from incomplete ones, so that I can scan progress quickly.
8. As a developer, I want the milestone data to be a simple in-memory model, so that I can iterate on the UI without setting up a database.
9. As a developer, I want the component tested with bUnit, so that I can verify rendering and toggle behavior without a browser.

## Implementation Decisions

- **Framework**: ASP.NET Core Blazor (Server or WebAssembly — determines whether we need a host. For this slice, either works since there's no backend).
- **Data model**: An in-memory `Job` object with a `JobName` string and a `List<Milestone>` where each Milestone has a `Label` (string), `IsComplete` (bool), and `Order` (int). No database, no API calls.
- **Milestone labels**: Hardcoded to the 12-item default template from Session 5 — Designed, Sent for approval, Approved to build, Production started, Components machined and assembled, Components finished, Final assembly done, Loaded, Delivered, Billed, Paid, Closed.
- **Component structure**: A single Blazor component (`.razor`) that takes a `Job` parameter and renders a titled card with the milestone checklist.
- **Style**: Functional only — no design framework, no CSS framework. Native HTML checkboxes with enough custom CSS to make completed vs. incomplete visually distinct.
- **bUnit tests**: Component rendered in-memory; assert milestone labels appear in order; simulate click and assert `IsComplete` toggles.

## Testing Decisions

- **What makes a good test**: Tests verify external behavior (what the user sees and what happens when they click), not internal state management. A good test renders the component, checks DOM output, and simulates user interactions.
- **Test seam**: Blazor component (bUnit) — the highest practical seam for a component that has no backend dependencies. No integration or end-to-end tests needed at this stage.
- **Module under test**: The Job Tracking Blazor component only.
- **Prior art**: No prior tests exist in this repo (this is the first code to be written). The bUnit tests will serve as the pattern for all future Blazor component tests.

## Out of Scope

- Database or persistence of any kind
- Job list or job creation UI
- Document upload or document buckets
- Role-based access / authentication
- Change order sub-milestones
- Parallel phases
- QuickBooks CSV export
- Gmail integration
- Any connection to the Design Component or Data Ingestion modules
- CSS framework or visual polish beyond basic readability

## Further Notes

This PRD has not been published to GitHub Issues because `gh` CLI is not available in this environment. The PRD is filed in the repo as `docs/prd-001-job-tracking-minimal-slice.md` and should be reviewed before implementation begins.
