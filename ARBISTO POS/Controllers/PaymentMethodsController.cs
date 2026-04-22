using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{
    [Permission("Manage Payment Methods")]
    public class PaymentMethodsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            var methods = await _context.PaymentMethods.ToListAsync();
            return View(methods);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.PaymentMethods
                .Select(p => new
                {
                    id = p.Id,
                    payName = p.PayName,
                    payDescription = p.PayDescription,
                    createdDate = p.CreatedDate.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return Json(data);
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentMethods model)
        {
            // Duplicate name check
            if (await _context.PaymentMethods.AnyAsync(p =>
                p.PayName.Trim().ToLower() == model.PayName.Trim().ToLower()))
            {
                ModelState.AddModelError("PayName", "Payment method already exists.");
            }

            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.UtcNow;
                _context.PaymentMethods.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Payment method created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var method = await _context.PaymentMethods.FindAsync(id);
            if (method == null) return NotFound();

            return View(method);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentMethods model)
        {
            if (id != model.Id) return NotFound();

            // Duplicate name check (exclude current)
            if (await _context.PaymentMethods.AnyAsync(p =>
                p.Id != id &&
                p.PayName.Trim().ToLower() == model.PayName.Trim().ToLower()))
            {
                ModelState.AddModelError("PayName", "Payment method already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Payment method updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var method = await _context.PaymentMethods.FindAsync(id);
                if (method == null)
                    return Json(new { success = false, message = "Payment method not found!" });

                _context.PaymentMethods.Remove(method);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Payment method deleted successfully!" });
            }
            catch
            {
                return Json(new { success = false, message = "Error deleting payment method!" });
            }
        }

    }
}
