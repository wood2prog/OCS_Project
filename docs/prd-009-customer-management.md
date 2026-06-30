# PRD 009: Customer Management

## Problem Statement

The Job Tracking application stores customers as a data dependency of jobs — customers can be created when a job is created, but there is no way to view, edit, or delete them independently. If a customer's email or phone number changes, or if a test customer was entered, there is no UI to correct it. As the shop grows and re-engages past customers, the absence of standalone customer management makes it hard to keep contact data accurate or find customer history at a glance.

## Solution

Add a Customers section as a new page at `/customers`, accessible from a "Customers" link in the left navigation. The page follows the same two-column layout pattern as the Jobs page: a scrollable customer list on the left and a detail panel on the right. Add and Edit use modals (same pattern as `AddJobModal`/`EditJobModal`). Delete uses a confirmation dialog and is blocked if the customer has active jobs.

## User Stories

1. As a job tracker user, I want to see a "Customers" link in the left navigation, so that I can navigate to the customer management section.

2. As a job tracker user, I want to see a scrollable list of all customers ordered alphabetically in the left panel, so that I can quickly find any customer.

3. As a job tracker user, I want each customer list item to show the customer name and primary contact method (email or phone), so that I can identify them at a glance.

4. As a job tracker user, I want to click a customer in the list to select them, so that their full details appear in the right detail panel.

5. As a job tracker user, I want the right detail panel to display the customer's Name, Email, Phone, and Notes, so that I can see all saved information.

6. As a job tracker user, I want the right detail panel to show the number of jobs associated with this customer, so that I can gauge their business volume at a glance.

7. As a job tracker user, I want an "Add Customer" button above the customer list, so that I can create a new customer record.

8. As a job tracker user, I want clicking "Add Customer" to open a modal form with fields for Name, Email, Phone, and Notes, so that I can enter the customer's details.

9. As a job tracker user, I want the Add Customer modal's Save button to be disabled until at least the Name field is filled, so that I cannot save an incomplete record.

10. As a job tracker user, I want clicking Save in the Add Customer modal to create the customer and add them to the list, with the new customer selected in the detail panel.

11. As a job tracker user, I want clicking Cancel in the Add Customer modal to dismiss it without saving, so that I can abandon the operation.

12. As a job tracker user, I want an Edit button in the detail panel header, so that I can modify an existing customer's details.

13. As a job tracker user, I want clicking Edit to open a modal pre-populated with the customer's current Name, Email, Phone, and Notes, so that I can make corrections.

14. As a job tracker user, I want clicking Save Changes in the Edit Customer modal to update the customer, so that the changes are persisted and reflected immediately in the list and detail panel.

15. As a job tracker user, I want a Delete button in the detail panel header, so that I can remove a customer record.

16. As a job tracker user, I want clicking Delete to show a confirmation dialog with the customer's name, so that I am sure which customer will be deleted.

17. As a job tracker user, I want confirming the deletion to remove the customer permanently and clear the detail panel, so that the UI reflects the deletion immediately.

18. As a job tracker user, I want deletion to be **blocked** if the customer has any associated jobs, with an error message explaining how many jobs reference them, so that I don't accidentally orphan job records.

19. As a job tracker user, I want clicking Cancel in the delete confirmation dialog to dismiss it without deleting, so that I can back out.

20. As a job tracker user, I want the detail panel to show an empty state when no customer is selected, so that the layout is clean on initial load.

21. As a job tracker user, I want the customer list to be searchable/filterable by name, so that I can find customers quickly as the list grows.

22. As an API consumer, I want to send a PATCH request to update specific customer fields, so that I can integrate with other tools.

23. As an API consumer, I want to send a DELETE request to remove a customer, so that I can clean up data programmatically.

24. As an API consumer, I want a 404 response when trying to update or delete a non-existent customer, so that I can handle errors appropriately.

25. As an API consumer, I want a 409 Conflict response when trying to delete a customer that has associated jobs, including a count of how many jobs reference them, so that I understand why the deletion failed.

## Implementation Decisions

### Navigation
A new "Customers" nav link is added to `NavMenu.razor` below the existing "Jobs" link, pointing to `/customers`.

### Page structure
A `Customers.razor` page at route `/customers` uses a two-column layout matching the job tracking page:
- **Left panel**: Scrollable customer list with search input and "Add Customer" button
- **Right panel**: Customer detail or empty state

The layout uses the same flex-based CSS pattern as `Home.razor` for consistency.

### Customer model (frontend)
The frontend `Customer` model is extended from `{ Id, Name }` to `{ Id, Name, Email?, Phone?, Notes? }` to match the backend entity. A `JobCount` property (`int`) is added for the detail panel display.

### API: New endpoints
Two new endpoints are added to `CustomersController`:
- `PATCH /api/customers/{id}` — partial update using `UpdateCustomerDto`
- `DELETE /api/customers/{id}` — hard delete, blocked by FK constraint check

### API: DTOs
Two new DTOs following the job pattern:

- `CreateCustomerDto` — `{ Name (required), Email?, Phone?, Notes? }`
- `UpdateCustomerDto` — `{ Name?, Email?, Phone?, Notes? }` (all nullable for PATCH)

The existing `POST /api/customers` is updated to accept `CreateCustomerDto` instead of the raw `Customer` entity.

