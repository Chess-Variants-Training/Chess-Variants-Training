using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IAttemptRepository
    {
        Task AddAsync(Attempt attempt);
        Task<List<Attempt>> GetAsync(int user, int skip, int limit);
        Task<long> CountAsync(int user);
    }
}
