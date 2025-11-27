using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.ViewModels;
using System.Linq;

namespace InsuraceCRM_Dapper.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(int? employeeId)
    {
        var remindersTask = _dashboardRepository.GetTodaysRemindersAsync(employeeId);
        var callCountTask = _dashboardRepository.GetTodaysCallCountAsync(employeeId);
        var assignedCustomerCountTask = _dashboardRepository.GetAssignedCustomerCountAsync(employeeId);

        await Task.WhenAll(remindersTask, callCountTask, assignedCustomerCountTask);

        var reminders = remindersTask.Result
            .Select(r => new ReminderViewModel
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                CustomerName = r.CustomerName ?? $"Customer #{r.CustomerId}",
                ReminderDateTime = r.ReminderDateTime,
                Note = r.Note,
                CustomerMobileNumber = r.CustomerMobileNumber
            })
            .ToList();

        return new DashboardViewModel
        {
            TodaysReminderCount = reminders.Count,
            TodaysCallCount = callCountTask.Result,
            AssignedCustomerCount = assignedCustomerCountTask.Result,
            TodaysReminders = reminders
        };
    }
}
