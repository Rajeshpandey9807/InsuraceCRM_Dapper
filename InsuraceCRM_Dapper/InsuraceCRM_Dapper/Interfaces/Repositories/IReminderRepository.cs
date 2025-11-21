using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Repositories;

public interface IReminderRepository
{
    Task<int> InsertAsync(Reminder reminder);
    Task UpdateAsync(Reminder reminder);
    Task DeleteAsync(int id);
    Task<Reminder?> GetByIdAsync(int id);
    Task<IEnumerable<Reminder>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Reminder>> GetDueRemindersAsync(int employeeId);
    Task MarkReminderShownAsync(int reminderId);
}
