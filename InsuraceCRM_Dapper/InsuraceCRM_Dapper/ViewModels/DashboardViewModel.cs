namespace InsuraceCRM_Dapper.ViewModels;

public class DashboardViewModel
{
    public int TodaysReminderCount { get; set; }
    public int TodaysCallCount { get; set; }
    public int AssignedCustomerCount { get; set; }
    public int TotalCustomerCount { get; set; }
    public IReadOnlyCollection<ReminderViewModel> TodaysReminders { get; set; } = Array.Empty<ReminderViewModel>();
}
