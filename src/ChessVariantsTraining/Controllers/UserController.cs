using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.HttpErrors;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using ChessVariantsTraining.Models;
using System.Linq;

namespace ChessVariantsTraining.Controllers
{
    public class UserController : ErrorCapableController
    {
        IUserRepository userRepository;
        IRatingRepository ratingRepository;
        IValidator validator;
        IPasswordHasher passwordHasher;
        ICounterRepository counterRepository;
        IPersistentLoginHandler loginHandler;

        public UserController(IUserRepository _userRepository, IRatingRepository _ratingRepository, IValidator _validator, IPasswordHasher _passwordHasher, ICounterRepository _counterRepository, IPersistentLoginHandler _loginHandler)
        {
            userRepository = _userRepository;
            ratingRepository = _ratingRepository;
            validator = _validator;
            passwordHasher = _passwordHasher;
            counterRepository = _counterRepository;
            loginHandler = _loginHandler;
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
            if (!validator.IsValidUsername(username))
            {
                ViewBag.Error.Add("Invalid username. Usernames can only contain the characters a-z, A-Z, 0-9, _ and -.");
            }
            if (!validator.IsValidEmail(email))
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
            Tuple<string, string> hashAndSalt = passwordHasher.HashPassword(password);
            string hash = hashAndSalt.Item1;
            string salt = hashAndSalt.Item2;
            int userId = counterRepository.GetAndIncrease(Counter.USER_ID);
            Models.User user = new Models.User(userId, username, email, hash, salt, "", 0, 0,
                new List<string>() { Models.UserRole.NONE }, new Dictionary<string, Models.Rating>()
                {
                    { "Atomic", new Models.Rating(1500, 350, 0.06) },
                    { "ThreeCheck", new Models.Rating(1500, 350, 0.06) },
                    { "KingOfTheHill", new Models.Rating(1500, 350, 0.06) },
                    { "Antichess", new Models.Rating(1500, 350, 0.06) },
                    { "Horde", new Models.Rating(1500, 350, 0.06) },
                    { "RacingKings", new Models.Rating(1500, 350, 0.06) }
                }, new List<int>());
            bool added = userRepository.Add(user);
            return RedirectToAction("Profile", new { name = username });
        }

        [Route("/User/Profile/{name}", Name = "Profile")]
        public IActionResult Profile(string name)
        {
            Models.User user = userRepository.FindByUsername(name);
            if (user == null)
            {
                return ViewResultForHttpError(HttpContext, new NotFound(string.Format("The user '{0}' could not be found.", name)));
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
            string hash = passwordHasher.HashPassword(password, salt);
            if (hash != user.PasswordHash)
            {
                return RedirectToAction("Login");
            }
            loginHandler.RegisterLogin(user.ID, HttpContext);
            return RedirectToAction("Profile", new { name = username });
        }

        [Route("/User/Logout")]
        public IActionResult Logout()
        {
            loginHandler.Logout(HttpContext);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("/User/Edit")]
        public IActionResult Edit()
        {
            int? userId = loginHandler.LoggedInUserId(HttpContext);
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }
            return View(userRepository.FindById(userId.Value));
        }

        [HttpPost]
        [Route("/User/Edit", Name = "EditPost")]
        public IActionResult Edit(string email, string about)
        {
            int? userId = loginHandler.LoggedInUserId(HttpContext);
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }
            Models.User user = userRepository.FindById(userId.Value);
            user.Email = email;
            user.About = about;
            userRepository.Update(user);
            return RedirectToAction("Profile", new { name = user.Username });
        }

        [HttpGet]
        [Route("/User/RatingChartData/{user}")]
        public IActionResult RatingChartData(string user, string range, string show)
        {
            if (!new string[] { "each", "bestDay", "endDay" }.Contains(show))
            {
                return Json(new { success = false, error = "Invalid 'show' parameter." });
            }

            List<RatingWithMetadata> ratings;
            DateTime utcNow = DateTime.UtcNow;

            User u = userRepository.FindByUsername(user);
            if (u == null)
            {
                return Json(new { success = false, error = "User not found." });
            }

            int userId = u.ID;

            if (range == "all")
            {
                ratings = ratingRepository.Get(userId, null, null, show);
            }
            else if (range == "1d")
            {
                ratings = ratingRepository.Get(userId, utcNow.Date, utcNow, show);
            }
            else if (range == "7d")
            {
                ratings = ratingRepository.Get(userId, (utcNow - new TimeSpan(7, 0, 0, 0)).Date, utcNow, show);
            }
            else if (range == "30d")
            {
                ratings = ratingRepository.Get(userId, (utcNow - new TimeSpan(30, 0, 0, 0)).Date, utcNow, show);
            }
            else if (range == "1y")
            {
                ratings = ratingRepository.Get(userId, new DateTime(utcNow.Year - 1, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, utcNow.Second, utcNow.Millisecond), utcNow, show);
            }
            else if (range == "ytd")
            {
                ratings = ratingRepository.Get(userId, new DateTime(utcNow.Year, 1, 1, 0, 0, 0, 0), utcNow, show);
            }
            else
            {
                return Json(new { success = false, error = "Invalid date range." });
            }
            RatingChartData chart = new RatingChartData(ratings, show == "each");
            return Json(new { success = true, labels = chart.Labels, ratings = chart.Ratings });
        }
    }
}
