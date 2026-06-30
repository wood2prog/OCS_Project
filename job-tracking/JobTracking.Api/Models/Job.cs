namespace JobTracking.Api.Models;

public class Job
{
    public int Id { get; set; }
    public int JobNumber { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string JobName { get; set; } = string.Empty;
    public DateTime? LeadDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public decimal? QuoteAmount { get; set; }
    public decimal? ChangeOrderTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Milestone> Milestones { get; set; } = [];
    public List<ChangeOrder> ChangeOrders { get; set; } = [];
    public List<Document> Documents { get; set; } = [];

    public string Status
    {
        get
        {
            var lastCompleted = Milestones
                .Where(m => m.CompletedAt != null && m.ChangeOrderId == null)
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
