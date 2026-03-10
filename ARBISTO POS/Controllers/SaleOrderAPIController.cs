using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Hubs;
using ARBISTO_POS.Models;
using ARBISTO_POS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace ARBISTO_POS.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    //[Permission("Manage Sales/Payments")]
    public class SaleOrderApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public SaleOrderApiController(ApplicationDbContext context , IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        //// ============================
        //// GET: api/SaleOrderApi - List all orders
        //// ============================
        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> GetAllOrders()
        //{
        //    var orders = await _context.SaleOrders
        //        .Include(o => o.Customer)
        //        .Include(o => o.OrderItems)
        //        .Select(o => new
        //        {
        //            o.OrderId,
        //            o.OrderNumber,
        //            o.OrderDate,
        //            o.OrderType,
        //            o.OrderStatus,
        //            Customer = o.Customer != null ? new { o.Customer.Id, o.Customer.Name } : null,
        //            o.SubTotal,
        //            o.TaxAmount,
        //            o.DiscountAmount,
        //            o.GrandTotal,
        //            o.PaymentStatus,
        //            OrderItems = o.OrderItems.Select(i => new
        //            {
        //                i.ItemId,
        //                i.ItemName,
        //                i.Price,
        //                i.Quantity,
        //                i.Total
        //            }).ToList()
        //        })
        //        .ToListAsync();

        //    return Ok(orders);
        //}
        // ============================
        // GET: api/SaleOrderApi/user-orders/{userId}
        // ============================

        [HttpGet("user-orders/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrdersByUser(int userId)
        {
            try
            {
                var orders = await _context.SaleOrders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                    .Include(o => o.CreatedByUser)
                    .Where(o => o.CreatedByUserId == userId)
                    .OrderByDescending(o => o.OrderId)
                    .ThenByDescending(o => o.OrderDate)
                    .Select(o => new
                    {
                        orderId = o.OrderId,
                        orderNumber = o.OrderNumber,
                        orderDate = o.OrderDate,
                        orderType = o.OrderType,
                        orderStatus = o.OrderStatus,
                        tableId = o.TableId,
                        paymentStatus = o.PaymentStatus,
                        subTotal = o.SubTotal,
                        taxAmount = o.TaxAmount,
                        discountAmount = o.DiscountAmount,
                        grandTotal = o.GrandTotal,
                        customerId = o.CustomerId,
                        createdByUserId = o.CreatedByUserId,

                        itemsCount = o.OrderItems.Count,

                        orderItems = o.OrderItems.Select(i => new
                        {
                            orderItemId = i.OrderItemId,
                            itemId = i.ItemId,
                            itemName = i.ItemName,
                            itemImage = i.ItemImage,
                            price = i.Price,
                            quantity = i.Quantity,
                            total = i.Total,
                            isPrepared = i.IsPrepared
                        })
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    totalOrders = orders.Count,
                    orders = orders
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
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
        public async Task<IActionResult> GetTables()
        {
            try
            {
                var activeStatuses = new[] { "Preparing", "Ready", "Pending" };

                bool isSingleTableMultiOrderEnabled = await IsSingleTableMultiOrderEnabled();

                var allTables = await _context.ServiceTables.ToListAsync();

                var occupiedTableIds = await _context.SaleOrders
                    .Where(o => activeStatuses.Contains(o.OrderStatus) && o.TableId > 0)
                    .Select(o => o.TableId)
                    .Distinct()
                    .ToListAsync();

                var tables = allTables.Select(table =>
                {
                    bool isServing = occupiedTableIds.Contains(table.Id);
                    bool selectable = !isServing || isSingleTableMultiOrderEnabled;

                    return new
                    {
                        id = table.Id,
                        tableName = table.TabName,
                        status = isServing ? "Serving" : "Available",
                        selectable = selectable,
                        statusMessage = isServing && isSingleTableMultiOrderEnabled
                            ? "Serving, but you can also add this table"
                            : (isServing ? "Serving" : "Available")
                    };
                }).ToList();

                return Ok(new
                {
                    success = true,
                    singleTableMultiOrderEnabled = isSingleTableMultiOrderEnabled,
                    tables = tables
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        private async Task<bool> IsSingleTableMultiOrderEnabled()
        {
            var setting = await _context.AppSetttings.FirstOrDefaultAsync();

            if (setting == null)
                return false;

            return string.Equals(setting.SingleTableMultiOrder, "Yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(setting.SingleTableMultiOrder, "True", StringComparison.OrdinalIgnoreCase);
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

            if (request == null)
                return BadRequest(new { message = "Invalid request data." });

            if (request.OrderItems == null || !request.OrderItems.Any())
                return BadRequest(new { message = "Order must contain at least one item." });

            if (request.SubTotal <= 0 || request.GrandTotal <= 0)
                return BadRequest(new { message = "Invalid order amount." });

            // ============================
            // OrderType validation
            // ============================
            if (request.OrderType == "DineIn")
            {
                if (request.TableId == null)
                    return BadRequest(new { message = "Table is required for Dine-In orders." });

                // 🔹 Table existence check
                var tableExists = await _context.ServiceTables
                    .AnyAsync(t => t.Id == request.TableId.Value);

                if (!tableExists)
                    return BadRequest(new { message = $"Table with Id {request.TableId.Value} does not exist." });
            }

            if (request.OrderType == "TakeAway")
            {
                if (request.PickUpId == null)
                    return BadRequest(new { message = "Pickup counter is required for Takeaway orders." });
            }

            if (request.OrderType == "Delivery" &&
                string.IsNullOrWhiteSpace(request.DelivaryAddress))
                return BadRequest(new { message = "Delivery address is required for delivery orders." });

            // ============================
            // Item existence check
            // ============================
            var itemIds = request.OrderItems.Select(x => x.ItemId).ToList();

            var dbItemIds = await _context.Items
                .Where(i => itemIds.Contains(i.ItemId))
                .Select(i => i.ItemId)
                .ToListAsync();

            var missingItemIds = itemIds.Except(dbItemIds).ToList();

            if (missingItemIds.Any())
            {
                return BadRequest(new
                {
                    message = "Some items were not found.",
                    missingItemIds
                });
            }

            // ============================
            // Item-level validation
            // ============================
            foreach (var item in request.OrderItems)
            {
                if (item.Quantity <= 0)
                    return BadRequest(new { message = "Item quantity must be greater than zero." });

                if (item.Price <= 0)
                    return BadRequest(new { message = "Item price must be greater than zero." });
            }

            //UserId Save 
            //var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // logged-in user id
            int userId;

            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(claimId) && int.TryParse(claimId, out var parsed))
            {
                userId = parsed; // logged-in user
            }
            else
            {
                if (request.CreatedByUserId == null || request.CreatedByUserId <= 0)
                    return BadRequest(new { message = "createdByUserId is required for anonymous requests." });

                // ✅ validate user exists
                var userExists = await _context.AppUsers.AnyAsync(u => u.Id == request.CreatedByUserId.Value);
                if (!userExists)
                    return BadRequest(new { message = $"User with Id {request.CreatedByUserId.Value} does not exist." });

                userId = request.CreatedByUserId.Value;
            }

            // ============================
            // Create Order
            // ============================
            var order = new SaleOrders
            {
                OrderNumber = $"ORD-{DateTime.UtcNow.Ticks}",
                OrderDate = DateTime.UtcNow,
                OrderType = request.OrderType,
                OrderStatus = "Preparing",
                CustomerId = request.CustomerId,  // NULL allowed
                TableId = request.TableId,
                PickUpId = request.PickUpId,
                DelivaryAddress = request.DelivaryAddress,
                SubTotal = request.SubTotal,
                TaxAmount = request.TaxAmount ?? 0,
                DiscountAmount = request.DiscountAmount ?? 0,
                GrandTotal = request.GrandTotal ?? 0,
                PaymentStatus = "Unpaid",
                Notes = request.Notes,
                CreatedByUserId = userId   // ✅ yahan set
            };

            _context.SaleOrders.Add(order);
            await _context.SaveChangesAsync();

            // ============================
            // Create Order Items
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

            // ===============================
            // CREATE NOTIFICATION (Kitchen)
            // ===============================

            // ✅ Order.Table nav property API flow me usually loaded nahi hoti
            // So table name safe tareeqe se le lo (logic same: title table name hi hai)
            string title = "Kitchen";

            if (order.TableId.HasValue && order.TableId.Value > 0)
            {
                var table = await _context.ServiceTables
                    .FirstOrDefaultAsync(t => t.Id == order.TableId.Value);

                title = table?.TabName ?? $"Table #{order.TableId.Value}";
            }

            var notification = new Notification
            {
                Title = title,
                Message = $"Order #{order.OrderNumber} is waiting in kitchen.",
                Type = "Kitchen",
                ReferenceId = order.OrderId,

                // ✅ Kitchen notifications are global (TargetUserId = null)
                TargetUserId = null,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // ===============================
            // SEND REAL-TIME SIGNALR ALERT
            // ===============================

            // ✅ Existing behavior untouched: kitchen page/clients ko broadcast
            await _hubContext.Clients.All.SendAsync(
                "ReceiveKitchenNotification",
                notification.Title,
                notification.Message
            );

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.OrderId },
                new
                {
                    success = true,
                    message = "Order successfully placed.",
                    orderId = order.OrderId,
                    orderNumber = order.OrderNumber
                });
        }

        // ============================
        // Item Ready Notification API (PER USER)
        // ============================
        // POST: api/SaleOrderApi/item-ready
        [HttpPost("item-ready")]
        public async Task<IActionResult> NotifyItemReady([FromBody] ItemReadyRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { success = false, message = "Invalid request." });

                var order = await _context.SaleOrders
                    .Include(o => o.Customer)
                    .Include(o => o.Table)
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);

                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                // ✅ IMPORTANT: jis user ne order place kiya tha, sirf usi ko notify karna hai
                var targetUserId = order.CreatedByUserId;

                var customerName = order.Customer?.Name ?? "Walking Customer";
                var locationInfo = "";

                if (order.OrderType == "DINE IN")
                    locationInfo = $"Table: {order.Table?.TabName}";
                else if (order.OrderType == "DELIVERY")
                    locationInfo = "Delivery Order";
                else if (order.OrderType == "PICKUP POINT")
                    locationInfo = "Pickup Order";

                var notification = new Notification
                {
                    Title = "Item Ready",
                    Message = $"{locationInfo} | {customerName} | Order #{order.OrderNumber} → {request.ItemName} Ready",
                    Type = "ItemReady",
                    ReferenceId = request.OrderId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,

                    // ✅ NEW: per-user bell/dropdown support
                    TargetUserId = targetUserId
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // ✅ NEW: realtime ONLY to order creator (POS screen)
                //await _hubContext.Clients.User(targetUserId.ToString())
                await _hubContext.Clients.All
                    .SendAsync("ReceiveItemReadyNotification", notification.Title, notification.Message);

                return Ok(new
                {
                    success = true,
                    notificationId = notification.Id,
                    title = notification.Title,
                    message = notification.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Request DTO
        public class ItemReadyRequest
    {
        public int OrderId { get; set; }
        public string ItemName { get; set; } = string.Empty;
    }

        // ============================
        // GET: api/SaleOrderApi/table/{tableId}/edit-order
        // ✅ Load existing order by TableId
        // ============================
        [HttpGet("{orderId}/edit")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrderForEditByOrderId(int orderId)
        {
            try
            {
                var order = await _context.SaleOrders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No order found for OrderId {orderId}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        orderId = order.OrderId,
                        orderNumber = order.OrderNumber,
                        orderDate = order.OrderDate,
                        orderType = order.OrderType,
                        orderStatus = order.OrderStatus,
                        tableId = order.TableId,
                        customerId = order.CustomerId,
                        pickUpId = order.PickUpId,
                        delivaryAddress = order.DelivaryAddress,
                        subTotal = order.SubTotal,
                        taxAmount = order.TaxAmount,
                        discountAmount = order.DiscountAmount,
                        grandTotal = order.GrandTotal,
                        notes = order.Notes,
                        orderItems = order.OrderItems.Select(i => new
                        {
                            orderItemId = i.OrderItemId,
                            itemId = i.ItemId,
                            itemName = i.ItemName,
                            itemImage = i.ItemImage,
                            price = i.Price,
                            quantity = i.Quantity,
                            total = i.Total,
                            isPrepared = i.IsPrepared
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        // ============================
        // PUT: api/SaleOrderApi/table/{tableId}/edit
        // ✅ Edit existing order by TableId
        // ✅ After edit OrderStatus => Preparing
        // ✅ Payment fields untouched
        // ============================
        [HttpPut("{orderId}/edit")]
        [AllowAnonymous]
        public async Task<IActionResult> EditOrderByOrderId(int orderId, [FromBody] CreateOrderRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { success = false, message = "Invalid request." });

                var order = await _context.SaleOrders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                    return NotFound(new { success = false, message = $"No order found for OrderId {orderId}" });

                // ============================
                // Update order header only
                // ============================
                order.OrderType = request.OrderType;
                order.CustomerId = request.CustomerId;
                order.TableId = request.TableId;
                order.PickUpId = request.PickUpId;
                order.DelivaryAddress = request.DelivaryAddress;
                order.SubTotal = request.SubTotal;
                order.TaxAmount = request.TaxAmount ?? order.TaxAmount;
                order.DiscountAmount = request.DiscountAmount ?? order.DiscountAmount;
                order.GrandTotal = request.GrandTotal ?? order.GrandTotal;
                order.Notes = request.Notes;

                // After edit order goes back to preparing
                order.OrderStatus = "Preparing";

                var existingItems = order.OrderItems?.ToList() ?? new List<SaleOrderItems>();
                var requestItems = request.OrderItems?.ToList() ?? new List<OrderItemDto>();

                // ============================
                // Update existing items / Add new items
                // ============================
                foreach (var reqItem in requestItems)
                {
                    if ((reqItem.Quantity ?? 0) <= 0)
                        return BadRequest(new { success = false, message = $"Invalid quantity for ItemId {reqItem.ItemId}" });

                    if ((reqItem.Price ?? 0) <= 0)
                        return BadRequest(new { success = false, message = $"Invalid price for ItemId {reqItem.ItemId}" });

                    // Better approach:
                    // old items should come with OrderItemId (or SaleOrderItemId) from frontend
                    SaleOrderItems? existingItem = null;

                    if (reqItem.OrderItemId > 0)
                    {
                        existingItem = existingItems.FirstOrDefault(x => x.OrderItemId == reqItem.OrderItemId);
                    }
                    else
                    {
                        existingItem = existingItems.FirstOrDefault(x => x.ItemId == reqItem.ItemId);
                    }

                    if (existingItem != null)
                    {
                        // Update existing row
                        // KEEP IsPrepared as it is
                        existingItem.ItemName = reqItem.ItemName;
                        existingItem.ItemImage = reqItem.ItemImage;
                        existingItem.Price = reqItem.Price ?? 0;
                        existingItem.Quantity = reqItem.Quantity ?? 0;
                        existingItem.Total = (reqItem.Price ?? 0) * (reqItem.Quantity ?? 0);
                        existingItem.CustomerId = request.CustomerId;
                    }
                    else
                    {
                        // Add new item
                        _context.SaleOrderItems.Add(new SaleOrderItems
                        {
                            OrderId = order.OrderId,
                            ItemId = reqItem.ItemId,
                            ItemName = reqItem.ItemName,
                            ItemImage = reqItem.ItemImage,
                            Price = reqItem.Price ?? 0,
                            Quantity = reqItem.Quantity ?? 0,
                            Total = (reqItem.Price ?? 0) * (reqItem.Quantity ?? 0),
                            CustomerId = request.CustomerId,
                            IsPrepared = false
                        });
                    }
                }

                // ============================
                // Remove deleted items
                // ============================
                var requestOrderItemIds = requestItems
                    .Where(x => x.OrderItemId > 0)
                    .Select(x => x.OrderItemId)
                    .ToList();

                var requestNewItemIds = requestItems
                    .Where(x => x.OrderItemId <= 0)
                    .Select(x => x.ItemId)
                    .ToList();

                var itemsToRemove = existingItems
                    .Where(x =>
                        !requestOrderItemIds.Contains(x.OrderItemId) &&
                        !requestNewItemIds.Contains(x.ItemId))
                    .ToList();

                if (itemsToRemove.Any())
                {
                    _context.SaleOrderItems.RemoveRange(itemsToRemove);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Order updated successfully. Existing item preparation status preserved, new items set to unprepared.",
                    orderId = order.OrderId,
                    tableId = order.TableId,
                    orderStatus = order.OrderStatus
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
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
        // POST: api/SaleOrderApi/hold
        // ============================
        [HttpPost("hold")]
        [AllowAnonymous]
        public async Task<IActionResult> HoldOrder([FromBody] HoldOrderViewModel model)
        {
            try
            {
                if (model == null || model.Items == null || !model.Items.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid order data or no items"
                    });
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
                    HeldOrderItems = new List<HeldOrdersItem>()
                };

                // Add items
                foreach (var item in model.Items)
                {
                    heldOrder.HeldOrderItems.Add(new HeldOrdersItem
                    {
                        CustomerId = model.CustomerId,
                        ItemId = item.ItemId,
                        ItemName = item.ItemName,
                        ItemImage = item.ItemImage,
                        Price = item.Price ?? 0,
                        Quantity = item.Quantity ?? 0,
                        Total = item.Total ?? 0
                    });
                }

                // Save
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
                return BadRequest(new
                {
                    success = false,
                    message = $"Error holding order: {ex.Message}"
                });
            }
        }
        
        // ============================
        // GET: api/SaleOrderApi/held-orders
        // ============================

        [AllowAnonymous]
        [HttpGet("held-orders")]
        public async Task<IActionResult> GetHeldOrders([FromQuery] int? id)
        {
            try
            {
                // ===============================
                // GET SINGLE ORDER
                // ===============================
                if (id.HasValue)
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

                // ===============================
                // GET ALL ORDERS
                // ===============================
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

                return Ok(new
                {
                    success = true,
                    totalRecords = heldOrders.Count,
                    orders = heldOrders
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
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
                var heldOrderToDelete = await _context.HeldOrders
                    .Include(h => h.HeldOrderItems)
                    .FirstOrDefaultAsync(h => h.OrderId == id);

                if (heldOrderToDelete == null)
                    return NotFound(new { success = false, message = "Order not found" });

                _context.HeldOrders.Remove(heldOrderToDelete);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Held order deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
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
            public int? CreatedByUserId { get; set; }
        }
         
        public class OrderItemDto
        {
            public int OrderItemId { get; set; }   // old item ke liye ayega, new item ke liye 0
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
