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
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public AdminController(IUserService userService, IRoleService roleService)
    {
        _userService = userService;
        _roleService = roleService;
    }

    public async Task<IActionResult> ManageRoles()
    {
        var viewModel = await BuildManageRolesViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRole(int userId, string role)
    {
        if (!await _roleService.ExistsAsync(role))
        {
            TempData["RoleError"] = "Invalid role selection.";
            return RedirectToAction(nameof(ManageRoles));
        }

        await _userService.UpdateRoleAsync(userId, role);
        TempData["RoleMessage"] = "User role updated.";
        return RedirectToAction(nameof(ManageRoles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole([Bind(Prefix = "NewRole")] RoleFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View("ManageRoles", await BuildManageRolesViewModel(form));
        }

        try
        {
            await _roleService.CreateAsync(new Role
            {
                Name = form.Name,
                Description = form.Description
            });

            TempData["RoleMessage"] = $"Role '{form.Name}' was created.";
            return RedirectToAction(nameof(ManageRoles));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError($"{nameof(ManageRolesViewModel.NewRole)}.{nameof(RoleFormViewModel.Name)}", ex.Message);
            return View("ManageRoles", await BuildManageRolesViewModel(form));
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditRole(int id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var viewModel = new RoleFormViewModel
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(RoleFormViewModel form)
    {
        if (form.Id is null)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        try
        {
            await _roleService.UpdateAsync(new Role
            {
                Id = form.Id.Value,
                Name = form.Name,
                Description = form.Description,
                IsSystem = form.IsSystem
            });

            TempData["RoleMessage"] = $"Role '{form.Name}' was updated.";
            return RedirectToAction(nameof(ManageRoles));
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(form);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            await _roleService.DeleteAsync(id);
            TempData["RoleMessage"] = "Role was deleted.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["RoleError"] = ex.Message;
        }

        return RedirectToAction(nameof(ManageRoles));
    }

    public async Task<IActionResult> Users()
    {
        var viewModel = await BuildManageUsersViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser([Bind(Prefix = "NewUser")] UserFormViewModel viewModel)
    {
        if (!await _roleService.ExistsAsync(viewModel.Role))
        {
            ModelState.AddModelError("NewUser.Role", "Invalid role selection.");
        }

        if (!ModelState.IsValid)
        {
            return View("Users", await BuildManageUsersViewModel(viewModel));
        }

        var user = new User
        {
            Name = viewModel.Name,
            Email = viewModel.Email,
            Mobile = viewModel.Mobile,
            Role = viewModel.Role,
            IsActive = true
        };

        await _userService.CreateUserAsync(user, viewModel.Password!);
        TempData["UserMessage"] = $"User '{user.Name}' was created.";
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

        var roles = await GetRoleNamesAsync();
        var viewModel = new EditUserViewModel
        {
            Form = new UserFormViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Mobile = user.Mobile,
                Role = user.Role,
                IsActive = user.IsActive
            },
            Roles = roles
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

        if (!await _roleService.ExistsAsync(viewModel.Form.Role))
        {
            ModelState.AddModelError("Form.Role", "Invalid role selection.");
        }

        if (!ModelState.IsValid)
        {
            viewModel.Roles = await GetRoleNamesAsync();
            return View(viewModel);
        }

        var user = new User
        {
            Id = viewModel.Form.Id.Value,
            Name = viewModel.Form.Name,
            Email = viewModel.Form.Email,
            Mobile = viewModel.Form.Mobile,
            Role = viewModel.Form.Role,
            IsActive = viewModel.Form.IsActive
        };

        var password = string.IsNullOrWhiteSpace(viewModel.Form.Password)
            ? null
            : viewModel.Form.Password;

        await _userService.UpdateUserAsync(user, password);
        TempData["UserMessage"] = $"User '{user.Name}' was updated.";
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
        var usersTask = _userService.GetAllUsersAsync(includeInactive: true);
        var rolesTask = GetRoleNamesAsync();

        await Task.WhenAll(usersTask, rolesTask);

        var users = await usersTask;
        var roleNames = (await rolesTask).ToList();
        var newUser = form ?? new UserFormViewModel();

        if (roleNames.Any())
        {
            var selectedRole = newUser.Role;
            var selectionExists = !string.IsNullOrWhiteSpace(selectedRole) &&
                roleNames.Any(r => string.Equals(r, selectedRole, StringComparison.OrdinalIgnoreCase));

            if (!selectionExists)
            {
                newUser.Role = roleNames.First();
            }
        }
        else
        {
            newUser.Role = string.Empty;
        }

        return new ManageUsersViewModel
        {
            Users = users.OrderBy(u => u.Name),
            NewUser = newUser,
            Roles = roleNames
        };
    }

    private async Task<ManageRolesViewModel> BuildManageRolesViewModel(RoleFormViewModel? newRoleForm = null)
    {
        var usersTask = _userService.GetAllUsersAsync(includeInactive: true);
        var rolesTask = _roleService.GetAllAsync();

        await Task.WhenAll(usersTask, rolesTask);

        var users = (await usersTask).OrderBy(u => u.Name);
        var roles = (await rolesTask).OrderBy(r => r.Name);

        return new ManageRolesViewModel
        {
            Users = users,
            Roles = roles,
            NewRole = newRoleForm ?? new RoleFormViewModel()
        };
    }

    private async Task<IEnumerable<string>> GetRoleNamesAsync()
    {
        var roles = await _roleService.GetAllAsync();
        return roles
            .OrderBy(r => r.Name)
            .Select(r => r.Name)
            .ToList();
    }
}
