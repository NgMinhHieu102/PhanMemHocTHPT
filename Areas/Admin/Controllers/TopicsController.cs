using Microsoft.AspNetCore.Mvc;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
public class TopicsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Topics", new { area = "" });
    }

    [HttpGet]
    public IActionResult Detail(int id)
    {
        return RedirectToAction("Detail", "Topics", new { area = "", id });
    }
}
