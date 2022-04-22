using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaceA.Model;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace SpaceA.WebApi.Hubs
{
    public class WebhookHub : Hub
    {
        public static List<ConnectionUser> Users { get; } = new List<ConnectionUser>();

        public override Task OnConnectedAsync()
        {
            var userName = Context.GetHttpContext().Request.Query["UserName"];
            var user = Users.SingleOrDefault(u => u.ConnectionID == Context.ConnectionId);
            if (user == null)
            {
                user = new ConnectionUser(userName, Context.ConnectionId);
                Users.Add(user);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var user = Users.FirstOrDefault(p => p.ConnectionID == Context.ConnectionId);
            if (user != null)
            {
                Users.Remove(user);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public void SendToMessage(string connectionId, string message)
        {
            var user = Users.FirstOrDefault(s => s.ConnectionID == connectionId);
            if (user != null)
            {
                Clients.Client(connectionId).SendAsync(message);
            }
        }

        public string GetUsers()
        {
            var list = Users.Select(s => new { s.Name, s.ConnectionID }).ToList();
            string jsonList = JsonConvert.SerializeObject(list);
            return jsonList;
        }
    }
}