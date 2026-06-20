namespace JobTracking.App.Models;

public class Job
{
    public string JobName { get; set; } = string.Empty;
    public List<Milestone> Milestones { get; set; } = [];

    public static Job SampleJob() => new()
    {
        JobName = "Smithers Residence",
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
    };
}
