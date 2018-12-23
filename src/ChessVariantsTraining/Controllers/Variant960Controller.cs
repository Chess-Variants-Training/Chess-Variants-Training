using ChessDotNet;
using ChessDotNet.Variants.Crazyhouse;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.DbRepositories.Variant960;
using ChessVariantsTraining.Extensions;
using ChessVariantsTraining.HttpErrors;
using ChessVariantsTraining.Models.Variant960;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Controllers
{
    public class Variant960Controller : CVTController
    {
        IRandomProvider randomProvider;
        IGameRepository gameRepository;
        IMoveCollectionTransformer moveCollectionTransformer;
        IGameConstructor gameConstructor;

        public Variant960Controller(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler, IRandomProvider _randomProvider, IGameRepository _gameRepository, IMoveCollectionTransformer _moveCollectionTransformer, IGameConstructor _gameConstructor) : base(_userRepository, _loginHandler)
        {
            randomProvider = _randomProvider;
            gameRepository = _gameRepository;
            moveCollectionTransformer = _moveCollectionTransformer;
            gameConstructor = _gameConstructor;
        }

        [Route("/Variant960")]
        public IActionResult Lobby()
        {
            return View();
        }

        [Route("/Variant960/Game/{id}")]
        public async Task<IActionResult> Game(string id)
        {
            id = id.ToLowerInvariant();
            Game game = await gameRepository.GetAsync(id);
            if (game == null)
            {
                return ViewResultForHttpError(HttpContext, new NotFound("This game could not be found."));
            }

            string whiteUsername;
            int? whiteId;
            string blackUsername;
            int? blackId;
            Player requester = Player.None;
            if (game.White is AnonymousPlayer)
            {
                whiteUsername = null;
                whiteId = null;

                string anonIdentifier = HttpContext.Session.GetString("anonymousIdentifier");
                if (anonIdentifier == (game.White as AnonymousPlayer).AnonymousIdentifier)
                {
                    requester = Player.White;
                }
            }
            else
            {
                whiteId = (game.White as RegisteredPlayer).UserId;
                whiteUsername = (await userRepository.FindByIdAsync(whiteId.Value)).Username;

                int? loggedOnUserId = await loginHandler.LoggedInUserIdAsync(HttpContext);
                if (loggedOnUserId.HasValue && loggedOnUserId.Value == whiteId)
                {
                    requester = Player.White;
                }
            }

            if (game.Black is AnonymousPlayer)
            {
                blackUsername = null;
                blackId = null;

                string anonIdentifier = HttpContext.Session.GetString("anonymousIdentifier");
                if (anonIdentifier == (game.Black as AnonymousPlayer).AnonymousIdentifier)
                {
                    requester = Player.Black;
                }
            }
            else
            {
                blackId = (game.Black as RegisteredPlayer).UserId;
                blackUsername = (await userRepository.FindByIdAsync(blackId.Value)).Username;

                int? loggedOnUserId = await loginHandler.LoggedInUserIdAsync(HttpContext);
                if (loggedOnUserId.HasValue && loggedOnUserId.Value == blackId)
                {
                    requester = Player.Black;
                }
            }

            bool finished = game.Result != Models.Variant960.Game.Results.ONGOING;
            string destsJson;
            ChessGame g = gameConstructor.Construct(game.ShortVariantName, game.LatestFEN);
            if (finished || requester == Player.None)
            {
                destsJson = "{}";
            }
            else
            {
                if (g.WhoseTurn != requester)
                {
                    destsJson = "{}";
                }
                destsJson = JsonConvert.SerializeObject(moveCollectionTransformer.GetChessgroundDestsForMoveCollection(g.GetValidMoves(g.WhoseTurn)));
            }
            string check = null;
            if (g.IsInCheck(Player.White))
            {
                check = "white";
            }
            else if (g.IsInCheck(Player.Black))
            {
                check = "black";
            }

            List<string> replay = new List<string>
            {
                game.InitialFEN
            };

            List<string> replayChecks = new List<string>
            {
                null
            };

            List<string> replayMoves = new List<string>();

            ChessGame replayGame = gameConstructor.Construct(game.ShortVariantName, game.InitialFEN);

            List<Dictionary<string, int>> replayPocket;
            if (replayGame is CrazyhouseChessGame)
            {
                replayPocket = new List<Dictionary<string, int>>
                {
                    replayGame.GenerateJsonPocket()
                };
            }
            else
            {
                replayPocket = null;
            }
            foreach (string uciMove in game.UciMoves)
            {
                if (!uciMove.Contains("@"))
                {
                    string from = uciMove.Substring(0, 2);
                    string to = uciMove.Substring(2, 2);
                    replayMoves.Add(string.Concat(from, "-", to));
                    char? promotion = null;
                    if (uciMove.Length == 5)
                    {
                        promotion = uciMove[4];
                    }
                    replayGame.MakeMove(new Move(from, to, replayGame.WhoseTurn, promotion), true);
                }
                else
                {
                    string[] typeAndPos = uciMove.Split('@');
                    Position pos = new Position(typeAndPos[1]);
                    Piece piece = replayGame.MapPgnCharToPiece(typeAndPos[0][0], replayGame.WhoseTurn);
                    Drop drop = new Drop(piece, pos, piece.Owner);
                    (replayGame as CrazyhouseChessGame).ApplyDrop(drop, true);
                    replayMoves.Add(pos.ToString().ToLowerInvariant() + "-" + pos.ToString().ToLowerInvariant());
                }
                replay.Add(replayGame.GetFen());
                if (replayGame.IsInCheck(Player.White))
                {
                    replayChecks.Add("white");
                }
                else if (replayGame.IsInCheck(Player.Black))
                {
                    replayChecks.Add("black");
                }
                else
                {
                    replayChecks.Add(null);
                }
                if (replayPocket != null)
                {
                    replayPocket.Add(replayGame.GenerateJsonPocket());
                }
            }
            if (game.PGN == null)
            {
                game.PGN = replayGame.GetPGN();
                await gameRepository.UpdateAsync(game);
            }

            string lastMove = game.UciMoves.LastOrDefault();
            if (lastMove != null && lastMove.Contains('@'))
            {
                string pos = lastMove.Split('@')[1].ToLowerInvariant();
                lastMove = pos + pos;
            }
            ViewModels.Game model = new ViewModels.Game(game.ID,
                whiteUsername,
                blackUsername,
                whiteId,
                blackId,
                game.ShortVariantName,
                game.FullVariantName,
                game.TimeControl,
                game.LatestFEN,
                requester != Player.None,
                requester == Player.None ? null : requester.ToString().ToLowerInvariant(),
                game.LatestFEN.Split(' ')[1] == "w" ? "white" : "black",
                game.Result != Models.Variant960.Game.Results.ONGOING,
                destsJson,
                game.Result,
                game.Termination,
                lastMove,
                check,
                game.UciMoves.Count,
                game.WhiteWantsDraw,
                game.BlackWantsDraw,
                game.WhiteWantsRematch,
                game.BlackWantsRematch,
                replay,
                replayMoves,
                replayChecks,
                g.GenerateJsonPocket(),
                replayPocket,
                game.PositionWhite,
                game.PositionBlack,
                game.PGN,
                game.InitialFEN);

            return View(model);
        }

        [Route("/Variant960/Lobby/StoreAnonymousIdentifier")]
        [Route("/Variant960/Game/StoreAnonymousIdentifier")]
        [HttpPost]
        public async Task<IActionResult> StoreAnonymousIdentifier()
        {
            if ((await loginHandler.LoggedInUserIdAsync(HttpContext)).HasValue || HttpContext.Session.GetString("anonymousIdentifier") != null)
            {
                return Json(new { success = true });
            }

            HttpContext.Session.SetString("anonymousIdentifier", randomProvider.RandomString(12));
            return Json(new { success = true });
        }
    }
}
