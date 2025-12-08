using System;

namespace InsuraceCRM_Dapper.Models;

public class SoldProductDetailInfo
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerMobileNumber { get; set; } = string.Empty;
    public string? CustomerLocation { get; set; }

    public int FollowUpId { get; set; }
    public DateTime? FollowUpDate { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public string SoldProductName { get; set; } = string.Empty;
    public decimal TicketSize { get; set; }
    public int TenureInYears { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime PolicyEnforceDate { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }

    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
}
