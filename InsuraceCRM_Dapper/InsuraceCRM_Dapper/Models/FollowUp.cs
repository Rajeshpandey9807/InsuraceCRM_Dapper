using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.Models;

public class FollowUp
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public DateTime FollowUpDate { get; set; }

    [StringLength(100)]
    public string InsuranceType { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal? Budget { get; set; }

    public bool HasExistingPolicy { get; set; }

    [StringLength(1000)]
    public string? FollowUpNote { get; set; }

    [StringLength(100)]
    public string? FollowUpStatus { get; set; }

    public DateTime? NextReminderDateTime { get; set; }

    public bool ReminderRequired { get; set; }

    public bool? IsConverted { get; set; }

    [StringLength(500)]
    public string? ConversionReason { get; set; }

    [StringLength(200)]
    public string? SoldProductName { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? TicketSize { get; set; }

    [StringLength(100)]
    public string? PolicyNumber { get; set; }

    public DateTime? PolicyEnforceDate { get; set; }
}
