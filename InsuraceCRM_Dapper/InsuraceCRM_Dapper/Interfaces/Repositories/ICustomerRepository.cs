using System.Collections.Generic;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<int> InsertAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(int id);
    Task<Customer?> GetByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<IEnumerable<Customer>> GetCustomersByEmployeeAsync(int employeeId);
    Task AssignCustomerAsync(int customerId, int employeeId);
    Task AssignCustomersAsync(IEnumerable<int> customerIds, int employeeId);
}
