namespace JobTracking.Api.Models;

public class CreateJobDto
{
    public int CustomerId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public DateTime? LeadDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public decimal? QuoteAmount { get; set; }
}
