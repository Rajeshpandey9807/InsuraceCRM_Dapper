using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;
using InsuraceCRM_Dapper.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InsuraceCRM_Dapper.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly IFollowUpService _followUpService;
    private readonly ISoldProductDetailService _soldProductDetailService;

    public AdminController(
        IUserService userService,
        IFollowUpService followUpService,
        ISoldProductDetailService soldProductDetailService)
    {
        _userService = userService;
        _followUpService = followUpService;
        _soldProductDetailService = soldProductDetailService;
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
        await _userService.UpdateRoleAsync(userId, roleId);
        return RedirectToAction(nameof(ManageRoles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            TempData["RoleError"] = "Role name is required.";
            return RedirectToAction(nameof(ManageRoles));
        }

        var normalizedName = roleName.Trim();
        var roles = await _userService.GetRolesAsync();
        if (roles.Any(r => r.RoleName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            TempData["RoleError"] = $"Role '{normalizedName}' already exists.";
            return RedirectToAction(nameof(ManageRoles));
        }

        await _userService.CreateRoleAsync(new Role { RoleName = normalizedName });
        TempData["RoleMessage"] = $"Role '{normalizedName}' was created.";
        return RedirectToAction(nameof(ManageRoles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(int roleId, string roleName)
    {
        if (roleId <= 0)
        {
            TempData["RoleError"] = "Invalid role identifier.";
            return RedirectToAction(nameof(ManageRoles));
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            TempData["RoleError"] = "Role name is required.";
            return RedirectToAction(nameof(ManageRoles));
        }

        var normalizedName = roleName.Trim();
        var roles = (await _userService.GetRolesAsync()).ToList();

        if (!roles.Any(r => r.RoleId == roleId))
        {
            TempData["RoleError"] = "Role could not be found.";
            return RedirectToAction(nameof(ManageRoles));
        }

        if (roles.Any(r => r.RoleId != roleId && r.RoleName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            TempData["RoleError"] = $"Role '{normalizedName}' already exists.";
            return RedirectToAction(nameof(ManageRoles));
        }

        await _userService.UpdateRoleNameAsync(new Role { RoleId = roleId, RoleName = normalizedName });
        TempData["RoleMessage"] = "Role was updated.";
        return RedirectToAction(nameof(ManageRoles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(int roleId)
    {
        if (roleId <= 0)
        {
            TempData["RoleError"] = "Invalid role identifier.";
            return RedirectToAction(nameof(ManageRoles));
        }

        try
        {
            await _userService.DeleteRoleAsync(roleId);
            TempData["RoleMessage"] = "Role was deleted.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["RoleError"] = ex.Message;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            TempData["RoleError"] = ex.Message;
        }

        return RedirectToAction(nameof(ManageRoles));
    }

    public async Task<IActionResult> Users(bool includeInactive = false)
    {
        var viewModel = await BuildManageUsersViewModel(includeInactive);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser([Bind(Prefix = "NewUser")] UserFormViewModel viewModel, bool includeInactive = false)
    {
        var roles = (await _userService.GetRolesAsync()).ToList();
        var selectedRole = roles.FirstOrDefault(r => r.RoleId == viewModel.RoleId);

        if (selectedRole is null)
        {
            ModelState.AddModelError("NewUser.RoleId", "Please select a valid role.");
        }
        else
        {
            viewModel.Role = selectedRole.RoleName;
            viewModel.RoleId = selectedRole.RoleId;
        }

        if (!ModelState.IsValid)
        {
            return View("Users", await BuildManageUsersViewModel(includeInactive, viewModel));
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
        return RedirectToAction(nameof(Users), new { includeInactive });
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

        var roles = (await _userService.GetRolesAsync()).ToList();
        var selectedRole = roles.FirstOrDefault(r => r.RoleId == viewModel.Form.RoleId);

        if (selectedRole is null)
        {
            ModelState.AddModelError("Form.RoleId", "Please select a valid role.");
        }
        else
        {
            viewModel.Form.Role = selectedRole.RoleName;
            viewModel.Form.RoleId = selectedRole.RoleId;
        }

        if (!ModelState.IsValid)
        {
            viewModel.Roles = roles;
            return View(viewModel);
        }

        var user = new User
        {
            Id = viewModel.Form.Id.Value,
            FullName = viewModel.Form.Name,
            Email = viewModel.Form.Email,
            Mobile = viewModel.Form.Mobile,
            Role = viewModel.Form.Role,
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
    public async Task<IActionResult> ToggleUserStatus(int id, bool activate, bool includeInactive = false)
    {
        await _userService.SetActiveStateAsync(id, activate);
        TempData["UserMessage"] = activate ? "User activated." : "User deactivated.";
        return RedirectToAction(nameof(Users), new { includeInactive });
    }

    [HttpGet]
    public async Task<IActionResult> ExportUsers(string format, bool includeInactive = false)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return BadRequest("Export format is required.");
        }

        var normalizedFormat = format.Trim().ToLowerInvariant();
        var allUsers = (await _userService.GetAllUsersAsync(includeInactive: true)).OrderBy(u => u.FullName).ToList();
        var filteredUsers = includeInactive ? allUsers : allUsers.Where(u => u.IsActive).ToList();

        return normalizedFormat switch
        {
            "excel" => File(GenerateUsersExcel(filteredUsers), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"users-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"),
            "pdf" => File(GenerateUsersPdf(filteredUsers), "application/pdf", $"users-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf"),
            _ => BadRequest("Unsupported export format.")
        };
    }

    private async Task<ManageUsersViewModel> BuildManageUsersViewModel(bool includeInactive, UserFormViewModel? form = null)
    {
        var users = (await _userService.GetAllUsersAsync(includeInactive: true)).OrderBy(u => u.FullName).ToList();
        var filteredUsers = includeInactive ? users : users.Where(u => u.IsActive).ToList();
        var activeCount = users.Count(u => u.IsActive);
        var inactiveCount = users.Count - activeCount;

        return new ManageUsersViewModel
        {
            Users = filteredUsers,
            NewUser = form ?? new UserFormViewModel(),
            Roles = await _userService.GetRolesAsync(),
            IncludeInactive = includeInactive,
            ActiveCount = activeCount,
            InactiveCount = inactiveCount
        };
    }

    private static byte[] GenerateUsersExcel(IReadOnlyCollection<User> users)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Users");

        var headers = new[] { "Name", "Email", "Mobile", "Role", "Status" };
        for (var column = 0; column < headers.Length; column++)
        {
            worksheet.Cell(1, column + 1).Value = headers[column];
            worksheet.Cell(1, column + 1).Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var user in users)
        {
            worksheet.Cell(row, 1).Value = user.FullName;
            worksheet.Cell(row, 2).Value = user.Email;
            worksheet.Cell(row, 3).Value = user.Mobile;
            worksheet.Cell(row, 4).Value = user.Role;
            worksheet.Cell(row, 5).Value = user.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] GenerateUsersPdf(IReadOnlyCollection<User> users)
    {
        byte[] pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header()
                    .Text("Existing Users Report")
                    .FontSize(20)
                    .SemiBold()
                    .FontColor(Colors.Blue.Darken2);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Name").SemiBold();
                        header.Cell().Element(CellStyle).Text("Email").SemiBold();
                        header.Cell().Element(CellStyle).Text("Mobile").SemiBold();
                        header.Cell().Element(CellStyle).Text("Role").SemiBold();
                        header.Cell().Element(CellStyle).Text("Status").SemiBold();
                    });

                    foreach (var user in users)
                    {
                        table.Cell().Element(CellStyle).Text(user.FullName);
                        table.Cell().Element(CellStyle).Text(user.Email);
                        table.Cell().Element(CellStyle).Text(string.IsNullOrWhiteSpace(user.Mobile) ? "-" : user.Mobile);
                        table.Cell().Element(CellStyle).Text(user.Role);
                        table.Cell().Element(CellStyle).Text(user.IsActive ? "Active" : "Inactive");
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text($"Generated on {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf();

        return pdfBytes;

        static IContainer CellStyle(IContainer container) =>
            container.PaddingVertical(4).PaddingHorizontal(2);
    }

    public async Task<IActionResult> UserInsights(int? userId)
    {
        var users = (await _userService.GetAllUsersAsync())
            .OrderBy(u => u.FullName)
            .ToList();

        var selectedUser = userId.HasValue
            ? users.FirstOrDefault(u => u.Id == userId)
            : null;

        var followUps = selectedUser is null
            ? Enumerable.Empty<UserFollowUpDetail>()
            : await _followUpService.GetFollowUpsForEmployeeAsync(selectedUser.Id);

        var soldProducts = selectedUser is null
            ? Enumerable.Empty<SoldProductDetailInfo>()
            : await _soldProductDetailService.GetAllWithDetailsAsync(employeeId: selectedUser.Id);

        var viewModel = new UserInsightsViewModel
        {
            Users = users,
            SelectedUserId = userId,
            SelectedUser = selectedUser,
            FollowUps = followUps,
            SoldProducts = soldProducts
        };

        return View(viewModel);
    }
}
