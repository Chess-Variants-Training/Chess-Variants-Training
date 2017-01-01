using ChessVariantsTraining.Models.Variant960;

namespace ChessVariantsTraining.DbRepositories.Variant960
{
    public interface IGameRepository
    {
        void Add(Game game);
        void Get(string id);
        void Update(Game game);
    }
}
