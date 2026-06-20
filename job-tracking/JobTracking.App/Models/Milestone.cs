namespace JobTracking.App.Models;

public class Milestone
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public int Order { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
}
