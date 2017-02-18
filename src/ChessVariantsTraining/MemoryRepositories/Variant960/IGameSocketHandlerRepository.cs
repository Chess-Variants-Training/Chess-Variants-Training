using ChessVariantsTraining.Models.Variant960;
using System;
using System.Threading.Tasks;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public interface IGameSocketHandlerRepository
    {
        void Add(GameSocketHandler handler);

        Task SendAll(string messageA, string messageB, Func<GamePlayer, bool> chooseA);
    }
}
