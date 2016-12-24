using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChessVariantsTraining.Controllers
{
    public class Variant960Controller : CVTController
    {
        public Variant960Controller(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler) : base(_userRepository, _loginHandler) { }

        [Route("/Variant960")]
        public IActionResult Lobby()
        {
            return View();
        }
    }
}
