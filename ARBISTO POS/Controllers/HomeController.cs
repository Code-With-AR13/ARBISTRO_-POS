using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{
    [Permission("Access Dashboard")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===========================
        // 📄 DASHBOARD VIEW
        // ===========================
        public IActionResult Index()
        {
            return View();
        }

        // ===========================
        // 🔥 DASHBOARD DATA API
        // ===========================
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var totalSale = await _context.SaleOrders
                .SumAsync(x => (decimal?)x.GrandTotal) ?? 0;

            var totalTax = await _context.SaleOrders
                .SumAsync(x => (decimal?)x.TaxAmount) ?? 0;

            var totalDiscount = await _context.SaleOrders
                .SumAsync(x => (decimal?)x.DiscountAmount) ?? 0;

            var totalCost = await _context.SaleOrderItems
                .Include(x => x.Item)
                .SumAsync(x => (decimal?)(x.Item.ItemPrice * x.Quantity)) ?? 0;

            var totalExpenses = await _context.Expenses
                .SumAsync(x => (decimal?)x.ExpenseAmount) ?? 0;

            var totalProfit = totalSale - totalCost ;
            var profitAfterExpenses = totalProfit - totalExpenses;

            var baseAmount = totalSale == 0 ? 1 : totalSale;

            int salePercent = totalSale > 0 ? 100 : 0;
            int costPercent = (int)((totalCost * 100) / baseAmount);
            int taxPercent = (int)((totalTax * 100) / baseAmount);
            int discountPercent = (int)((totalDiscount * 100) / baseAmount);
            int expensesPercent = (int)((totalExpenses * 100) / baseAmount);
            int profitPercent = (int)((totalProfit * 100) / baseAmount);
            int profitAfterPercent = (int)((profitAfterExpenses * 100) / baseAmount);

            costPercent = Math.Clamp(costPercent, 0, 100);
            taxPercent = Math.Clamp(taxPercent, 0, 100);
            discountPercent = Math.Clamp(discountPercent, 0, 100);
            expensesPercent = Math.Clamp(expensesPercent, 0, 100);
            profitPercent = Math.Clamp(profitPercent, 0, 100);
            profitAfterPercent = Math.Clamp(profitAfterPercent, 0, 100);

            return Json(new
            {
                totalSale,
                totalCost,
                totalDiscount,
                totalProfit,
                totalTax,
                totalExpenses,
                profitAfterExpenses,

                salePercent,
                costPercent,
                discountPercent,
                profitPercent,
                taxPercent,
                expensesPercent,
                profitAfterPercent
            });
        }

        // ===========================
        // 📊 FIXED GRAPH API (CURVED LINE READY 🔥)
        // ===========================
        [HttpGet]
        public async Task<IActionResult> GetChartData()
        {
            // 👉 Sales grouped by month
            var monthlySales = await _context.SaleOrders
                .GroupBy(x => x.OrderDate.Month)
                .Select(g => new
                {
                    month = g.Key,
                    sale = g.Sum(x => (decimal?)x.GrandTotal) ?? 0
                })
                .ToListAsync();

            // 👉 Cost grouped by month
            var monthlyCosts = await _context.SaleOrderItems
                .Include(x => x.Order)
                .Include(x => x.Item)
                .GroupBy(x => x.Order.OrderDate.Month)
                .Select(g => new
                {
                    month = g.Key,
                    cost = g.Sum(x => (decimal?)(x.Item.ItemPrice * x.Quantity)) ?? 0
                })
                .ToListAsync();

            // 🔥 IMPORTANT: Generate FULL 12 months data
            var finalData = Enumerable.Range(1, 12).Select(month => new
            {
                month = month,
                sale = monthlySales.FirstOrDefault(x => x.month == month)?.sale ?? 0,
                cost = monthlyCosts.FirstOrDefault(x => x.month == month)?.cost ?? 0
            }).ToList();

            return Json(finalData);
        }


        [HttpGet]
        public IActionResult GetOrders()
        {
            var orders = _context.SaleOrders
                .OrderByDescending(o => o.OrderDate) // 🔥 Latest first from DB
                .Select(o => new
                {
                    orderId = o.OrderId,
                    customerName = o.Customer.Name,
                    totalAmount = o.GrandTotal,
                    orderDate = o.OrderDate
                })
                .ToList();

            return Json(new { data = orders });
        }

        // ===========================
        // 📋 DATATABLE API
        // ===========================
        [HttpGet]
        public async Task<IActionResult> GetLatestLeads()
        {
            var latestData = await _context.SaleOrders
                .Include(x => x.Customer)
                .OrderByDescending(x => x.OrderDate)
                .Take(10)
                .Select(x => new
                {
                    name = x.Customer != null ? x.Customer.Name : "Walk-in",
                    phone = x.Customer != null ? x.Customer.PhoneNo : "-",
                    status = x.PaymentStatus,
                    date = x.OrderDate.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return Json(new { data = latestData });
        }
    }

}