using Microsoft.AspNetCore.SignalR;

namespace Kytame.Hubs
{
    public class CounterHub : Hub
    {
        private static readonly Dictionary<string, int> GroupCounters = new();

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            if (!GroupCounters.ContainsKey(groupName))
            {
                GroupCounters[groupName] = 0;
            }
            await SendCurrentCountToClient(groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task IncrementCounter(string groupName)
        {
            if (GroupCounters.ContainsKey(groupName))
            {
                GroupCounters[groupName]++;
                await Clients.Group(groupName).SendAsync("ReceiveCount", GroupCounters[groupName]);
            }
        }

        public async Task ResetCounter(string groupName)
        {
            if (GroupCounters.ContainsKey(groupName))
            {
                GroupCounters[groupName] = 0;
                await Clients.Group(groupName).SendAsync("ReceiveCount", GroupCounters[groupName]);
            }
        }

        private async Task SendCurrentCountToClient(string groupName)
        {
            if (GroupCounters.ContainsKey(groupName))
            {
                await Clients.Caller.SendAsync("ReceiveCount", GroupCounters[groupName]);
            }
        }
    }
}
