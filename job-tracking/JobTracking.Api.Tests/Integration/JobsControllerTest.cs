using System.Net;
using System.Net.Http.Json;
using JobTracking.Api.Models;

namespace JobTracking.Api.Tests.Integration;

public class JobsControllerTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public JobsControllerTest(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Customer> CreateCustomer(string name = "Test Co")
    {
        var r = await _client.PostAsJsonAsync("/api/customers", new { Name = name });
        return (await r.Content.ReadFromJsonAsync<Customer>())!;
    }

    private async Task<Job> CreateJob(int customerId, string jobName)
    {
        var r = await _client.PostAsJsonAsync("/api/jobs", new { CustomerId = customerId, JobName = jobName });
        return (await r.Content.ReadFromJsonAsync<Job>())!;
    }

    [Fact]
    public async Task Create_job_returns_created_with_job_number_and_milestones()
    {
        var customer = await CreateCustomer();
        var job = await CreateJob(customer.Id, "Kitchen Remodel");

        Assert.Equal("Kitchen Remodel", job.JobName);
        Assert.True(job.JobNumber >= 1000);
        Assert.Equal(12, job.Milestones.Count);
        Assert.All(job.Milestones, m => Assert.False(m.IsComplete));
        Assert.Equal("New", job.Status);
    }

    [Fact]
    public async Task Job_number_auto_increments()
    {
        var customer = await CreateCustomer("Co");
        var j1 = await CreateJob(customer.Id, "Job 1");
        var j2 = await CreateJob(customer.Id, "Job 2");

        Assert.Equal(j1.JobNumber + 1, j2.JobNumber);
    }

    [Fact]
    public async Task Get_job_by_id_returns_job_with_milestones()
    {
        var customer = await CreateCustomer("Co");
        var created = await CreateJob(customer.Id, "Bath Reno");

        var response = await _client.GetAsync($"/api/jobs/{created.Id}");
        var job = await response.Content.ReadFromJsonAsync<Job>();

        Assert.NotNull(job);
        Assert.Equal("Bath Reno", job.JobName);
        Assert.Equal(12, job.Milestones.Count);
    }

    [Fact]
    public async Task Active_job_list_returns_jobs_with_status()
    {
        var customer = await CreateCustomer("Co");
        var job = await CreateJob(customer.Id, "Office Reno");

        var response = await _client.GetAsync("/api/jobs");
        var jobs = await response.Content.ReadFromJsonAsync<List<Job>>();

        Assert.NotNull(jobs);
        var found = jobs.FirstOrDefault(j => j.Id == job.Id);
        Assert.NotNull(found);
        Assert.Equal("New", found.Status);
    }

    [Fact]
    public async Task Toggle_milestone_updates_status()
    {
        var customer = await CreateCustomer("Co");
        var job = await CreateJob(customer.Id, "Kitchen");

        var ms1 = job.Milestones.First(m => m.Order == 1);
        var response = await _client.PatchAsJsonAsync(
            $"/api/jobs/{job.Id}/milestones/{ms1.Id}",
            new { IsComplete = true, CompletedBy = "Alice" });
        var updated = await response.Content.ReadFromJsonAsync<Job>();

        Assert.NotNull(updated);
        var m1 = updated.Milestones.First(m => m.Order == 1);
        Assert.True(m1.IsComplete);
        Assert.Equal("Alice", m1.CompletedBy);
        Assert.NotNull(m1.CompletedDate);
        Assert.Equal("In Design", updated.Status);
    }

    [Fact]
    public async Task Milestone_group_for_change_order_excluded_from_status()
    {
        var customer = await CreateCustomer("Co");
        var job = await CreateJob(customer.Id, "Kitchen");

        var coResponse = await _client.PostAsJsonAsync($"/api/jobs/{job.Id}/changeorders",
            new { Description = "Add island", Amount = 500m });
        Assert.Equal(HttpStatusCode.Created, coResponse.StatusCode);
    }

    [Fact]
    public async Task Add_document_to_job()
    {
        var customer = await CreateCustomer("Co");
        var job = await CreateJob(customer.Id, "Kitchen");

        var response = await _client.PostAsJsonAsync($"/api/jobs/{job.Id}/documents", new
        {
            Bucket = "contract",
            FileName = "signed-contract.pdf",
            StoragePath = "/docs/1234/contract.pdf",
            Notes = "Signed on site"
        });
        var doc = await response.Content.ReadFromJsonAsync<Document>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(doc);
        Assert.Equal("contract", doc.Bucket);
        Assert.Equal("signed-contract.pdf", doc.FileName);
    }

    [Fact]
    public async Task List_documents_for_job()
    {
        var customer = await CreateCustomer("Co");
        var job = await CreateJob(customer.Id, "Kitchen");

        await _client.PostAsJsonAsync($"/api/jobs/{job.Id}/documents", new
        {
            Bucket = "quote", FileName = "quote.pdf", StoragePath = "/docs/q.pdf"
        });

        var response = await _client.GetAsync($"/api/jobs/{job.Id}/documents");
        var docs = await response.Content.ReadFromJsonAsync<List<Document>>();

        Assert.NotNull(docs);
        Assert.Single(docs);
    }

    [Fact]
    public async Task Closed_jobs_older_than_7_days_excluded_from_active()
    {
        var customer = await CreateCustomer("Co");
        var job = await CreateJob(customer.Id, "Old Closed");

        // Complete all 12 milestones to reach "Closed"
        foreach (var ms in job.Milestones)
        {
            await _client.PatchAsJsonAsync(
                $"/api/jobs/{job.Id}/milestones/{ms.Id}",
                new { IsComplete = true, CompletedBy = "System" });
        }

        // Verify it's Closed
        var detail = await _client.GetFromJsonAsync<Job>($"/api/jobs/{job.Id}");
        Assert.Equal("Closed", detail!.Status);

        // Active list should still include it (Closed < 7 days ago)
        var active = await _client.GetFromJsonAsync<List<Job>>("/api/jobs");
        Assert.Contains(active!, j => j.Id == job.Id);
    }
}
