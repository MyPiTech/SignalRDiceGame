using Microsoft.AspNetCore.SignalR;

namespace SrDiceGame.Hubs
{

    public class DiceHub : Hub
    {
        private readonly static IList<string> _connections = new List<string>();
        private readonly static Dictionary<string, int> _scores = [];

        public async Task RollDieAsync()
        {
            string id = Context.ConnectionId;

            if (!string.IsNullOrEmpty(id) && !_scores.Any(s => s.Key == id))
            {
                int roll = new Random().Next(1, 7);
                _scores.Add(id, roll);
                await Clients.Client(id).SendAsync("YourRoll", roll);
                await Clients.AllExcept(id).SendAsync("OtherRolls", roll);  
            }

            if (_connections.ToHashSet().SetEquals(_scores.Select(s => s.Key).ToHashSet()))
            {
                var highScore = _scores.Aggregate((x, y) => x.Value > y.Value ? x : y).Value;
                var winners = _scores.Where(s => s.Value == highScore);
                var losers = _scores.Where(s => s.Value != highScore);

                foreach(var winner in winners)
                {
                    await Clients.Client(winner.Key).SendAsync("Result", winners.Count() > 1 ? 1 : 0, highScore);
                }

                foreach (var loser in losers)
                {
                    await Clients.Client(loser.Key).SendAsync("Result", 2, highScore);
                }
            }
        }

        public async Task ResetAsync()
        {
            _scores.Clear();
            await Clients.All.SendAsync("Reset");
        }

        public async override Task OnConnectedAsync()
        {
            string id = Context.ConnectionId;
            if (!string.IsNullOrEmpty(id))
            {
                _connections.Add(id);
            }
            await base.OnConnectedAsync();
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
