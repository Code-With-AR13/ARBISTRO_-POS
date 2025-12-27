using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ARBISTO_POS.Controllers
{
    public class SaleOrderController : Controller
    {
        private ApplicationDbContext _context;

        public SaleOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SaleOrderController
        // Sale Order View (Index)
        public async Task<IActionResult> Index()
        {
            var orders = await _context.SaleOrders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ToListAsync();

            return View(orders);
        }

        // GET: SaleOrderController
        // Kitchen View (Index)
        public async Task<IActionResult> Kitchen()
        {
            var orders = await _context.SaleOrders
                .Where(o => o.OrderStatus == "Preparing")
                .Include(o => o.Customer)
                .ToListAsync();
            return View(orders);
        }
        [HttpGet]
        public IActionResult GetCustomers(string search)
        {
            var customers = _context.Customers
                .Where(c => string.IsNullOrEmpty(search) || c.Name.Contains(search))
                .Select(c => new { id = c.Id, text = c.Name })
                .ToList();

            return Json(new { results = customers });
        }
        [HttpGet]
        public IActionResult GetPickupPoints(string search)
        {
            var customers = _context.PickPoints
                .Where(c => string.IsNullOrEmpty(search) || c.PicTittle.Contains(search))
                .Select(c => new { id = c.Id, text = c.PicTittle })
                .ToList();

            return Json(new { results = customers });
        }        
        // GET: SaleOrderController/Create
        // POS View (Create)
        public ActionResult Create(int? categoryId)
        {
            var model = new PosViewModel
            {
                Categories = _context.FoodCategories.ToList(),
                Items = categoryId == null
                    ? _context.Items.ToList()
                    : _context.Items.Where(i => i.FoodCategoryId == categoryId).ToList(),

                Customers = _context.Customers.ToList()
            };

            return View(model);
        }

        // POST: SaleOrderController/Create
        // POS View (Create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PosViewModel model)
        {
            try
            {
                var order = new SaleOrders
                {
                    OrderNumber = "ORD-" + new Random().Next(1000, 9999),
                    OrderDate = DateTime.UtcNow,

                    // Bind OrderType from dropdown
                    OrderType = model.Order.OrderType,
                    OrderStatus = "Preparing",

                    // ✅ CUSTOMER SAVED IN SALEORDER
                    CustomerId = model.CustomerId ?? 1, // Walk-in ID

                    TableId = model.Order.TableId,
                    PickUpId = model.Order.PickUpId,
                    PaymentId = model.Order.PaymentId,
                    PaymentStatus = "Unpaid",

                    SubTotal = model.Order.SubTotal,
                    TaxAmount = model.Order.TaxAmount,
                    DiscountAmount = model.Order.DiscountAmount,
                    GrandTotal = model.Order.GrandTotal,

                    ChefId = model.Order.ChefId,
                    Notes = model.Order.Notes,

                    // ✅ CUSTOMER SAVED IN EVERY ORDER ITEM
                    OrderItems = model.Order.OrderItems.Select(i => new SaleOrderItems
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName,
                        Price = i.Price,
                        Quantity = i.Quantity,
                        Total = i.Price * i.Quantity,

                        CustomerId = model.CustomerId ?? 1 // 🔥 KEY LINE
                    }).ToList()
                };

                _context.SaleOrders.Add(order);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }


        // POST: SaleOrderController/ConfirmOrder/5
        // Kitchen view: Confirm the order and update status to "Ready"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            var order = await _context.SaleOrders.FindAsync(id);
            if (order != null)
            {
                order.OrderStatus = "Ready"; // Change order status to Ready
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Kitchen));
        }

        // POST: SaleOrderController/CompleteOrder/5
        // Mark order as completed when payment is clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            var order = await _context.SaleOrders.FindAsync(id);
            if (order != null)
            {
                order.OrderStatus = "Completed"; // Change order status to Completed
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
