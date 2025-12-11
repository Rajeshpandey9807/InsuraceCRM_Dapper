using InsuraceCRM_Dapper.Interfaces.Repositories;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;
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
        var totalCustomerCountTask = employeeId is null
            ? _dashboardRepository.GetTotalCustomerCountAsync()
            : Task.FromResult(0);

        await Task.WhenAll(remindersTask, callCountTask, assignedCustomerCountTask, totalCustomerCountTask);

        var reminders = MapReminders(await remindersTask);
        var assignedCount = await assignedCustomerCountTask;
        var totalCustomers = employeeId is null ? await totalCustomerCountTask : assignedCount;

        return new DashboardViewModel
        {
            TodaysReminderCount = reminders.Count,
            TodaysCallCount = await callCountTask,
            AssignedCustomerCount = assignedCount,
            TotalCustomerCount = totalCustomers,
            TodaysReminders = reminders
        };
    }

    public async Task<IReadOnlyCollection<ReminderViewModel>> GetTodaysReminderDetailsAsync(int? employeeId)
    {
        var reminders = await _dashboardRepository.GetTodaysRemindersAsync(employeeId);
        return MapReminders(reminders);
    }

    private static IReadOnlyCollection<ReminderViewModel> MapReminders(IEnumerable<Reminder> reminders) =>
        reminders
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
}
