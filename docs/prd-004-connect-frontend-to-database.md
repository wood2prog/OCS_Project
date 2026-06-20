# PRD 004: Connect Frontend to Database

## Problem Statement

PRD 003 built the API and database backend (`JobTracking.Api` with SQLite), but the Blazor WASM frontend (`JobTracking.App`) still runs entirely on in-memory mock data from `MockJobService`. The 5 hardcoded jobs — Smithers Residence, Johnson Kitchen, Lakewood Renovation, Oakwood Office, Elm Street Bath — are the only data the UI can display. There are zero customers in the database, no seed data to work with during development, and the frontend has no way to call the API it was designed to serve.

Each browser refresh loses all state, and the API sits unused on port 5271 while the app talks to itself.

## Solution

Wire the Blazor WASM frontend to the `JobTracking.Api` backend via HTTP. Create an `ApiJobService` that implements the existing `IJobService` interface by calling the RESTful endpoints. Seed the database with 4 realistic customers and jobs so the UI has data to display immediately on first load. Remove `MockJobService` and all hardcoded sample data.

The three-column layout, milestone checklist, and status badges remain unchanged — only the data source swaps from memory to the database.

## User Stories

1. As a shop user, I want the job list to load from the database when I open the page, so that I see real persisted data instead of hardcoded samples.

2. As a shop user, I want each job in the list to show its job number and customer name alongside the job name, so that I can identify jobs at a glance.

3. As a shop user, I want checking a milestone checkbox to persist immediately to the database, so that my progress survives a page refresh.

4. As a shop user, I want the job's status badge to update automatically when a milestone is toggled, so that the status always reflects the latest milestone state from the database.

5. As a developer, I want a seed script that populates the database with realistic records on first run, so that I can start the app and see data immediately without manual setup.

6. As a developer, I want the `IJobService` interface to support milestone toggling as a first-class operation, so that components don't mutate state directly.

7. As a developer, I want the API response to be the single source of truth after every toggle operation, so that the UI state is always consistent with the database.

8. As a developer, I want CORS configured on the API to allow requests from the Blazor WASM dev server origin, so that frontend-backend communication works out of the box.

## Implementation Decisions

### Frontend Model Alignment

The App's `Job` and `Milestone` models are expanded to match the API response shape. Changes from the current models:

