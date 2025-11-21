using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuraceCRM_Dapper.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private static readonly string[] AllowedRoles = { "Admin", "Manager", "Employee" };
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> ManageRoles()
    {
        var users = await _userService.GetAllUsersAsync();
        var viewModel = new ManageRolesViewModel
        {
            Users = users
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRole(int userId, string role)
    {
        if (!AllowedRoles.Contains(role))
        {
            ModelState.AddModelError(string.Empty, "Invalid role selection.");
            return await ManageRoles();
        }

        await _userService.UpdateRoleAsync(userId, role);
        return RedirectToAction(nameof(ManageRoles));
    }
}
