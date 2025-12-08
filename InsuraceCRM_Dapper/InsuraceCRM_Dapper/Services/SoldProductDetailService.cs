using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Services;

public class SoldProductDetailService : ISoldProductDetailService
{
    private readonly ISoldProductDetailRepository _repository;

    public SoldProductDetailService(ISoldProductDetailRepository repository)
    {
        _repository = repository;
    }

    public Task<int> UpsertAsync(SoldProductDetail detail) =>
        _repository.UpsertAsync(detail);

    public Task DeleteByFollowUpIdAsync(int followUpId) =>
        _repository.DeleteByFollowUpIdAsync(followUpId);

    public Task<IEnumerable<SoldProductDetail>> GetByCustomerIdAsync(int customerId) =>
        _repository.GetByCustomerIdAsync(customerId);

    public Task<IEnumerable<SoldProductDetailInfo>> GetAllWithDetailsAsync(int? customerId = null, int? employeeId = null) =>
        _repository.GetAllWithDetailsAsync(customerId, employeeId);
}
