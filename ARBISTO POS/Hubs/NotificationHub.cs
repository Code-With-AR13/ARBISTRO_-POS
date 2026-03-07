//using Microsoft.AspNetCore.SignalR;
//using System.Threading.Tasks;

//namespace ARBISTO_POS.Hubs
//{
//    public class NotificationHub : Hub
//    {
//        public async Task SendKitchenNotification(string title, string message)
//        {
//            await Clients.All.SendAsync("ReceiveKitchenNotification", title, message);
//        }
//    }
//}
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ARBISTO_POS.Hubs
{
    public class NotificationHub : Hub
    {
        //public override async Task OnConnectedAsync()
        //{
        //    var httpContext = Context.GetHttpContext();

        //    // Flutter / custom client
        //    var queryUserId = httpContext?.Request.Query["userId"].ToString();

        //    // Website / MVC / cookie auth
        //    var claimUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    var userId = !string.IsNullOrWhiteSpace(queryUserId)
        //        ? queryUserId
        //        : claimUserId;

        //    if (!string.IsNullOrWhiteSpace(userId))
        //    {
        //        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        //        System.Console.WriteLine($"✅ Connected: {Context.ConnectionId} => user-{userId}");
        //    }
        //    else
        //    {
        //        System.Console.WriteLine($"⚠ Connected without user mapping: {Context.ConnectionId}");
        //    }

        //    await base.OnConnectedAsync();
        //}

        //public override async Task OnDisconnectedAsync(System.Exception? exception)
        //{
        //    var httpContext = Context.GetHttpContext();

        //    var queryUserId = httpContext?.Request.Query["userId"].ToString();
        //    var claimUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    var userId = !string.IsNullOrWhiteSpace(queryUserId)
        //        ? queryUserId
        //        : claimUserId;

        //    if (!string.IsNullOrWhiteSpace(userId))
        //    {
        //        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        //        System.Console.WriteLine($"🔌 Disconnected: {Context.ConnectionId} => user-{userId}");
        //    }

        //    await base.OnDisconnectedAsync(exception);
        //}
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            // Flutter / custom client query string se userId bhejta hai
            var queryUserId = httpContext?.Request.Query["userId"].ToString();

            // Website POS claims se kaam karegi through Clients.User(...)
            if (!string.IsNullOrWhiteSpace(queryUserId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{queryUserId}");
                System.Console.WriteLine($"✅ Flutter connected => user-{queryUserId}");
            }
            else
            {
                System.Console.WriteLine($"✅ Website connected => {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var queryUserId = httpContext?.Request.Query["userId"].ToString();

            if (!string.IsNullOrWhiteSpace(queryUserId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{queryUserId}");
                System.Console.WriteLine($"🔌 Flutter disconnected => user-{queryUserId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendKitchenNotification(string title, string message)
        {
            await Clients.All.SendAsync("ReceiveKitchenNotification", title, message);
        }
    }
}