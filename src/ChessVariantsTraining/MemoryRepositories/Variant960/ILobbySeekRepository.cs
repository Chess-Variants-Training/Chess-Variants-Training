using ChessVariantsTraining.Models.Variant960;
using System.Threading.Tasks;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public interface ILobbySeekRepository
    {
        LobbySeek Get(string id);

        Task<string> Add(LobbySeek seek);

        Task Remove(string id, GamePlayer client);

        void Bump(string id, GamePlayer client);
    }
}
