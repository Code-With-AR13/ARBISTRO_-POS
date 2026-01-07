using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{
    [Permission("Manage Expenses")]
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Index: List all expenses with their ExpenseType
        public async Task<IActionResult> Index()
        {
            var expenses = await _context.Expenses
                .Include(e => e.ExpenseType)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return View(expenses);
        }

        // ===== CREATE =====

        // GET: Expense/Create
        public async Task<IActionResult> Create()
        {
            await LoadExpenseTypesAsync();
            return View(new Expenses());
        }

        // POST: Expense/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expenses model)
        {
            ModelState.Remove("ExpenseType"); // nav prop

            if (!ModelState.IsValid)
            {
                await LoadExpenseTypesAsync(model.ExpenseTypeId);
                return View(model);
            }

            model.CreatedDate = DateTime.UtcNow;

            _context.Expenses.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Expense created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ===== EDIT =====

        // GET: Expense/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.ExpenseType)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
                return NotFound();

            await LoadExpenseTypesAsync(expense.ExpenseTypeId);
            return View(expense);
        }

        // POST: Expense/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expenses model)
        {
            if (id != model.Id)
                return NotFound();

            var existing = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existing == null)
                return NotFound();

            ModelState.Remove("ExpenseType");

            if (!ModelState.IsValid)
            {
                await LoadExpenseTypesAsync(model.ExpenseTypeId);
                return View(model);
            }

            existing.ExpenseName = model.ExpenseName;
            existing.ExpenseAmount = model.ExpenseAmount;
            existing.ExpenseTypeId = model.ExpenseTypeId;
            existing.ExpDescription = model.ExpDescription;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Expense updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ===== DELETE (AJAX) =====

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (expense == null)
                    return Json(new { success = false, message = "Expense not found!" });

                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Expense deleted successfully!" });
            }
            catch
            {
                return Json(new { success = false, message = "Unable to delete expense!" });
            }
        }

        // ===== helper: dropdown fill =====
        private async Task LoadExpenseTypesAsync(int? selectedId = null)
        {
            var types = await _context.ExpenseTypes.ToListAsync();
            ViewBag.ExpenseTypes = new SelectList(types, "Id", "ExpenseName", selectedId);
        }

        [HttpGet]
        public async Task<IActionResult> GetExpenseTypes(string search)
        {
            var query = _context.ExpenseTypes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.ExpenseName.Contains(search));
            }

            var results = await query
                .Select(e => new { id = e.Id, text = e.ExpenseName })
                .Take(20)
                .ToListAsync();

            return Json(new { results });
        }

    }
}
