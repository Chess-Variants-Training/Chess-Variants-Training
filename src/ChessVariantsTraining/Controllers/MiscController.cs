using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChessVariantsTraining.Controllers
{
    public class MiscController : CVTController
    {

        public MiscController(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler) : base(_userRepository, _loginHandler) { }

        [Route("/Terms-of-Service")]
        public IActionResult TermsOfService()
        {
            return View();
        }

        [Route("/Privacy-Policy")]
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
    }
}
