namespace JobTracking.App.Models;

public class UpdateJobRequest
{
    public string? JobName { get; set; }
    public DateTime? LeadDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public decimal? QuoteAmount { get; set; }
}
