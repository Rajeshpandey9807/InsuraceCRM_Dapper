using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;
using InsuraceCRM_Dapper.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CsvHelper;
using ExcelDataReader;
using ExcelDataReader.DataSet;

namespace InsuraceCRM_Dapper.Controllers;

[Authorize]
public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IUserService _userService;
    private static readonly string[] RequiredImportColumns = new[] { "Name", "MobileNumber", "Location" };

    public CustomerController(ICustomerService customerService, IUserService userService)
    {
        _customerService = customerService;
        _userService = userService;
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpload(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError("BulkUpload", "Please select a CSV or Excel file to upload.");
        }
        else
        {
            try
            {
                var importResult = ParseCustomersFromFile(file);

                if (importResult.Customers.Count > 0)
                {
                    foreach (var customer in importResult.Customers)
                    {
                        await _customerService.CreateCustomerAsync(customer);
                    }

                    var successMessage = $"{importResult.Customers.Count} customer(s) uploaded successfully.";
                    if (importResult.Errors.Count > 0)
                    {
                        successMessage += $" {importResult.Errors.Count} row(s) skipped.";
                        TempData["CustomerImportErrors"] = string.Join('\n', importResult.Errors);
                    }

                    TempData["CustomerSuccess"] = successMessage;
                    return RedirectToAction(nameof(Index));
                }

                if (importResult.Errors.Count > 0)
                {
                    ModelState.AddModelError("BulkUpload", "Unable to import any rows. Review the errors below.");
                    ViewData["BulkUploadErrors"] = importResult.Errors;
                }
                else
                {
                    ModelState.AddModelError("BulkUpload", "No valid data rows were found in the file.");
                }
            }
            catch (CustomerImportException ex)
            {
                ModelState.AddModelError("BulkUpload", ex.Message);
            }
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser is null)
        {
            return Challenge();
        }

        var viewModel = await BuildCustomerListViewModelAsync(currentUser);
        return View("Index", viewModel);
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser is null)
        {
            return Challenge();
        }

        var viewModel = await BuildCustomerListViewModelAsync(currentUser);
        return View(viewModel);
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInline(CustomerInputModel inputModel)
    {
        if (!ModelState.IsValid)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser is null)
            {
                return Challenge();
            }

            var viewModel = await BuildCustomerListViewModelAsync(currentUser, inputModel);
            return View("Index", viewModel);
        }

        await _customerService.CreateCustomerAsync(inputModel.ToCustomer());
        TempData["CustomerSuccess"] = "Customer added successfully.";
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

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _customerService.DeleteCustomerAsync(id);
        TempData["CustomerSuccess"] = "Customer deleted successfully.";
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
        var employeeName = employee?.FullName ?? "selected employee";
        TempData["CustomerSuccess"] = $"{selectedCustomerIds.Count} customer(s) assigned to {employeeName}.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<CustomerListViewModel> BuildCustomerListViewModelAsync(
        User currentUser,
        CustomerInputModel? newCustomer = null)
    {
        var customers = (await _customerService.GetCustomersForUserAsync(currentUser)).ToList();
        var userLookup = (await _userService.GetAllUsersAsync(includeInactive: true))
            .ToDictionary(u => u.Id, u => u.FullName);

        foreach (var customer in customers)
        {
            if (customer.AssignedEmployeeId.HasValue &&
                userLookup.TryGetValue(customer.AssignedEmployeeId.Value, out var employeeName))
            {
                customer.AssignedEmployeeName = employeeName;
            }
        }

        return new CustomerListViewModel
        {
            Customers = customers,
            CanEdit = IsManagerOrAdmin(currentUser.Role),
            NewCustomer = newCustomer ?? new CustomerInputModel()
        };
    }

    private async Task<BulkAssignCustomersViewModel> BuildBulkAssignViewModelAsync(
        IEnumerable<int>? selectedCustomerIds = null,
        int? selectedEmployeeId = null)
    {
        var customers = (await _customerService.GetAllCustomersAsync()).ToList();
        var allUsers = (await _userService.GetAllUsersAsync(includeInactive: true)).ToList();

        var userLookup = allUsers.ToDictionary(u => u.Id, u => u.FullName);
        foreach (var customer in customers)
        {
            if (customer.AssignedEmployeeId.HasValue &&
                userLookup.TryGetValue(customer.AssignedEmployeeId.Value, out var employeeName))
            {
                customer.AssignedEmployeeName = employeeName;
            }
        }

        var employees = allUsers
            .Where(u => u.IsActive && u.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            .OrderBy(u => u.FullName)
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

    private ImportResult ParseCustomersFromFile(IFormFile file)
    {
        try
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return extension switch
            {
                ".csv" => ParseCsvFile(file),
                ".xlsx" or ".xls" => ParseExcelFile(file),
                _ => throw new CustomerImportException("Unsupported file format. Please upload a CSV or Excel file.")
            };
        }
        catch (CustomerImportException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CustomerImportException($"Unable to process the uploaded file. {ex.Message}");
        }
    }

    private ImportResult ParseCsvFile(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        if (!csv.Read())
        {
            throw new CustomerImportException("The uploaded file is empty.");
        }

        csv.ReadHeader();
        var headerRecord = csv.HeaderRecord;
        if (headerRecord is null)
        {
            throw new CustomerImportException("Unable to read the header row in the uploaded file.");
        }

        var headerAliases = BuildHeaderAliases(headerRecord);

        var result = new ImportResult();
        var rowNumber = 1;

        while (csv.Read())
        {
            rowNumber++;
            var rowValues = ExtractRowValues(csv, headerAliases);
            if (IsRowEmpty(rowValues))
            {
                continue;
            }

            TryAddCustomerFromRow(rowValues, rowNumber, result);
        }

        return result;
    }

    private ImportResult ParseExcelFile(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        });

        if (dataSet.Tables.Count == 0)
        {
            throw new CustomerImportException("No worksheets were found in the uploaded Excel file.");
        }

        var table = dataSet.Tables[0];
        if (table.Columns.Count == 0)
        {
            throw new CustomerImportException("The worksheet does not contain any columns.");
        }

        var headers = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
        var headerAliases = BuildHeaderAliases(headers);

        var result = new ImportResult();
        var rowNumber = 1;

        foreach (DataRow row in table.Rows)
        {
            rowNumber++;
            var rowValues = ExtractRowValues(row, headerAliases);
            if (IsRowEmpty(rowValues))
            {
                continue;
            }

            TryAddCustomerFromRow(rowValues, rowNumber, result);
        }

        return result;
    }

    private static Dictionary<string, string> BuildHeaderAliases(IEnumerable<string> headers)
    {
        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in headers)
        {
            var canonical = NormalizeHeader(header);
            if (canonical is null || aliases.ContainsKey(canonical))
            {
                continue;
            }

            aliases[canonical] = header;
        }

        foreach (var required in RequiredImportColumns)
        {
            if (!aliases.ContainsKey(required))
            {
                throw new CustomerImportException($"Missing required column '{required}'.");
            }
        }

        return aliases;
    }

    private static Dictionary<string, string?> ExtractRowValues(CsvReader csv, Dictionary<string, string> headerAliases)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var alias in headerAliases)
        {
            csv.TryGetField<string>(alias.Value, out var fieldValue);
            values[alias.Key] = string.IsNullOrWhiteSpace(fieldValue) ? null : fieldValue.Trim();
        }

        return values;
    }

    private static Dictionary<string, string?> ExtractRowValues(DataRow row, Dictionary<string, string> headerAliases)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var alias in headerAliases)
        {
            var rawValue = row[alias.Value];
            if (rawValue is DBNull)
            {
                values[alias.Key] = null;
                continue;
            }

            values[alias.Key] = string.IsNullOrWhiteSpace(rawValue?.ToString())
                ? null
                : rawValue!.ToString()!.Trim();
        }

        return values;
    }

    private static bool IsRowEmpty(Dictionary<string, string?> rowValues) =>
        rowValues.Values.All(string.IsNullOrWhiteSpace);

    private static void TryAddCustomerFromRow(
        Dictionary<string, string?> rowValues,
        int rowNumber,
        ImportResult result)
    {
        var name = GetTrimmedValue(rowValues, "Name");
        var mobile = GetTrimmedValue(rowValues, "MobileNumber");
        var location = GetTrimmedValue(rowValues, "Location");

        if (string.IsNullOrWhiteSpace(name) &&
            string.IsNullOrWhiteSpace(mobile) &&
            string.IsNullOrWhiteSpace(location) &&
            rowValues.Values.All(string.IsNullOrWhiteSpace))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            result.Errors.Add($"Row {rowNumber}: Name is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(mobile))
        {
            result.Errors.Add($"Row {rowNumber}: MobileNumber is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            result.Errors.Add($"Row {rowNumber}: Location is required.");
            return;
        }

        decimal? income = null;
        var incomeValue = GetTrimmedValue(rowValues, "Income");
        if (!string.IsNullOrWhiteSpace(incomeValue))
        {
            if (decimal.TryParse(incomeValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedIncome) && parsedIncome >= 0)
            {
                income = parsedIncome;
            }
            else
            {
                result.Errors.Add($"Row {rowNumber}: Income value '{incomeValue}' is invalid.");
                return;
            }
        }

        int? familyMembers = null;
        var familyValue = GetTrimmedValue(rowValues, "FamilyMembers");
        if (!string.IsNullOrWhiteSpace(familyValue))
        {
            if (int.TryParse(familyValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedFamily) && parsedFamily >= 0)
            {
                familyMembers = parsedFamily;
            }
            else
            {
                result.Errors.Add($"Row {rowNumber}: Family members value '{familyValue}' is invalid.");
                return;
            }
        }

        var customer = new Customer
        {
            Name = name!,
            MobileNumber = mobile!,
            Location = location!,
            InsuranceType = GetTrimmedValue(rowValues, "InsuranceType"),
            Income = income,
            SourceOfIncome = GetTrimmedValue(rowValues, "SourceOfIncome"),
            FamilyMembers = familyMembers
        };

        result.Customers.Add(customer);
    }

    private static string? GetTrimmedValue(Dictionary<string, string?> values, string key) =>
        values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value!.Trim()
            : null;

    private static string? NormalizeHeader(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return null;
        }

        var compact = new string(header.Where(ch => !char.IsWhiteSpace(ch) && ch != '_' && ch != '-').ToArray())
            .ToLowerInvariant();

        return compact switch
        {
            "name" or "customername" => "Name",
            "mobile" or "mobilenumber" or "mobileno" or "phonenumber" or "phone" or "contactnumber" => "MobileNumber",
            "location" or "city" or "area" => "Location",
            "insurancetype" or "insurance" => "InsuranceType",
            "income" => "Income",
            "sourceofincome" or "sourceincome" or "incomesource" => "SourceOfIncome",
            "familymembers" or "familymember" or "familycount" or "family" => "FamilyMembers",
            _ => null
        };
    }

    private sealed class ImportResult
    {
        public List<Customer> Customers { get; } = new();
        public List<string> Errors { get; } = new();
    }

    private sealed class CustomerImportException : Exception
    {
        public CustomerImportException(string message) : base(message)
        {
        }
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