### API: Job count
The `GET /api/customers` response returns a computed `jobCount` per customer. A new method `GetCustomerJobCountsAsync()` on `ICustomerRepository` returns a `Dictionary<int, int>` mapping customer ID to job count, attached to the response via a DTO or anonymous object.

### Delete guard
The DELETE endpoint checks whether any jobs reference the customer. If so, it returns `409 Conflict` with a message body like `{ "message": "Cannot delete: 3 jobs reference this customer." }`. The check is done via `ICustomerRepository.HasJobsAsync(int customerId)`.

### Frontend: Customer service
A new `ICustomerService` interface (and `ApiCustomerService` implementation) is added, separate from `IJobService`:
- `Task<List<Customer>> GetCustomersAsync()`
- `Task<Customer> CreateCustomerAsync(CreateCustomerRequest dto)`
- `Task<Customer> UpdateCustomerAsync(int id, UpdateCustomerRequest dto)`
- `Task DeleteCustomerAsync(int id)`

The existing `GetCustomersAsync()` on `IJobService` is kept for backward compatibility with the job-creation dropdown, but its implementation delegates to the new service.

### Frontend: Data models
- `CreateCustomerRequest` — `{ Name (required), Email?, Phone?, Notes? }`
- `UpdateCustomerRequest` — `{ Name?, Email?, Phone?, Notes? }`

### UI components
The `Customers` page renders inline (no separate component files for list/detail/modals — kept in-page for simplicity, matching the Home.razor pattern):
- **Customer list panel**: `.customer-list` with search filter, click-to-select, active highlight
- **Detail panel**: `.customer-detail` with field labels and values, job count badge
- **Add modal**: Matches `AddJobModal.razor` pattern — form fields, disabled Save until Name filled, Cancel dismisses
- **Edit modal**: Pre-populated from selected customer, same form layout as Add
- **Delete confirmation**: Inline confirmation dialog matching the Home.razor pattern

### PATCH semantics
Customer PATCH follows the same pattern as jobs: all DTO fields are nullable, and only non-null values update the entity. This allows partial updates without sending all fields.

## Testing Decisions

### What makes a good test
- Test external behavior, not implementation details
- For components: verify rendering output and callback invocation, not internal state
- For services: verify HTTP method, URL, request body, and response deserialization
- For API: verify HTTP status codes and response bodies via full round-trip

### Test modules and seams

| Test file | Seam | What it tests | Prior art |
|---|---|---|---|
| `CustomersControllerTest.cs` (extend) | API integration with `CustomWebApplicationFactory` — full round-trip against in-memory SQLite | PATCH update returns updated customer; PATCH nonexistent returns 404; DELETE removes customer with no jobs; DELETE returns 409 when customer has jobs; GET returns job counts; POST with DTO creates customer; POST missing name returns 400 | `CustomersControllerTest.cs` — 4 existing tests (create, list, get by id, 404) |
| `CustomerPageTest.cs` (new) | bUnit component test — render full `/customers` page with mocked `ICustomerService` | Renders list panel; clicking customer shows detail with Name/Email/Phone/Notes/JobCount; Add modal opens/callbacks; Edit modal pre-populates; Delete confirmation; Delete blocked shows message; empty state; search filters list | `JobListTest.cs:26` (callback invocation), `AddJobModalTest.cs:91` (form interaction + callback capture) |
| `ApiCustomerServiceTest.cs` (new) | Unit test with `MockHttpMessageHandler` | GetCustomersAsync returns list; CreateCustomerAsync sends POST with correct body; UpdateCustomerAsync sends PATCH with correct URL/body; DeleteCustomerAsync sends DELETE; DeleteCustomerAsync with 409 error propagates | `ApiJobServiceTest.cs:58` (PATCH test), `:96` (POST test with body capture) |

### New seams
No new seam types are needed. All new tests use the existing patterns (bUnit for components, `MockHttpMessageHandler` for service layer, `CustomWebApplicationFactory` for integration).

## Out of Scope

- **Bulk operations** — No multi-select, batch import, or batch delete of customers.
- **Customer merge** — No ability to merge two customer records into one.
- **Job history on the customer page** — The job count is shown, but the customer page does not list or link to individual jobs. That belongs in a future cross-reference feature.
- **Audit trail** — No logging of who created, edited, or deleted a customer.
- **Customer search on the jobs page** — No customer search/filter on the job list page (only available on the customer page).
- **Undo after delete** — Delete is immediate and irreversible. No soft-delete or recycle bin.
- **Customer field customization** — No configurable fields or custom field types.
- **Phone/email format validation** — No format validation beyond basic required-field checks.

## Further Notes

- The term "Customer" was added to `CONTEXT.md` to clarify shop-specific usage: "A person or organization that orders cabinets. Each job is associated with exactly one customer."
- No new ADR is needed — the decisions here are straightforward extensions of existing patterns (PATCH semantics, DTO pattern, two-column layout, modal patterns) that are already documented in prior PRDs.
- The `Customers` page at `/customers` follows the same structural pattern as `Home.razor` (inline modals, inline confirmation, style block at page level). The two-column layout CSS will be shared or duplicated.
- The existing `IJobService.GetCustomersAsync()` is kept for the job-creation dropdown, but the new `ICustomerService` becomes the canonical client for customer operations. This avoids a breaking change on the job-creation flow.
