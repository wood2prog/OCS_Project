namespace JobTracking.Api.Models;

public class UpdateCustomerDto
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Notes { get; init; }
}
