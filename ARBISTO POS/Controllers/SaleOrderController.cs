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
                .Where(o => o.OrderStatus == "Preparing")
                .ToListAsync();

            ViewBag.Chefs = await _context.Employees
                .Where(e => e.EmpRole == "Chef")
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AssignChef(int orderId, int chefId)
        {
            try
            {
                Console.WriteLine($"Received: orderId={orderId}, chefId={chefId}"); // Debug log

                var order = await _context.SaleOrders.FindAsync(orderId);
                if (order != null)
                {
                    order.ChefId = chefId;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Chef assigned successfully" });
                }
                return Json(new { success = false, message = "Order not found" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Debug log
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                Console.WriteLine($"Received: orderId={orderId}, status={status}"); // Debug log

                var order = await _context.SaleOrders.FindAsync(orderId);
                if (order != null)
                {
                    order.OrderStatus = status;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Status updated successfully" });
                }
                return Json(new { success = false, message = "Order not found" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Debug log
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        public IActionResult GetCustomers(string search)
        {
            var query = _context.Customers.AsQueryable();

            if (string.IsNullOrEmpty(search))
            {
                // ✅ When no search, show "Walking Customer" first
                var customers = query
                    .OrderByDescending(c => c.Name.ToLower().Contains("walking"))
                    .ThenBy(c => c.Name)
                    .Select(c => new { id = c.Id, text = c.Name })
                    .ToList();

                return Json(new { results = customers });
            }
            else
            {
                // Normal search
                var customers = query
                    .Where(c => c.Name.Contains(search))
                    .Select(c => new { id = c.Id, text = c.Name })
                    .ToList();

                return Json(new { results = customers });
            }
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
                OrderStatus = "Preparing",
                CustomerId = model.CustomerId ?? 0,
                TableId = model.Order.TableId,
                PickUpId = model.Order.PickUpId,
                DelivaryAddress = model.Order.DelivaryAddress,                
                SubTotal = model.Order.SubTotal,
                TaxAmount = model.Order.TaxAmount,        // ✅ Ab ye properly bind hoga
                DiscountAmount = model.Order.DiscountAmount, // ✅ Ab ye properly bind hoga
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
                    ItemImage = item.ItemImage,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Total = item.Price * item.Quantity,
                    CustomerId = model.CustomerId
                };

                _context.SaleOrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            // ✅ Success message TempData mein store karein
            TempData["SuccessMessage"] = $"Order successfully placed and go to Kitchen!" ;


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





        // GET: SaleOrderController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.SaleOrders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Include(o => o.Table)
                .Include(o => o.PickUp)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var vm = new PosViewModel
            {
                Order = order,
                CustomerId = order.CustomerId,
                Categories = await _context.FoodCategories.ToListAsync(),
                Items = await _context.Items.ToListAsync(),
                Employees = await _context.Employees.ToListAsync(),
                Tables = await _context.ServiceTables.ToListAsync(),
                PickupPoints = await _context.PickPoints.ToListAsync(),
                PaymentMethods = await _context.PaymentMethods.ToListAsync()
            };

            return View(vm);
        }

        // POST: SaleOrderController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PosViewModel model,
            string OrderItemsJson)
        {
            if (id != model.Order.OrderId)
            {
                return NotFound();
            }

            try
            {
                var order = await _context.SaleOrders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    return NotFound();
                }

                // Update order properties
                order.OrderType = model.Order.OrderType;
                order.CustomerId = model.CustomerId ?? 0;
                order.TableId = model.Order.TableId;
                order.PickUpId = model.Order.PickUpId;
                order.DelivaryAddress = model.Order.DelivaryAddress;
                order.SubTotal = model.Order.SubTotal;
                order.TaxAmount = model.Order.TaxAmount;          // ✅ Ab properly bind hoga
                order.DiscountAmount = model.Order.DiscountAmount; // ✅ Ab properly bind hoga
                order.GrandTotal = model.Order.GrandTotal;
                order.Notes = model.Order.Notes;

                // Remove existing order items
                _context.SaleOrderItems.RemoveRange(order.OrderItems);

                // Add updated order items
                if (!string.IsNullOrEmpty(OrderItemsJson))
                {
                    var orderItems = JsonConvert.DeserializeObject<List<SaleOrderItems>>(OrderItemsJson);

                    foreach (var item in orderItems)
                    {
                        var orderItem = new SaleOrderItems
                        {
                            OrderId = order.OrderId,
                            ItemId = item.ItemId,
                            ItemName = item.ItemName,
                            ItemImage = item.ItemImage,
                            Price = item.Price,
                            Quantity = item.Quantity,
                            Total = item.Price * item.Quantity,
                            CustomerId = model.CustomerId
                        };

                        _context.SaleOrderItems.Add(orderItem);
                    }
                }

                _context.Update(order);
                await _context.SaveChangesAsync();

                // ✅ Success message TempData mein store karein
                TempData["SuccessMessage"] = $"Order successfully Updated!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // Helper method to check if order exists
        private async Task<bool> OrderExists(int id)
        {
            return await _context.SaleOrders.AnyAsync(e => e.OrderId == id);
        }









        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoldOrder([FromBody] HoldOrderViewModel model)
        {
            try
            {
                if (model == null || model.Items == null || !model.Items.Any())
                {
                    return Json(new { success = false, message = "Invalid order data or no items" });
                }

                // Generate order number if not provided
                if (string.IsNullOrEmpty(model.OrderNumber))
                {
                    model.OrderNumber = $"HLD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
                }

                // Create HeldOrders entity
                var heldOrder = new HeldOrders
                {
                    OrderNumber = model.OrderNumber,
                    OrderDate = DateTime.UtcNow,
                    OrderType = model.OrderType,
                    OrderStatus = "Held",
                    CustomerId = model.CustomerId,
                    TableId = model.TableId,
                    PickUpId = model.PickUpId,
                    DelivaryAddress = model.DelivaryAddress,
                    PaymentId = model.PaymentId,
                    PaymentStatus = model.PaymentStatus ?? "Unpaid",
                    SubTotal = model.SubTotal ?? 0,
                    TaxAmount = model.TaxAmount ?? 0,
                    DiscountAmount = model.DiscountAmount ?? 0,
                    GrandTotal = model.GrandTotal ?? 0,
                    ChefId = null,
                    HeldOrderItems = new List<HeldOrdersItem>() // ✅ Initialize collection
                };

                // Create HeldOrderItems and add to parent
                foreach (var item in model.Items)
                {
                    var heldOrderItem = new HeldOrdersItem
                    {
                        CustomerId = model.CustomerId,
                        ItemId = item.ItemId,
                        ItemName = item.ItemName,
                        ItemImage = item.ItemImage,
                        Price = item.Price ?? 0,
                        Quantity = item.Quantity ?? 0,
                        Total = item.Total ?? 0
                    };

                    heldOrder.HeldOrderItems.Add(heldOrderItem); // ✅ Add to collection
                }

                // Add order with items in one transaction
                _context.HeldOrders.Add(heldOrder);
                await _context.SaveChangesAsync();

                // Return success
                return Json(new
                {
                    success = true,
                    message = "Order held successfully",
                    orderId = heldOrder.OrderId,
                    orderNumber = heldOrder.OrderNumber
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error holding order: {ex.Message}" });
            }
        }


        // Add this method to get held orders count
        [HttpGet]
        public async Task<IActionResult> GetHeldOrdersCount()
        {
            var count = await _context.HeldOrders.CountAsync();
            return Json(new { count });
        }

        public async Task<IActionResult> GetHeldOrders()
        {
            var heldOrders = await _context.HeldOrders
                .Include(h => h.Customer)
                .Include(h => h.Table)
                .Include(h => h.HeldOrderItems)  // ✅ YEH ADD KAREIN
                .OrderByDescending(h => h.OrderDate)
                .ToListAsync();

            return PartialView("_HeldOrdersPartial", heldOrders);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteHeldOrder(int id)
        {
            try
            {
                var heldOrder = await _context.HeldOrders
                    .Include(h => h.HeldOrderItems)
                    .FirstOrDefaultAsync(h => h.OrderId == id);

                if (heldOrder == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                _context.HeldOrders.Remove(heldOrder);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadHeldOrder(int id)
        {
            try
            {
                var heldOrder = await _context.HeldOrders
                    .Include(h => h.HeldOrderItems)
                    .Include(h => h.Customer)
                    .Include(h => h.Table)
                    .FirstOrDefaultAsync(h => h.OrderId == id);

                if (heldOrder == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                // Convert items to proper format
                var items = heldOrder.HeldOrderItems.Select(item => new
                {
                    id = item.ItemId,
                    name = item.ItemName,
                    price = item.Price,
                    image = item.ItemImage ?? "/assets/images/upload.jpg",
                    qty = item.Quantity,                    
                }).ToList();

                return Json(new
                {
                    success = true,
                    order = new
                    {
                        id = heldOrder.OrderId,
                        orderNumber = heldOrder.OrderNumber,
                        orderType = heldOrder.OrderType,
                        customerId = heldOrder.CustomerId,
                        tableId = heldOrder.TableId,
                        pickUpId = heldOrder.PickUpId,
                        deliveryAddress = heldOrder.DelivaryAddress,
                        subTotal = heldOrder.SubTotal,
                        taxAmount = heldOrder.TaxAmount,
                        discountAmount = heldOrder.DiscountAmount,
                        grandTotal = heldOrder.GrandTotal,
                        items = items,
                        customerName = heldOrder.Customer?.Name,
                        tableName = heldOrder.Table?.TabName
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            try
            {
                var order = await _context.SaleOrders.FindAsync(request.OrderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                // Update order payment status
                order.PaymentId = request.PaymentMethodId;
                order.PaymentStatus = "Paid";
                order.OrderStatus = "Completed";

                // Save payment transaction (if you have a Payments table)
                // var payment = new Payment
                // {
                //     OrderId = request.OrderId,
                //     PaymentMethodId = request.PaymentMethodId,
                //     TotalAmount = request.TotalAmount,
                //     ReceivedAmount = request.ReceivedAmount,
                //     ChangeAmount = request.ChangeAmount,
                //     PaymentDate = DateTime.UtcNow
                // };
                // _context.Payments.Add(payment);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Payment processed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Payment Request Model (add this class)
        public class PaymentRequest
        {
            public int OrderId { get; set; }
            public int PaymentMethodId { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal ReceivedAmount { get; set; }           
        }




        [HttpGet]
        public JsonResult GetItemsByCategory(int categoryId)
        {
            try
            {
                var items = _context.Items
                    .Where(i => i.FoodCategoryId == categoryId)
                    .Select(i => new
                    {
                        itemId = i.ItemId,
                        itemName = i.ItemName,
                        itemPrice = i.ItemPrice,
                        cateImage = i.CateImage,
                        foodCategoryId = i.FoodCategoryId
                    })
                    .ToList();

                return Json(new { success = true, items = items });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public JsonResult GetAllItems()
        {
            var items = _context.Items
                .Select(x => new {
                    itemId = x.ItemId,
                    itemName = x.ItemName,
                    itemPrice = x.ItemPrice,
                    cateImage = x.CateImage,
                    foodCategoryId = x.FoodCategoryId
                })
                .ToList();

            // agar success property ki zarurat nahi to simple return
            return Json(new { items });
        }

        // ============================
        // INVOICE VIEW (DETAIL)
        // ============================
        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _context.SaleOrders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .Include(o => o.PickUp)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var vm = new PosViewModel
            {
                Order = order,
                CustomerId = order.CustomerId,
                Categories = await _context.FoodCategories.ToListAsync(),
                Items = await _context.Items.ToListAsync(),
                Employees = await _context.Employees.ToListAsync(),
                Tables = await _context.ServiceTables.ToListAsync(),
                PickupPoints = await _context.PickPoints.ToListAsync(),
                PaymentMethods = await _context.PaymentMethods.ToListAsync()
            };

            return View(vm);          // View name: Invoice.cshtml
        }

        // ============================
        // INVOICE PRINT VIEW
        // ============================
        public async Task<IActionResult> InvoicePrint(int id)
        {
            var order = await _context.SaleOrders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .Include(o => o.PickUp)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var vm = new PosViewModel
            {
                Order = order,
                CustomerId = order.CustomerId,
                Categories = await _context.FoodCategories.ToListAsync(),
                Items = await _context.Items.ToListAsync(),
                Employees = await _context.Employees.ToListAsync(),
                Tables = await _context.ServiceTables.ToListAsync(),
                PickupPoints = await _context.PickPoints.ToListAsync(),
                PaymentMethods = await _context.PaymentMethods.ToListAsync()
            };

            return View(vm);          // View name: InvoicePrint.cshtml
        }


    }
}
