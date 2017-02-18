using ChessVariantsTraining.Models.Variant960;
using System;
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

        public async Task SendAll(string messageA, string messageB, Func<GamePlayer, bool> chooseA)
        {
            foreach (GameSocketHandler handler in handlers)
            {
                if (chooseA(handler.Client))
                {
                    await handler.Send(messageA);
                }
                else
                {
                    await handler.Send(messageB);
                }
            }
        }
    }
}
