# PRD 006: Edit and Delete Job

## Problem Statement

The Job Tracking application currently supports creating jobs and toggling their milestones, but there is no way to correct mistakes or remove jobs that were entered in error. When a job is created with wrong details, the user must either live with the error or delete the entire database entry manually. Jobs that were test entries or duplicate entries accumulate in the list with no way to clean them up.

## Solution

Add Edit and Delete actions accessible via a "..." (kebab) menu on each job row in the job list.

- **Edit** opens a modal pre-populated with the job's current values (Job Name, Lead Date, Start Date, Delivery Date, Quote Amount). The Customer is not editable — if the customer was wrong, the job should be deleted and re-entered.
- **Delete** shows a confirmation modal warning that the action cannot be undone. On confirm, the job and all related entities (milestones, change orders, documents) are hard-deleted from the database. The row is removed from the list and the detail panel is cleared.

## User Stories

1. As a job tracker user, I want to see a "..." (kebab) menu on each job row, so that I know what actions are available for that job.

2. As a job tracker user, I want to click the kebab menu to reveal Edit and Delete options, so that I can choose the action I want.

3. As a job tracker user, I want to click "Edit" from the kebab menu, so that an edit modal opens with the job's current values pre-populated.

4. As a job tracker user, I want to change the Job Name in the edit modal and save, so that I can correct a mistyped name.

5. As a job tracker user, I want to change the Lead Date, Start Date, or Delivery Date in the edit modal and save, so that I can keep dates accurate.

6. As a job tracker user, I want to change the Quote Amount in the edit modal and save, so that I can update pricing.

7. As a job tracker user, I want to see a read-only Customer display in the edit modal for context, but not be able to change it, so that I understand which customer the job belongs to.

8. As a job tracker user, I want the edit modal's Save button to be disabled until at least the Job Name is filled in, so that I cannot save an incomplete job.

9. As a job tracker user, I want to click Cancel in the edit modal to discard my changes, so that I can exit without saving.

10. As a job tracker user, I want to click "Delete" from the kebab menu, so that a confirmation dialog appears warning that this action cannot be undone.

11. As a job tracker user, I want to see the job's name in the delete confirmation dialog, so that I know which job I am about to delete.

12. As a job tracker user, I want to confirm deletion in the dialog, so that the job and all its milestones, change orders, and documents are permanently removed from the database.

13. As a job tracker user, I want to click Cancel in the delete confirmation dialog, so that the job is not deleted.

14. As a job tracker user, I want the job row to immediately disappear from the list after deletion, so that I have immediate feedback that the action succeeded.

15. As a job tracker user, I want the detail panel to clear if I delete the currently selected job, so that I'm not looking at stale data.

16. As a job tracker user, I want the edited job to update in-place in the list (no full re-fetch), so that the UI feels responsive.

17. As a job tracker user, I want the kebab menu to close when I click outside of it, so that the interface stays clean.

18. As a job tracker user, I want clicking the kebab menu toggle to not also select the job row, so that I don't accidentally change my selection.

19. As an API consumer, I want to send a PATCH request to update specific job fields, so that I can integrate with other tools.

20. As an API consumer, I want to send a DELETE request to remove a job, so that I can clean up data programmatically.

21. As an API consumer, I want a 404 response when trying to update or delete a non-existent job, so that I can handle errors appropriately.

22. As an API consumer, I want the DELETE endpoint to cascade-delete milestones, change orders, and documents, so that no orphaned data remains.

## Implementation Decisions

### Delete semantics: Hard delete
Jobs are hard-deleted from the database, departing from the earlier "never deleted" invariant. This is justified by the need to clean up test entries, duplicates, and mistaken creations. The change is documented in a new ADR.

### Cascade delete
When a job is deleted, all related milestones, change orders, and documents are also deleted. EF Core cascade delete is configured on the `Job → Milestone`, `Job → ChangeOrder`, and `Job → Document` relationships.

### API: New endpoint
A `DELETE /api/jobs/{id}` endpoint is added to `JobsController`. The repository gets a new `DeleteAsync(int id)` method that removes the job (cascade handles children).

### API: Existing PATCH endpoint
The existing `PATCH /api/jobs/{id}` already returns the full updated `Job` object. No changes needed to the endpoint itself. The frontend uses it as-is.

### Frontend: Customer not editable
The edit modal omits the customer dropdown. Instead, it shows the customer name as a static label for context. If the customer was entered wrong, the job must be deleted and re-created.

### Frontend: Job model extended
The frontend `Job` model gets `LeadDate`, `StartDate`, `DeliveryDate`, and `QuoteAmount` properties (all nullable). These are already returned by the API but were being dropped by the deserializer.

