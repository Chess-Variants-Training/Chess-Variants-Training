using ChessDotNet;
using ChessDotNet.Variants.Crazyhouse;
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

        public MoveType RegisterMove(Game subject, Move move)
        {
            MoveType ret;
            ret = subject.ChessGame.ApplyMove(move, true);
            subject.LatestFEN = subject.ChessGame.GetFen();
            subject.UciMoves.Add(move.OriginalPosition.ToString().ToLowerInvariant() +
                move.NewPosition.ToString().ToLowerInvariant() +
                (move.Promotion.HasValue ? move.Promotion.Value.ToString().ToLowerInvariant() : ""));
            ClockSwitchAfterMove(subject, move.Player == Player.White);
            gameRepository.Update(subject);
            return ret;
        }

        public void RegisterDrop(Game subject, Drop drop)
        {
            CrazyhouseChessGame zhGame = subject.ChessGame as CrazyhouseChessGame;
            zhGame.ApplyDrop(drop, true);
            subject.LatestFEN = subject.ChessGame.GetFen();
            subject.UciMoves.Add(char.ToUpperInvariant(drop.ToDrop.GetFenCharacter()) + "@" + drop.Destination.ToString().ToLowerInvariant());
            ClockSwitchAfterMove(subject, drop.Player == Player.White);
            gameRepository.Update(subject);
        }

        void ClockSwitchAfterMove(Game subject, bool didWhiteMove)
        {
            if (subject.ChessGame.Moves.Count > 1)
            {
                if (didWhiteMove)
                {
                    subject.ClockWhite.MoveMade();
                    subject.ClockTimes.Add(subject.ClockWhite.GetSecondsLeft());
                    subject.ClockBlack.Start();
                }
                else
                {
                    if (subject.ChessGame.Moves.Count != 2)
                    {
                        subject.ClockBlack.MoveMade();
                    }
                    subject.ClockTimes.Add(subject.ClockBlack.GetSecondsLeft());
                    subject.ClockWhite.Start();
                }
            }
            else
            {
                subject.ClockTimes.Add(subject.ClockWhite.GetSecondsLeft());
            }
        }

        public void RegisterGameResult(Game subject, string result, string termination)
        {
            subject.ClockWhite.End();
            subject.ClockBlack.End();
            subject.Result = result;
            subject.Termination = termination;
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

        public void RegisterWhiteDrawOffer(Game subject)
        {
            subject.WhiteWantsDraw = true;
            gameRepository.Update(subject);
        }

        public void RegisterBlackDrawOffer(Game subject)
        {
            subject.BlackWantsDraw = true;
            gameRepository.Update(subject);
        }

        public void ClearDrawOffers(Game subject)
        {
            subject.WhiteWantsDraw = false;
            subject.BlackWantsDraw = false;
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
