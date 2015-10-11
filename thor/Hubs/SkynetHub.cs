using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;

using Rnet_Base.Handlers.RealTime;
using Rnet_Base.Handlers.Commands;

namespace thor.Hubs
{
    [HubName("Skynet")]
    public class SkynetHub : Hub
    {
        public void SendEvent(string message)
        {
            var msg = JsonConvert.DeserializeObject<Event>(message);
            Clients.Group("webclients").broadcastMessage(msg.ServerName, message);
            //Clients.Others.broadcastMessage(msg.ServerName, message);
        }

        public void SendIngameMessage(SendIngameMessage msg)
        {
            Clients.Group("nodes").IngameMessage(msg);
        }

        #region Group management
        public void JoinGroup(string groupName)
        {
            // TODO: Add authentication for this request
            Groups.Add(Context.ConnectionId, groupName);
            Clients.Group(groupName).GroupMessage(Context.User.Identity.Name + " joined.");
        }

        public Task LeaveGroup(string groupName)
        {
            return Groups.Remove(Context.ConnectionId, groupName);
        }
        #endregion

        public void SpawnInstance(string groupName, CreateInstance instance)
        {
            Clients.Group(groupName).createInstance(instance);
        }

        #region On_Commands
        public override Task OnConnected()
        {
            // On connect investigate what type of client, and then add it to list
            Clients.Caller.UserId = Context.User.Identity.Name;
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // If a reporting server disconnects, then report this to all other clients
            return base.OnDisconnected(stopCalled);
        }
        #endregion
    }
}