using System;

namespace InsuraceCRM_Dapper.Models;

public class UserFollowUpDetail
{
    public int FollowUpId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerMobileNumber { get; set; } = string.Empty;
    public string? CustomerLocation { get; set; }
    public DateTime FollowUpDate { get; set; }
    public string? InsuranceType { get; set; }
    public decimal? Budget { get; set; }
    public bool HasExistingPolicy { get; set; }
    public string? FollowUpStatus { get; set; }
    public string? FollowUpNote { get; set; }
    public DateTime? NextReminderDateTime { get; set; }
    public bool ReminderRequired { get; set; }
    public bool? IsConverted { get; set; }
    public string? SoldProductName { get; set; }
    public decimal? TicketSize { get; set; }
    public int? TenureInYears { get; set; }
    public string? PolicyNumber { get; set; }
    public DateTime? PolicyEnforceDate { get; set; }
}
