using System.Net.Http.Json;
using System.Text.Json;
using JobTracking.App.Models;

namespace JobTracking.App.Services;

public class ApiCustomerService : ICustomerService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public ApiCustomerService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        var customers = await _http.GetFromJsonAsync<List<Customer>>("/api/customers", JsonOptions);
        return customers ?? [];
    }

    public async Task<Customer> CreateCustomerAsync(CreateCustomerRequest dto)
    {
        var response = await _http.PostAsJsonAsync("/api/customers", dto, JsonOptions);
        response.EnsureSuccessStatusCode();
        var customer = await response.Content.ReadFromJsonAsync<Customer>(JsonOptions);
        return customer!;
    }

    public async Task<Customer> UpdateCustomerAsync(int id, UpdateCustomerRequest dto)
    {
        var response = await _http.PatchAsJsonAsync($"/api/customers/{id}", dto, JsonOptions);
        response.EnsureSuccessStatusCode();
        var customer = await response.Content.ReadFromJsonAsync<Customer>(JsonOptions);
        return customer!;
    }

    public async Task DeleteCustomerAsync(int id)
    {
        var response = await _http.DeleteAsync($"/api/customers/{id}");
        response.EnsureSuccessStatusCode();
    }
}
