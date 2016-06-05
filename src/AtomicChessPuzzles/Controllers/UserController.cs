using AtomicChessPuzzles.DbRepositories;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;

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
            ViewBag.Error = null;
            return View();
        }

        [HttpPost]
        [Route("/User/Register", Name = "NewUser")]
        public IActionResult New(string username, string email, string password)
        {
            ViewBag.Error = new List<string>();
            if (!Utilities.IsValidUsername(username))
            {
                ViewBag.Error.Add("Invalid username. Usernames can only contain the characters a-z, A-Z, 0-9, _ and -.");
            }
            if (!Utilities.IsValidEmail(email))
            {
                ViewBag.Error.Add("Invalid email address.");
            }
            if (ViewBag.Error.Count > 0)
            {
                return View("Register");
            }
            else
            {
                ViewBag.Error = null;
            }
            Tuple<string, string> hashAndSalt = Utilities.HashPassword(password);
            string hash = hashAndSalt.Item1;
            string salt = hashAndSalt.Item2;
            Models.User user = new Models.User(username.ToLowerInvariant(), username, email, hash, salt, "", 0, 0,
                Models.UserRole.None, new Models.Rating(1500, 350, 0.06), new List<string>());
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

        [HttpGet]
        [Route("/User/Login", Name = "Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("/User/Login", Name = "LoginPost")]
        public IActionResult Login(string username, string password)
        {
            Models.User user = userRepository.FindByUsername(username);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            string salt = user.Salt;
            string hash = Utilities.HashPassword(password, salt);
            if (hash != user.PasswordHash)
            {
                return RedirectToAction("Login");
            }
            HttpContext.Session.SetString("user", user.Username);
            return RedirectToAction("Profile", new { name = username });
        }

        [Route("/User/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("user");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("/User/Edit")]
        public IActionResult Edit()
        {
            string username = HttpContext.Session.GetString("user");
            if (username == null)
            {
                return RedirectToAction("Login");
            }
            return View(userRepository.FindByUsername(username));
        }

        [HttpPost]
        [Route("/User/Edit", Name = "EditPost")]
        public IActionResult Edit(string email, string about)
        {
            string username = HttpContext.Session.GetString("user");
            if (username == null)
            {
                return RedirectToAction("Login");
            }
            Models.User user = userRepository.FindByUsername(username);
            user.Email = email;
            user.About = about;
            userRepository.Update(user);
            return RedirectToAction("Profile", new { name = user.Username });
        }
    }
}
