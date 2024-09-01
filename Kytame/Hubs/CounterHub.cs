using Microsoft.AspNetCore.SignalR;

namespace Kytame.Hubs
{
    public class CounterHub : Hub
    {
        private static int _currentCount = 0; 

        public async Task IncrementCounter()
        {
            _currentCount++;
            await Clients.All.SendAsync("ReceiveCount", _currentCount); // Enviar la actualización a todos los clientes conectados
        }

        public async Task ResetCounter()
        {
            _currentCount = 0;
            await Clients.All.SendAsync("ReceiveCount", _currentCount); // Enviar la actualización a todos los clientes conectados
        }
    }
}
