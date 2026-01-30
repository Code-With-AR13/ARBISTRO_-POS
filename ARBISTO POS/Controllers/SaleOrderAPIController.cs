using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ARBISTO_POS.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    //[Permission("Manage Sales/Payments")]
    public class SaleOrderApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SaleOrderApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================
        // GET: api/SaleOrderApi - List all orders
        // ============================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.SaleOrders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderNumber,
                    o.OrderDate,
                    o.OrderType,
                    o.OrderStatus,
                    Customer = o.Customer != null ? new { o.Customer.Id, o.Customer.Name } : null,
                    o.SubTotal,
                    o.TaxAmount,
                    o.DiscountAmount,
                    o.GrandTotal,
                    o.PaymentStatus,
                    OrderItems = o.OrderItems.Select(i => new
                    {
                        i.ItemId,
                        i.ItemName,
                        i.Price,
                        i.Quantity,
                        i.Total
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }
        // ============================
        // GET: api/SaleOrderApi/categories - Get all categories
        // ============================

        [AllowAnonymous]
        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _context.FoodCategories
                    .Select(c => new
                    {
                        id = c.Id,
                        cateName = c.CateName,
                        cateImage = c.CateImage ?? "/assets/images/upload.jpg",
                        //description = c.Description,
                        //itemCount = _context.Items.Count(i => i.FoodCategoryId == c.Id)
                    })
                    .ToListAsync();

                return Ok(new { success = true, categories });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: api/SaleOrderApi/categories/{id} - Get category by ID
        // ============================
        [AllowAnonymous]
        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _context.FoodCategories
                    .Where(c => c.Id == id)
                    .Select(c => new
                    {
                        id = c.Id,
                        cateName = c.CateName,
                        cateImage = c.CateImage ?? "/assets/images/upload.jpg",
                        //description = c.Description,
                        //itemCount = _context.Items.Count(i => i.FoodCategoryId == c.Id)
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                    return NotFound(new { success = false, message = "Category not found" });

                return Ok(new { success = true, category });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        // ============================
        // GET: api/SaleOrderApi/{id} - Get order by ID
        // ============================
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.SaleOrders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Include(o => o.Table)
                .Include(o => o.PickUp)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            return Ok(order);
        }

        // ============================
        // GET: api/SaleOrderApi/kitchen - Kitchen orders
        // ============================
        [AllowAnonymous]
        [HttpGet("kitchen")]
        //[Permission("Manage Kitchen")]
        public async Task<IActionResult> GetKitchenOrders()
        {
            var orders = await _context.SaleOrders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderStatus == "Preparing")
                .ToListAsync();

            var chefs = await _context.Employees
                .Where(e => e.EmpRole == "Chef")
                .Select(e => new { e.Id, e.FullName })
                .ToListAsync();

            return Ok(new { orders, chefs });
        }

        // ============================
        // POST: api/SaleOrderApi/assign-chef - Assign chef to order
        // ============================
        [AllowAnonymous]
        [HttpPost("assign-chef")]
        public async Task<IActionResult> AssignChef([FromBody] AssignChefRequest request)
        {
            try
            {
                var order = await _context.SaleOrders.FindAsync(request.OrderId);
                if (order == null)
                    return NotFound(new { success = false, message = "Order not found" });

                order.ChefId = request.ChefId;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Chef assigned successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // PATCH: api/SaleOrderApi/{id}/status - Update order status
        // ============================
        [AllowAnonymous]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var order = await _context.SaleOrders.FindAsync(id);
                if (order == null)
                    return NotFound(new { success = false, message = "Order not found" });

                order.OrderStatus = request.Status;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: api/SaleOrderApi/customers - Search customers
        // ============================
        [AllowAnonymous]
        [HttpGet("customers")]
        public IActionResult GetCustomers([FromQuery] string? search)
        {
            var query = _context.Customers.AsQueryable();

            if (string.IsNullOrEmpty(search))
            {
                var customers = query
                    .OrderByDescending(c => c.Name.ToLower().Contains("walking"))
                    .ThenBy(c => c.Name)
                    .Select(c => new { id = c.Id, text = c.Name })
                    .ToList();

                return Ok(new { results = customers });
            }
            else
            {
                var customers = query
                    .Where(c => c.Name.Contains(search))
                    .Select(c => new { id = c.Id, text = c.Name })
                    .ToList();

                return Ok(new { results = customers });
            }
        }

        // ============================
        // GET: api/SaleOrderApi/pickup-points - Get pickup points
        // ============================
        [AllowAnonymous]
        [HttpGet("pickup-points")]
        public IActionResult GetPickupPoints([FromQuery] string? search)
        {
            var pickupPoints = _context.PickPoints
                .Where(c => string.IsNullOrEmpty(search) || c.PicTittle.Contains(search))
                .Select(c => new { id = c.Id, text = c.PicTittle })
                .ToList();

            return Ok(new { results = pickupPoints });
        }

        // ============================
        // GET: api/SaleOrderApi/tables - Get all tables with status
        // ============================
        [AllowAnonymous]
        [HttpGet("tables")]
        public IActionResult GetTables()
        {
            try
            {
                var activeStatuses = new[] { "Preparing", "Ready", "Pending" };

                var allTables = _context.ServiceTables.ToList();

                var occupiedTableIds = _context.SaleOrders
                    .Where(o => activeStatuses.Contains(o.OrderStatus) && o.TableId > 0)
                    .Select(o => o.TableId)
                    .Distinct()
                    .ToList();

                var tables = allTables.Select(table => new
                {
                    id = table.Id,
                    tableName = table.TabName,
                    status = occupiedTableIds.Contains(table.Id) ? "Serving" : "Available"
                }).ToList();

                return Ok(new { tables });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ============================
        // GET: api/SaleOrderApi/customers/{customerId}/address
        // ============================
        [AllowAnonymous]
        [HttpGet("customers/{customerId}/address")]
        public IActionResult GetCustomerAddress(int customerId)
        {
            var customer = _context.Customers.Find(customerId);

            if (customer == null)
                return NotFound(new { hasAddress = false, customerName = "Unknown" });

            bool hasAddress = !string.IsNullOrEmpty(customer.Address);

            return Ok(new
            {
                hasAddress,
                address = customer.Address ?? "",
                customerName = customer.Name
            });
        }

        [AllowAnonymous]
        [HttpPost("CreateOrder")]
        //[Permission("Manage POS")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }

            // ============================
            // 1️⃣ Basic request validation
            // ============================
            if (request == null)
                return BadRequest(new { message = "Invalid request data." });

            if (request.OrderItems == null || !request.OrderItems.Any())
                return BadRequest(new { message = "Order must contain at least one item." });

            if (request.SubTotal <= 0 || request.GrandTotal <= 0)
                return BadRequest(new { message = "Invalid order amount." });

            // ============================
            // 2️⃣ OrderType based validation
            // ============================
            if (request.OrderType == "DineIn" && request.TableId == null)
                return BadRequest(new { message = "Table is required for Dine-In orders." });

            if (request.OrderType == "TakeAway" && request.PickUpId == null)
                return BadRequest(new { message = "Pickup counter is required for Takeaway orders." });

            if (request.OrderType == "Delivery" &&
                string.IsNullOrWhiteSpace(request.DelivaryAddress))
                return BadRequest(new { message = "Delivery address is required for delivery orders." });

            // ============================
            // 3️⃣ ItemId exists in DB check
            // ============================
            var itemIds = request.OrderItems.Select(x => x.ItemId).ToList();

            var dbItems = await _context.Items
                .Where(i => itemIds.Contains(i.ItemId))
                .Select(i => new { i.ItemId, i.ItemName, })
                .ToListAsync();

            var missingItemIds = itemIds.Except(dbItems.Select(i => i.ItemId)).ToList();

            if (missingItemIds.Any())
            {
                return BadRequest(new
                {
                    message = "Some items were not found.",
                    missingItemIds = missingItemIds
                });
            }

            var inactiveItems = dbItems
                .Select(i => i.ItemName)
                .ToList();

            if (inactiveItems.Any())
            {
                return BadRequest(new
                {
                    message = "Some items are currently unavailable.",
                    items = inactiveItems
                });
            }

            // ============================
            // 4️⃣ Item-level validation
            // ============================
            foreach (var item in request.OrderItems)
            {
                if (item.Quantity <= 0)
                    return BadRequest(new
                    {
                        message = $"Invalid quantity for item {item.ItemName ?? "Unknown"}."
                    });

                if (item.Price <= 0)
                    return BadRequest(new
                    {
                        message = $"Invalid price for item {item.ItemName ?? "Unknown"}."
                    });
            }

            // ============================
            // 5️⃣ Create Order (same logic)
            // ============================
            var order = new SaleOrders
            {
                OrderNumber = $"ORD-{DateTime.Now.Ticks}",
                OrderDate = DateTime.UtcNow,
                OrderType = request.OrderType,
                OrderStatus = "Preparing",
                CustomerId = request.CustomerId ?? 0,
                TableId = request.TableId,
                PickUpId = request.PickUpId,
                DelivaryAddress = request.DelivaryAddress,
                SubTotal = request.SubTotal,
                TaxAmount = (decimal)request.TaxAmount,
                DiscountAmount = (decimal)request.DiscountAmount,
                GrandTotal = (decimal)request.GrandTotal,
                PaymentStatus = "Unpaid",
                Notes = request.Notes
            };

            _context.SaleOrders.Add(order);
            await _context.SaveChangesAsync();

            // ============================
            // 6️⃣ Create Order Items
            // ============================
            foreach (var item in request.OrderItems)
            {
                var orderItem = new SaleOrderItems
                {
                    OrderId = order.OrderId,
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    ItemImage = item.ItemImage,
                    Price = (decimal)item.Price,
                    Quantity = (int)item.Quantity,
                    Total = (decimal)(item.Price * item.Quantity),
                    CustomerId = request.CustomerId
                };

                _context.SaleOrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            // ============================
            // 7️⃣ Success response
            // ============================
            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.OrderId },
                new
                {
                    success = true,
                    message = "Order successfully placed and sent to kitchen.",
                    orderId = order.OrderId,
                    orderNumber = order.OrderNumber
                });
        }


        // ============================
        // PUT: api/SaleOrderApi/{id} - Update order
        // ============================
        [AllowAnonymous]
        [HttpPut("{id}")]
        [Permission("Manage Sales/Payments")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] CreateOrderRequest request)
        {
            try
            {
                var order = await _context.SaleOrders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                    return NotFound(new { message = "Order not found" });

                order.OrderType = request.OrderType;
                order.CustomerId = request.CustomerId ?? 0;
                order.TableId = request.TableId;
                order.PickUpId = request.PickUpId;
                order.DelivaryAddress = request.DelivaryAddress;
                order.SubTotal = request.SubTotal;
                order.TaxAmount = (decimal)request.TaxAmount;
                order.DiscountAmount = (decimal)request.DiscountAmount;
                order.GrandTotal = (decimal)request.GrandTotal;
                order.Notes = request.Notes;

                _context.SaleOrderItems.RemoveRange(order.OrderItems);

                if (request.OrderItems != null && request.OrderItems.Any())
                {
                    foreach (var item in request.OrderItems)
                    {
                        var orderItem = new SaleOrderItems
                        {
                            OrderId = order.OrderId,
                            ItemId = item.ItemId,
                            ItemName = item.ItemName,
                            ItemImage = item.ItemImage,
                            Price = (decimal)item.Price,
                            Quantity = (int)item.Quantity,
                            Total = (decimal)(item.Price * item.Quantity),
                            CustomerId = request.CustomerId
                        };

                        _context.SaleOrderItems.Add(orderItem);
                    }
                }

                _context.Update(order);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Order successfully updated!"
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await OrderExists(id))
                    return NotFound();
                else
                    throw;
            }
        }

        // ============================
        // POST: api/SaleOrderApi/{id}/confirm - Confirm order (Kitchen)
        // ============================
        [AllowAnonymous]
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            var order = await _context.SaleOrders.FindAsync(id);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            order.OrderStatus = "Ready";
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Order marked as Ready" });
        }

        // ============================
        // POST: api/SaleOrderApi/{id}/complete - Complete order
        // ============================
        [AllowAnonymous]
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            var order = await _context.SaleOrders.FindAsync(id);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            order.OrderStatus = "Completed";
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Order completed" });
        }

        // ============================
        // POST: api/SaleOrderApi/hold - Hold order
        // ============================
        [AllowAnonymous]
        [HttpPost("hold")]
        public async Task<IActionResult> HoldOrder([FromBody] HoldOrderViewModel model)
        {
            try
            {
                if (model == null || model.Items == null || !model.Items.Any())
                    return BadRequest(new { success = false, message = "Invalid order data or no items" });

                if (string.IsNullOrEmpty(model.OrderNumber))
                {
                    model.OrderNumber = $"HLD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
                }

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
                    HeldOrderItems = new List<HeldOrdersItem>()
                };

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

                    heldOrder.HeldOrderItems.Add(heldOrderItem);
                }

                _context.HeldOrders.Add(heldOrder);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Order held successfully",
                    orderId = heldOrder.OrderId,
                    orderNumber = heldOrder.OrderNumber
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error holding order: {ex.Message}" });
            }
        }

        // ============================
        // GET: api/SaleOrderApi/held-orders/count
        // ============================
        [AllowAnonymous]
        [HttpGet("held-orders/count")]
        public async Task<IActionResult> GetHeldOrdersCount()
        {
            var count = await _context.HeldOrders.CountAsync();
            return Ok(new { count });
        }

        // ============================
        // GET: api/SaleOrderApi/held-orders - Get all held orders
        // ============================
        [HttpGet("held-orders")]
        public async Task<IActionResult> GetHeldOrders()
        {
            var heldOrders = await _context.HeldOrders
                .Include(h => h.Customer)
                .Include(h => h.Table)
                .Include(h => h.HeldOrderItems)
                .OrderByDescending(h => h.OrderDate)
                .Select(h => new
                {
                    h.OrderId,
                    h.OrderNumber,
                    h.OrderDate,
                    h.OrderType,
                    Customer = h.Customer != null ? new { h.Customer.Id, h.Customer.Name } : null,
                    Table = h.Table != null ? new { h.Table.Id, h.Table.TabName } : null,
                    h.GrandTotal,
                    Items = h.HeldOrderItems.Select(i => new
                    {
                        i.ItemId,
                        i.ItemName,
                        i.ItemImage,
                        i.Price,
                        i.Quantity,
                        i.Total
                    }).ToList()
                })
                .ToListAsync();

            return Ok(heldOrders);
        }

        // ============================
        // DELETE: api/SaleOrderApi/held-orders/{id}
        // ============================
        [AllowAnonymous]
        [HttpDelete("held-orders/{id}")]
        public async Task<IActionResult> DeleteHeldOrder(int id)
        {
            try
            {
                var heldOrder = await _context.HeldOrders
                    .Include(h => h.HeldOrderItems)
                    .FirstOrDefaultAsync(h => h.OrderId == id);

                if (heldOrder == null)
                    return NotFound(new { success = false, message = "Order not found" });

                _context.HeldOrders.Remove(heldOrder);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Held order deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: api/SaleOrderApi/held-orders/{id} - Load held order
        // ============================
        [AllowAnonymous]
        [HttpGet("held-orders/{id}")]
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
                    return NotFound(new { success = false, message = "Order not found" });

                var items = heldOrder.HeldOrderItems.Select(item => new
                {
                    id = item.ItemId,
                    name = item.ItemName,
                    price = item.Price,
                    image = item.ItemImage ?? "/assets/images/upload.jpg",
                    qty = item.Quantity
                }).ToList();

                return Ok(new
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
                        items,
                        customerName = heldOrder.Customer?.Name,
                        tableName = heldOrder.Table?.TabName
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // POST: api/SaleOrderApi/process-payment - Process payment
        // ============================
        [AllowAnonymous]
        [HttpPost("process-payment")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            try
            {
                var order = await _context.SaleOrders.FindAsync(request.OrderId);

                if (order == null)
                    return NotFound(new { success = false, message = "Order not found" });

                order.PaymentId = request.PaymentMethodId;
                order.PaymentStatus = "Paid";
                order.OrderStatus = "Completed";

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Payment processed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: api/SaleOrderApi/items-by-category/{categoryId}
        // ============================
        [AllowAnonymous]
        [HttpGet("items-by-category/{categoryId}")]
        public IActionResult GetItemsByCategory(int categoryId)
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

                return Ok(new { success = true, items });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // GET: api/SaleOrderApi/items - Get all items
        // ============================
        [AllowAnonymous]
        [HttpGet("items")]
        public IActionResult GetAllItems()
        {
            var items = _context.Items
                .Select(x => new
                {
                    itemId = x.ItemId,
                    itemName = x.ItemName,
                    itemPrice = x.ItemPrice,
                    cateImage = x.CateImage,
                    foodCategoryId = x.FoodCategoryId
                })
                .ToList();

            return Ok(new { items });
        }

        // ============================
        // GET: api/SaleOrderApi/{id}/invoice - Get invoice data
        // ============================
        [AllowAnonymous]
        [HttpGet("{id}/invoice")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var order = await _context.SaleOrders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .Include(o => o.PickUp)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            return Ok(order);
        }

        // Helper method
        private async Task<bool> OrderExists(int id)
        {
            return await _context.SaleOrders.AnyAsync(e => e.OrderId == id);
        }

        // ============================
        // DTOs
        // ============================
        public class AssignChefRequest
        {
            public int OrderId { get; set; }
            public int ChefId { get; set; }
        }

        public class UpdateStatusRequest
        {
            public string Status { get; set; } = default!;
        }

        public class CreateOrderRequest
        {
            public string OrderType { get; set; } = default!;
            public int? CustomerId { get; set; }
            public int? TableId { get; set; }
            public int? PickUpId { get; set; }
            public string? DelivaryAddress { get; set; }
            public decimal SubTotal { get; set; }
            public decimal? TaxAmount { get; set; }
            public decimal? DiscountAmount { get; set; }
            public decimal? GrandTotal { get; set; }
            public string? Notes { get; set; }
            public List<OrderItemDto> OrderItems { get; set; } = new();
        }

        public class OrderItemDto
        {
            public int ItemId { get; set; }
            public string? ItemName { get; set; } = default!;
            public string? ItemImage { get; set; }
            public decimal? Price { get; set; }
            public int? Quantity { get; set; }
        }

        public class PaymentRequest
        {
            public int OrderId { get; set; }
            public int PaymentMethodId { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal ReceivedAmount { get; set; }
        }
    }
}
