# PRD 003: Database Backend for Job Tracking

## Problem Statement

The Job Tracking module currently runs entirely on in-memory mock data (`MockJobService`). When the page refreshes, all milestone checkmarks, job names, and status badges are lost. There is no way to persist progress, record who completed a milestone or when, store customer information alongside a job, handle change orders, or upload documents.

Without a database, the tool cannot replace Task3 scheduling or serve as the shop's centralized job record — every browser refresh resets the shop's view of where each job stands. The mock served its purpose for validating the milestone model and three-column layout, but the next step requires real persistence.

## Solution

Add a new `JobTracking.Api` ASP.NET Core Web API project that acts as the backend between the existing Blazor WebAssembly frontend and a SQLite database managed by Entity Framework Core. The existing `IJobService` interface gains an HTTP client implementation that calls the new API, while the `MockJobService` is retired.

The API exposes RESTful endpoints for the full data model defined in Sessions 5 of the conversation record and refined in the database design session. The database schema covers Customers, Jobs, Milestones, Change Orders, and Documents, with all timestamps stored in UTC and displayed in the user's local timezone.

## User Stories

1. As a shop user, I want the job list to persist across browser sessions, so that I don't lose progress when I refresh the page.

2. As a shop user, I want to create a new job with a customer name, job name, and auto-generated job number, so that I can track new work from initial customer meeting onward.

3. As a shop user, I want each job to show a human-readable job number (starting at 1000, auto-incrementing) appended to the job name with a hyphen, so that jobs with the same name are uniquely identifiable.

4. As a shop user, I want to see a customer name associated with each job, so that I know who the job is for without opening the detail view.

5. As a shop user, I want each job to track lead date, start date, and delivery date, so that I can see scheduling information at a glance.

6. As a shop user, I want each job to show a quote amount and a running change order total, so that I can track financial scope without opening QuickBooks.

7. As a shop user, I want to mark milestones complete and see the completion timestamp and who completed it, so that there is an audit trail of when work happened.

8. As a shop user, I want the job's status badge to be computed automatically from the highest-completed milestone, so that I don't need to maintain a separate status field.

9. As a shop user, I want closed jobs to drop off the active job list 7 days after they close, so that the list stays focused on active work without manual cleanup.

10. As a shop user, I want a future archive page that lets me browse and search all jobs including closed ones, so that I can reference historical work.

11. As a shop user, I want to create a change order on a job, so that I can track unplanned revisions during production.

12. As a shop user, I want a change order to introduce its own sub-milestone group (redesign, requote, reapprove) that runs alongside the main milestone sequence, so that the main job progress bar isn't disrupted by rework.

13. As a shop user, I want to upload documents into three buckets (Customer, Design, Production), so that all job artifacts are stored in one place.

14. As a shop user, I want to view a list of documents for a given job and download them, so that I can find any artifact without hunting through email or the Job Folder.

15. As a developer, I want the API to expose RESTful endpoints for CRUD operations on all entities, so that future modules (Reporting, Data Ingestion) can integrate via HTTP.

16. As a developer, I want the API to use an injectable repository pattern, so that I can swap the data access layer or mock it in tests.

17. As a developer, I want all timestamps stored in UTC with conversion to local time in the UI, so that timezone handling is consistent and testable.

## Implementation Decisions

### Architecture

- **New project**: `JobTracking.Api` — ASP.NET Core Web API project targeting .NET 10, alongside the existing `JobTracking.App` (Blazor WASM) and `JobTracking.Tests`.
- **Communication**: The Blazor WASM frontend calls the API via HTTP. The existing `IJobService` interface gets a new `ApiJobService` implementation that uses `HttpClient`. `MockJobService` is removed.
- **Database**: SQLite file stored at a configurable path (e.g., `Data/ocs-jobtracking.db`). The `.db` file is gitignored; EF Core migrations are checked in.
- **ORM**: Entity Framework Core with `Microsoft.EntityFrameworkCore.Sqlite` provider, code-first migrations.
- **Repository pattern**: Each aggregate (Jobs, Customers) gets an injectable repository interface (`IJobRepository`, `ICustomerRepository`). Repositories encapsulate EF Core queries and are testable against a real SQLite database.

### Database Schema

**Customers**
| Column | Type | Notes |
|--------|------|-------|
| Id | int, PK, auto-increment | |
| Name | string, required | |
| Email | string? | |
| Phone | string? | |
| Notes | string? | |

**Jobs**
| Column | Type | Notes |
|--------|------|-------|
| Id | int, PK, auto-increment | |
| JobNumber | int, auto-increment, starts at 1000 | |
| CustomerId | int, FK → Customers, required | |
| JobName | string, required | |
| LeadDate | DateTime? | UTC |
| StartDate | DateTime? | UTC |
| DeliveryDate | DateTime? | UTC |
| QuoteAmount | decimal? | |
| ChangeOrderTotal | decimal? | Running total |
| CreatedAt | DateTime, required | UTC |
| Status | (not stored) | Computed from milestones |

**Milestones**
| Column | Type | Notes |
|--------|------|-------|
| Id | int, PK, auto-increment | |
| JobId | int, FK → Jobs, required | |
| ChangeOrderId | int?, FK → ChangeOrders | Null for main milestones |
| Order | int | Position in milestone list |
| Label | string, required | e.g., "Designed", "Approved to build" |
| IsComplete | bool | |
| CompletedAt | DateTime? | UTC |

**ChangeOrders**
| Column | Type | Notes |
|--------|------|-------|
| Id | int, PK, auto-increment | |
| JobId | int, FK → Jobs, required | |
| Description | string | |
| Amount | decimal? | |
| CreatedAt | DateTime, required | UTC |
| Status | string | "In Redesign", "Awaiting Requote", "Awaiting Reapproval", "Complete" |

