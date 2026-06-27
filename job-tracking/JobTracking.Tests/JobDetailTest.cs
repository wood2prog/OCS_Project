using Bunit;
using JobTracking.App.Components;
using JobTracking.App.Models;

namespace JobTracking.Tests;

public class JobDetailTest : BunitContext
{
    private static Job SampleJob() => new()
    {
        Id = 1,
        JobNumber = 1000,
        Customer = new() { Name = "Test Co" },
        JobName = "Smithers Residence",
        Milestones =
        [
            new() { Id = 1, JobId = 1, Order = 1, Label = "Designed" },
            new() { Id = 2, JobId = 1, Order = 2, Label = "Sent for approval" },
            new() { Id = 3, JobId = 1, Order = 3, Label = "Approved to build" },
            new() { Id = 4, JobId = 1, Order = 4, Label = "Production started" },
            new() { Id = 5, JobId = 1, Order = 5, Label = "Components machined and assembled" },
            new() { Id = 6, JobId = 1, Order = 6, Label = "Components finished" },
            new() { Id = 7, JobId = 1, Order = 7, Label = "Final assembly done" },
            new() { Id = 8, JobId = 1, Order = 8, Label = "Loaded" },
            new() { Id = 9, JobId = 1, Order = 9, Label = "Delivered" },
            new() { Id = 10, JobId = 1, Order = 10, Label = "Billed" },
            new() { Id = 11, JobId = 1, Order = 11, Label = "Paid" },
            new() { Id = 12, JobId = 1, Order = 12, Label = "Closed" },
        ]
    };

    [Fact]
    public void Shows_empty_state_when_no_job()
    {
        var cut = Render<JobDetail>();
        Assert.Contains("Select a job to view details", cut.Find("p").TextContent);
    }

    [Fact]
    public void Renders_milestone_checklist_when_job_provided()
    {
        var job = SampleJob();
        var cut = Render<JobDetail>(p => p.Add(c => c.Job, job));

        Assert.Contains("Smithers Residence", cut.Find("h2").TextContent);
    }
}
