using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.HttpErrors;
using AtomicChessPuzzles.Services;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using AtomicChessPuzzles.Models;
using System.Linq;

namespace AtomicChessPuzzles.Controllers
{
    public class UserController : ErrorCapableController
    {
        IUserRepository userRepository;
        IRatingRepository ratingRepository;
        IValidator validator;
        IPasswordHasher passwordHasher;

        public UserController(IUserRepository _userRepository, IRatingRepository _ratingRepository, IValidator _validator, IPasswordHasher _passwordHasher)
        {
            userRepository = _userRepository;
            ratingRepository = _ratingRepository;
            validator = _validator;
            passwordHasher = _passwordHasher;
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
            Models.User user = new Models.User(username.ToLowerInvariant(), username, email, hash, salt, "", 0, 0,
                new List<string>() { Models.UserRole.NONE }, new Models.Rating(1500, 350, 0.06), new List<string>());
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
            HttpContext.Session.SetString("username", user.Username);
            HttpContext.Session.SetString("userid", user.ID);
            return RedirectToAction("Profile", new { name = username });
        }

        [Route("/User/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("username");
            HttpContext.Session.Remove("userid");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("/User/Edit")]
        public IActionResult Edit()
        {
            string username = HttpContext.Session.GetString("username");
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
            string username = HttpContext.Session.GetString("username");
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

        [HttpGet]
        [Route("/User/RatingChartData/{user}")]
        public IActionResult RatingChartData(string user, string range, string show)
        {
            List<RatingWithMetadata> ratings;
            DateTime utcNow = DateTime.UtcNow;

            string label;
            if (show == "each")
            {
                label = "Rating";
            }
            else if (show == "bestDay")
            {
                label = "Best rating of the day";
            }
            else if (show == "endDay")
            {
                label = "Rating at end of day";
            }
            else
            {
                return Json(new { success = false, error = "Invalid 'show' parameter." });
            }

            if (range == "all")
            {
                ratings = ratingRepository.Get(user, null, null, show);
            }
            else if (range == "1d")
            {
                ratings = ratingRepository.Get(user, utcNow.Date, utcNow, show);
            }
            else if (range == "7d")
            {
                ratings = ratingRepository.Get(user, (utcNow - new TimeSpan(7, 0, 0, 0)).Date, utcNow, show);
            }
            else if (range == "30d")
            {
                ratings = ratingRepository.Get(user, (utcNow - new TimeSpan(30, 0, 0, 0)).Date, utcNow, show);
            }
            else if (range == "1y")
            {
                ratings = ratingRepository.Get(user, new DateTime(utcNow.Year - 1, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, utcNow.Second, utcNow.Millisecond), utcNow, show);
            }
            else if (range == "ytd")
            {
                ratings = ratingRepository.Get(user, new DateTime(utcNow.Year, 1, 1, 0, 0, 0, 0), utcNow, show);
            }
            else
            {
                return Json(new { success = false, error = "Invalid date range." });
            }
            List<string> labels = new List<string>();
            List<int> values = new List<int>();
            for (int i = 0; i < ratings.Count; i++)
            {
                if (show == "each")
                {
                    labels.Add(ratings[i].TimestampUtc.ToString());
                }
                else
                {
                    labels.Add(ratings[i].TimestampUtc.ToShortDateString());
                }
                values.Add((int)ratings[i].Rating.Value);
            }
            return Json(new { success = true, label = label, labels = labels, ratings = values });
        }
    }
}
