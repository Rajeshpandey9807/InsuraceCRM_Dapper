using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Services;

public interface ISoldProductDetailService
{
    Task<int> UpsertAsync(SoldProductDetail detail);
    Task DeleteByFollowUpIdAsync(int followUpId);
    Task<IEnumerable<SoldProductDetail>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<SoldProductDetailInfo>> GetAllWithDetailsAsync();
}
