using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;


namespace Team1.VitalBridge.Frontend.Hubs
{
    public class ChatHub : Hub
    {
        public static Dictionary<string, string> UserConnectionMap = new();
        public static List<string> customerServiceId = new List<string>(); //服務中客服
        public static Dictionary<string, string> UserServiceConnectionMap = new();
        public static List<string> customerServiceIdReady = new List<string>(); //空閒客服
        /// <summary>
        /// 傳遞訊息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// 
        public async Task SendMessage(string user, string message)
        {
            var id = Context.ConnectionId;
            UserConnectionMap[user] = id;
            if (UserServiceConnectionMap.TryGetValue(user, out var connectionId) && customerServiceId.Contains(connectionId))
            {
                await Clients.Client(connectionId).SendAsync("UpdContent", message);
            }
            else if(customerServiceIdReady.Count!=0)
            {
                //隨機從customerServiceId選一個客服
                var random = new Random();
                var index = random.Next(customerServiceIdReady.Count);
                var serviceId = customerServiceIdReady[index];
                customerServiceIdReady.Remove(serviceId);
                customerServiceId.Add(serviceId);
                UserServiceConnectionMap[user] = serviceId;
                await Clients.Client(serviceId).SendAsync("user",user);
                await Clients.Client(serviceId).SendAsync("system", message+"已進入對話");
                await Clients.Client(serviceId).SendAsync("userName",message);
                await Clients.Caller.SendAsync("UpdContent", $"**已連線真人客服**");
            }
            else
            {
                await Clients.Caller.SendAsync("UpdContent", $"當前無空閒客服，請稍後在試。");
                Thread.Sleep(2000);
                await Clients.Caller.SendAsync("UpdContent", "**正在轉接智能客服**");
                Thread.Sleep(3000);
                await Clients.Caller.SendAsync("openAgent");

            }
        }

        public async Task addUser(string user, string message)
        {
            var id = Context.ConnectionId;
            UserConnectionMap[user] = id;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            // 判斷是否為客服
            if (customerServiceId.Contains(connectionId) || customerServiceIdReady.Contains(connectionId))
            {
                var serviceUser = UserServiceConnectionMap.FirstOrDefault(x=>x.Value==connectionId).Key;
                if(UserConnectionMap.TryGetValue(serviceUser,out var userconnectionId))
                {
                    await Clients.Client(userconnectionId).SendAsync("UpdContent", "**客服已中斷連線**");
                    Thread.Sleep(2000);
                    await Clients.Client(userconnectionId).SendAsync("UpdContent", "**正在轉接智能客服**");
                    Thread.Sleep(3000);
                    await Clients.Client(userconnectionId).SendAsync("openAgent");
                }
                // 呼叫登出邏輯
                await CustomerServiceLogout(string.Empty, "連線中斷自動登出");
            }

            // 處理使用者斷線
            var user = UserConnectionMap.FirstOrDefault(x => x.Value == connectionId).Key;
            if (!string.IsNullOrEmpty(user))
            {
                UserConnectionMap.Remove(user);
                if (UserServiceConnectionMap.TryGetValue(user, out var serviceId))
                {
                    await Clients.Client(serviceId).SendAsync("system", $"使用者已離線");
                    await Clients.Client(serviceId).SendAsync("clearUser");
                    customerServiceIdReady.Add(serviceId);
                    customerServiceId.Remove(serviceId);
                    UserServiceConnectionMap.Remove(user);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToUser(string user, string message)
        {
            if (UserConnectionMap.TryGetValue(user, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("UpdContent", message);
            }
            else
            {
                // 使用者不在線上或尚未建立連線
                await Clients.Caller.SendAsync("system", $"使用者 {user} 已離線");
            }
        }

        public async Task customerServiceLogin(string user, string message)
        {
            var id = Context.ConnectionId;
            if (!customerServiceIdReady.Contains(id) && !customerServiceId.Contains(id))
            {
                customerServiceIdReady.Add(id);
            }
        }
        public async Task CustomerServiceLogout(string user, string message)
        {
            var id = Context.ConnectionId;
            customerServiceId.Remove(id);
            customerServiceIdReady.Remove(id);

            await Clients.Caller.SendAsync("UpdContent", "客服已成功登出");

        }
        public async Task newNotify(string user, string message)
        {
            if (UserConnectionMap.TryGetValue(user, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("newNotify", message);
            }

        }
        public async Task CloseConversation(string user, string message)
        {
            var id = Context.ConnectionId;
            customerServiceIdReady.Add(id);
            customerServiceId.Remove(id);
            if (UserConnectionMap.TryGetValue(user, out var connectionId))
            {
                await Clients.Client(UserConnectionMap[user]).SendAsync("UpdContent", "客服已結束對話，感謝您的聯繫");
                UserServiceConnectionMap.Remove(user);
            }
        }

    }
}
