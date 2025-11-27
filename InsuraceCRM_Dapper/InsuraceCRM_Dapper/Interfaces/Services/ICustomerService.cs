using System.Collections.Generic;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Services;

public interface ICustomerService
{
    Task<int> CreateCustomerAsync(Customer customer);
    Task UpdateCustomerAsync(Customer customer);
    Task DeleteCustomerAsync(int id);
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<IEnumerable<Customer>> GetCustomersForUserAsync(User user);
    Task AssignCustomerAsync(int customerId, int employeeId);
    Task AssignCustomersAsync(IEnumerable<int> customerIds, int employeeId);
}
