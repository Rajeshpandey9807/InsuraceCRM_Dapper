using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.Models;

public class FollowUp
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public DateTime FollowUpDate { get; set; }

    [StringLength(1000)]
    public string? FollowUpNote { get; set; }

    [StringLength(100)]
    public string? FollowUpStatus { get; set; }

    public DateTime? NextReminderDateTime { get; set; }
}
