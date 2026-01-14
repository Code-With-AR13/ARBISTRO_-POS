using Microsoft.AspNetCore.Mvc;
using ARBISTO_POS.Models;
using ARBISTO_POS.Data;
using ARBISTO_POS.Attributes;

namespace ARBISTO_POS.Controllers
{
    [Permission("Printer Configuration")]
    public class AppSetttingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppSetttingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var setting = _context.AppSetttingPrinter.FirstOrDefault();
            if (setting == null)
            {
                setting = new AppSetttingPrinter();
            }
            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(AppSetttingPrinter model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingSetting = _context.AppSetttingPrinter.FirstOrDefault();

                    if (existingSetting == null)
                    {
                        _context.AppSetttingPrinter.Add(model);
                    }
                    else
                    {
                        existingSetting.KitchenPrinter = model.KitchenPrinter;
                    }

                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Settings saved successfully!";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving settings.";
                return View(model);
            }
        }
    }
}
