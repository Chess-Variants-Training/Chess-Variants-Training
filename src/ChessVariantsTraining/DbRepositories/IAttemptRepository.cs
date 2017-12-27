using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IAttemptRepository
    {
        /*void Add(Attempt attempt);
        List<Attempt> Get(int user, int skip, int limit);
        long Count(int user);*/

        Task AddAsync(Attempt attempt);
        Task<List<Attempt>> GetAsync(int user, int skip, int limit);
        Task<long> CountAsync(int user);
    }
}
