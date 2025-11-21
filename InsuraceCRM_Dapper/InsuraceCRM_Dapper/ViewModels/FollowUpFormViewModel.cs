using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.ViewModels;

public class FollowUpFormViewModel
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime FollowUpDate { get; set; } = DateTime.UtcNow.Date;

    [StringLength(1000)]
    public string? FollowUpNote { get; set; }

    [StringLength(100)]
    public string? FollowUpStatus { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? NextReminderDateTime { get; set; }
}
