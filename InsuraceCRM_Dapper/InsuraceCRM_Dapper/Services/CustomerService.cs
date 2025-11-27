using System.Collections.Generic;
using System.Linq;
using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<int> CreateCustomerAsync(Customer customer)
    {
        customer.CreatedDate = DateTime.UtcNow;
        return await _customerRepository.InsertAsync(customer);
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        await _customerRepository.UpdateAsync(customer);
    }

    public async Task DeleteCustomerAsync(int id)
    {
        await _customerRepository.DeleteAsync(id);
    }

    public Task<Customer?> GetCustomerByIdAsync(int id) =>
        _customerRepository.GetByIdAsync(id);

    public Task<IEnumerable<Customer>> GetAllCustomersAsync() =>
        _customerRepository.GetAllAsync();

    public async Task<IEnumerable<Customer>> GetCustomersForUserAsync(User user)
    {
        if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
            user.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            return await _customerRepository.GetAllAsync();
        }

        if (user.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase) && user.CustomerID > 0)
        {
            return await _customerRepository.GetCustomersByEmployeeAsync(user.CustomerID);
        }

        return Enumerable.Empty<Customer>();
    }

    public async Task AssignCustomerAsync(int customerId, int employeeId)
    {
        await _customerRepository.AssignCustomerAsync(customerId, employeeId);
    }

    public async Task AssignCustomersAsync(IEnumerable<int> customerIds, int employeeId)
    {
        if (customerIds is null)
        {
            return;
        }

        var ids = customerIds
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return;
        }

        await _customerRepository.AssignCustomersAsync(ids, employeeId);
    }
}
