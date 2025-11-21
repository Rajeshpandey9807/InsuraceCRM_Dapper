namespace InsuraceCRM_Dapper.ViewModels;

public class ReminderViewModel
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public string? Note { get; set; }
    public string? CustomerMobileNumber { get; set; }
}
