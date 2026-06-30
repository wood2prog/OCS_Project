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
    public void Renders_job_name()
    {
        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, SampleJob()));
        Assert.Contains("Smithers Residence", cut.Find(".job-name").TextContent);
    }

    [Fact]
    public void Renders_all_12_milestones_in_order()
    {
        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, SampleJob()));
        var nodes = cut.FindAll(".step-node");
        Assert.Equal(12, nodes.Count);

        var expectedLabels = new[]
        {
            "Designed", "Sent for approval", "Approved to build",
            "Production started", "Components machined and assembled",
            "Components finished", "Final assembly done", "Loaded",
            "Delivered", "Billed", "Paid", "Closed"
        };

        for (int i = 0; i < nodes.Count; i++)
        {
            var label = nodes[i].QuerySelector(".step-label")!.TextContent.Trim();
            Assert.Equal(expectedLabels[i], label);
        }
    }

    [Fact]
    public void All_steps_locked_when_none_complete()
    {
        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, SampleJob()));
        var nodes = cut.FindAll(".step-node");

        Assert.Contains("step-active", nodes[0].ClassName);
        for (int i = 1; i < nodes.Count; i++)
        {
            Assert.Contains("step-locked", nodes[i].ClassName);
        }
    }

    [Fact]
    public void Active_step_advances_as_milestones_complete()
    {
        var job = SampleJob();
        job.Milestones[0].CompletedAt = DateTime.UtcNow;
        job.Milestones[1].CompletedAt = DateTime.UtcNow;
        job.Milestones[2].CompletedAt = DateTime.UtcNow;

        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, job));
        var nodes = cut.FindAll(".step-node");

        for (int i = 0; i < 3; i++)
            Assert.Contains("step-completed", nodes[i].ClassName);

        Assert.Contains("step-active", nodes[3].ClassName);

        for (int i = 4; i < 12; i++)
            Assert.Contains("step-locked", nodes[i].ClassName);
    }

    [Fact]
    public void All_steps_completed_when_all_done()
    {
        var job = SampleJob();
        foreach (var m in job.Milestones)
            m.CompletedAt = DateTime.UtcNow;

        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, job));
        var nodes = cut.FindAll(".step-node");

        Assert.All(nodes, n => Assert.Contains("step-completed", n.ClassName));
    }

    [Fact]
    public void Clicking_active_step_fires_callback_with_Complete_true()
    {
        var job = SampleJob();
        (int JobId, int MilestoneId, bool Complete)? captured = null;
        var cut = Render<MilestoneChecklist>(p =>
        {
            p.Add(c => c.Job, job);
            p.Add(c => c.OnMilestoneToggled, EventCallback.Factory.Create<(int, int, bool)>(this, args => captured = args));
        });

        cut.Find(".step-active").Click();

        Assert.NotNull(captured);
        Assert.Equal(job.Id, captured.Value.JobId);
        Assert.Equal(job.Milestones[0].Id, captured.Value.MilestoneId);
        Assert.True(captured.Value.Complete);
    }

    [Fact]
    public void Clicking_locked_step_does_not_fire_callback()
    {
        var job = SampleJob();
        (int JobId, int MilestoneId, bool Complete)? captured = null;
        var cut = Render<MilestoneChecklist>(p =>
        {
            p.Add(c => c.Job, job);
            p.Add(c => c.OnMilestoneToggled, EventCallback.Factory.Create<(int, int, bool)>(this, args => captured = args));
        });

        var lockedStep = cut.FindAll(".step-locked")[0];
        lockedStep.Click();

        Assert.Null(captured);
    }
}
