using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Permission("Expense Report")]
public class ExpenseReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExpenseReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================
    // 📄 INDEX (UNCHANGED)
    // =========================
    [HttpGet]
    public async Task<IActionResult> Index(int? expenseTypeId, DateTime? fromDate, DateTime? toDate)
    {
        var model = new ExpenseReportVm();

        // Dropdown
        model.ExpenseTypes = await _context.ExpenseTypes
            .OrderBy(x => x.ExpenseName)
            .ToListAsync();

        model.ExpenseTypeId = expenseTypeId;
        model.FromDate = fromDate;
        model.ToDate = toDate;

        var query = _context.Expenses
            .Include(x => x.ExpenseType)
            .AsQueryable();

        if (expenseTypeId.HasValue && expenseTypeId.Value > 0)
            query = query.Where(x => x.ExpenseTypeId == expenseTypeId.Value);

        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedDate >= fromDate.Value.Date);

        if (toDate.HasValue)
        {
            var end = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.CreatedDate <= end);
        }

        model.Items = await query
            .OrderBy(x => x.CreatedDate)
            .Select(x => new ExpenseReportViewModel
            {
                CreatedDate = x.CreatedDate,
                ExpenseName = x.ExpenseName,
                ExpenseTypeName = x.ExpenseType.ExpenseName,
                ExpenseAmount = x.ExpenseAmount
            })
            .ToListAsync();

        return View(model);
    }

    // =========================
    // 🔥 AJAX: REPORT DATA
    // =========================
    [HttpGet]
    public async Task<IActionResult> GetReportData(int? expenseTypeId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Expenses
            .Include(x => x.ExpenseType)
            .AsQueryable();

        // Filter: category
        if (expenseTypeId.HasValue && expenseTypeId.Value > 0)
            query = query.Where(x => x.ExpenseTypeId == expenseTypeId.Value);

        // Filter: from date
        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedDate >= fromDate.Value.Date);

        // Filter: to date
        if (toDate.HasValue)
        {
            var end = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.CreatedDate <= end);
        }

        var data = await query
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new
            {
                createdDate = x.CreatedDate.ToString("dd MMM yyyy"),
                expenseName = x.ExpenseName,
                expenseTypeName = x.ExpenseType != null ? x.ExpenseType.ExpenseName : "",
                expenseAmount = x.ExpenseAmount
            })
            .ToListAsync();

        return Json(new { data });
    }
}