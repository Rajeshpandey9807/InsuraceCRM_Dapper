using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Services;

public interface IReminderService
{
    Task<IEnumerable<Reminder>> GetDueRemindersAsync(int employeeId);
    Task MarkReminderAsShownAsync(int reminderId);
    Task<int> CreateReminderAsync(Reminder reminder);
}
