using Bunit;
using JobTracking.App.Components;
using JobTracking.App.Models;
using Microsoft.AspNetCore.Components;

namespace JobTracking.Tests;

public class JobListTest : BunitContext
{
    private static List<Job> SampleJobs() =>
    [
        new() { Id = 1, JobName = "Job A" },
        new() { Id = 2, JobName = "Job B" },
    ];

    [Fact]
    public void Renders_all_job_names()
    {
        var cut = Render<JobList>(p => p.Add(c => c.Jobs, SampleJobs()));
        var rows = cut.FindAll(".job-row");
        Assert.Equal(2, rows.Count);
        Assert.Contains("Job A", rows[0].TextContent);
        Assert.Contains("Job B", rows[1].TextContent);
    }

    [Fact]
    public void Clicking_job_row_invokes_callback_with_correct_job()
    {
        var jobs = SampleJobs();
        Job? selected = null;
        var cut = Render<JobList>(p =>
        {
            p.Add(c => c.Jobs, jobs);
            p.Add(c => c.OnJobSelected, EventCallback.Factory.Create<Job>(this, j => selected = j));
        });

        cut.FindAll(".job-row")[1].Click();

        Assert.NotNull(selected);
        Assert.Equal("Job B", selected!.JobName);
    }

    [Fact]
    public void Selected_job_is_highlighted()
    {
        var jobs = SampleJobs();
        var cut = Render<JobList>(p =>
        {
            p.Add(c => c.Jobs, jobs);
            p.Add(c => c.SelectedJob, jobs[0]);
        });

        var rows = cut.FindAll(".job-row");
        Assert.Contains("selected", rows[0].ClassName);
        Assert.DoesNotContain("selected", rows[1].ClassName);
    }

    [Fact]
    public void Each_job_has_status_badge()
    {
        var cut = Render<JobList>(p => p.Add(c => c.Jobs, SampleJobs()));
        var badges = cut.FindAll(".status-badge");
        Assert.Equal(2, badges.Count);
    }

    [Fact]
    public void Job_row_shows_job_number_and_customer_name()
    {
        var jobs = new List<Job>
        {
            new() { Id = 1, JobNumber = 1001, Customer = new Customer { Name = "Alpha Co" }, JobName = "Alpha Kitchen" }
        };
        var cut = Render<JobList>(p => p.Add(c => c.Jobs, jobs));
        var row = cut.Find(".job-row");

        Assert.Contains("#1001", row.TextContent);
        Assert.Contains("Alpha Co", row.TextContent);
    }
}
