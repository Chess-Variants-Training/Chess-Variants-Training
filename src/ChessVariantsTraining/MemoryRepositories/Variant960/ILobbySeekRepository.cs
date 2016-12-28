using ChessVariantsTraining.Models.Variant960;
using System.Threading.Tasks;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public interface ILobbySeekRepository
    {
        LobbySeek Get(string id);

        Task<string> Add(LobbySeek seek);

        Task Remove(string id, int? user, string clientId);

        void Bump(string id, int? user, string clientId);
    }
}
