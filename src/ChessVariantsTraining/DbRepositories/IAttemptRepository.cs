using ChessVariantsTraining.Models;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IAttemptRepository
    {
        void Add(Attempt attempt);
    }
}
