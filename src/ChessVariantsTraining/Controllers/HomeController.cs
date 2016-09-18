using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChessVariantsTraining.Controllers
{
    public class HomeController : CVTController
    {
        public HomeController(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler) : base(_userRepository, _loginHandler) { }

        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
