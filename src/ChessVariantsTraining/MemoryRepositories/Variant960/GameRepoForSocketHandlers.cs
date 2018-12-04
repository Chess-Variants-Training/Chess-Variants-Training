using ChessDotNet;
using ChessDotNet.Variants.Crazyhouse;
using ChessVariantsTraining.DbRepositories.Variant960;
using ChessVariantsTraining.Models.Variant960;
using ChessVariantsTraining.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<Game> GetAsync(string id)
        {
            if (cache.ContainsKey(id))
            {
                return cache[id];
            }
            else
            {
                Game g = await gameRepository.GetAsync(id);
                if (g == null)
                {
                    return null;
                }
                g.ChessGame = gameConstructor.Construct(g.ShortVariantName, g.LatestFEN);
                cache[id] = g;
                return cache[id];
            }
        }

        public async Task<MoveType> RegisterMoveAsync(Game subject, Move move)
        {
            MoveType ret;
            ret = subject.ChessGame.MakeMove(move, true);
            subject.LatestFEN = subject.ChessGame.GetFen();
            subject.UciMoves.Add(move.OriginalPosition.ToString().ToLowerInvariant() +
                move.NewPosition.ToString().ToLowerInvariant() +
                (move.Promotion.HasValue ? move.Promotion.Value.ToString().ToLowerInvariant() : ""));
            ClockSwitchAfterMove(subject, move.Player == Player.White);
            await gameRepository.UpdateAsync(subject);
            return ret;
        }

        public async Task RegisterDropAsync(Game subject, Drop drop)
        {
            CrazyhouseChessGame zhGame = subject.ChessGame as CrazyhouseChessGame;
            zhGame.ApplyDrop(drop, true);
            subject.LatestFEN = subject.ChessGame.GetFen();
            subject.UciMoves.Add(char.ToUpperInvariant(drop.ToDrop.GetFenCharacter()) + "@" + drop.Destination.ToString().ToLowerInvariant());
            ClockSwitchAfterMove(subject, drop.Player == Player.White);
            await gameRepository.UpdateAsync(subject);
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

        public async Task RegisterGameResultAsync(Game subject, string result, string termination)
        {
            subject.ClockWhite.End();
            subject.ClockBlack.End();
            subject.Result = result;
            subject.Termination = termination;
            subject.EndedUtc = DateTime.UtcNow;
            await gameRepository.UpdateAsync(subject);
        }

        public async Task RegisterPlayerChatMessageAsync(Game subject, ChatMessage msg)
        {
            subject.PlayerChats.Add(msg);
            await gameRepository.UpdateAsync(subject);
        }

        public async Task RegisterSpectatorChatMessageAsync(Game subject, ChatMessage msg)
        {
            subject.SpectatorChats.Add(msg);
            await gameRepository.UpdateAsync(subject);
        }

        public async Task RegisterWhiteRematchOfferAsync(Game subject)
        {
            subject.WhiteWantsRematch = true;
            await gameRepository.UpdateAsync(subject);
        }

        public async Task RegisterBlackRematchOfferAsync(Game subject)
        {
            subject.BlackWantsRematch = true;
            await gameRepository.UpdateAsync(subject);
        }

        public async Task ClearRematchOffersAsync(Game subject)
        {
            subject.WhiteWantsRematch = false;
            subject.BlackWantsRematch = false;
            await gameRepository.UpdateAsync(subject);
        }

        public async Task RegisterWhiteDrawOfferAsync(Game subject)
        {
            subject.WhiteWantsDraw = true;
            await gameRepository.UpdateAsync(subject);
        }

        public async Task RegisterBlackDrawOfferAsync(Game subject)
        {
            subject.BlackWantsDraw = true;
            await gameRepository.UpdateAsync(subject);
        }

        public async Task ClearDrawOffersAsync(Game subject)
        {
            subject.WhiteWantsDraw = false;
            subject.BlackWantsDraw = false;
            await gameRepository.UpdateAsync(subject);
        }

        public async Task<string> GenerateIdAsync()
        {
            return await gameRepository.GenerateIdAsync();
        }

        public async Task AddAsync(Game subject)
        {
            await gameRepository.AddAsync(subject);
        }

        public async Task SetRematchIDAsync(Game subject, string rematchId)
        {
            subject.RematchID = rematchId;
            await gameRepository.UpdateAsync(subject);
        }
    }
}
