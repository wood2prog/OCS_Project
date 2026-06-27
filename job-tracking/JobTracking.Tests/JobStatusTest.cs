using JobTracking.App.Models;

namespace JobTracking.Tests;

public class JobStatusTest
{
    [Fact]
    public void Status_is_New_when_no_milestones_completed()
    {
        var job = new Job();

        Assert.Equal("New", job.Status);
    }

    [Fact]
    public void Status_is_In_Design_when_milestone_1_completed()
    {
        var job = new Job();
        job.Milestones.Add(new Milestone { Order = 1, Label = "Designed", IsComplete = true });

        Assert.Equal("In Design", job.Status);
    }

    [Fact]
    public void Status_is_Awaiting_Approval_when_milestones_1_and_2_completed()
    {
        var job = new Job();
        job.Milestones.Add(new Milestone { Order = 1, Label = "Designed", IsComplete = true });
        job.Milestones.Add(new Milestone { Order = 2, Label = "Sent for approval", IsComplete = true });

        Assert.Equal("Awaiting Approval", job.Status);
    }

    [Fact]
    public void Status_uses_highest_completed_milestone()
    {
        var job = new Job();
        job.Milestones.Add(new Milestone { Order = 1, Label = "Designed", IsComplete = true });
        job.Milestones.Add(new Milestone { Order = 2, Label = "Sent for approval" });
        job.Milestones.Add(new Milestone { Order = 3, Label = "Approved to build", IsComplete = true });

        Assert.Equal("Approved", job.Status);
    }

    [Fact]
    public void Status_is_Closed_when_all_milestones_completed()
    {
        var job = new Job();
        for (int i = 1; i <= 12; i++)
        {
            job.Milestones.Add(new Milestone { Order = i, Label = $"M{i}", IsComplete = true });
        }

        Assert.Equal("Closed", job.Status);
    }

    [Fact]
    public void Status_is_unaffected_by_JobNumber_and_CustomerName()
    {
        var job = new Job
        {
            Id = 42,
            JobNumber = 1005,
            Customer = new Customer { Name = "Test Corp" },
            JobName = "Test Job"
        };
        job.Milestones.Add(new Milestone { Id = 1, JobId = 42, Order = 1, Label = "Designed", IsComplete = true });

        Assert.Equal("In Design", job.Status);
    }
}
