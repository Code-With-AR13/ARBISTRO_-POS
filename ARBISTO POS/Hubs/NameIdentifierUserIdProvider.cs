using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ARBISTO_POS.Hubs
{
    /// <summary>
    /// ✅ SignalR ko batata hai ke "Clients.User(userId)" ke liye userId kya hoga.
    /// Hum ClaimTypes.NameIdentifier use kar rahe hain (ASP.NET Identity ka standard).
    /// </summary>
    public class NameIdentifierUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Logged-in user's unique id (string) return karega
            return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}