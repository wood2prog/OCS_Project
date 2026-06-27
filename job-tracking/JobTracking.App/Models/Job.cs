namespace JobTracking.App.Models;

public class Job
{
    public int Id { get; set; }
    public int JobNumber { get; set; }
    public Customer? Customer { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string CustomerName => Customer?.Name ?? string.Empty;
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
}
