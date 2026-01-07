// using ARBISTO_POS.Migrations;  <-- YE LINE HATA DO

using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class OverallReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public OverallReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string orderType, int? chefId, DateTime? fromDate, DateTime? toDate)
    {
        var model = new OverAllReportVm();

        // dropdown: chefs
        model.Chefs = await _context.Employees
            .Where(e => e.EmpRole == "Chef")
            .OrderBy(e => e.FullName)
            .ToListAsync();

        // dropdown: order types (agar VM me hard-coded list hai to ye skip karo)
        // model.OrderTypes already set in constructor / property.

        // base query
        var query = _context.SaleOrders
            .Include(o => o.Customer)
            .Include(o => o.Chef)
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(orderType))
        {
            model.OrderType = orderType;
            query = query.Where(o => o.OrderType == orderType);
        }

        if (chefId.HasValue && chefId.Value > 0)
        {
            model.ChefId = chefId;
            query = query.Where(o => o.ChefId == chefId.Value);
        }

        if (fromDate.HasValue)
        {
            model.FromDate = fromDate;
            query = query.Where(o => o.OrderDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            model.ToDate = toDate;
            var end = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(o => o.OrderDate <= end);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        model.Items = orders.Select(o => new OverallReportsViewModel
        {
            OrderId = o.OrderId,
            OrderNumber = o.OrderNumber,
            OrderDate = o.OrderDate,
            OrderType = o.OrderType,
            OrderStatus = o.OrderStatus,
            CustomerName = o.Customer!.Name,
            ChefName = o.Chef != null ? o.Chef.FullName : "N/A",
            SubTotal = o.SubTotal,
            TaxAmount = o.TaxAmount,
            DiscountAmount = o.DiscountAmount,
            GrandTotal = o.GrandTotal
        }).ToList();

        // cost + totals
        model.TotalCostAmount = orders
            .SelectMany(o => o.OrderItems)
            .Sum(i => i.Quantity * i.Price);

        return View(model);
    }
}
