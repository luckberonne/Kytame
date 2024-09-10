using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ScoreboardHub : Hub
{
    private static readonly Dictionary<string, (int team1Score, int team2Score, int team1Penalty, int team2Penalty, int timerMinutes, int timerSeconds, bool timerRunning, int currentRound)> GroupStates = new();

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        if (!GroupStates.ContainsKey(groupName))
        {
            GroupStates[groupName] = (0, 0, 0, 0, 2, 00, false, 1); // Initialize default values
        }

        var state = GroupStates[groupName];
        await Clients.Caller.SendAsync("ReceiveScoreUpdate", state.team1Score, state.team2Score, state.team1Penalty, state.team2Penalty);
        await Clients.Caller.SendAsync("ReceiveTimerUpdate", state.timerMinutes, state.timerSeconds, state.timerRunning);
        await Clients.Caller.SendAsync("ReceiveRoundUpdate", state.currentRound);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
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
        if (GroupStates.ContainsKey(groupName))
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}