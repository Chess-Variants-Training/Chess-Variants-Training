using AtomicChessPuzzles.DbRepositories;
using Microsoft.AspNet.Mvc;
using System;

namespace AtomicChessPuzzles.Controllers
{
    public class UserController : Controller
    {
        IUserRepository userRepository;

        public UserController(IUserRepository _userRepository)
        {
            userRepository = _userRepository;
        }
        [HttpGet]
        [Route("/User/Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("/User/New", Name = "NewUser")]
        public IActionResult New(string username, string email, string password)
        {
            Tuple<string, string> hashAndSalt = PasswordUtilities.HashPassword(password);
            string hash = hashAndSalt.Item1;
            string salt = hashAndSalt.Item2;
            Models.User user = new Models.User();
            user.Username = username;
            user.Email = email;
            user.PasswordHash = hash;
            user.Salt = salt;
            bool added = userRepository.Add(user);
            return RedirectToAction("Profile", new { name = username });
        }

        [Route("/User/Profile/{name}", Name = "Profile")]
        public IActionResult Profile(string name)
        {
            Models.User user = userRepository.FindByUsername(name);
            if (user == null)
            {
                return View(new ViewModels.User("Not found"));
            }
            ViewModels.User userViewModel = new ViewModels.User(user);
            return View(userViewModel);
        }
    }
}
