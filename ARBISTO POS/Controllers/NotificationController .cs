using ARBISTO_POS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
public class NotificationController : Controller
{
    private readonly ApplicationDbContext _context;

    public NotificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();

        // ✅ Only this user's unread notifications
        var count = await _context.Notifications
            .Where(n => !n.IsRead && n.TargetUserId == userId)
            .CountAsync();

        return Json(new { count });
    }
    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();

        // ✅ Only this user's unread notifications
        var unread = await _context.Notifications
            .Where(n => !n.IsRead && n.TargetUserId == userId)
            .ToListAsync();

        unread.ForEach(n => n.IsRead = true);
        await _context.SaveChangesAsync();

        return Ok();
    }
    [HttpGet]
    public async Task<IActionResult> GetLatest()
    {
        var userId = GetCurrentUserId();

        // ✅ Only this user's notifications
        var grouped = await _context.Notifications
            .Where(n => n.TargetUserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .GroupBy(n => n.ReferenceId)
            .Select(g => new
            {
                referenceId = g.Key,
                title = g.First().Title,
                lastMessage = g.First().Message,
                createdAt = g.First().CreatedAt.ToString("hh:mm tt"),
                CreatedAtRaw = g.First().CreatedAt,
                unreadCount = g.Count(x => !x.IsRead),
                totalCount = g.Count()
            })
            .Take(10)
            .ToListAsync();

        return Json(grouped);
    }
    [HttpPost]
    public async Task<IActionResult> MarkSingleAsRead(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return Ok();
    }
    [HttpGet]
    public async Task<IActionResult> GetConversation(int referenceId)
    {
        var userId = GetCurrentUserId();

        // ✅ Only messages of this user for this order reference
        var messages = await _context.Notifications
            .Where(n =>
                n.ReferenceId == referenceId &&
                n.Type == "ItemReady" &&
                n.TargetUserId == userId
            )
            .OrderBy(n => n.CreatedAt)
            .Select(n => new
            {
                n.Id,
                n.Message,
                n.IsRead,
                Time = n.CreatedAt.ToString("hh:mm tt")
            })
            .ToListAsync();

        return Json(messages);
    }
    [HttpPost]
    public async Task<IActionResult> MarkByReference(int referenceId)
    {
        var userId = GetCurrentUserId();

        var notifications = await _context.Notifications
            .Where(n => n.ReferenceId == referenceId && !n.IsRead && n.TargetUserId == userId)
            .ToListAsync();

        notifications.ForEach(n => n.IsRead = true);
        await _context.SaveChangesAsync();

        return Ok();
    }
    [HttpPost]
    public async Task<IActionResult> DeleteAll()
    {
        var userId = GetCurrentUserId();

        // ✅ Only delete this user's notifications
        var notifications = await _context.Notifications
            .Where(n => n.TargetUserId == userId)
            .ToListAsync();

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    private int GetCurrentUserId()
    {
        // ✅ POS user id (same as CreatedByUserId mapping)
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    }
}