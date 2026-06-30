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
                    new() { Id = 1, JobId = 1, Order = 1, Label = "Designed", CompletedAt = DateTime.UtcNow },
                    new() { Id = 2, JobId = 1, Order = 2, Label = "Sent for approval", CompletedAt = DateTime.UtcNow },
                    new() { Id = 3, JobId = 1, Order = 3, Label = "Approved to build", CompletedAt = DateTime.UtcNow },
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
        Assert.NotNull(job.Milestones[0].CompletedAt);
    }

    [Fact]
    public async Task UpdateMilestoneAsync_calls_patch_and_returns_updated_job()
    {
        var updatedJob = new Job
        {
            Id = 1,
            JobNumber = 1000,
            Customer = new() { Name = "Thompson" },
            JobName = "Thompson Kitchen Remodel",
            Milestones =
            [
                new() { Id = 1, JobId = 1, Order = 1, Label = "Designed", CompletedAt = DateTime.UtcNow },
                new() { Id = 2, JobId = 1, Order = 2, Label = "Sent for approval", CompletedAt = DateTime.UtcNow },
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

        var result = await service.UpdateMilestoneAsync(1, 1, true);

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Patch, captured!.Method);
        Assert.Equal("http://localhost:5271/api/jobs/1/milestones/1", captured.RequestUri!.ToString());
        Assert.Equal(1, result.Id);
        Assert.NotNull(result.Milestones[0].CompletedAt);
    }

    [Fact]
    public async Task CreateJobAsync_sends_post_and_returns_created_job()
    {
        var request = new CreateJobRequest
        {
            CustomerId = 1,
            JobName = "New Job",
            LeadDate = new DateTime(2026, 6, 1),
            StartDate = new DateTime(2026, 6, 15),
            DeliveryDate = new DateTime(2026, 7, 15),
            QuoteAmount = 5000m,
        };

        var createdJob = new Job
        {
            Id = 42,
            JobNumber = 1005,
            Customer = new() { Name = "Thompson" },
            JobName = "New Job",
            Milestones = []
        };

        HttpRequestMessage? captured = null;
        var handler = new MockHttpMessageHandler(requestMessage =>
        {
            captured = requestMessage;
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent.Create(createdJob, options: JsonOptions)
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5271") };
        var service = new ApiJobService(client);

        var result = await service.CreateJobAsync(request);

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("http://localhost:5271/api/jobs", captured.RequestUri!.ToString());

        var body = await captured.Content.ReadFromJsonAsync<CreateJobRequest>(JsonOptions) ?? new();
        Assert.NotNull(body);
        Assert.Equal(1, body!.CustomerId);
        Assert.Equal("New Job", body.JobName);
        Assert.Equal(new DateTime(2026, 6, 1), body.LeadDate);
        Assert.Equal(new DateTime(2026, 6, 15), body.StartDate);
        Assert.Equal(new DateTime(2026, 7, 15), body.DeliveryDate);
        Assert.Equal(5000m, body.QuoteAmount);

        Assert.Equal(42, result.Id);
        Assert.Equal("New Job", result.JobName);
    }

    [Fact]
    public async Task UpdateJobAsync_sends_patch_and_returns_updated_job()
    {
        var request = new UpdateJobRequest
        {
            JobName = "Renamed Job",
            LeadDate = new DateTime(2026, 7, 1),
            QuoteAmount = 7500m,
        };

        var updatedJob = new Job
        {
            Id = 1,
            JobNumber = 1000,
            Customer = new() { Name = "Thompson" },
            JobName = "Renamed Job",
        };

        HttpRequestMessage? captured = null;
        var handler = new MockHttpMessageHandler(requestMessage =>
        {
            captured = requestMessage;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(updatedJob, options: JsonOptions)
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5271") };
        var service = new ApiJobService(client);

        var result = await service.UpdateJobAsync(1, request);

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Patch, captured!.Method);
        Assert.Equal("http://localhost:5271/api/jobs/1", captured.RequestUri!.ToString());

        var body = await captured.Content.ReadFromJsonAsync<UpdateJobRequest>(JsonOptions) ?? new();
        Assert.Equal("Renamed Job", body!.JobName);
        Assert.Equal(7500m, body.QuoteAmount);

        Assert.Equal("Renamed Job", result.JobName);
    }

    [Fact]
    public async Task DeleteJobAsync_sends_delete()
    {
        HttpRequestMessage? captured = null;
        var handler = new MockHttpMessageHandler(requestMessage =>
        {
            captured = requestMessage;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5271") };
        var service = new ApiJobService(client);

        await service.DeleteJobAsync(42);

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Delete, captured!.Method);
        Assert.Equal("http://localhost:5271/api/jobs/42", captured.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetCustomersAsync_returns_deserialized_customers()
    {
        var apiResponse = new[]
        {
            new Customer { Id = 1, Name = "Alpha Co" },
            new Customer { Id = 2, Name = "Beta Inc" },
        };

        var handler = new MockHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(apiResponse, options: JsonOptions)
            });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5271") };
        var service = new ApiJobService(client);

        var customers = await service.GetCustomersAsync();

        Assert.Equal(2, customers.Count);
        Assert.Equal(1, customers[0].Id);
        Assert.Equal("Alpha Co", customers[0].Name);
        Assert.Equal(2, customers[1].Id);
        Assert.Equal("Beta Inc", customers[1].Name);
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
