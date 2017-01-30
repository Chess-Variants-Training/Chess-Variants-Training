using ChessDotNet;
using ChessVariantsTraining.DbRepositories.Variant960;
using ChessVariantsTraining.Models.Variant960;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                return cache[id];
            }
        }

        public void RegisterMove(Move move)
        {

        }
    }
}
