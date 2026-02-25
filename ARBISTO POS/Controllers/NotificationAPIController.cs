using ARBISTO_POS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ARBISTO_POS.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get unread notifications count
        /// GET: api/notification/unread-count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<object>> GetUnreadCount()
        {
            var count = await _context.Notifications
                .Where(n => !n.IsRead)
                .CountAsync();

            return Ok(new { count });
        }

        /// <summary>
        /// Mark all notifications as read
        /// POST: api/notification/mark-all-read
        /// </summary>
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var unread = await _context.Notifications
                .Where(n => !n.IsRead)
                .ToListAsync();

            if (unread.Count == 0)
                return Ok(new { success = true, message = "No unread notifications." });

            unread.ForEach(n => n.IsRead = true);

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        /// <summary>
        /// Get latest grouped notifications (by ReferenceId)
        /// GET: api/notification/latest
        /// </summary>
        [HttpGet("latest")]
        public async Task<ActionResult<object>> GetLatest()
        {
            var grouped = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .GroupBy(n => n.ReferenceId)
                .Select(g => new
                {
                    referenceId = g.Key,
                    title = g.First().Title,
                    lastMessage = g.First().Message,
                    createdAt = g.First().CreatedAt.ToString("hh:mm tt"),
                    createdAtRaw = g.First().CreatedAt,
                    unreadCount = g.Count(x => !x.IsRead),
                    totalCount = g.Count()
                })
                .Take(10)
                .ToListAsync();

            return Ok(grouped);
        }

        /// <summary>
        /// Mark single notification as read by Id
        /// POST: api/notification/mark-single-read/{id}
        /// </summary>
        [HttpPost("mark-single-read/{id:int}")]
        public async Task<IActionResult> MarkSingleAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound(new { success = false, message = "Notification not found." });

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }

        /// <summary>
        /// Get conversation by ReferenceId (only Type = ItemReady)
        /// GET: api/notification/conversation/{referenceId}
        /// </summary>
        [HttpGet("conversation/{referenceId:int}")]
        public async Task<ActionResult<object>> GetConversation(int referenceId)
        {
            var messages = await _context.Notifications
                .Where(n => n.ReferenceId == referenceId
                            && n.Type == "ItemReady")   // same filter
                .OrderBy(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.IsRead,
                    time = n.CreatedAt.ToString("hh:mm tt")
                })
                .ToListAsync();

            return Ok(messages);
        }

        /// <summary>
        /// Mark all notifications of a ReferenceId as read
        /// POST: api/notification/mark-by-reference/{referenceId}
        /// </summary>
        [HttpPost("mark-by-reference/{referenceId:int}")]
        public async Task<IActionResult> MarkByReference(int referenceId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.ReferenceId == referenceId && !n.IsRead)
                .ToListAsync();

            if (notifications.Count == 0)
                return Ok(new { success = true, message = "No unread notifications for this reference." });

            notifications.ForEach(n => n.IsRead = true);

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        /// <summary>
        /// Delete all notifications
        /// POST: api/notification/delete-all
        /// </summary>
        [HttpPost("delete-all")]
        public async Task<IActionResult> DeleteAll()
        {
            var notifications = await _context.Notifications.ToListAsync();

            if (notifications.Count == 0)
                return Ok(new { success = true, message = "No notifications to delete." });

            _context.Notifications.RemoveRange(notifications);

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
