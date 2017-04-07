using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ChessVariantsTraining.DbRepositories.Variant960;
using ChessVariantsTraining.HttpErrors;
using ChessVariantsTraining.Models.Variant960;
using ChessDotNet;
using Newtonsoft.Json;
using System.Linq;
using ChessDotNet.Pieces;

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
        public IActionResult Game(string id)
        {
            id = id.ToLowerInvariant();
            Game game = gameRepository.Get(id);
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
                whiteUsername = userRepository.FindById(whiteId.Value).Username;

                int? loggedOnUserId = loginHandler.LoggedInUserId(HttpContext);
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
                blackUsername = userRepository.FindById(blackId.Value).Username;

                int? loggedOnUserId = loginHandler.LoggedInUserId(HttpContext);
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

            ViewModels.Game model = new ViewModels.Game(game.ID,
                whiteUsername,
                blackUsername,
                whiteId,
                blackId,
                game.ShortVariantName,
                game.FullVariantName,
                game.TimeControl.ToString(),
                game.LatestFEN,
                requester != Player.None,
                requester == Player.None ? null : requester.ToString().ToLowerInvariant(),
                game.LatestFEN.Split(' ')[1] == "w" ? "white" : "black",
                game.Result != Models.Variant960.Game.Results.ONGOING,
                destsJson,
                game.Result,
                game.Termination,
                game.UciMoves.LastOrDefault(),
                check);

            return View(model);
        }

        [Route("/Variant960/Lobby/StoreAnonymousIdentifier")]
        [Route("/Variant960/Game/StoreAnonymousIdentifier")]
        [HttpPost]
        public IActionResult StoreAnonymousIdentifier()
        {
            if (loginHandler.LoggedInUserId(HttpContext).HasValue || HttpContext.Session.GetString("anonymousIdentifier") != null)
            {
                return Json(new { success = true });
            }

            HttpContext.Session.SetString("anonymousIdentifier", randomProvider.RandomString(12));
            return Json(new { success = true });
        }
    }
}
