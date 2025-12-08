using System.Linq;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;
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
        var users = await _userService.GetAllUsersAsync(includeInactive: true);
        var viewModel = new ManageRolesViewModel
        {
            Users = users,
            Roles = await _userService.GetRolesAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRole(int userId, int roleId)
    {
        //if (!AllowedRoles.Contains(role))
        //{
        //    ModelState.AddModelError(string.Empty, "Invalid role selection.");
        //    return await ManageRoles();
        //}
      

        await _userService.UpdateRoleAsync(userId, roleId);
        return RedirectToAction(nameof(ManageRoles));
    }

    public async Task<IActionResult> Users()
    {
        var Roles = await _userService.GetRolesAsync();

        var viewModel = await BuildManageUsersViewModel();
        viewModel.Role = Roles;
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser([Bind(Prefix = "NewUser")] UserFormViewModel viewModel)
    {

        if (!AllowedRoles.Contains(viewModel.Role))
        {
            ModelState.AddModelError("NewUser.Role", "Invalid role selection.");
        }

        if (!ModelState.IsValid)
        {
            return View("Users", await BuildManageUsersViewModel(viewModel));
        }

        var user = new User
        {
            FullName = viewModel.Name,
            Email = viewModel.Email,
            Mobile = viewModel.Mobile,
            Role = viewModel.Role,
            RoleId = viewModel.RoleId,
            IsActive = true
        };

        await _userService.CreateUserAsync(user, viewModel.Password!);
        TempData["UserMessage"] = $"User '{user.FullName}' was created.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {


        var user = await _userService.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var viewModel = new EditUserViewModel
        {
            Form = new UserFormViewModel
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                Role = user.Role,
                RoleId = user.RoleId,
                IsActive = user.IsActive
            },
            Roles = await _userService.GetRolesAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(EditUserViewModel viewModel)
    {
        if (viewModel.Form.Id is null)
        {
            return BadRequest();
        }

        if (!AllowedRoles.Contains(viewModel.Form.Role))
        {
            ModelState.AddModelError("Form.Role", "Invalid role selection.");
        }

        if (!ModelState.IsValid)
        {
            viewModel.Roles = await _userService.GetRolesAsync();
            return View(viewModel);
        }

        var user = new User
        {
            Id = viewModel.Form.Id.Value,
            FullName = viewModel.Form.Name,
            Email = viewModel.Form.Email,
            Mobile = viewModel.Form.Mobile,
            //Role = viewModel.Form.Role,
            IsActive = viewModel.Form.IsActive,
            RoleId = viewModel.Form.RoleId
        };

        var password = string.IsNullOrWhiteSpace(viewModel.Form.Password)
            ? null
            : viewModel.Form.Password;

        await _userService.UpdateUserAsync(user, password);
        TempData["UserMessage"] = $"User '{user.FullName}' was updated.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(int id, bool activate)
    {
        await _userService.SetActiveStateAsync(id, activate);
        TempData["UserMessage"] = activate ? "User activated." : "User deactivated.";
        return RedirectToAction(nameof(Users));
    }

    private async Task<ManageUsersViewModel> BuildManageUsersViewModel(UserFormViewModel? form = null)
    {
        var users = await _userService.GetAllUsersAsync(includeInactive: true);
        return new ManageUsersViewModel
        {
            Users = users.OrderBy(u => u.FullName),
            NewUser = form ?? new UserFormViewModel(),
            Roles = AllowedRoles
        };
    }
}
