using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Index()
    {
        var details = await _soldProductDetailService.GetAllWithDetailsAsync();
        var viewModel = new SoldProductDetailListViewModel
        {
            Details = details
        };

        return View(viewModel);
    }
}
