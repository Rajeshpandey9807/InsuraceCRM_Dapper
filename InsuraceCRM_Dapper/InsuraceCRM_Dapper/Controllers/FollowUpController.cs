using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;
using InsuraceCRM_Dapper.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuraceCRM_Dapper.Controllers;

[Authorize]
public class FollowUpController : Controller
{
    private readonly IFollowUpService _followUpService;
    private readonly ICustomerService _customerService;
    private readonly IUserService _userService;

    public FollowUpController(
        IFollowUpService followUpService,
        ICustomerService customerService,
        IUserService userService)
    {
        _followUpService = followUpService;
        _customerService = customerService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Add(int customerId)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customer is null)
        {
            return NotFound();
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser is null || !CanAccessCustomer(customer, currentUser))
        {
            return Forbid();
        }

        var viewModel = new FollowUpFormViewModel
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerMobileNumber = customer.MobileNumber,
            CustomerLocation = customer.Location,
            InsuranceType = customer.InsuranceType ?? string.Empty,
            FollowUpDate = DateTime.UtcNow.Date
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(FollowUpFormViewModel viewModel)
    {
        var customer = await _customerService.GetCustomerByIdAsync(viewModel.CustomerId);
        if (customer is null)
        {
            return NotFound();
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser is null || !CanAccessCustomer(customer, currentUser))
        {
            return Forbid();
        }

        ValidateConversionDetails(viewModel);

        if (!ModelState.IsValid)
        {
            viewModel.CustomerName = customer.Name;
            viewModel.CustomerMobileNumber = customer.MobileNumber;
            viewModel.CustomerLocation = customer.Location;
            return View(viewModel);
        }

        var followUp = MapFollowUp(viewModel);
        var employeeId = customer.AssignedEmployeeId;
        if (employeeId is null)
        {
            ModelState.AddModelError(string.Empty, "Customer must be assigned to an employee before recording follow-ups.");
            viewModel.CustomerName = customer.Name;
            return View(viewModel);
        }

        await _followUpService.CreateFollowUpAsync(followUp, employeeId.Value);
        return RedirectToAction(nameof(History), new { customerId = viewModel.CustomerId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var followUp = await _followUpService.GetFollowUpByIdAsync(id);
        if (followUp is null)
        {
            return NotFound();
        }

        var customer = await _customerService.GetCustomerByIdAsync(followUp.CustomerId);
        if (customer is null)
        {
            return NotFound();
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser is null || !CanAccessCustomer(customer, currentUser))
        {
            return Forbid();
        }

        var viewModel = MapToViewModel(followUp, customer);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(FollowUpFormViewModel viewModel)
    {
        var customer = await _customerService.GetCustomerByIdAsync(viewModel.CustomerId);
        if (customer is null)
        {
            return NotFound();
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser is null || !CanAccessCustomer(customer, currentUser))
        {
            return Forbid();
        }

        ValidateConversionDetails(viewModel);

        if (!ModelState.IsValid)
        {
            viewModel.CustomerName = customer.Name;
            viewModel.CustomerMobileNumber = customer.MobileNumber;
            viewModel.CustomerLocation = customer.Location;
            return View(viewModel);
        }

        var followUp = MapFollowUp(viewModel);
        await _followUpService.UpdateFollowUpAsync(followUp);
        return RedirectToAction(nameof(History), new { customerId = viewModel.CustomerId });
    }

    [HttpGet]
    public async Task<IActionResult> History(int customerId)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customer is null)
        {
            return NotFound();
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser is null || !CanAccessCustomer(customer, currentUser))
        {
            return Forbid();
        }

        var followUps = await _followUpService.GetFollowUpsForCustomerAsync(customerId);
        var viewModel = new FollowUpHistoryViewModel
        {
            Customer = customer,
            FollowUps = followUps
        };

        return View(viewModel);
    }

    private static FollowUp MapFollowUp(FollowUpFormViewModel viewModel) => new()
    {
        Id = viewModel.Id,
        CustomerId = viewModel.CustomerId,
        FollowUpDate = viewModel.FollowUpDate,
        InsuranceType = viewModel.InsuranceType,
        Budget = viewModel.Budget,
        HasExistingPolicy = viewModel.HasExistingPolicy,
        FollowUpNote = viewModel.FollowUpNote,
        FollowUpStatus = viewModel.FollowUpStatus,
        NextReminderDateTime = viewModel.ReminderRequired ? viewModel.NextReminderDateTime : null,
        ReminderRequired = viewModel.ReminderRequired,
        IsConverted = viewModel.IsConverted,
        ConversionReason = viewModel.ConversionReason,
        SoldProductName = viewModel.IsConverted == true ? viewModel.SoldProductName : null,
        TicketSize = viewModel.IsConverted == true ? viewModel.TicketSize : null,
        PolicyNumber = viewModel.IsConverted == true ? viewModel.PolicyNumber : null,
        PolicyEnforceDate = viewModel.IsConverted == true ? viewModel.PolicyEnforceDate : null
    };

    private static FollowUpFormViewModel MapToViewModel(FollowUp followUp, Customer customer) => new()
    {
        Id = followUp.Id,
        CustomerId = followUp.CustomerId,
        CustomerName = customer.Name,
        CustomerMobileNumber = customer.MobileNumber,
        CustomerLocation = customer.Location,
        FollowUpDate = followUp.FollowUpDate,
        InsuranceType = followUp.InsuranceType ?? string.Empty,
        Budget = followUp.Budget,
        HasExistingPolicy = followUp.HasExistingPolicy,
        FollowUpStatus = followUp.FollowUpStatus ?? string.Empty,
        FollowUpNote = followUp.FollowUpNote,
        NextReminderDateTime = followUp.NextReminderDateTime,
        ReminderRequired = followUp.ReminderRequired,
        IsConverted = followUp.IsConverted,
        ConversionReason = followUp.ConversionReason,
        SoldProductName = followUp.SoldProductName,
        TicketSize = followUp.TicketSize,
        PolicyNumber = followUp.PolicyNumber,
        PolicyEnforceDate = followUp.PolicyEnforceDate
    };

    private async Task<User?> GetCurrentUserAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId))
        {
            return await _userService.GetByIdAsync(userId);
        }

        return null;
    }

    private static bool CanAccessCustomer(Customer customer, User user)
    {
        if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
            user.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return customer.AssignedEmployeeId == user.Id;
    }

    private void ValidateConversionDetails(FollowUpFormViewModel viewModel)
    {
        if (viewModel.IsConverted != true)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(viewModel.SoldProductName))
        {
            ModelState.AddModelError(nameof(viewModel.SoldProductName), "Please specify the product that was sold.");
        }

        if (!viewModel.TicketSize.HasValue || viewModel.TicketSize <= 0)
        {
            ModelState.AddModelError(nameof(viewModel.TicketSize), "Ticket size must be greater than zero for converted deals.");
        }

        if (string.IsNullOrWhiteSpace(viewModel.PolicyNumber))
        {
            ModelState.AddModelError(nameof(viewModel.PolicyNumber), "Policy number is required for converted deals.");
        }

        if (!viewModel.PolicyEnforceDate.HasValue)
        {
            ModelState.AddModelError(nameof(viewModel.PolicyEnforceDate), "Provide the policy enforce date to complete the sale details.");
        }
    }
}
