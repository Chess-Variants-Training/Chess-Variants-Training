using ChessVariantsTraining.Models.Variant960;
using System.Threading.Tasks;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public interface ILobbySocketHandlerRepository
    {
        void Add(LobbySocketHandler handler);

        Task SendAll(string text);

        Task SendSeekAddition(LobbySeek seek);

        Task SendSeekRemoval(string id);
    }
}
