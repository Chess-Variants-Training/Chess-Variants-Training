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

        public async Task SendAll(string gameId, string messageA, string messageB, Func<GamePlayer, bool> chooseA)
        {
            foreach (GameSocketHandler handler in handlers)
            {
                if (handler.SubjectID != gameId)
                {
                    continue;
                }
                if (chooseA(handler.Client))
                {
                    if (!string.IsNullOrEmpty(messageA))
                    {
                        await handler.Send(messageA);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(messageB))
                    {
                        await handler.Send(messageB);
                    }
                }
            }
        }
    }
}
