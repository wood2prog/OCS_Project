# PRD-010: Repository Refactoring and Cleanup

## Problem Statement

The OCS repository has grown organically from a documentation-only store into a hybrid docs + code repository. Several key documents have not kept pace with architectural decisions, creating misleading onboarding (AGENTS.md claims "no code") and contradictory positions (PRD-003 asserts jobs are never deleted, but ADR-0002 implemented hard deletion). The codebase has minor functional inconsistencies (Status computation differs between frontend and backend Job models), structural duplication (two identical milestone arrays), and orphaned schema fields (CompletedBy). Build artifacts (SQLite DB, vendored Bootstrap, .vs/ config) are checked into git.

## Solution

Perform a targeted cleanup across three layers:

1. **Documentation** — Update AGENTS.md, PRD-002, PRD-003, and ARCHITECTURE.md to reflect current reality. Rename ADR-0002 to follow the established prefix convention.
2. **Code** — Fix the Status computation bug in the App model, consolidate duplicated DefaultMilestones into a single source, rename CompletedDate → CompletedAt, remove orphaned CompletedBy field.
3. **Repository hygiene** — Add SQLite DB and .vs/ to .gitignore, remove vendored Bootstrap from git tracking, strip redundant PRD boilerplate.

## User Stories

1. As a new agent or developer onboarding to this repo, I want AGENTS.md to accurately describe the repo contents, so that I don't waste time looking for code the docs say doesn't exist.
2. As a developer reviewing PRD-003, I want the "no deletion" statement annotated as superseded, so that I don't rely on an invariant that was deliberately overturned.
3. As a developer reviewing PRD-002, I want the "no CSS framework" statement removed, so that I don't get confused when I see Bootstrap classes throughout the app.
4. As a developer working on the Design Component, I want ARCHITECTURE.md to reflect the Python script paradigm shift from Session 4, so that I don't base work on a superseded JSON-only data flow.
5. As a user of the Job Tracking app, I want the Job Status to correctly exclude change-order milestones, so that a job isn't shown as "In Design" when only a change-order milestone was completed.
6. As a developer working on milestones, I want DefaultMilestones defined in a single location, so that I don't have to keep two arrays in sync.
7. As a developer reviewing milestone schemas, I want CompletedDate consistently renamed to CompletedAt, so that earlier docs match the implementation.
8. As a developer maintaining the API schema, I want the orphaned CompletedBy field removed from the Milestone entity, so that I don't maintain a dead column.
9. As a developer cloning the repo, I want the SQLite database file and .vs/ config excluded from git, so that I don't deal with machine-specific files in source control.
10. As a developer reading PRD files, I want the redundant "gh CLI not available" closing line removed from all 9 PRDs, so that I don't read the same boilerplate repeatedly.
11. As a developer reading ADRs, I want ADR-0002 to follow the same prefix convention as ADR-0003, so that filename conventions are consistent.
12. As a developer working on the job-tracking module, I want a top-level README for the module, so that I can understand the module structure without exploring subdirectories.

## Implementation Decisions

### Documentation updates

- **AGENTS.md**: Replace "No code, no build system, no test framework, no package manifests" with an accurate description: "Code now exists under `job-tracking/` — a .NET 10 Blazor/Web API solution with bUnit/xUnit tests."
- **PRD-002 lines 81-82**: Remove "No CSS framework" sentence. Add note that Bootstrap is used.
- **PRD-003 line 128**: Replace "No deletion: Customers and Jobs are never deleted from the database." with a deprecation notice referencing ADR-0002.
- **ARCHITECTURE.md**: Add a note at the top indicating the JSON-only data flow was superseded in Session 4. Reference the Python-based Construction Script model. Do not rewrite the full document — just add a versioning / status note.
- **ADR-0002**: Rename file to `0002-hard-delete-jobs.md` → `0002-hard-delete-jobs.md` is already the filename. The issue is the title heading: change `# Hard delete jobs` to `# ADR 0002: Hard delete jobs` and consider adding a status line (Accepted).

### Code changes

- **Status computation fix** (`JobTracking.App.Models.Job`): Add `m.ChangeOrderId == null` filter to the Status computed property, matching the API model's logic. This is the only semantic bug in the cleanup set.
- **DefaultMilestones consolidation**: Extract the array to a new static class `DefaultMilestones` in a shared location (e.g., `JobTracking.Api/Data/DefaultMilestones.cs`). Reference it from both `JobRepository` and `DataSeeder`.
- **CompletedAt rename**: No code change needed — PRD-007 already implemented this on the API model. The remaining work is updating the stale references in `docs/conversation-record.md` (Session 2 milestone table) and PRD-003.
- **CompletedBy removal**: Remove the `CompletedBy` property from `JobTracking.Api.Models.Milestone`. It was removed from the DTO in PRD-007 but the entity field was left behind.

### Repository hygiene

- **.gitignore**: Add `**/Data/*.db` (or `*.db`) and `/.vs/` entries.
- **Vendored Bootstrap**: Remove from git tracking with `git rm -r --cached` and add to .gitignore. The NuGet/package manager should restore it.
- **PRD boilerplate**: Remove the "This PRD was not published to GitHub Issues..." closing sentence from all 9 PRD files. Replace with nothing, or a one-line note: "Published as docs/prd-NNN-*.md".

## Testing Decisions

- **What makes a good test**: Test external behavior, not implementation details. For the Status bug, test that a job with only change-order milestones completed shows "New" (not "In Design"). For schema changes (CompletedBy removal), verify the API response no longer includes the field. For consolidation (DefaultMilestones), existing integration tests exercise the creation path — no new tests needed if behavior is preserved.
- **Modules tested**:
  - **JobTracking.Tests** (bUnit + xUnit) — Status computation unit tests
  - **JobTracking.Api.Tests** (integration) — API contract tests verify Milestone schema shape
- **Prior art**:
  - `JobStatusTest.cs` — 6 existing tests for Status computation. Add 2-3 cases for change-order milestone filtering.
  - `JobsControllerTest.cs` — Integration tests that exercise the full Milestone lifecycle. Update assertions if schema changes affect them.
- **No testing for**: Documentation changes, .gitignore changes, PRD boilerplate removal.

## Out of Scope

- Architectural changes to the Design Component or Data Ingestion modules (the ARCHITECTURE.md update is scoped to a deprecation notice, not a full rewrite).
- Adding Room, Run, Cabinet, or Part entities to the codebase (they remain unimplemented entities from the documented hierarchy).
- Authentication / authorization (CompletedBy is deferred to a future PRD, not part of this cleanup).
- CSS framework migration (Bootstrap stays; this PRD only fixes the incorrect documentation claim).
- Full test suite overhaul or CI pipeline setup.
- Re-evaluation of the cascade levels inconsistency (global→room→cabinet vs. Job→Cabinet→Part) — that requires a design session.

## Further Notes

- This PRD was produced from a refactoring audit and the user did not write it. It synthesises findings from the exploration agent's analysis of the repository.
- The `gh` CLI is not available in this environment. This PRD is filed as `docs/prd-010-refactoring-cleanup.md` and should be published to the project issue tracker with the `ready-for-agent` triage label once `gh` is configured.
- All changes in this PRD are safe to apply in a single pass. The Status computation bug is the only risk — it changes observable behavior, but the new behavior (excluding change-order milestones) aligns with the API side and the original design intent from PRD-003.
