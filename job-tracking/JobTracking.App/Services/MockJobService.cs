using JobTracking.App.Models;

namespace JobTracking.App.Services;

public class MockJobService : IJobService
{
    private readonly List<Job> _jobs =
    [
        new()
        {
            JobName = "Smithers Residence",
            Milestones =
            [
                new() { Order = 1, Label = "Designed", IsComplete = true },
                new() { Order = 2, Label = "Sent for approval", IsComplete = true },
                new() { Order = 3, Label = "Approved to build" },
                new() { Order = 4, Label = "Production started" },
                new() { Order = 5, Label = "Components machined and assembled" },
                new() { Order = 6, Label = "Components finished" },
                new() { Order = 7, Label = "Final assembly done" },
                new() { Order = 8, Label = "Loaded" },
                new() { Order = 9, Label = "Delivered" },
                new() { Order = 10, Label = "Billed" },
                new() { Order = 11, Label = "Paid" },
                new() { Order = 12, Label = "Closed" },
            ]
        },
        new()
        {
            JobName = "Johnson Kitchen",
            Milestones =
            [
                new() { Order = 1, Label = "Designed", IsComplete = true },
                new() { Order = 2, Label = "Sent for approval" },
                new() { Order = 3, Label = "Approved to build" },
                new() { Order = 4, Label = "Production started" },
                new() { Order = 5, Label = "Components machined and assembled" },
                new() { Order = 6, Label = "Components finished" },
                new() { Order = 7, Label = "Final assembly done" },
                new() { Order = 8, Label = "Loaded" },
                new() { Order = 9, Label = "Delivered" },
                new() { Order = 10, Label = "Billed" },
                new() { Order = 11, Label = "Paid" },
                new() { Order = 12, Label = "Closed" },
            ]
        },
        new()
        {
            JobName = "Lakewood Renovation",
            Milestones =
            [
                new() { Order = 1, Label = "Designed" },
                new() { Order = 2, Label = "Sent for approval" },
                new() { Order = 3, Label = "Approved to build" },
                new() { Order = 4, Label = "Production started" },
                new() { Order = 5, Label = "Components machined and assembled" },
                new() { Order = 6, Label = "Components finished" },
                new() { Order = 7, Label = "Final assembly done" },
                new() { Order = 8, Label = "Loaded" },
                new() { Order = 9, Label = "Delivered" },
                new() { Order = 10, Label = "Billed" },
                new() { Order = 11, Label = "Paid" },
                new() { Order = 12, Label = "Closed" },
            ]
        },
        new()
        {
            JobName = "Oakwood Office",
            Milestones =
            [
                new() { Order = 1, Label = "Designed", IsComplete = true },
                new() { Order = 2, Label = "Sent for approval", IsComplete = true },
                new() { Order = 3, Label = "Approved to build", IsComplete = true },
                new() { Order = 4, Label = "Production started", IsComplete = true },
                new() { Order = 5, Label = "Components machined and assembled", IsComplete = true },
                new() { Order = 6, Label = "Components finished", IsComplete = true },
                new() { Order = 7, Label = "Final assembly done" },
                new() { Order = 8, Label = "Loaded" },
                new() { Order = 9, Label = "Delivered" },
                new() { Order = 10, Label = "Billed" },
                new() { Order = 11, Label = "Paid" },
                new() { Order = 12, Label = "Closed" },
            ]
        },
        new()
        {
            JobName = "Elm Street Bath",
            Milestones =
            [
                new() { Order = 1, Label = "Designed", IsComplete = true },
                new() { Order = 2, Label = "Sent for approval", IsComplete = true },
                new() { Order = 3, Label = "Approved to build", IsComplete = true },
                new() { Order = 4, Label = "Production started", IsComplete = true },
                new() { Order = 5, Label = "Components machined and assembled", IsComplete = true },
                new() { Order = 6, Label = "Components finished", IsComplete = true },
                new() { Order = 7, Label = "Final assembly done", IsComplete = true },
                new() { Order = 8, Label = "Loaded", IsComplete = true },
                new() { Order = 9, Label = "Delivered", IsComplete = true },
                new() { Order = 10, Label = "Billed", IsComplete = true },
                new() { Order = 11, Label = "Paid", IsComplete = true },
                new() { Order = 12, Label = "Closed", IsComplete = true },
            ]
        },
    ];

    public Task<List<Job>> GetJobsAsync()
    {
        return Task.FromResult(_jobs);
    }
}
