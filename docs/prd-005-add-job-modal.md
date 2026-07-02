# PRD 005: Add Job Modal

## Problem Statement

The Job Tracking app has no way to create a new job from the UI. Jobs can only be added by seeding the database at startup or by hitting `POST /api/jobs` directly via a tool like curl or Postman. The shop needs to add jobs organically as new work comes in — the owner or user should be able to create a job while sitting in the app without leaving their browser.

The job list currently shows 4 seed records with no way to add more.

## Solution

Add a toolbar row at the top of the job list panel (left column) containing an "Add Job" button. Clicking it opens a modal dialog with a form to enter: customer (dropdown of existing customers), job name, lead date, start date, delivery date, and quote amount. On submit, the modal calls `POST /api/jobs`, refreshes the job list from the API, and selects the newly created job in the sidebar.

The customer management button is deferred — this PRD covers only adding jobs.

## User Stories

1. As a shop user, I want to see a toolbar above the job list with an "Add Job" button, so that I know where to go when a new job arrives.

2. As a shop user, I want clicking "Add Job" to open a modal dialog, so that I can enter job details without navigating away from the job list.

3. As a shop user, I want a dropdown of existing customers in the modal, so that I can assign the job to the right customer without leaving the form.

4. As a shop user, I want to enter a job name for the new job, so that I can identify it later in the list.

5. As a shop user, I want to optionally set a lead date, start date, and delivery date when creating the job, so that I can capture scheduling info upfront.

6. As a shop user, I want to optionally enter a quote amount, so that I can record the estimated value at creation time.

7. As a shop user, I want the "Create" button to be disabled until a customer and job name are provided, so that I can't submit an incomplete form.

8. As a shop user, I want the modal to close and the job list to refresh automatically after I create a job, so that I immediately see the new job in the sidebar.

9. As a shop user, I want the freshly created job to be selected and visible in the detail panel after creation, so that I can start working on it right away.

10. As a shop user, I want a "Cancel" button in the modal, so that I can dismiss it without making changes.

11. As a shop user, I want to be able to create a job when there are no customers yet (the dropdown will be empty, but the disabled Create button prevents submission), so that the UI doesn't crash on an empty state.

12. As a developer, I want the modal component to be a pure presentational component that fires callbacks rather than injecting services, so that it follows the existing architecture pattern established by `MilestoneChecklist`.

13. As a developer, I want the "Add Job" button to load the customer list when clicked, so that the customer dropdown always reflects the current database state.

## Implementation Decisions

### Toolbar Location

A toolbar row is added inside `JobList.razor` above the existing "Jobs" header. It contains a single button: "Add Job". The toolbar is a child of the job list component to keep the three-column layout untouched.

The `JobList` component gains an `OnAddJob` `EventCallback` parameter. The toolbar button fires this callback on click. The parent (`Home.razor`) handles it by showing the modal.

### Modal Component

A new `AddJobModal.razor` component in the `Components/` directory. It is a pure presentational component — no service injection. Parameters:

| Parameter | Type | Purpose |
|-----------|------|---------|
| `Customers` | `List<Customer>` | Populates the customer dropdown |
| `OnJobCreated` | `EventCallback<CreateJobRequest>` | Fired when the user clicks Create |
| `OnDismiss` | `EventCallback` | Fired when the user clicks Cancel or clicks outside the modal |

The modal renders a semi-transparent backdrop with a centered white card containing the form.

### CreateJobRequest DTO

A new client-side model in `JobTracking.App/Models/CreateJobRequest.cs`:

```
class CreateJobRequest
    int CustomerId
    string JobName
    DateTime? LeadDate
    DateTime? StartDate
    DateTime? DeliveryDate
    decimal? QuoteAmount
```

Maps to the API's existing `CreateJobDto` (same shape, same field names).

### Form Layout

A single-column form inside the modal:

