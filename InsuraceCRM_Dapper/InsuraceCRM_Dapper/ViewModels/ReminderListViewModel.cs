namespace InsuraceCRM_Dapper.ViewModels;

public class ReminderListViewModel
{
    public string Title { get; set; } = "Today's Reminders";
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public IReadOnlyCollection<ReminderViewModel> Reminders { get; set; } = Array.Empty<ReminderViewModel>();

    public int Count => Reminders.Count;
}
