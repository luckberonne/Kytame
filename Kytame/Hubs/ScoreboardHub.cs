using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

public class ScoreboardHub : Hub
{
    private static readonly ConcurrentDictionary<string, GroupState> GroupStates = new();
    private static readonly ConcurrentDictionary<string, ConnectionCounter> GroupConnectionCounts = new();
    private static readonly ConcurrentDictionary<string, DateTime> GroupCreationTimes = new();
    private const int MaxGroups = 3;
    private const int MaxConnectionsPerGroup = 4;
    private static readonly TimeSpan GroupLifetime = TimeSpan.FromHours(1);

    public class GroupState
    {
        public int Team1Score { get; set; }
        public int Team2Score { get; set; }
        public int Team1Penalty { get; set; }
        public int Team2Penalty { get; set; }
        public int TimerMinutes { get; set; } = 2;
        public int TimerSeconds { get; set; } = 0;
        public bool TimerRunning { get; set; } = false;
        public int CurrentRound { get; set; } = 1;
    }

    private class ConnectionCounter
    {
        public int Count { get; set; } = 0;
    }

    public async Task JoinGroup(string groupName)
    {
        // Check if the group exists and if it has not exceeded the connection limit
        if (GroupConnectionCounts.ContainsKey(groupName))
        {
            if (GroupConnectionCounts[groupName].Count >= MaxConnectionsPerGroup)
            {
                await Clients.Caller.SendAsync("Error", "The group already has the maximum number of connections.");
                return;
            }
        }

        // Ensure there are no more than 3 groups
        if (GroupStates.Count >= MaxGroups)
        {
            CleanUpOldGroups();
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var state = GroupStates.GetOrAdd(groupName, new GroupState());
        GroupCreationTimes.TryAdd(groupName, DateTime.UtcNow);

        // Update connection count
        GroupConnectionCounts.AddOrUpdate(groupName, new ConnectionCounter { Count = 1 }, (key, counter) =>
        {
            counter.Count++;
            return counter;
        });

        await Clients.Caller.SendAsync("ReceiveScoreUpdate", state.Team1Score, state.Team2Score, state.Team1Penalty, state.Team2Penalty);
        await Clients.Caller.SendAsync("ReceiveTimerUpdate", state.TimerMinutes, state.TimerSeconds, state.TimerRunning);
        await Clients.Caller.SendAsync("ReceiveRoundUpdate", state.CurrentRound);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        if (GroupConnectionCounts.ContainsKey(groupName))
        {
            GroupConnectionCounts.AddOrUpdate(groupName, new ConnectionCounter(), (key, counter) =>
            {
                counter.Count--;
                return counter.Count > 0 ? counter : null;
            });
        }
    }

    public async Task UpdateScore(string groupName, int team1Score, int team2Score, int team1Penalty, int team2Penalty)
    {
        if (GroupStates.TryGetValue(groupName, out var state))
        {
            state.Team1Score = team1Score;
            state.Team2Score = team2Score;
            state.Team1Penalty = team1Penalty;
            state.Team2Penalty = team2Penalty;

            await Clients.Group(groupName).SendAsync("ReceiveScoreUpdate", team1Score, team2Score, team1Penalty, team2Penalty);
        }
    }

    public async Task UpdateTimer(string groupName, int minutes, int seconds, bool isRunning)
    {
        if (GroupStates.TryGetValue(groupName, out var state))
        {
            state.TimerMinutes = minutes;
            state.TimerSeconds = seconds;
            state.TimerRunning = isRunning;

            await Clients.Group(groupName).SendAsync("ReceiveTimerUpdate", minutes, seconds, isRunning);
        }
    }

    public async Task UpdateRound(string groupName, int round)
    {
        if (GroupStates.TryGetValue(groupName, out var state))
        {
            state.CurrentRound = round;
            await Clients.Group(groupName).SendAsync("ReceiveRoundUpdate", round);
        }
    }

    private void CleanUpOldGroups()
    {
        var groupsToRemove = GroupCreationTimes
            .Where(kvp => DateTime.UtcNow - kvp.Value > GroupLifetime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var groupName in groupsToRemove)
        {
            GroupStates.TryRemove(groupName, out _);
            GroupCreationTimes.TryRemove(groupName, out _);
            GroupConnectionCounts.TryRemove(groupName, out _);
        }
    }

    public Task<bool> AreClientsConnected(string groupName)
    {
        if (GroupConnectionCounts.TryGetValue(groupName, out var counter))
        {
            return Task.FromResult(counter.Count > 0);
        }
        return Task.FromResult(false);
    }
}
