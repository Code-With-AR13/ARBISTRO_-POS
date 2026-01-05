using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{
    public class ExpenseTypeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseTypeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            var methods = await _context.ExpenseTypes.ToListAsync();
            return View(methods);
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseType model)
        {
            // Duplicate name check
            if (await _context.ExpenseTypes.AnyAsync(p =>
                p.ExpenseName.Trim().ToLower() == model.ExpenseName.Trim().ToLower()))
            {
                ModelState.AddModelError("ExpenseName", "Expense Type already exists.");
            }

            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.UtcNow;
                _context.ExpenseTypes.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Expense Type created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var method = await _context.ExpenseTypes.FindAsync(id);
            if (method == null) return NotFound();

            return View(method);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseType model)
        {
            if (id != model.Id) return NotFound();

            // Duplicate name check (exclude current)
            if (await _context.ExpenseTypes.AnyAsync(p =>
                p.Id != id &&
                p.ExpenseName.Trim().ToLower() == model.ExpenseName.Trim().ToLower()))
            {
                ModelState.AddModelError("ExpenseName", "Expense Type already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Expense Type updated successfully!";
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
                var method = await _context.ExpenseTypes.FindAsync(id);
                if (method == null)
                    return Json(new { success = false, message = "Expense Type not found!" });

                _context.ExpenseTypes.Remove(method);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Expense Type deleted successfully!" });
            }
            catch
            {
                return Json(new { success = false, message = "Error deleting Expense Type!" });
            }
        }

    }
}
