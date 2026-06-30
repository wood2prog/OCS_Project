using System.ComponentModel.DataAnnotations;

namespace JobTracking.Api.Models;

public class CreateCustomerDto
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Notes { get; init; }
}
