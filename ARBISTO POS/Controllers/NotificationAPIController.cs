using ARBISTO_POS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ARBISTO_POS.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            ApplicationDbContext context,
            ILogger<NotificationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/notification/unread-count?userId=5
        [HttpGet("unread-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUnreadCount([FromQuery] int userId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid userId."
                    });

                var count = await _context.Notifications
                    .CountAsync(n => !n.IsRead && n.TargetUserId == userId);

                return Ok(new
                {
                    success = true,
                    userId,
                    count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUnreadCount. userId: {UserId}", userId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching unread count.",
                    error = ex.Message
                });
            }
        }

        // POST: api/notification/mark-all-read?userId=5
        [HttpPost("mark-all-read")]
        [AllowAnonymous]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] int userId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid userId."
                    });

                var unreadNotifications = await _context.Notifications
                    .Where(n => !n.IsRead && n.TargetUserId == userId)
                    .ToListAsync();

                if (!unreadNotifications.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No unread notifications found.",
                        affectedRows = 0
                    });
                }

                unreadNotifications.ForEach(n => n.IsRead = true);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "All notifications marked as read.",
                    affectedRows = unreadNotifications.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MarkAllAsRead. userId: {UserId}", userId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while marking notifications as read.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("latest")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLatest([FromQuery] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid userId."
                    });
                }

                var notifications = await _context.Notifications
                    .Where(n => n.TargetUserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new
                    {
                        n.Id,
                        n.ReferenceId,
                        n.Title,
                        n.Message,
                        n.CreatedAt,
                        n.IsRead,
                        n.Type,
                        n.TargetUserId
                    })
                    .ToListAsync();

                var result = notifications
                    .GroupBy(n => n.ReferenceId)
                    .Select(g =>
                    {
                        var latest = g.OrderByDescending(x => x.CreatedAt).First();

                        return new
                        {
                            referenceId = g.Key,
                            title = latest.Title,
                            lastMessage = latest.Message,
                            createdAt = latest.CreatedAt.ToString("hh:mm tt"),
                            createdAtRaw = latest.CreatedAt,
                            unreadCount = g.Count(x => !x.IsRead),
                            totalCount = g.Count()
                        };
                    })
                    .OrderByDescending(x => x.createdAtRaw)
                    .Take(10)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    userId,
                    count = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLatest. userId: {UserId}", userId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching latest notifications.",
                    error = ex.Message
                });
            }
        }

        // POST: api/notification/mark-single-read/10
        [HttpPost("mark-single-read/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> MarkSingleAsRead(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid notification id."
                    });

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (notification == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Notification not found."
                    });
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = "Notification marked as read.",
                    id = notification.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MarkSingleAsRead. id: {Id}", id);

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while marking single notification as read.",
                    error = ex.Message
                });
            }
        }

        // GET: api/notification/conversation/123
        [HttpGet("conversation/{referenceId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConversation(int referenceId)
        {
            try
            {
                if (referenceId <= 0)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid referenceId."
                    });

                var userId = await GetUserIdByOrderIdAsync(referenceId);

                if (userId == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Order not found for the given referenceId."
                    });
                }

                // NOTE:
                // If Type == "ItemReady" is causing empty data,
                // first remove this filter and test again.
                var messages = await _context.Notifications
                    .Where(n =>
                        n.ReferenceId == referenceId &&
                        n.TargetUserId == userId.Value &&
                        n.Type != null &&
                        n.Type.Trim().ToLower() == "itemready"
                    )
                    .OrderBy(n => n.CreatedAt)
                    .Select(n => new
                    {
                        n.Id,
                        n.ReferenceId,
                        n.TargetUserId,
                        n.Type,
                        n.Message,
                        n.IsRead,
                        time = n.CreatedAt.ToString("hh:mm tt"),
                        createdAtRaw = n.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    referenceId,
                    userId,
                    count = messages.Count,
                    data = messages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetConversation. referenceId: {ReferenceId}", referenceId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching conversation.",
                    error = ex.Message
                });
            }
        }

        // POST: api/notification/mark-by-reference/123
        [HttpPost("mark-by-reference/{referenceId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> MarkByReference(int referenceId)
        {
            try
            {
                if (referenceId <= 0)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid referenceId."
                    });

                var userId = await GetUserIdByOrderIdAsync(referenceId);

                if (userId == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Order not found for the given referenceId."
                    });
                }

                var notifications = await _context.Notifications
                    .Where(n =>
                        n.ReferenceId == referenceId &&
                        !n.IsRead &&
                        n.TargetUserId == userId.Value)
                    .ToListAsync();

                if (!notifications.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No unread notifications found for this reference.",
                        affectedRows = 0
                    });
                }

                notifications.ForEach(n => n.IsRead = true);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Notifications marked as read by reference.",
                    affectedRows = notifications.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MarkByReference. referenceId: {ReferenceId}", referenceId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while marking notifications by reference.",
                    error = ex.Message
                });
            }
        }

        // POST: api/notification/delete-all?userId=5
        [HttpPost("delete-all")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteAll([FromQuery] int userId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid userId."
                    });

                var notifications = await _context.Notifications
                    .Where(n => n.TargetUserId == userId)
                    .ToListAsync();

                if (!notifications.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No notifications found to delete.",
                        affectedRows = 0
                    });
                }

                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "All notifications deleted successfully.",
                    affectedRows = notifications.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAll. userId: {UserId}", userId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting notifications.",
                    error = ex.Message
                });
            }
        }

        // OPTIONAL DEBUG API
        // GET: api/notification/debug-by-reference/123
        [HttpGet("debug-by-reference/{referenceId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugByReference(int referenceId)
        {
            try
            {
                if (referenceId <= 0)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid referenceId."
                    });

                var order = await _context.SaleOrders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderId == referenceId);

                var notifications = await _context.Notifications
                    .Where(n => n.ReferenceId == referenceId)
                    .OrderBy(n => n.CreatedAt)
                    .Select(n => new
                    {
                        n.Id,
                        n.ReferenceId,
                        n.TargetUserId,
                        n.Type,
                        n.Message,
                        n.IsRead,
                        n.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    referenceId,
                    orderFound = order != null,
                    orderUserId = order != null ? order.CreatedByUserId : (int?)null,
                    notificationCount = notifications.Count,
                    data = notifications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DebugByReference. referenceId: {ReferenceId}", referenceId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while debugging notification data.",
                    error = ex.Message
                });
            }
        }

        // HELPER: get user id from order id
        private async Task<int?> GetUserIdByOrderIdAsync(int orderId)
        {
            var order = await _context.SaleOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return null;

            // IMPORTANT:
            // If Notifications.TargetUserId is linked with CustomerId or some other field,
            // replace CreatedByUserId with the correct property.
            return order.CreatedByUserId;
        }
    }
}