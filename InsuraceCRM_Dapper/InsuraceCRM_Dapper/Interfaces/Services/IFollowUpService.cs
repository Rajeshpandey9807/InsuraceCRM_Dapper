using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Services;

public interface IFollowUpService
{
    Task<int> CreateFollowUpAsync(FollowUp followUp, int employeeId);
    Task UpdateFollowUpAsync(FollowUp followUp);
    Task<FollowUp?> GetFollowUpByIdAsync(int id);
    Task<IEnumerable<FollowUp>> GetFollowUpsForCustomerAsync(int customerId);
}
