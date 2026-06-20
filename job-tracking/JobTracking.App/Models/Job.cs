namespace JobTracking.App.Models;

public class Job
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string JobName { get; set; } = string.Empty;
    public List<Milestone> Milestones { get; set; } = [];

    public string Status
    {
        get
        {
            var lastCompleted = Milestones
                .Where(m => m.IsComplete)
                .MaxBy(m => m.Order);

            return lastCompleted?.Order switch
            {
                1 => "In Design",
                2 => "Awaiting Approval",
                3 => "Approved",
                4 => "In Production",
                5 => "Machined",
                6 => "Finished",
                7 => "Final Assembly",
                8 => "Loaded",
                9 => "Delivered",
                10 => "Billing",
                11 => "Paid",
                12 => "Closed",
                _ => "New"
            };
        }
    }

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