- `Job.Id`: `string` → `int` (matches API primary key)
- `Job.JobNumber`: new `int` field (auto-incremented by API, starts at 1000)
- `Job.CustomerName`: new `string` field (denormalized from the API's `Customer.Name` via JSON serializer)
- `Milestone.Id`: new `int` field (required for `PATCH /api/jobs/{id}/milestones/{milestoneId}`)
- `Milestone.JobId`: new `int` field (not displayed, used for API routing)
- `Job.Milestones`: keeps existing type, but each `Milestone` now carries `Id` and `JobId`

The `Job.Status` computed property is unchanged — same logic, same switch expression.

The `CustomerName` is populated by the API's `Job.Customer.Name` navigation property via JSON serialization (`ReferenceHandler.IgnoreCycles` is already configured on the API).

### Expanded IJobService Interface

```
interface IJobService
    Task<List<Job>> GetJobsAsync()
    Task<Job> ToggleMilestoneAsync(int jobId, int milestoneId, bool isComplete)
```

`GetJobsAsync()` calls `GET /api/jobs` (active jobs list). `ToggleMilestoneAsync` calls `PATCH /api/jobs/{jobId}/milestones/{milestoneId}` and returns the full updated `Job` from the response body, which replaces the local version.

### Milestone Toggle Flow

The `MilestoneChecklist` component stops mutating milestone state directly. Instead:

1. Checkbox `@onchange` fires → component invokes a new `OnMilestoneToggled` `EventCallback<(int JobId, int MilestoneId, bool IsComplete)>`
2. `Home.razor` handles the callback, calls `JobService.ToggleMilestoneAsync()`
3. On success, `Home.razor` replaces the updated `Job` in both `_jobs` list and `_selectedJob`
4. Blazor's re-rendering propagates the new state down to all child components

This keeps `MilestoneChecklist` as a pure presentational component. No service injection in the component layer.

### CORS

The API's `Program.cs` is configured with a CORS policy allowing `http://localhost:5089` (the Blazor WASM dev server). This is scoped to development only.

### Seed Data

A `DataSeeder` class runs during API startup. If the `Customers` table is empty, it inserts 4 customers each with one job:

| Customer | Job | Milestones complete | Status |
|----------|-----|-------------------|--------|
| Thompson | Thompson Kitchen Remodel | Milestones 1-3 | Approved |
| Riverbend Construction | Riverbend Office Buildout | Milestones 1-6 | Finished |
| Garcia | Garcia Bathroom Vanity | Milestones 1-7 | Final Assembly |
| Maplewood Homes | Maplewood Spec House | None | New |

### ApiJobService (New Implementation)

- Lives in the `JobTracking.App` project under `Services/`
- Injected with `HttpClient` (same `IHttpClientFactory` pattern or direct `HttpClient` as already registered in `Program.cs`)
- Deserializes API JSON directly into the expanded App models using `System.Text.Json`
- Base URL configured to `http://localhost:5271` via typed client or `appsettings`

### DI Registration Change

`Program.cs` (App):
- `builder.Services.AddSingleton<IJobService, MockJobService>()` → `builder.Services.AddScoped<IJobService, ApiJobService>()`
- `HttpClient` base address updated to API origin (from `builder.HostEnvironment.BaseAddress` to `http://localhost:5271`)

### Removal of Hardcoded Samples

- `MockJobService.cs` — deleted entirely (no more mock data path)
- `Job.SampleJob()` static method — deleted (only used by tests, tests are updated to construct their own test data)
- The 5 hardcoded job strings (Smithers Residence, etc.) are erased from all source files

### API: EnsureCreatedAsync → EnsureCreated

The API `Program.cs` already calls `db.Database.EnsureCreatedAsync()` somewhere? Actually it doesn't — it relies on migrations. For development, the seed path will call `EnsureCreated()` before seeding. In production, EF migrations would be used. For this phase, `EnsureCreated` is sufficient.

## Testing Decisions

- **What makes a good test**: Tests verify external behavior, not internal wiring. An API test sends HTTP requests and asserts response shape. A component test renders markup and verifies DOM output. A service test feeds known HTTP responses and verifies the returned model is correct. Tests do not depend on order or shared mutable state.

- **API integration tests** (`JobTracking.Api.Tests`, existing): No changes needed. The existing `JobsControllerTest` and `CustomersControllerTest` already exercise the full stack. The seed data path (`EnsureCreated` + initial inserts) is tested by creating records through the controllers. Prior art: 14 tests in 2 fixture classes.

- **Component tests** (`JobTracking.Tests`, existing bUnit): Updated for model changes. `JobListTest` — `Job.Id` is now `int`, fixture data updated. `JobDetailTest` — `Job.SampleJob()` factory is gone; tests construct their own test `Job` inline. `MilestoneChecklistTest` — adds tests verifying that clicking a checkbox invokes `OnMilestoneToggled` EventCallback with the correct parameters. Prior art: 14 tests in 3 fixture classes.

- **Model tests** (`JobTracking.Tests`, existing `JobStatusTest.cs`): No changes needed. The `Status` computed property logic is identical. Add a test verifying that a job with `CustomerName` and `JobNumber` set still computes status correctly. Prior art: 5 tests.

- **ApiJobService tests** (new — xUnit in `JobTracking.Api.Tests` or a new project): Test the HTTP client wrapper with a mock `HttpMessageHandler`. Verify that `GetJobsAsync()` calls `GET /api/jobs` and deserializes the response correctly. Verify that `ToggleMilestoneAsync()` calls `PATCH /api/jobs/{id}/milestones/{id}` with the correct JSON body. This is a new seam; the component tests at higher seams already cover the integration path, so these stay focused on the raw serialization contract.

## Out of Scope

- Change order display or creation in the UI (API supports it, but no UI components for it yet — deferred from PRD 003)
- Document upload/download in the UI (API supports it, no UI yet)
- Job creation form (currently jobs must be created via API or seed data)
- Archive/search page for closed jobs
- Authentication or user identity beyond `CompletedBy` string
- Any connection to the Design Component or Reporting modules
- Migrations strategy — `EnsureCreated()` is sufficient for development; production migration strategy is deferred

## Further Notes

This PRD was not published to GitHub Issues because `gh` CLI is not available in this environment. The PRD is filed in the repo as `docs/prd-004-connect-frontend-to-database.md`.
