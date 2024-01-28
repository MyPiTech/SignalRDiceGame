using Microsoft.AspNetCore.SignalR;
using SrDiceGame.Objects;

namespace SrDiceGame.Hubs
{

    public class DiceHub : Hub
    {
        private readonly static IList<string> _connections = new List<string>();
        private readonly static Dictionary<string, int> _scores = [];

        public async Task RollDieAsync()
        {
            string id = Context.ConnectionId;

            //!_scores.Any(s => s.Key == id) to make sure no one is cheating :)
            if (!string.IsNullOrEmpty(id) && !_scores.Any(s => s.Key == id))
            {
                int roll = new Random().Next(1, 7);
                _scores.Add(id, roll);
                await Clients.Client(id).SendAsync("YourRoll", roll);
                await Clients.AllExcept(id).SendAsync("OtherRolls", roll);
                
            }
            //if we have the same amount of scores as connections show the result
            if (_scores.Count == _connections.Count)
            {
                KeyValuePair<string, int> winner = _scores.Aggregate((x, y) => x.Value > y.Value ? x : y);
                await Clients.Client(winner.Key).SendAsync("Result", true, winner.Value);
                await Clients.AllExcept(winner.Key).SendAsync("Result", false, winner.Value);
            }
        }

        public async Task ResetAsync()
        {
            _scores.Clear();
            await Clients.All.SendAsync("Reset");
        }
        public override Task OnConnectedAsync()
        {
            string id = Context.ConnectionId;
            if (!string.IsNullOrEmpty(id))
            {
                _connections.Add(id);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string id = Context.ConnectionId;
            if (!string.IsNullOrEmpty(id))
            {
                _connections.Remove(id);
            }
            return base.OnDisconnectedAsync(exception);
        }


    }
}
