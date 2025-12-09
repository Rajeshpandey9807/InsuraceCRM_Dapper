using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;
using InsuraceCRM_Dapper.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Security.Claims;

namespace InsuraceCRM_Dapper.Controllers;

[Authorize]
public class FollowUpController : Controller
{
    private readonly IFollowUpService _followUpService;
    private readonly ICustomerService _customerService;
    private readonly IUserService _userService;
    private readonly IProductService _productService;

    public FollowUpController(
        IFollowUpService followUpService,
        ICustomerService customerService,
        IUserService userService,
        IProductService productService)
    {
        _followUpService = followUpService;
        _customerService = customerService;
        _userService = userService;
        _productService = productService;
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

        await PopulateProductOptionsAsync(viewModel);
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

        await ValidateAndAssignProductAsync(viewModel);
        ValidateConversionDetails(viewModel);

        if (!ModelState.IsValid)
        {
            viewModel.CustomerName = customer.Name;
            viewModel.CustomerMobileNumber = customer.MobileNumber;
            viewModel.CustomerLocation = customer.Location;
            await PopulateProductOptionsAsync(viewModel);
            return View(viewModel);
        }

        var followUp = MapFollowUp(viewModel);
        followUp.CreatedBy = currentUser.Id;
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
        await PopulateProductOptionsAsync(viewModel);
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

        var existingFollowUp = await _followUpService.GetFollowUpByIdAsync(viewModel.Id);
        if (existingFollowUp is null)
        {
            return NotFound();
        }
        if (existingFollowUp.CustomerId != viewModel.CustomerId)
        {
            return BadRequest();
        }

        await ValidateAndAssignProductAsync(viewModel);
        ValidateConversionDetails(viewModel);

        if (!ModelState.IsValid)
        {
            viewModel.CustomerName = customer.Name;
            viewModel.CustomerMobileNumber = customer.MobileNumber;
            viewModel.CustomerLocation = customer.Location;
            await PopulateProductOptionsAsync(viewModel);
            return View(viewModel);
        }

        var followUp = MapFollowUp(viewModel);
        followUp.CreatedBy = existingFollowUp.CreatedBy == 0
            ? currentUser.Id
            : existingFollowUp.CreatedBy;
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
        SoldProductId = viewModel.IsConverted == true ? viewModel.SoldProductId : null,
        SoldProductName = viewModel.IsConverted == true ? viewModel.SoldProductName : null,
        TicketSize = viewModel.IsConverted == true ? viewModel.TicketSize : null,
        TenureInYears = viewModel.IsConverted == true ? viewModel.TenureInYears : null,
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
        SoldProductId = followUp.SoldProductId,
        SoldProductName = followUp.SoldProductName,
        TicketSize = followUp.TicketSize,
        TenureInYears = followUp.TenureInYears,
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

        if (!viewModel.TicketSize.HasValue || viewModel.TicketSize <= 0)
        {
            ModelState.AddModelError(nameof(viewModel.TicketSize), "Ticket size must be greater than zero for converted deals.");
        }

        if (!viewModel.TenureInYears.HasValue)
        {
            ModelState.AddModelError(nameof(viewModel.TenureInYears), "Select the policy tenure in years.");
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

    private async Task PopulateProductOptionsAsync(FollowUpFormViewModel viewModel)
    {
        var products = await _productService.GetAllAsync() ?? Enumerable.Empty<Product>();
        var selectedId = viewModel.SoldProductId?.ToString();
        viewModel.ProductOptions = products
            .OrderBy(p => p.Name)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name,
                Selected = selectedId == p.Id.ToString()
            })
            .ToList();

        if (viewModel.SoldProductId.HasValue && string.IsNullOrWhiteSpace(viewModel.SoldProductName))
        {
            var selected = viewModel.ProductOptions.FirstOrDefault(option => option.Value == selectedId);
            viewModel.SoldProductName = selected?.Text ?? viewModel.SoldProductName;
        }
    }

    private async Task ValidateAndAssignProductAsync(FollowUpFormViewModel viewModel)
    {
        if (viewModel.IsConverted != true)
        {
            return;
        }

        if (!viewModel.SoldProductId.HasValue)
        {
            ModelState.AddModelError(nameof(viewModel.SoldProductId), "Select the product that was sold.");
            return;
        }

        var product = await _productService.GetByIdAsync(viewModel.SoldProductId.Value);
        if (product is null)
        {
            ModelState.AddModelError(nameof(viewModel.SoldProductId), "Selected product could not be found.");
            return;
        }

        viewModel.SoldProductName = product.Name;
    }
}
