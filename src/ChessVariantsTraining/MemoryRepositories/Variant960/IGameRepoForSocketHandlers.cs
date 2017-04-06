using ChessDotNet;
using ChessVariantsTraining.Models.Variant960;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public interface IGameRepoForSocketHandlers
    {
        Game Get(string id);

        void RegisterMove(Game subject, Move move);

        void RegisterGameOutcome(Game subject, string outcome);

        void RegisterPlayerChatMessage(Game subject, ChatMessage msg);

        void RegisterSpectatorChatMessage(Game subject, ChatMessage msg);

        void RegisterWhiteRematchOffer(Game subject);

        void RegisterBlackRematchOffer(Game subject);

        void ClearRematchOffers(Game subject);

        string GenerateId();

        void Add(Game subject);
    }
}
