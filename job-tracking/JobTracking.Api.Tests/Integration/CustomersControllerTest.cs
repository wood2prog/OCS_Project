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
}
