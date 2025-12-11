using InsuraceCRM_Dapper.Models;

namespace InsuraceCRM_Dapper.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<IEnumerable<Reminder>> GetTodaysRemindersAsync(int? employeeId);
    Task<int> GetTodaysCallCountAsync(int? employeeId);
    Task<int> GetAssignedCustomerCountAsync(int? employeeId);
    Task<int> GetTotalCustomerCountAsync();
}
