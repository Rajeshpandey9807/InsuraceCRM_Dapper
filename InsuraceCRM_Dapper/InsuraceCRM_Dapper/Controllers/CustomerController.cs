using System.Collections.Generic;
using System.Linq;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;
using InsuraceCRM_Dapper.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuraceCRM_Dapper.Controllers;

[Authorize]
public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IUserService _userService;

    public CustomerController(ICustomerService customerService, IUserService userService)
    {
        _customerService = customerService;
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser is null)
        {
            return Challenge();
        }

        var customers = (await _customerService.GetCustomersForUserAsync(currentUser)).ToList();
        var userLookup = (await _userService.GetAllUsersAsync())
            .ToDictionary(u => u.CustomerID, u => u.Name);

        foreach (var customer in customers)
        {
            if (customer.AssignedEmployeeId.HasValue &&
                userLookup.TryGetValue(customer.AssignedEmployeeId.Value, out var employeeName))
            {
                customer.AssignedEmployeeName = employeeName;
            }
        }
        var viewModel = new CustomerListViewModel
        {
            Customers = customers,
            CanEdit = IsManagerOrAdmin(currentUser.Role)
        };
        return View(viewModel);
        //return View();
    }
    public async Task<IEnumerable<Customer>> GetAllCustomers()
    {
        var customer = await _customerService.GetAllCustomersAsync();
        return customer;
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public IActionResult Add()
    {
        return View(new Customer());
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Customer customer)
    {
        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        await _customerService.CreateCustomerAsync(customer);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        return View(customer);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Customer customer)
    {
        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        await _customerService.UpdateCustomerAsync(customer);
        return RedirectToAction(nameof(Index));
    }

    //[Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public async Task<IActionResult> AssignCustomer(int customerId)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customer is null)
        {
            return NotFound();
        }

        var employees = (await _userService.GetAllUsersAsync())
            .Where(u => u.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase));

        var viewModel = new AssignCustomerViewModel
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            AssignedEmployeeId = customer.AssignedEmployeeId,
            Employees = employees
        };

        return View(viewModel);
    }


    //[Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignCustomer(AssignCustomerViewModel viewModel)
    {
        if (viewModel.AssignedEmployeeId is null)
        {
            ModelState.AddModelError(nameof(viewModel.AssignedEmployeeId), "Employee is required.");
        }

        if (!ModelState.IsValid)
        {
            viewModel.Employees = (await _userService.GetAllUsersAsync())
                .Where(u => u.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase));
            return View(viewModel);
        }

        await _customerService.AssignCustomerAsync(viewModel.CustomerId, viewModel.AssignedEmployeeId!.Value);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public async Task<IActionResult> BulkAssign()
    {
        var viewModel = await BuildBulkAssignViewModelAsync();
        return View(viewModel);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkAssign(BulkAssignCustomersViewModel viewModel)
    {
        var selectedCustomerIds = viewModel.SelectedCustomerIds ?? new List<int>();

        if (viewModel.SelectedEmployeeId is null)
        {
            ModelState.AddModelError(nameof(viewModel.SelectedEmployeeId), "Employee is required.");
        }

        if (!selectedCustomerIds.Any())
        {
            ModelState.AddModelError(nameof(viewModel.SelectedCustomerIds), "Select at least one customer to assign.");
        }

        if (!ModelState.IsValid)
        {
            var hydratedViewModel = await BuildBulkAssignViewModelAsync(selectedCustomerIds, viewModel.SelectedEmployeeId);
            return View(hydratedViewModel);
        }

        await _customerService.AssignCustomersAsync(selectedCustomerIds, viewModel.SelectedEmployeeId.Value);

        var employee = await _userService.GetByIdAsync(viewModel.SelectedEmployeeId.Value);
        var employeeName = employee?.Name ?? "selected employee";
        TempData["CustomerSuccess"] = $"{selectedCustomerIds.Count} customer(s) assigned to {employeeName}.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<BulkAssignCustomersViewModel> BuildBulkAssignViewModelAsync(
        IEnumerable<int>? selectedCustomerIds = null,
        int? selectedEmployeeId = null)
    {
        var customers = (await _customerService.GetAllCustomersAsync()).ToList();
        var allUsers = (await _userService.GetAllUsersAsync()).ToList();

        var userLookup = allUsers.ToDictionary(u => u.CustomerID, u => u.Name);
        foreach (var customer in customers)
        {
            if (customer.AssignedEmployeeId.HasValue &&
                userLookup.TryGetValue(customer.AssignedEmployeeId.Value, out var employeeName))
            {
                customer.AssignedEmployeeName = employeeName;
            }
        }

        var employees = allUsers
            .Where(u => u.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            .OrderBy(u => u.Name)
            .ToList();

        var selectedIds = selectedCustomerIds?
            .Where(id => id > 0)
            .Distinct()
            .ToList() ?? new List<int>();

        return new BulkAssignCustomersViewModel
        {
            Customers = customers,
            Employees = employees,
            SelectedCustomerIds = selectedIds,
            SelectedEmployeeId = selectedEmployeeId
        };
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId))
        {
            return await _userService.GetByIdAsync(userId);
        }

        return null;
    }

    private static bool IsManagerOrAdmin(string role) =>
        role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
        role.Equals("Manager", StringComparison.OrdinalIgnoreCase);
}
