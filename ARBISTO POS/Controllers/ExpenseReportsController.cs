using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ExpenseReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExpenseReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? expenseTypeId, DateTime? fromDate, DateTime? toDate)
    {
        var model = new ExpenseReportVm();

        // Dropdown ke liye saare expense types
        model.ExpenseTypes = await _context.ExpenseTypes
            .OrderBy(x => x.ExpenseName)
            .ToListAsync();

        model.ExpenseTypeId = expenseTypeId;
        model.FromDate = fromDate;
        model.ToDate = toDate;

        // Base query
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
            var end = toDate.Value.Date.AddDays(1).AddTicks(-1); // din ka end
            query = query.Where(x => x.CreatedDate <= end);
        }

        // Data ko ViewModel me map karo
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
}
