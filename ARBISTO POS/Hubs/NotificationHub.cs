using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ARBISTO_POS.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendKitchenNotification(string title, string message)
        {
            await Clients.All.SendAsync("ReceiveKitchenNotification", title, message);
        }
    }
}