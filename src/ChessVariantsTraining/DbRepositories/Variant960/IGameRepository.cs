using ChessVariantsTraining.Models.Variant960;
using System.Collections.Generic;

namespace ChessVariantsTraining.DbRepositories.Variant960
{
    public interface IGameRepository
    {
        void Add(Game game);
        Game Get(string id);
        void Update(Game game);
        string GenerateId();
        List<Game> GetByPlayerId(int id, int skip, int limit);
        long CountByPlayerId(int id);
    }
}
