using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ChessVariantsTraining.Controllers
{
    public class Variant960Controller : CVTController
    {
        IRandomProvider randomProvider;

        public Variant960Controller(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler, IRandomProvider _randomProvider) : base(_userRepository, _loginHandler)
        {
            randomProvider = _randomProvider;
        }

        [Route("/Variant960")]
        public IActionResult Lobby()
        {
            return View();
        }

        [Route("/Variant960/Game/{id}")]
        public IActionResult Game()
        {
            return View();
        }

        [Route("/Variant960/Lobby/StoreAnonymousIdentifier")]
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
