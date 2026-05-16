using Microsoft.AspNetCore.Mvc;

namespace TinHocTHPT.Areas.Admin.Controllers;

[Area("Admin")]
public class ExercisePortalController : Controller
{
    [HttpGet]
    public IActionResult Do(int id)
    {
        return RedirectToAction("Do", "ExercisePortal", new { area = "", id });
    }

    [HttpGet]
    public IActionResult Result(int submissionId)
    {
        return RedirectToAction("Result", "ExercisePortal", new { area = "", submissionId });
    }

    [HttpPost]
    [ActionName("Submit")]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitPost(int exerciseId)
    {
        return RedirectPreserveMethod(Url.Action("Submit", "ExercisePortal", new { area = "" }) ?? "/ExercisePortal/Submit");
    }
}
