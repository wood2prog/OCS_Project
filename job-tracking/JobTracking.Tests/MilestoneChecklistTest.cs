using Bunit;
using JobTracking.App.Components;
using JobTracking.App.Models;
using Microsoft.AspNetCore.Components;

namespace JobTracking.Tests;

public class MilestoneChecklistTest : BunitContext
{
    private static Job SampleJob() => new()
    {
        Id = 1,
        JobNumber = 1000,
        CustomerName = "Test Co",
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
    public void Renders_job_name()
    {
        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, SampleJob()));
        Assert.Contains("Smithers Residence", cut.Find(".job-name").TextContent);
    }

    [Fact]
    public void Renders_all_12_milestones_in_order()
    {
        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, SampleJob()));
        var items = cut.FindAll(".milestone-item");
        Assert.Equal(12, items.Count);

        var expectedLabels = new[]
        {
            "Designed", "Sent for approval", "Approved to build",
            "Production started", "Components machined and assembled",
            "Components finished", "Final assembly done", "Loaded",
            "Delivered", "Billed", "Paid", "Closed"
        };

        for (int i = 0; i < items.Count; i++)
        {
            var text = items[i].QuerySelector(".milestone-text")!.TextContent.Trim();
            Assert.Equal(expectedLabels[i], text);
        }
    }

    [Fact]
    public void All_milestones_start_unchecked()
    {
        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, SampleJob()));
        var checkboxes = cut.FindAll(".milestone-checkbox");
        Assert.All(checkboxes, cb => Assert.False(cb.HasAttribute("checked")));
    }

    [Fact]
    public void Clicking_milestone_invokes_OnMilestoneToggled_with_correct_args()
    {
        var job = SampleJob();
        (int JobId, int MilestoneId, bool IsComplete)? captured = null;
        var cut = Render<MilestoneChecklist>(p =>
        {
            p.Add(c => c.Job, job);
            p.Add(c => c.OnMilestoneToggled, EventCallback.Factory.Create<(int, int, bool)>(this, args => captured = args));
        });

        var firstCheckbox = cut.Find(".milestone-checkbox");
        firstCheckbox.Change(true);

        Assert.NotNull(captured);
        Assert.Equal(job.Id, captured.Value.JobId);
        Assert.Equal(job.Milestones[0].Id, captured.Value.MilestoneId);
        Assert.True(captured.Value.IsComplete);
    }

    [Fact]
    public void Clicking_checked_milestone_invokes_with_IsComplete_false()
    {
        var job = SampleJob();
        job.Milestones[0].IsComplete = true;

        (int JobId, int MilestoneId, bool IsComplete)? captured = null;
        var cut = Render<MilestoneChecklist>(p =>
        {
            p.Add(c => c.Job, job);
            p.Add(c => c.OnMilestoneToggled, EventCallback.Factory.Create<(int, int, bool)>(this, args => captured = args));
        });

        var firstCheckbox = cut.Find(".milestone-checkbox");
        firstCheckbox.Change(false);

        Assert.NotNull(captured);
        Assert.False(captured.Value.IsComplete);
    }

    [Fact]
    public void Completed_milestone_has_strikethrough_class()
    {
        var job = SampleJob();
        job.Milestones[0].IsComplete = true;

        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, job));

        var firstItem = cut.Find(".milestone-item");
        Assert.Contains("completed", firstItem.ClassName);
    }

    [Fact]
    public void Toggling_one_milestone_only_fires_callback_for_that_milestone()
    {
        var job = SampleJob();
        var calls = new List<(int JobId, int MilestoneId, bool IsComplete)>();
        var cut = Render<MilestoneChecklist>(p =>
        {
            p.Add(c => c.Job, job);
            p.Add(c => c.OnMilestoneToggled, EventCallback.Factory.Create<(int, int, bool)>(this, args => calls.Add(args)));
        });

        var checkboxes = cut.FindAll(".milestone-checkbox");
        checkboxes[0].Change(true);

        Assert.Single(calls);
        Assert.Equal(job.Milestones[0].Id, calls[0].MilestoneId);
    }
}
