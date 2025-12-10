using System.ComponentModel.DataAnnotations;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.ViewModels;

public class CustomerInputModel
{
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

    public Customer ToCustomer(int createdBy) =>
        new()
        {
            Name = Name?.Trim() ?? string.Empty,
            Email = Email?.Trim() ?? string.Empty,
            MobileNumber = MobileNumber?.Trim() ?? string.Empty,
            Location = Location?.Trim() ?? string.Empty,
            InsuranceType = string.IsNullOrWhiteSpace(InsuranceType) ? null : InsuranceType.Trim(),
            Income = Income,
            SourceOfIncome = string.IsNullOrWhiteSpace(SourceOfIncome) ? null : SourceOfIncome.Trim(),
            CreatedBy = createdBy
        };
}
