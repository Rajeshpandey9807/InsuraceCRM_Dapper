using InsuraceCRM_Dapper.Interfaces.Services;
using InsuraceCRM_Dapper.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuraceCRM_Dapper.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IUserService userService, ILogger<AccountController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel viewModel, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var user = await _userService.AuthenticateAsync(viewModel.Email, viewModel.Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(viewModel);
        }

        var role = "";
        if (user.RoleId == "1")
        {
            role = "Admin";
        }
        else if (user.RoleId == "2")
        {
            role = "Manager";
        }
        else if (user.RoleId == "3")
        {
            role = "Employee";
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.CustomerID.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = viewModel.RememberMe
        });

        _logger.LogInformation("User {Email} signed in.", user.Email);
        return RedirectToLocal(returnUrl);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
