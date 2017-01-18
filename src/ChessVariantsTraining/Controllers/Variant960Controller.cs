using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ChessVariantsTraining.DbRepositories.Variant960;
using ChessVariantsTraining.HttpErrors;
using ChessVariantsTraining.Models.Variant960;

namespace ChessVariantsTraining.Controllers
{
    public class Variant960Controller : CVTController
    {
        IRandomProvider randomProvider;
        IGameRepository gameRepository;

        public Variant960Controller(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler, IRandomProvider _randomProvider, IGameRepository _gameRepository) : base(_userRepository, _loginHandler)
        {
            randomProvider = _randomProvider;
            gameRepository = _gameRepository;
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
            if (game.White is AnonymousPlayer)
            {
                whiteUsername = null;
                whiteId = null;
            }
            else
            {
                whiteId = (game.White as RegisteredPlayer).UserId;
                whiteUsername = userRepository.FindById(whiteId.Value).Username;
            }

            if (game.Black is AnonymousPlayer)
            {
                blackUsername = null;
                blackId = null;
            }
            else
            {
                blackId = (game.Black as RegisteredPlayer).UserId;
                blackUsername = userRepository.FindById(blackId.Value).Username;
            }

            ViewModels.Game model = new ViewModels.Game(whiteUsername, blackUsername, whiteId, blackId, game.Variant, game.TimeControl.ToString());

            return View(model);
        }

        [Route("/Variant960/Lobby/StoreAnonymousIdentifier")]
        [HttpPost]
        public IActionResult StoreAnonymousIdentifier()
        {
            if (loginHandler.LoggedInUserId(HttpContext).HasValue)
            {
                return Json(new { success = true });
            }

            HttpContext.Session.SetString("anonymousIdentifier", randomProvider.RandomString(12));
            return Json(new { success = true });
        }
    }
}
