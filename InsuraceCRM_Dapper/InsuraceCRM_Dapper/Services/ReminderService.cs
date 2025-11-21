using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Services;

public class ReminderService : IReminderService
{
    private readonly IReminderRepository _reminderRepository;

    public ReminderService(IReminderRepository reminderRepository)
    {
        _reminderRepository = reminderRepository;
    }

    public Task<IEnumerable<Reminder>> GetDueRemindersAsync(int employeeId) =>
        _reminderRepository.GetDueRemindersAsync(employeeId);

    public Task MarkReminderAsShownAsync(int reminderId) =>
        _reminderRepository.MarkReminderShownAsync(reminderId);

    public Task<int> CreateReminderAsync(Reminder reminder) =>
        _reminderRepository.InsertAsync(reminder);
}
