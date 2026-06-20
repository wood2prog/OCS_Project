using Bunit;
using JobTracking.App.Components;
using JobTracking.App.Models;

namespace JobTracking.Tests;

public class JobDetailTest : BunitContext
{
    [Fact]
    public void Shows_empty_state_when_no_job()
    {
        var cut = Render<JobDetail>();
        Assert.Contains("Select a job to view details", cut.Find("p").TextContent);
    }

    [Fact]
    public void Renders_milestone_checklist_when_job_provided()
    {
        var job = Job.SampleJob();
        var cut = Render<JobDetail>(p => p.Add(c => c.Job, job));

        Assert.Contains("Smithers Residence", cut.Find("h2").TextContent);
    }
}
