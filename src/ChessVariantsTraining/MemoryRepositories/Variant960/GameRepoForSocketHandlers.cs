using ChessDotNet;
using ChessVariantsTraining.DbRepositories.Variant960;
using ChessVariantsTraining.Models.Variant960;
using System.Collections.Generic;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public class GameRepoForSocketHandlers : IGameRepoForSocketHandlers
    {
        Dictionary<string, Game> cache = new Dictionary<string, Game>();
        IGameRepository gameRepository;

        public GameRepoForSocketHandlers(IGameRepository _gameRepository)
        {
            gameRepository = _gameRepository;
        }

        public Game Get(string id)
        {
            if (cache.ContainsKey(id))
            {
                return cache[id];
            }
            else
            {
                cache[id] = gameRepository.Get(id);
                cache[id].ChessGame = new ChessGame(cache[id].LatestFEN);
                return cache[id];
            }
        }

        public void RegisterMove(Game subject, Move move)
        {
            subject.ChessGame.ApplyMove(move, true);
            subject.LatestFEN = subject.ChessGame.GetFen();
            gameRepository.Update(subject);
        }
    }
}
