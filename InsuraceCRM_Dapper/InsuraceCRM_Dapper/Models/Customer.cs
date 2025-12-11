using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string MobileNumber { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [StringLength(100)]
    public string? InsuranceType { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Income must be a positive value.")]
    public decimal? Income { get; set; }

    [StringLength(150)]
    public string? SourceOfIncome { get; set; }

    [Range(0, 50, ErrorMessage = "Family members must be between 0 and 50.")]
    public int? FamilyMembers { get; set; }

    public int? AssignedEmployeeId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int CreatedBy { get; set; }

    public string? AssignedEmployeeName { get; set; }
}
