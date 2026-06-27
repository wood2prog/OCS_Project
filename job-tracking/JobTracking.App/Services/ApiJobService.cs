using System.Net.Http.Json;
using System.Text.Json;
using JobTracking.App.Models;

namespace JobTracking.App.Services;

public class ApiJobService : IJobService
{
    private readonly HttpClient _http;

    public ApiJobService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Job>> GetJobsAsync()
    {
        var jobs = await _http.GetFromJsonAsync<List<Job>>("/api/jobs", new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        return jobs ?? [];
    }

    public async Task<Job> ToggleMilestoneAsync(int jobId, int milestoneId, bool isComplete)
    {
        var response = await _http.PatchAsJsonAsync(
            $"/api/jobs/{jobId}/milestones/{milestoneId}",
            new { isComplete });

        response.EnsureSuccessStatusCode();

        var job = await response.Content.ReadFromJsonAsync<Job>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        return job!;
    }

    public async Task<Job> CreateJobAsync(CreateJobRequest dto)
    {
        var response = await _http.PostAsJsonAsync("/api/jobs", dto, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        response.EnsureSuccessStatusCode();

        var job = await response.Content.ReadFromJsonAsync<Job>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        return job!;
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        var customers = await _http.GetFromJsonAsync<List<Customer>>("/api/customers", new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        return customers ?? [];
    }
}
