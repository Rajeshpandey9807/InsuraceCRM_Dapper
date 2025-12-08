using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InsuraceCRM_Dapper.ViewModels;

public class FollowUpFormViewModel
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    [Display(Name = "Mobile No.")]
    public string? CustomerMobileNumber { get; set; }

    [Display(Name = "Location")]
    public string? CustomerLocation { get; set; }

    [Required]
    [Display(Name = "Insurance Type")]
    public string InsuranceType { get; set; } = string.Empty;

    [Display(Name = "Budget")]
    [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive value.")]
    public decimal? Budget { get; set; }

    [Display(Name = "Any Existing Policy?")]
    public bool HasExistingPolicy { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime FollowUpDate { get; set; } = DateTime.UtcNow.Date;

    [Display(Name = "Call Status")]
    [Required(ErrorMessage = "Select a call status.")]
    [StringLength(100)]
    public string FollowUpStatus { get; set; } = string.Empty;

    [Display(Name = "Notes / What was discussed?")]
    [StringLength(1000)]
    public string? FollowUpNote { get; set; }

    [Display(Name = "Next Follow-Up Date & Time")]
    [DataType(DataType.DateTime)]
    public DateTime? NextReminderDateTime { get; set; }

    [Display(Name = "Reminder Required?")]
    public bool ReminderRequired { get; set; }

    [Display(Name = "Converted?")]
    public bool? IsConverted { get; set; }

    [StringLength(500)]
    [Display(Name = "Reason (if not converted)")]
    public string? ConversionReason { get; set; }

    [Display(Name = "Sold Product")]
    public int? SoldProductId { get; set; }

    [StringLength(200)]
    [Display(Name = "Sold Product")]
    public string? SoldProductName { get; set; }

    [Display(Name = "Ticket Size")]
    [Range(0, double.MaxValue, ErrorMessage = "Ticket size must be a positive value.")]
    public decimal? TicketSize { get; set; }

    [Display(Name = "Tenure (years)")]
    [Range(1, 5, ErrorMessage = "Tenure must be between 1 and 5 years.")]
    public int? TenureInYears { get; set; }

    [StringLength(100)]
    [Display(Name = "Policy Number")]
    public string? PolicyNumber { get; set; }

    [Display(Name = "Policy Enforce Date")]
    [DataType(DataType.Date)]
    public DateTime? PolicyEnforceDate { get; set; }

    public IEnumerable<SelectListItem> ProductOptions { get; set; } = Array.Empty<SelectListItem>();
}
