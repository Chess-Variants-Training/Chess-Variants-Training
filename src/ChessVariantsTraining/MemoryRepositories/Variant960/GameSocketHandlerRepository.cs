using ChessVariantsTraining.Models.Variant960;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public class GameSocketHandlerRepository : IGameSocketHandlerRepository
    {
        List<GameSocketHandler> handlers = new List<GameSocketHandler>();

        public void Add(GameSocketHandler handler)
        {
            handlers.Add(handler);
        }

        public async Task SendAll(string message)
        {
            foreach (GameSocketHandler handler in handlers)
            {
                await handler.Send(message);
            }
        }
    }
}
