using ChessVariantsTraining.Models;

namespace ChessVariantsTraining.MemoryRepositories
{
    public interface IEndgameTrainingSessionRepository
    {
        void Add(EndgameTrainingSession session);

        EndgameTrainingSession Get(string sessionId);

        void Remove(string sessionId);

        bool Exists(string sessionId);
    }
}
