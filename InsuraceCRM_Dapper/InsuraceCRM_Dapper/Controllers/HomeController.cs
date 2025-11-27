using System.Diagnostics;
using System.Security.Claims;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuraceCRM_Dapper.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDashboardService _dashboardService;

    public HomeController(ILogger<HomeController> logger, IDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var hasFullAccess = User.IsInRole("Admin") || User.IsInRole("Manager");

        if (!hasFullAccess && userId is null)
        {
            return Forbid();
        }

        var dashboard = await _dashboardService.GetDashboardAsync(hasFullAccess ? null : userId);
        ViewData["Title"] = "Dashboard";

        return View(dashboard);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private int? GetCurrentUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var userId) ? userId : null;
    }
}
