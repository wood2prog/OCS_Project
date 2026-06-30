using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JobTracking.App.Models;
using JobTracking.App.Services;

namespace JobTracking.Tests;

public class ApiCustomerServiceTest
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task GetCustomersAsync_returns_list()
    {
        var apiResponse = new[]
        {
            new Customer { Id = 1, Name = "Alpha", Email = "a@test.com", Phone = "555-0101", Notes = "Note A", JobCount = 2 },
            new Customer { Id = 2, Name = "Beta", Email = "b@test.com", Phone = "555-0102", Notes = null, JobCount = 0 },
        };

        var handler = new MockHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(apiResponse, options: JsonOptions)
            });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5271") };
        var service = new ApiCustomerService(client);

        var customers = await service.GetCustomersAsync();

        Assert.Equal(2, customers.Count);
        Assert.Equal("Alpha", customers[0].Name);
        Assert.Equal("a@test.com", customers[0].Email);
        Assert.Equal("555-0101", customers[0].Phone);
        Assert.Equal("Note A", customers[0].Notes);
        Assert.Equal(2, customers[0].JobCount);
        Assert.Equal("Beta", customers[1].Name);
        Assert.Equal(0, customers[1].JobCount);
    }

    [Fact]
    public async Task CreateCustomerAsync_sends_post()
    {
        var request = new CreateCustomerRequest
        {
            Name = "New Co",
            Email = "new@co.com",
            Phone = "555-8888",
            Notes = "Brand new",
        };

        var createdCustomer = new Customer { Id = 42, Name = "New Co", Email = "new@co.com" };

        HttpRequestMessage? captured = null;
        var handler = new MockHttpMessageHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent.Create(createdCustomer, options: JsonOptions)
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5271") };
        var service = new ApiCustomerService(client);

        var result = await service.CreateCustomerAsync(request);

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("http://localhost:5271/api/customers", captured.RequestUri!.ToString());

        var body = await captured.Content.ReadFromJsonAsync<CreateCustomerRequest>(JsonOptions) ?? new();
        Assert.Equal("New Co", body!.Name);
        Assert.Equal("new@co.com", body.Email);
        Assert.Equal("555-8888", body.Phone);
        Assert.Equal("Brand new", body.Notes);

        Assert.Equal(42, result.Id);
        Assert.Equal("New Co", result.Name);
    }

    [Fact]
    public async Task UpdateCustomerAsync_sends_patch()
    {
        var request = new UpdateCustomerRequest
        {
            Name = "Renamed Co",
            Email = "updated@co.com",
        };

        var updatedCustomer = new Customer { Id = 1, Name = "Renamed Co", Email = "updated@co.com" };

        HttpRequestMessage? captured = null;
        var handler = new MockHttpMessageHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(updatedCustomer, options: JsonOptions)
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5271") };
        var service = new ApiCustomerService(client);

        var result = await service.UpdateCustomerAsync(1, request);

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Patch, captured!.Method);
        Assert.Equal("http://localhost:5271/api/customers/1", captured.RequestUri!.ToString());

        var body = await captured.Content.ReadFromJsonAsync<UpdateCustomerRequest>(JsonOptions) ?? new();
        Assert.Equal("Renamed Co", body!.Name);
        Assert.Equal("updated@co.com", body.Email);

        Assert.Equal("Renamed Co", result.Name);
    }

    [Fact]
    public async Task DeleteCustomerAsync_sends_delete()
    {
        HttpRequestMessage? captured = null;
        var handler = new MockHttpMessageHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5271") };
        var service = new ApiCustomerService(client);

        await service.DeleteCustomerAsync(42);

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Delete, captured!.Method);
        Assert.Equal("http://localhost:5271/api/customers/42", captured.RequestUri!.ToString());
    }

    [Fact]
    public async Task DeleteCustomerAsync_with_409_throws()
    {
        var handler = new MockHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = JsonContent.Create(new { message = "Cannot delete: 3 jobs reference this customer." })
            });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5271") };
        var service = new ApiCustomerService(client);

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => service.DeleteCustomerAsync(1));
        Assert.Contains("409", ex.Message);
    }
}
