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
            // ===========================
            // 📊 TOTAL ORDERS
            // ===========================
            var totalOrders = await _context.SaleOrders.CountAsync();

            // ===========================
            // 💰 TOTAL PAID / UNPAID (AMOUNT)
            // ===========================
            var totalPaid = await _context.SaleOrders
                .Where(x => x.PaymentStatus == "Paid")
                .SumAsync(x => (decimal?)x.GrandTotal) ?? 0;

            var totalUnpaid = await _context.SaleOrders
                .Where(x => x.PaymentStatus == "Unpaid")
                .SumAsync(x => (decimal?)x.GrandTotal) ?? 0;

            // ===========================
            // 📉 TOTAL EXPENSE
            // ===========================
            var totalExpense = await _context.Expenses
                .SumAsync(x => (decimal?)x.ExpenseAmount) ?? 0;

            // ===========================
            // 📊 TOTAL AMOUNT
            // ===========================
            var totalAmount = totalPaid + totalUnpaid;

            // ===========================
            // 📊 PERCENTAGES (PAID / UNPAID)
            // ===========================
            var paidPercent = totalAmount == 0 ? 0 : (int)((totalPaid * 100) / totalAmount);
            var unpaidPercent = totalAmount == 0 ? 0 : (int)((totalUnpaid * 100) / totalAmount);

            // ===========================
            // 📊 EXTRA PERCENTAGES (FOR CARDS)
            // ===========================

            // Orders → full if exists
            var ordersPercent = totalOrders > 0 ? 100 : 0;

            // Expense → compared with total revenue
            var expensePercent = totalAmount == 0 ? 0 : (int)((totalExpense * 100) / totalAmount);

            if (expensePercent > 100)
                expensePercent = 100;

            // ===========================
            // 📊 LINE CHART (LAST 7 DAYS)
            // ===========================
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var dbChartData = await _context.SaleOrders
                .Where(x => x.PaymentStatus == "Paid")
                .GroupBy(x => x.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.GrandTotal)
                })
                .ToListAsync();

            var chartDates = new List<string>();
            var chartPayments = new List<decimal>();

            foreach (var day in last7Days)
            {
                var match = dbChartData.FirstOrDefault(x => x.Date == day.Date);

                chartDates.Add(day.ToString("dd MMM"));
                chartPayments.Add(match != null ? match.Total : 0);
            }

            // ===========================
            // 🍩 DONUT CHART (COUNT)
            // ===========================
            var paidOrdersCount = await _context.SaleOrders
                .Where(x => x.PaymentStatus == "Paid")
                .CountAsync();

            var unpaidOrdersCount = await _context.SaleOrders
                .Where(x => x.PaymentStatus == "Unpaid")
                .CountAsync();

            // ===========================
            // ✅ FINAL RESPONSE
            // ===========================
            return Json(new
            {
                totalOrders,
                totalOrdersAll = totalOrders,

                totalPaid,
                totalUnpaid,
                totalExpense,

                paidPercent,
                unpaidPercent,

                // 🔥 IMPORTANT (FOR PROGRESS BARS)
                ordersPercent,
                expensePercent,

                chartDates,
                chartPayments,

                leadsData = new List<int> { paidOrdersCount, unpaidOrdersCount },
                leadsLabels = new List<string> { "Paid", "Unpaid" }
            });
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