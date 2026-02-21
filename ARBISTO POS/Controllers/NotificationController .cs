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
        var notifications = await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.IsRead,
                n.ReferenceId,
                CreatedAt = n.CreatedAt.ToString("hh:mm tt")
            })
            .ToListAsync();

        return Json(notifications);
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
}