namespace JobTracking.Api.Models;

public class ChangeOrder
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public Job Job { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Awaiting Requote";
    public List<Milestone> Milestones { get; set; } = [];
}
