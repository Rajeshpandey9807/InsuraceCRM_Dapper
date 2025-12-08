using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.Models;
using InsuraceCRM_Dapper.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace InsuraceCRM_Dapper.Controllers;

[Authorize]
public class SoldProductDetailController : Controller
{
    private readonly ISoldProductDetailService _soldProductDetailService;

    public SoldProductDetailController(ISoldProductDetailService soldProductDetailService)
    {
        _soldProductDetailService = soldProductDetailService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? customerId = null, string? customerName = null)
    {
        var details = await _soldProductDetailService.GetAllWithDetailsAsync(customerId);
        var detailList = details?.ToList() ?? new List<SoldProductDetailInfo>();
        var resolvedName = customerName;

        if (string.IsNullOrWhiteSpace(resolvedName) && customerId.HasValue)
        {
            resolvedName = detailList.FirstOrDefault()?.CustomerName;
        }

        var viewModel = new SoldProductDetailListViewModel
        {
            Details = detailList,
            FilteredCustomerId = customerId,
            FilteredCustomerName = resolvedName
        };

        return View(viewModel);
    }
}
