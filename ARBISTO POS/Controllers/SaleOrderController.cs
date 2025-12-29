using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.Json;
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

        public async Task<IActionResult> Kitchen()
        {
            var orders = await _context.SaleOrders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderStatus == "Pending")
                .ToListAsync();

            ViewBag.Chefs = await _context.Employees
                .Where(e => e.EmpRole == "Chef")
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

        // Get Held Invoices (Pending Status)
        public async Task<IActionResult> GetHeldInvoices()
        {
            var heldInvoices = await _context.SaleOrders
                .Where(o => o.OrderStatus == "Pending")
                .Include(o => o.OrderItems)
                .ToListAsync();

            return Json(new { invoices = heldInvoices });
        }

        // Mark as restored
        [HttpPost]
        public async Task<IActionResult> RestoreOrder(int id)
        {
            var order = await _context.SaleOrders.FindAsync(id);
            if (order != null)
            {
                order.OrderStatus = "Restored"; // Mark as restored or change status as needed
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(GetHeldInvoices));  // To reload the modal with updated data
        }

        // GET: Get all tables with status
        [HttpGet]
        public IActionResult GetTables()
        {
            try
            {
                // Get active order statuses
                var activeStatuses = new[] { "Preparing", "Ready", "Pending" };

                // Get all tables
                var allTables = _context.ServiceTables.ToList();

                // Get tables that have active orders
                var occupiedTableIds = _context.SaleOrders
                    .Where(o => activeStatuses.Contains(o.OrderStatus) && o.TableId > 0)
                    .Select(o => o.TableId)
                    .Distinct()
                    .ToList();

                // Map tables with status
                var tables = allTables.Select(table => new
                {
                    id = table.Id,
                    tableName = table.TabName,
                    status = occupiedTableIds.Contains(table.Id) ? "Serving" : "Available"
                }).ToList();

                return Json(new { tables = tables });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // GET: Get Customer Address
        [HttpGet]
        public IActionResult GetCustomerAddress(int customerId)
        {
            var customer = _context.Customers.Find(customerId);

            if (customer == null)
            {
                return Json(new { hasAddress = false, customerName = "Unknown" });
            }

            // Check if customer has address
            bool hasAddress = !string.IsNullOrEmpty(customer.Address);

            return Json(new
            {
                hasAddress = hasAddress,
                address = customer.Address ?? "",
                customerName = customer.Name
            });
        }




        // ============================
        // POS CREATE (GET)
        // ============================
        public async Task<IActionResult> Create(int? categoryId)
        {
            var vm = new PosViewModel
            {
                Categories = await _context.FoodCategories.ToListAsync(),
                Items = await _context.Items
                    .Where(x => !categoryId.HasValue || x.FoodCategoryId == categoryId)
                    .ToListAsync(),

                Employees = await _context.Employees.ToListAsync(),
                Tables = await _context.ServiceTables.ToListAsync(),
                PickupPoints = await _context.PickPoints.ToListAsync(),
                PaymentMethods = await _context.PaymentMethods.ToListAsync()
            };

            return View(vm);
        }

        // ============================
        // POS CREATE (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PosViewModel model,
            string OrderItemsJson)
        {
            if (string.IsNullOrEmpty(OrderItemsJson))
            {
                ModelState.AddModelError("", "No items in order.");
                return RedirectToAction(nameof(Create));
            }

            var orderItems = JsonConvert.DeserializeObject<List<SaleOrderItems>>(OrderItemsJson);

            // 🔹 Create Order
            var order = new SaleOrders
            {
                OrderNumber = $"ORD-{DateTime.Now.Ticks}",
                OrderDate = DateTime.UtcNow,
                OrderType = model.Order.OrderType,
                OrderStatus = "Pending",
                CustomerId = model.CustomerId ?? 0,
                TableId = model.Order.TableId,
                PickUpId = model.Order.PickUpId,
                DelivaryAddress = model.Order.DelivaryAddress,                
                SubTotal = model.Order.SubTotal,
                TaxAmount = model.Order.TaxAmount,
                DiscountAmount = model.Order.DiscountAmount,
                GrandTotal = model.Order.GrandTotal,
                PaymentStatus = "Unpaid",
                Notes = model.Order.Notes
            };

            _context.SaleOrders.Add(order);
            await _context.SaveChangesAsync();

            // 🔹 Save Order Items
            foreach (var item in orderItems)
            {
                var orderItem = new SaleOrderItems
                {
                    OrderId = order.OrderId,
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Total = item.Price * item.Quantity,
                    CustomerId = model.CustomerId
                };

                _context.SaleOrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Create));
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