### Frontend: Kebab menu
A dropdown menu (".job-actions-menu") is added to each `.job-row` in `JobList.razor`. The kebab button uses `@onclick:stopPropagation` to prevent triggering row selection. Clicking outside the menu closes it via a document-level click handler.

### Frontend: EditJobModal component
A new `EditJobModal.razor` component following the same pure-presentational pattern as `AddJobModal.razor`:
- Parameters: `Job` (the job to edit), `OnJobUpdated` (EventCallback<UpdateJobRequest>), `OnDismiss` (EventCallback)
- Pre-populates form fields from the Job parameter
- No customer dropdown — static label only
- Save button disabled when Job Name is empty

### Frontend: UpdateJobRequest model
A new `UpdateJobRequest` model with the editable fields (no CustomerId): `JobName`, `LeadDate`, `StartDate`, `DeliveryDate`, `QuoteAmount`.

### Frontend: Delete confirmation
A confirmation dialog is rendered inline in `Home.razor` (no separate component, given its simplicity). Shown/hidden via `_showDeleteConfirm` flag. Displays job name, red "Delete" button, and "Cancel" button.

### Frontend: IJobService extended
Two new methods:
- `Task<Job> UpdateJobAsync(int jobId, UpdateJobRequest dto)` — calls `PATCH /api/jobs/{id}`
- `Task DeleteJobAsync(int jobId)` — calls `DELETE /api/jobs/{id}`

### Frontend: In-place list update
After edit: the returned `Job` replaces the matching entry in `_jobs` by index, and replaces `_selectedJob` if it was the edited job. After delete: the job is removed from `_jobs` and `_selectedJob` is cleared if it was the deleted job.

### Kebab menu scope
The kebab menu is built directly into `JobList.razor` rather than as a separate component, keeping the scope small.

## Testing Decisions

### What makes a good test
- Test external behavior, not implementation details
- For components: verify rendering output and callback invocation, not internal state
- For services: verify HTTP method, URL, request body, and response deserialization
- For API: verify HTTP status codes and response bodies via full round-trip

### Test modules and seams

| Test file | Seam | Prior art |
|---|---|---|
| `JobListTest.cs` (extend) | BUnit component test — render JobList with kebab menu, verify kebab button renders, clicking toggles dropdown, clicking Edit/Delete fires callbacks | `JobListTest.cs:26` (callback invocation), `JobListTest.cs:44` (CSS class assertions) |
| `EditJobModalTest.cs` (new) | BUnit component test — render with pre-populated Job, verify fields show correct values, save fires callback with modified data, cancel fires dismiss | `AddJobModalTest.cs:91` (form interaction + callback capture), `AddJobModalTest.cs:115` (dismiss callback) |
| `ApiJobServiceTest.cs` (extend) | Unit test with `MockHttpMessageHandler` — verify UpdateJobAsync sends PATCH with correct URL/body, DeleteJobAsync sends DELETE, both handle responses | `ApiJobServiceTest.cs:58` (PATCH test), `ApiJobServiceTest.cs:96` (POST test with body capture) |
| `JobsControllerTest.cs` (extend) | Integration test with `CustomWebApplicationFactory` — verify full round-trip: create → PATCH with update → GET to confirm fields changed; create → DELETE → GET returns 404; DELETE of nonexistent job returns 404 | `JobsControllerTest.cs:28` (create + verify), `JobsControllerTest.cs:80` (PATCH milestone + verify) |

### New seams
No new seam types are needed. All new tests use the existing patterns (BUnit for components, `MockHttpMessageHandler` for service layer, `CustomWebApplicationFactory` for integration).

## Out of Scope

- **Change Orders UI** — API endpoints exist but no frontend is built. Edit/delete for change orders is not covered.
- **Documents UI** — API endpoints exist but no frontend. Deleting a job cascade-deletes its documents, but there is no UI to view or manage them separately.
- **Customer edit** — Customers are created but never edited or deleted via the UI. Not covered here.
- **Undo after delete** — Delete is immediate and irreversible. No soft-delete or recycle bin.
- **Bulk operations** — No multi-select, batch delete, or batch edit.
- **Keyboard shortcuts** — No accelerator keys for edit/delete.
- **Audit trail** — No logging of who edited or deleted a job.

## Further Notes

- A new ADR should be created to document the departure from the "never deleted" invariant established in PRD-003.
- The term "delete" in this context means permanent database removal. The existing glossary term "Close" (milestone 12) remains the standard way to complete a job; "delete" is reserved for error correction.
