using ChessDotNet;
using ChessVariantsTraining.DbRepositories.Variant960;
using ChessVariantsTraining.Models.Variant960;
using ChessVariantsTraining.Services;
using System;
using System.Collections.Generic;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public class GameRepoForSocketHandlers : IGameRepoForSocketHandlers
    {
        Dictionary<string, Game> cache = new Dictionary<string, Game>();
        IGameRepository gameRepository;
        IGameConstructor gameConstructor;

        public GameRepoForSocketHandlers(IGameRepository _gameRepository, IGameConstructor _gameConstructor)
        {
            gameRepository = _gameRepository;
            gameConstructor = _gameConstructor;
        }

        public Game Get(string id)
        {
            if (cache.ContainsKey(id))
            {
                return cache[id];
            }
            else
            {
                Game g = gameRepository.Get(id);
                if (g == null)
                {
                    return null;
                }
                g.ChessGame = gameConstructor.Construct(g.ShortVariantName, g.LatestFEN);
                cache[id] = g;
                return cache[id];
            }
        }

        public void RegisterMove(Game subject, Move move)
        {
            subject.ChessGame.ApplyMove(move, true);
            subject.LatestFEN = subject.ChessGame.GetFen();
            subject.MoveTimeStampsUtc.Add(DateTime.UtcNow);
            if (subject.ChessGame.Moves.Count > 1)
            {
                if (move.Player == Player.White)
                {
                    subject.ClockWhite.MoveMade();
                    subject.ClockBlack.Start();
                }
                else
                {
                    if (subject.ChessGame.Moves.Count != 2)
                    {
                        subject.ClockBlack.MoveMade();
                    }
                    subject.ClockWhite.Start();
                }
            }
            gameRepository.Update(subject);
        }

        public void RegisterGameResult(Game subject, string result, string termination)
        {
            subject.ClockWhite.End();
            subject.ClockBlack.End();
            subject.Result = result;
            subject.EndedUtc = DateTime.UtcNow;
            gameRepository.Update(subject);
        }

        public void RegisterPlayerChatMessage(Game subject, ChatMessage msg)
        {
            subject.PlayerChats.Add(msg);
            gameRepository.Update(subject);
        }

        public void RegisterSpectatorChatMessage(Game subject, ChatMessage msg)
        {
            subject.SpectatorChats.Add(msg);
            gameRepository.Update(subject);
        }

        public void RegisterWhiteRematchOffer(Game subject)
        {
            subject.WhiteWantsRematch = true;
            gameRepository.Update(subject);
        }

        public void RegisterBlackRematchOffer(Game subject)
        {
            subject.BlackWantsRematch = true;
            gameRepository.Update(subject);
        }

        public void ClearRematchOffers(Game subject)
        {
            subject.WhiteWantsRematch = false;
            subject.BlackWantsRematch = false;
            gameRepository.Update(subject);
        }

        public string GenerateId()
        {
            return gameRepository.GenerateId();
        }

        public void Add(Game subject)
        {
            gameRepository.Add(subject);
        }

        public void SetRematchID(Game subject, string rematchId)
        {
            subject.RematchID = rematchId;
            gameRepository.Update(subject);
        }
    }
}
