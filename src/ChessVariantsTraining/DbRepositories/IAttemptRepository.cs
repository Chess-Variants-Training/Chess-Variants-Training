using ChessVariantsTraining.Models;
using System.Collections.Generic;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IAttemptRepository
    {
        void Add(Attempt attempt);
        List<Attempt> Get(int user, int skip, int limit);
        long Count(int user);
    }
}