1. **Customer** — `<select>` dropdown populated from `Customers` parameter. First option is disabled placeholder: "Select a customer…". Required.
2. **Job Name** — `<input type="text">`. Required.
3. **Lead Date** — `<input type="date">`. Optional.
4. **Start Date** — `<input type="date">`. Optional.
5. **Delivery Date** — `<input type="date">`. Optional.
6. **Quote Amount** — `<input type="number" step="0.01">`. Optional.

Two buttons at the bottom: "Cancel" (calls `OnDismiss`) and "Create" (disabled until CustomerId != 0 and JobName is non-empty).

### Extended IJobService Interface

```
interface IJobService
    Task<List<Job>> GetJobsAsync()
    Task<Job> ToggleMilestoneAsync(int jobId, int milestoneId, bool isComplete)
    Task<Job> CreateJobAsync(CreateJobRequest dto)          // new
    Task<List<Customer>> GetCustomersAsync()                 // new
```

`CreateJobAsync` calls `POST /api/jobs` and returns the created `Job`. `GetCustomersAsync` calls `GET /api/customers` and returns the full customer list.

### ApiJobService Changes

- `CreateJobAsync` — serializes `CreateJobRequest` to JSON, sends `POST /api/jobs`, deserializes response to `Job`.
- `GetCustomersAsync` — calls `GET /api/customers`, deserializes response to `List<Customer>`.

### Home.razor Orchestration

The page-level flow:

1. On `OnAddJob` → load customers from `JobService.GetCustomersAsync()`, show modal.
2. On `OnJobCreated` → call `JobService.CreateJobAsync(dto)`, close modal, reload job list from API, set the returned job as `_selectedJob`.
3. On `OnDismiss` → close modal, no side effects.

The modal's visibility is controlled by a `bool _showAddJobModal` field in `Home.razor`.

### API No Changes

The existing `POST /api/jobs` endpoint and `GET /api/customers` endpoint already accept the required data. No backend changes needed.

## Testing Decisions

- **What makes a good test**: Tests verify external behavior (what renders, what happens on click), not internal component state. A service test feeds a known HTTP response and asserts the returned model. A component test renders markup, verifies DOM output, and simulates user interaction. Tests do not depend on shared mutable state or test execution order.

- **ApiJobService test** (existing seam, `ApiJobServiceTest.cs`): Extend with a test for `CreateJobAsync`. Send a `CreateJobRequest` to the mock handler, assert the request is `POST /api/jobs` with the correct JSON body, and assert the returned `Job` matches the expected shape. Also extend with a test for `GetCustomersAsync`. Prior art: 2 tests in `ApiJobServiceTest.cs`.

- **JobList component test** (existing seam, `JobListTest.cs`): Extend with a test that the toolbar renders an "Add Job" button. Extend with a test that clicking the button fires the `OnAddJob` callback. Prior art: 5 tests in `JobListTest.cs`.

- **AddJobModal component test** (new seam): bUnit test file `AddJobModalTest.cs`. Tests: modal renders with correct title, customer dropdown is populated from parameter, dropdown has a placeholder option, Create button is disabled when CustomerId is 0 or JobName is empty, Create button is enabled when both are filled, clicking Create fires `OnJobCreated` with the correct `CreateJobRequest`, clicking Cancel fires `OnDismiss`. This is a new seam; it follows the same bUnit pattern established by `JobListTest` and `MilestoneChecklistTest`.

- **API integration tests** (existing seam, `JobsControllerTest.cs`): No changes needed. The existing `Create_job_returns_created_with_job_number_and_milestones` test already covers `POST /api/jobs`. Prior art: 12 tests.

## Out of Scope

- Customer management (add/edit/delete customer) — deferred to a future PRD; the customer dropdown only shows existing records
- Edit Job functionality (the toolbar mentions an edit button, but it is not implemented in this PRD)
- Job deletion or archiving from the UI
- Form validation beyond the required fields (date range validation, etc.)
- UI polish beyond functional CSS (animations, transitions, design system components)
- Keyboard navigation or accessibility beyond basic form semantics
- Mobile-responsive layout for the modal
- Any connection to the Design Component, Data Ingestion, or Reporting modules

## Further Notes

Published as `docs/prd-005-add-job-modal.md`.
