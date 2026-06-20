using Bunit;
using JobTracking.App.Components;
using JobTracking.App.Models;

namespace JobTracking.Tests;

public class MilestoneChecklistTest : BunitContext
{
    private static Job SampleJob() => Job.SampleJob();

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
    public void Clicking_milestone_toggles_it_to_checked()
    {
        var job = SampleJob();
        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, job));

        var firstCheckbox = cut.Find(".milestone-checkbox");
        firstCheckbox.Change(true);

        Assert.True(job.Milestones[0].IsComplete);
        Assert.Contains("completed", cut.Find(".milestone-item").ClassName);
    }

    [Fact]
    public void Clicking_checked_milestone_unchecks_it()
    {
        var job = SampleJob();
        job.Milestones[0].IsComplete = true;

        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, job));

        var firstCheckbox = cut.Find(".milestone-checkbox");
        firstCheckbox.Change(false);

        Assert.False(job.Milestones[0].IsComplete);
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
    public void Toggling_one_milestone_does_not_affect_others()
    {
        var job = SampleJob();
        var cut = Render<MilestoneChecklist>(p => p.Add(c => c.Job, job));

        var checkboxes = cut.FindAll(".milestone-checkbox");
        checkboxes[0].Change(true);

        Assert.True(job.Milestones[0].IsComplete);
        for (int i = 1; i < job.Milestones.Count; i++)
        {
            Assert.False(job.Milestones[i].IsComplete);
        }
    }
}
