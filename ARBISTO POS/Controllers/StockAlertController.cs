using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{
    [Permission("Stock Reports")]
    public class StockAlertController : Controller
    {
        private ApplicationDbContext _context;

        public StockAlertController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stock = await _context.Ingredients
                                      .Where(x => x.AvailableQuantity < x.QuantityAlert)
                                      .ToListAsync();

            return View(stock);
        }

        // ================= 🔥 AJAX METHOD =================
        [HttpGet]
        public async Task<IActionResult> GetStockAlerts()
        {
            var data = await _context.Ingredients
                .Where(x => x.AvailableQuantity < x.QuantityAlert)
                .OrderBy(x => x.AvailableQuantity)
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    cost = x.Cost,
                    price = x.Price,
                    availableQuantity = x.AvailableQuantity,
                    quantityAlert = x.QuantityAlert,
                    createdDate = x.CreatedDate.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Json(new { data });
        }
    }
}