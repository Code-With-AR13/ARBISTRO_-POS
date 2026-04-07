using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
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

        // Dashboard View
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
            // ✅ COUNTS
            var totalOrders = await _context.SaleOrders.CountAsync();

            var paidOrders = await _context.SaleOrders
                .Where(x => x.PaymentStatus == "Paid")
                .CountAsync();

            var unpaidOrders = await _context.SaleOrders
                .Where(x => x.PaymentStatus == "Unpaid")
                .CountAsync();

            var totalExpense = await _context.Expenses
                .SumAsync(x => (decimal?)x.ExpenseAmount) ?? 0;

            // ✅ PERCENTAGES
            var paidPercent = totalOrders == 0 ? 0 : (paidOrders * 100 / totalOrders);
            var unpaidPercent = totalOrders == 0 ? 0 : (unpaidOrders * 100 / totalOrders);

            // ===========================
            // 📊 LINE CHART (LAST 7 DAYS)
            // ===========================
            var last7Days = await _context.SaleOrders
                .Where(x => x.PaymentStatus == "Paid")
                .GroupBy(x => x.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.GrandTotal)
                })
                .OrderByDescending(x => x.Date)
                .Take(7)
                .OrderBy(x => x.Date) // correct order for chart
                .ToListAsync();

            var chartDates = last7Days
                .Select(x => x.Date.ToString("dd MMM"))
                .ToArray();

            var chartPayments = last7Days
                .Select(x => x.Total)
                .ToArray();

            // ===========================
            // 🍩 DONUT CHART (DYNAMIC)
            // ===========================
            var leadsData = new[] { paidOrders, unpaidOrders };
            var leadsLabels = new[] { "Paid", "Unpaid" };

            // ===========================
            // FINAL RESPONSE
            // ===========================
            return Json(new
            {
                // Counters
                totalOrders,
                totalOrdersAll = totalOrders,

                totalPaid = paidOrders,
                totalUnpaid = unpaidOrders,

                totalExpense,

                // Progress
                ordersPercent = 100,
                paidPercent,
                unpaidPercent,
                expensePercent = 50, // can improve later

                // Charts
                chartDates,
                chartPayments,
                leadsData,
                leadsLabels
            });
        }

        // ===========================
        // 🔥 DATATABLE API (LATEST LEADS)
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

            // ✅ MUST wrap in "data" for DataTable
            return Json(new { data = latestData });
        }
    }
}