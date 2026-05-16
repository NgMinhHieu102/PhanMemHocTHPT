using Microsoft.AspNetCore.Mvc;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
public class StudentPortalController : Controller
{
    [HttpGet]
    public IActionResult Dashboard()
    {
        return RedirectToAction("Dashboard", "StudentPortal", new { area = "" });
    }
}
