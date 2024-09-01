using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ScoreboardHub : Hub
{
    public async Task UpdateScore(int team1Score, int team2Score, int team1Penalty, int team2Penalty)
    {
        // Notify all connected clients about the updated scores
        await Clients.All.SendAsync("ReceiveScoreUpdate", team1Score, team2Score, team1Penalty, team2Penalty);
    }

    public async Task UpdateTimer(int minutes, int seconds, bool isRunning)
    {
        // Notify all connected clients about the updated timer
        await Clients.All.SendAsync("ReceiveTimerUpdate", minutes, seconds, isRunning);
    }

    public async Task UpdateRound(int round)
    {
        // Notify all connected clients about the updated round
        await Clients.All.SendAsync("ReceiveRoundUpdate", round);
    }
}
