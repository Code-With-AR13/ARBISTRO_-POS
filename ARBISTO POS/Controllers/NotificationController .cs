using ARBISTO_POS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        var count = await _context.Notifications
            .Where(n => !n.IsRead).CountAsync();

        return Json(new { count });
    }
    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var unread = await _context.Notifications
            .Where(n => !n.IsRead)
            .ToListAsync();

        unread.ForEach(n => n.IsRead = true);

        await _context.SaveChangesAsync();

        return Ok();
    }
    [HttpGet]
    public async Task<IActionResult> GetLatest()
    {
        var grouped = await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .GroupBy(n => n.ReferenceId)
            .Select(static g => new
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
        var messages = await _context.Notifications
            .Where(n => n.ReferenceId == referenceId
                        && n.Type == "ItemReady")   // 🔥 FILTER ADDED
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
        var notifications = await _context.Notifications
            .Where(n => n.ReferenceId == referenceId && !n.IsRead)
            .ToListAsync();

        notifications.ForEach(n => n.IsRead = true);

        await _context.SaveChangesAsync();

        return Ok();
    }
    [HttpPost]
    public async Task<IActionResult> DeleteAll()
    {
        var notifications = await _context.Notifications.ToListAsync();

        _context.Notifications.RemoveRange(notifications);

        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }
}