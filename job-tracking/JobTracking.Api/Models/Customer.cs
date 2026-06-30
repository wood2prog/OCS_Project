using System.ComponentModel.DataAnnotations.Schema;

namespace JobTracking.Api.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }

    [NotMapped]
    public int JobCount { get; set; }
}
