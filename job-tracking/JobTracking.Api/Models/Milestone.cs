namespace JobTracking.Api.Models;

public class Milestone
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public Job Job { get; set; } = null!;
    public int? ChangeOrderId { get; set; }
    public ChangeOrder? ChangeOrder { get; set; }
    public int Order { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? CompletedBy { get; set; }
    public DateTime? CompletedAt { get; set; }
}
