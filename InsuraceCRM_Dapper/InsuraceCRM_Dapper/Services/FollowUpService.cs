using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Services;

public class FollowUpService : IFollowUpService
{
    private readonly IFollowUpRepository _followUpRepository;
    private readonly IReminderRepository _reminderRepository;
    private readonly ISoldProductDetailService _soldProductDetailService;

    public FollowUpService(
        IFollowUpRepository followUpRepository,
        IReminderRepository reminderRepository,
        ISoldProductDetailService soldProductDetailService)
    {
        _followUpRepository = followUpRepository;
        _reminderRepository = reminderRepository;
        _soldProductDetailService = soldProductDetailService;
    }

    public async Task<int> CreateFollowUpAsync(FollowUp followUp, int employeeId)
    {
        var followUpId = await _followUpRepository.InsertAsync(followUp);
        await SyncSoldProductDetailAsync(followUp, followUpId);

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
        await SyncSoldProductDetailAsync(followUp, followUp.Id);
    }

    public Task<FollowUp?> GetFollowUpByIdAsync(int id) =>
        _followUpRepository.GetByIdAsync(id);

    public Task<IEnumerable<FollowUp>> GetFollowUpsForCustomerAsync(int customerId) =>
        _followUpRepository.GetByCustomerIdAsync(customerId);

    public Task<IEnumerable<UserFollowUpDetail>> GetFollowUpsForEmployeeAsync(int employeeId) =>
        _followUpRepository.GetByEmployeeIdAsync(employeeId);

    private async Task SyncSoldProductDetailAsync(FollowUp followUp, int followUpId)
    {
        var hasSaleDetails = followUp.IsConverted == true
            && followUp.SoldProductId.HasValue
            && !string.IsNullOrWhiteSpace(followUp.SoldProductName)
            && followUp.TicketSize.HasValue
            && followUp.TenureInYears.HasValue
            && !string.IsNullOrWhiteSpace(followUp.PolicyNumber)
            && followUp.PolicyEnforceDate.HasValue;

        if (hasSaleDetails)
        {
            var detail = new SoldProductDetail
            {
                CustomerId = followUp.CustomerId,
                FollowUpId = followUpId,
                SoldProductId = followUp.SoldProductId!.Value,
                SoldProductName = followUp.SoldProductName!,
                TicketSize = followUp.TicketSize!.Value,
                TenureInYears = followUp.TenureInYears!.Value,
                PolicyNumber = followUp.PolicyNumber!,
                PolicyEnforceDate = followUp.PolicyEnforceDate!.Value
            };

            await _soldProductDetailService.UpsertAsync(detail);
            return;
        }

        await _soldProductDetailService.DeleteByFollowUpIdAsync(followUpId);
    }
}
