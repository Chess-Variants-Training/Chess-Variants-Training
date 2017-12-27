using ChessVariantsTraining.Models.Variant960;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories.Variant960
{
    public interface IGameRepository
    {
        Game Get(string id);
        Task AddAsync(Game game);
        Task<Game> GetAsync(string id);
        Task UpdateAsync(Game game);
        Task<string> GenerateIdAsync();
        Task<List<Game>> GetByPlayerIdAsync(int id, int skip, int limit);
        Task<long> CountByPlayerIdAsync(int id);
    }
}
