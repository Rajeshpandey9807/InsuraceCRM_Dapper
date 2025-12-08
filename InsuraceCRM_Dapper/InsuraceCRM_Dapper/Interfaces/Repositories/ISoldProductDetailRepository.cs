using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Repositories;

public interface ISoldProductDetailRepository
{
    Task<int> UpsertAsync(SoldProductDetail detail);
    Task DeleteByFollowUpIdAsync(int followUpId);
    Task<IEnumerable<SoldProductDetail>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<SoldProductDetailInfo>> GetAllWithDetailsAsync(int? customerId = null, int? employeeId = null);
}
