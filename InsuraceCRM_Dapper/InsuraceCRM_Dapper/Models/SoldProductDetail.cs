using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.Models;

public class SoldProductDetail
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int FollowUpId { get; set; }

    [Required]
    public int SoldProductId { get; set; }

    [Required]
    [StringLength(200)]
    public string SoldProductName { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal TicketSize { get; set; }

    public int TenureInYears { get; set; }

    [Required]
    [StringLength(100)]
    public string PolicyNumber { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime PolicyEnforceDate { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int CreatedBy { get; set; }
}
