using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Services;

public class FollowUpService : IFollowUpService
{
    private readonly IFollowUpRepository _followUpRepository;
    private readonly IReminderRepository _reminderRepository;

    public FollowUpService(
        IFollowUpRepository followUpRepository,
        IReminderRepository reminderRepository)
    {
        _followUpRepository = followUpRepository;
        _reminderRepository = reminderRepository;
    }

    public async Task<int> CreateFollowUpAsync(FollowUp followUp, int employeeId)
    {
        var followUpId = await _followUpRepository.InsertAsync(followUp);

        if (followUp.ReminderRequired && followUp.NextReminderDateTime.HasValue)
        {
            var reminder = new Reminder
            {
                CustomerId = followUp.CustomerId,
                EmployeeId = employeeId,
                ReminderDateTime = followUp.NextReminderDateTime.Value,
                Note = followUp.FollowUpNote ?? "Follow-up reminder",
                IsShown = false
            };

            await _reminderRepository.InsertAsync(reminder);
        }

        return followUpId;
    }

    public async Task UpdateFollowUpAsync(FollowUp followUp)
    {
        await _followUpRepository.UpdateAsync(followUp);
    }

    public Task<FollowUp?> GetFollowUpByIdAsync(int id) =>
        _followUpRepository.GetByIdAsync(id);

    public Task<IEnumerable<FollowUp>> GetFollowUpsForCustomerAsync(int customerId) =>
        _followUpRepository.GetByCustomerIdAsync(customerId);
}
