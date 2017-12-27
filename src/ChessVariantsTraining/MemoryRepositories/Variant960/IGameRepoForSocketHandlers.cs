using ChessDotNet;
using ChessDotNet.Variants.Crazyhouse;
using ChessVariantsTraining.Models.Variant960;
using System.Threading.Tasks;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public interface IGameRepoForSocketHandlers
    {
        Game Get(string id);
        Task<Game> GetAsync(string id);
        Task<MoveType> RegisterMoveAsync(Game subject, Move move);
        Task RegisterDropAsync(Game subject, Drop drop);
        Task RegisterGameResultAsync(Game subject, string result, string termination);
        Task RegisterPlayerChatMessageAsync(Game subject, ChatMessage msg);
        Task RegisterSpectatorChatMessageAsync(Game subject, ChatMessage msg);
        Task RegisterWhiteRematchOfferAsync(Game subject);
        Task RegisterBlackRematchOfferAsync(Game subject);
        Task ClearRematchOffersAsync(Game subject);
        Task RegisterWhiteDrawOfferAsync(Game subject);
        Task RegisterBlackDrawOfferAsync(Game subject);
        Task ClearDrawOffersAsync(Game subject);
        Task<string> GenerateIdAsync();
        Task AddAsync(Game subject);
        Task SetRematchIDAsync(Game subject, string rematchId);
    }
}
