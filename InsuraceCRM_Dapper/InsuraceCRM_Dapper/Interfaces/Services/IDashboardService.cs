using InsuraceCRM_Dapper.ViewModels;

namespace InsuraceCRM_Dapper.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(int? employeeId);
    Task<IReadOnlyCollection<ReminderViewModel>> GetTodaysReminderDetailsAsync(int? employeeId);
}
