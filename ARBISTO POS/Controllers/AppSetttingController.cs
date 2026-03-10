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
            var setting = _context.AppSetttings.FirstOrDefault();
            if (setting == null)
            {
                setting = new AppSettting();
            }
            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(AppSettting model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingSetting = _context.AppSetttings.FirstOrDefault();

                    if (existingSetting == null)
                    {
                        _context.AppSetttings.Add(model);
                    }
                    else
                    {
                        existingSetting.KitchenPrinter = model.KitchenPrinter;
                        existingSetting.SingleTableMultiOrder = model.SingleTableMultiOrder;
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