**Documents**
| Column | Type | Notes |
|--------|------|-------|
| Id | int, PK, auto-increment | |
| JobId | int, FK → Jobs, required | |
| Bucket | string, required | "Customer", "Design", "Production" |
| FileName | string | Original filename |
| StoragePath | string | Relative path under uploads directory |
| UploadedAt | DateTime, required | UTC |
| Notes | string? | |

### Key Behaviors

- **Status computation**: Derived from the highest-completed main milestone (where `ChangeOrderId IS NULL`). Not stored in the database. Same logic as the existing `Job.Status` computed property.
- **JobNumber auto-increment**: EF Core `ValueGeneratedOnAdd()` with starting value 1000. Displayed as `"{JobNumber}-{JobName}"`.
- **Closed job visibility**: Application-layer filter — jobs where the highest-completed milestone is "Closed" (Order = 12) and `CompletedAt` is more than 7 days ago are excluded from the default list. No schema change. An archive endpoint returns all jobs including closed ones for the future archive page.
- **Change order sub-milestones**: A `ChangeOrderId` FK on `Milestones` groups sub-milestones under their parent change order. They are excluded from the main status computation. When rendered, they appear as a grouped sub-list within the job detail view.
- **Document storage**: Files are written to a configured upload directory on disk (e.g., `uploads/{jobId}/{bucket}/`). The `Documents` table stores only metadata and the relative path. Blobs are served via a static file or dedicated download endpoint.
- **UTC convention**: All `DateTime` columns store UTC. The API returns ISO 8601 dates with `Z` suffix. The Blazor frontend converts to local time using `TimeZoneInfo` or JavaScript interop.
- **No deletion** (superseded by ADR-0002): The original invariant stated that Customers and Jobs are never deleted. ADR-0002 (hard-delete) overturns this for jobs — hard DELETE is implemented for error correction (test entries, duplicates, mistaken creations). Customers remain undeleted; the `409 Conflict` guard in the DELETE endpoint prevents deletion of customers with active jobs.

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | /api/customers | List all customers |
| POST | /api/customers | Create customer |
| GET | /api/customers/{id} | Get customer by ID |
| GET | /api/jobs | List active jobs (excludes jobs closed >7 days) |
| GET | /api/jobs/archive | List all jobs including closed |
| GET | /api/jobs/{id} | Get job with milestones and change orders |
| POST | /api/jobs | Create job (sets up 12 default milestones) |
| PATCH | /api/jobs/{id} | Update job fields |
| PATCH | /api/jobs/{id}/milestones/{milestoneId} | Set milestone completion (sets CompletedAt) |
| POST | /api/jobs/{id}/changeorders | Create change order (creates sub-milestones) |
| GET | /api/jobs/{id}/changeorders | List change orders for job |
| GET | /api/jobs/{id}/documents | List documents for job |
| POST | /api/jobs/{id}/documents | Upload document (multipart form) |
| GET | /api/documents/{id}/download | Download document file |

### Existing Patterns Unchanged

- The three-column layout (`Home.razor`, `JobList`, `JobDetail`, `MilestoneChecklist`) keeps the same structure.
- The `IJobService` interface is retained but its implementation swaps from mock to HTTP-based.
- The milestone checklist model (ADR 0001) is unchanged — sub-milestones are rendered as grouped sub-lists within the checklist, not as a separate state machine.

## Testing Decisions

- **What makes a good test**: Tests verify external behavior, not implementation details. A good API test sends an HTTP request and asserts the response (status code, JSON shape, side effects). A good repository test calls a query method and asserts the returned entities are correct. Tests should not depend on the order of test execution or shared mutable state.

- **API integration tests** (`JobTracking.Api.Tests`): Use `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) with a temporary SQLite database (e.g., `:memory:` or a temp file). Each test class gets its own fresh database instance. Tests exercise the full HTTP → controller → service → EF Core → SQLite stack without mocking. No prior art — this is a new seam for the project.

- **Repository tests** (`JobTracking.Api.Tests` or a dedicated project): Test `IJobRepository` and `ICustomerRepository` methods directly against an in-memory SQLite database. Verify CRUD operations, query filtering (e.g., active jobs exclude closed >7 days), and edge cases (null fields, zero milestones). No prior art — new seam.

- **Model tests** (existing `JobTracking.Tests`): Continue testing `Job.Status` computation, milestone ordering, and change order grouping as unit tests alongside the existing `JobStatusTest.cs`. Prior art: `JobStatusTest.cs` (5 tests, xUnit).

- **Component tests** (existing `JobTracking.Tests`): bUnit tests for `JobList`, `JobDetail`, and `MilestoneChecklist` remain unchanged. The test's injected `IJobService` is replaced with a mock or test double that returns known data without HTTP calls. Prior art: `MilestoneChecklistTest.cs` (7 tests), `JobListTest.cs` (4 tests), `JobDetailTest.cs` (2 tests).

## Out of Scope

- Role-based access or authentication (user identity is tracked as a nullable `CompletedBy` string; auth integration deferred)
- Gmail API integration or automatic email-to-document import
- QuickBooks API integration (CSV export path unchanged — not affected by this PRD)
- Job creation from Data Ingestion module (this API supports it, but the Data Ingestion → API bridge is a separate effort)
- Archive/search page UI (the schema supports it, but the UI page is deferred)
- Job scheduling beyond the three date fields (stage-level estimates deferred from Session 5)
- Any connection to the Design Component or Reporting modules
- File storage beyond local disk (S3, Azure Blob, etc.)
- Graphical workflow or milestone template designer

## Further Notes

Published as `docs/prd-003-database-backend-for-job-tracking.md`.
