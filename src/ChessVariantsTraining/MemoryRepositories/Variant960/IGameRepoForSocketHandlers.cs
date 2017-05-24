using ChessDotNet;
using ChessDotNet.Variants.Crazyhouse;
using ChessVariantsTraining.Models.Variant960;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public interface IGameRepoForSocketHandlers
    {
        Game Get(string id);

        MoveType RegisterMove(Game subject, Move move);

        void RegisterDrop(Game subject, Drop drop);

        void RegisterGameResult(Game subject, string result, string termination);

        void RegisterPlayerChatMessage(Game subject, ChatMessage msg);

        void RegisterSpectatorChatMessage(Game subject, ChatMessage msg);

        void RegisterWhiteRematchOffer(Game subject);

        void RegisterBlackRematchOffer(Game subject);

        void ClearRematchOffers(Game subject);

        void RegisterWhiteDrawOffer(Game subject);

        void RegisterBlackDrawOffer(Game subject);

        void ClearDrawOffers(Game subject);

        string GenerateId();

        void Add(Game subject);

        void SetRematchID(Game subject, string rematchId);
    }
}
