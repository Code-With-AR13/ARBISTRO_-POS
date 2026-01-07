using ARBISTO_POS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARBISTO_POS.Controllers
{
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
    }
}
