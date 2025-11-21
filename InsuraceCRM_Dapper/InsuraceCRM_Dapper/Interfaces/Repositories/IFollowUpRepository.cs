using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Repositories;

public interface IFollowUpRepository
{
    Task<int> InsertAsync(FollowUp followUp);
    Task UpdateAsync(FollowUp followUp);
    Task DeleteAsync(int id);
    Task<FollowUp?> GetByIdAsync(int id);
    Task<IEnumerable<FollowUp>> GetByCustomerIdAsync(int customerId);
}
