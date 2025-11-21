using System.ComponentModel.DataAnnotations;

namespace InsuraceCRM_Dapper.Models;

public class Reminder
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime ReminderDateTime { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public bool IsShown { get; set; }

    public string? CustomerName { get; set; }
    public string? CustomerMobileNumber { get; set; }
}
