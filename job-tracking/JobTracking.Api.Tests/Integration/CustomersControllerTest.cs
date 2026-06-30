using System.Net;
using System.Net.Http.Json;
using JobTracking.Api.Models;

namespace JobTracking.Api.Tests.Integration;

public class CustomersControllerTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CustomersControllerTest(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_customer_returns_created_with_id()
    {
        var response = await _client.PostAsJsonAsync("/api/customers", new
        {
            Name = "Acme Corp"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<Customer>();
        Assert.NotNull(customer);
        Assert.Equal("Acme Corp", customer.Name);
        Assert.True(customer.Id > 0);
    }

    [Fact]
    public async Task List_customers_returns_all()
    {
        await _client.PostAsJsonAsync("/api/customers", new { Name = "Alpha" });
        await _client.PostAsJsonAsync("/api/customers", new { Name = "Beta" });

        var response = await _client.GetAsync("/api/customers");
        var customers = await response.Content.ReadFromJsonAsync<List<Customer>>();

        Assert.NotNull(customers);
        Assert.Contains(customers, c => c.Name == "Alpha");
        Assert.Contains(customers, c => c.Name == "Beta");
    }

    [Fact]
    public async Task Get_customer_by_id_returns_customer()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/customers", new { Name = "Gamma" });
        var created = await createResponse.Content.ReadFromJsonAsync<Customer>();

        var response = await _client.GetAsync($"/api/customers/{created!.Id}");
        var customer = await response.Content.ReadFromJsonAsync<Customer>();

        Assert.NotNull(customer);
        Assert.Equal("Gamma", customer.Name);
    }

    [Fact]
    public async Task Get_customer_by_nonexistent_id_returns_not_found()
    {
        var response = await _client.GetAsync("/api/customers/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Patch_customer_updates_fields()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/customers", new { Name = "Orig Co" });
        var created = await createResponse.Content.ReadFromJsonAsync<Customer>();
        Assert.NotNull(created);

        var patchResponse = await _client.PatchAsJsonAsync($"/api/customers/{created!.Id}", new
        {
            Name = "Updated Co",
            Email = "new@email.com",
            Phone = "555-9999",
            Notes = "Updated notes"
        });

        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
        var updated = await patchResponse.Content.ReadFromJsonAsync<Customer>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Co", updated!.Name);
        Assert.Equal("new@email.com", updated.Email);
        Assert.Equal("555-9999", updated.Phone);
        Assert.Equal("Updated notes", updated.Notes);
    }

    [Fact]
    public async Task Patch_nonexistent_customer_returns_not_found()
    {
        var response = await _client.PatchAsJsonAsync("/api/customers/99999", new { Name = "Nope" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_customer_without_jobs_succeeds()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/customers", new { Name = "Delete Me" });
        var created = await createResponse.Content.ReadFromJsonAsync<Customer>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/customers/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/customers/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_customer_with_jobs_returns_conflict()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/customers", new { Name = "Has Jobs" });
        var customer = await createResponse.Content.ReadFromJsonAsync<Customer>();
        Assert.NotNull(customer);

        await _client.PostAsJsonAsync("/api/jobs", new { CustomerId = customer!.Id, JobName = "Test Job" });

        var deleteResponse = await _client.DeleteAsync($"/api/customers/{customer.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);

        var body = await deleteResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.Contains("message", body!.Keys);
    }

    [Fact]
    public async Task Delete_nonexistent_customer_returns_not_found()
    {
        var response = await _client.DeleteAsync("/api/customers/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_customers_returns_job_counts()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/customers", new { Name = "Count Co" });
        var customer = await createResponse.Content.ReadFromJsonAsync<Customer>();
        Assert.NotNull(customer);

        await _client.PostAsJsonAsync("/api/jobs", new { CustomerId = customer!.Id, JobName = "Count Job" });

        var response = await _client.GetAsync("/api/customers");
        var customers = await response.Content.ReadFromJsonAsync<List<Customer>>();
        Assert.NotNull(customers);

        var counted = customers!.FirstOrDefault(c => c.Id == customer.Id);
        Assert.NotNull(counted);
        Assert.Equal(1, counted!.JobCount);
    }

    [Fact]
    public async Task Create_customer_with_dto_returns_created()
    {
        var response = await _client.PostAsJsonAsync("/api/customers", new
        {
            Name = "DTO Co",
            Email = "dto@test.com",
            Phone = "555-0000",
            Notes = "Created via DTO"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<Customer>();
        Assert.NotNull(customer);
        Assert.Equal("DTO Co", customer!.Name);
        Assert.Equal("dto@test.com", customer.Email);
        Assert.Equal("555-0000", customer.Phone);
        Assert.Equal("Created via DTO", customer.Notes);
    }

    [Fact]
    public async Task Create_customer_missing_name_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync("/api/customers", new
        {
            Email = "noname@test.com"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
