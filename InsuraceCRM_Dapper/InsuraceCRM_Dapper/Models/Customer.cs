using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, Phone]
    public string MobileNumber { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Location { get; set; }

    [StringLength(100)]
    public string? InsuranceType { get; set; }

    public int? AssignedEmployeeId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public string? AssignedEmployeeName { get; set; }
}
