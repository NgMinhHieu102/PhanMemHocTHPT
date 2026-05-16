using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
[AllowAnonymous]
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var targetUrl = Url.Action("Login", "Account", new { area = "", returnUrl });
        return Redirect(targetUrl ?? "/Account/Login");
    }

    [HttpPost]
    [ActionName("Login")]
    [ValidateAntiForgeryToken]
    public IActionResult LoginPost(string? returnUrl = null)
    {
        var targetUrl = Url.Action("Login", "Account", new { area = "", returnUrl });
        return RedirectPreserveMethod(targetUrl ?? "/Account/Login");
    }
}
