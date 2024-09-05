using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ScoreboardHub : Hub
{
    private static readonly Dictionary<string, (int team1Score, int team2Score, int team1Penalty, int team2Penalty, int timerMinutes, int timerSeconds, bool timerRunning, int currentRound)> GroupStates = new();
    private static readonly Dictionary<string, HashSet<string>> GroupConnections = new();

    public override Task OnConnectedAsync()
    {
        // No need to do anything special here, but it's good to override if needed
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(System.Exception exception)
    {
        // Remove connection from all groups the client was part of
        foreach (var group in GroupConnections.Where(g => g.Value.Contains(Context.ConnectionId)).ToList())
        {
            group.Value.Remove(Context.ConnectionId);
            if (!group.Value.Any())
            {
                GroupConnections.Remove(group.Key);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        if (!GroupStates.ContainsKey(groupName))
        {
            GroupStates[groupName] = (0, 0, 0, 0, 1, 50, false, 1); // Initialize default values
        }

        if (!GroupConnections.ContainsKey(groupName))
        {
            GroupConnections[groupName] = new HashSet<string>();
        }
        GroupConnections[groupName].Add(Context.ConnectionId);

        var state = GroupStates[groupName];
        await Clients.Caller.SendAsync("ReceiveScoreUpdate", state.team1Score, state.team2Score, state.team1Penalty, state.team2Penalty);
        await Clients.Caller.SendAsync("ReceiveTimerUpdate", state.timerMinutes, state.timerSeconds, state.timerRunning);
        await Clients.Caller.SendAsync("ReceiveRoundUpdate", state.currentRound);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        if (GroupConnections.ContainsKey(groupName))
        {
            GroupConnections[groupName].Remove(Context.ConnectionId);
            if (!GroupConnections[groupName].Any())
            {
                GroupConnections.Remove(groupName);
            }
        }
    }

    public async Task UpdateScore(string groupName, int team1Score, int team2Score, int team1Penalty, int team2Penalty)
    {
        if (GroupStates.ContainsKey(groupName))
        {
            GroupStates[groupName] = (team1Score, team2Score, team1Penalty, team2Penalty, GroupStates[groupName].timerMinutes, GroupStates[groupName].timerSeconds, GroupStates[groupName].timerRunning, GroupStates[groupName].currentRound);
            await Clients.Group(groupName).SendAsync("ReceiveScoreUpdate", team1Score, team2Score, team1Penalty, team2Penalty);
        }
    }

    public async Task UpdateTimer(string groupName, int minutes, int seconds, bool isRunning)
    {
        if (GroupStates.ContainsKey(groupName))
        {
            GroupStates[groupName] = (GroupStates[groupName].team1Score, GroupStates[groupName].team2Score, GroupStates[groupName].team1Penalty, GroupStates[groupName].team2Penalty, minutes, seconds, isRunning, GroupStates[groupName].currentRound);
            await Clients.Group(groupName).SendAsync("ReceiveTimerUpdate", minutes, seconds, isRunning);
        }
    }

    public async Task UpdateRound(string groupName, int round)
    {
        if (GroupStates.ContainsKey(groupName))
        {
            GroupStates[groupName] = (GroupStates[groupName].team1Score, GroupStates[groupName].team2Score, GroupStates[groupName].team1Penalty, GroupStates[groupName].team2Penalty, GroupStates[groupName].timerMinutes, GroupStates[groupName].timerSeconds, GroupStates[groupName].timerRunning, round);
            await Clients.Group(groupName).SendAsync("ReceiveRoundUpdate", round);
        }
    }

    public Task<bool> AreClientsConnected(string groupName)
    {
        if (GroupConnections.ContainsKey(groupName))
        {
            return Task.FromResult(GroupConnections[groupName].Any());
        }
        return Task.FromResult(false);
    }
}
