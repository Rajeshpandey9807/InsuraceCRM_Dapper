using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuraceCRM_Dapper.Controllers;

[Authorize]
[Route("[controller]/[action]")]
public class ReminderController : Controller
{
    private readonly IReminderService _reminderService;

    public ReminderController(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDueReminders()
    {
        var employeeId = GetCurrentUserId();
        if (employeeId is null)
        {
            return Unauthorized();
        }

        var reminders = await _reminderService.GetDueRemindersAsync(employeeId.Value);
        var payload = reminders.Select(r => new ReminderViewModel
        {
            Id = r.Id,
            CustomerId = r.CustomerId,
            CustomerName = r.CustomerName ?? $"Customer #{r.CustomerId}",
            ReminderDateTime = r.ReminderDateTime,
            Note = r.Note,
            CustomerMobileNumber = r.CustomerMobileNumber
        });

        return Json(payload);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsShown(int reminderId)
    {
        var employeeId = GetCurrentUserId();
        if (employeeId is null)
        {
            return Unauthorized();
        }

        await _reminderService.MarkReminderAsShownAsync(reminderId);
        return Ok();
    }

    private int? GetCurrentUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var userId) ? userId : null;
    }
}
