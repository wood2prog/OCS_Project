using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using JobTracking.App.Models;
using JobTracking.App.Services;

namespace JobTracking.Tests;

public class ApiJobServiceTest
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task GetJobsAsync_returns_deserialized_jobs_from_api()
    {
        var apiResponse = new[]
        {
            new Job
            {
                Id = 1,
                JobNumber = 1000,
                Customer = new() { Name = "Thompson" },
                JobName = "Thompson Kitchen Remodel",
                Milestones =
                [
                    new() { Id = 1, JobId = 1, Order = 1, Label = "Designed", IsComplete = true },
                    new() { Id = 2, JobId = 1, Order = 2, Label = "Sent for approval", IsComplete = true },
                    new() { Id = 3, JobId = 1, Order = 3, Label = "Approved to build", IsComplete = true },
                ]
            }
        };

        var handler = new MockHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(apiResponse, options: JsonOptions)
            });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5271") };
        var service = new ApiJobService(client);

        var jobs = await service.GetJobsAsync();

        var job = Assert.Single(jobs);
        Assert.Equal(1, job.Id);
        Assert.Equal(1000, job.JobNumber);
        Assert.Equal("Thompson", job.CustomerName);
        Assert.Equal("Thompson Kitchen Remodel", job.JobName);
        Assert.Equal(3, job.Milestones.Count);
        Assert.True(job.Milestones[0].IsComplete);
    }

    [Fact]
    public async Task ToggleMilestoneAsync_calls_patch_and_returns_updated_job()
    {
        var updatedJob = new Job
        {
            Id = 1,
            JobNumber = 1000,
            Customer = new() { Name = "Thompson" },
            JobName = "Thompson Kitchen Remodel",
            Milestones =
            [
                new() { Id = 1, JobId = 1, Order = 1, Label = "Designed", IsComplete = true },
                new() { Id = 2, JobId = 1, Order = 2, Label = "Sent for approval", IsComplete = true },
            ]
        };

        HttpRequestMessage? captured = null;
        var handler = new MockHttpMessageHandler(request =>
        {
            captured = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(updatedJob, options: JsonOptions)
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5271") };
        var service = new ApiJobService(client);

        var result = await service.ToggleMilestoneAsync(1, 1, true);

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Patch, captured!.Method);
        Assert.Equal("http://localhost:5271/api/jobs/1/milestones/1", captured.RequestUri!.ToString());
        Assert.Equal(1, result.Id);
        Assert.True(result.Milestones[0].IsComplete);
    }
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

    public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_handler(request));
    }
}
