using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.HttpErrors;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using ChessVariantsTraining.Models;
using System.Linq;
using ChessVariantsTraining.Attributes;

namespace ChessVariantsTraining.Controllers
{
    public class UserController : CVTController
    {
        IRatingRepository ratingRepository;
        IValidator validator;
        IPasswordHasher passwordHasher;
        ICounterRepository counterRepository;
        ITimedTrainingScoreRepository timedTrainingScoreRepository;
        IUserVerifier userVerifier;
        IEmailSender emailSender;

        public UserController(IUserRepository _userRepository,
            IRatingRepository _ratingRepository,
            IValidator _validator,
            IPasswordHasher _passwordHasher,
            ICounterRepository _counterRepository,
            IPersistentLoginHandler _loginHandler,
            ITimedTrainingScoreRepository _timedTrainingScoreRepository,
            IUserVerifier _userVerifier,
            IEmailSender _emailSender)
            : base(_userRepository, _loginHandler)
        {
            userRepository = _userRepository;
            ratingRepository = _ratingRepository;
            validator = _validator;
            passwordHasher = _passwordHasher;
            counterRepository = _counterRepository;
            loginHandler = _loginHandler;
            timedTrainingScoreRepository = _timedTrainingScoreRepository;
            userVerifier = _userVerifier;
            emailSender = _emailSender;
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
            if (userRepository.FindByUsername(username) != null)
            {
                ViewBag.Error.Add("The username is already taken.");
            }
            if (userRepository.FindByEmail(email) != null)
            {
                ViewBag.Error.Add("The email address is already taken.");
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
            User user = new User(userId, username, email, hash, salt, "", 0, 0,
                new List<string>() { UserRole.NONE }, new Dictionary<string, Rating>()
                {
                    { "Atomic", new Rating(1500, 350, 0.06) },
                    { "ThreeCheck", new Rating(1500, 350, 0.06) },
                    { "KingOfTheHill", new Rating(1500, 350, 0.06) },
                    { "Antichess", new Rating(1500, 350, 0.06) },
                    { "Horde", new Rating(1500, 350, 0.06) },
                    { "RacingKings", new Rating(1500, 350, 0.06) }
                }, new List<int>());
            bool added = userRepository.Add(user);
            userVerifier.SendVerificationEmailTo(user.ID);
            loginHandler.RegisterLogin(user.ID, HttpContext);
            return RedirectToAction("Profile", new { id = user.ID });
        }

        [Route("/User/Profile/{id:int}")]
        public IActionResult Profile(int id)
        {
            User user = userRepository.FindById(id);
            if (user == null)
            {
                return ViewResultForHttpError(HttpContext, new NotFound(string.Format("The user with ID '{0}' could not be found.", id)));
            }
            ViewModels.User userViewModel = new ViewModels.User(user);
            return View(userViewModel);
        }

        [HttpGet]
        [Route("/User/Login", Name = "Login")]
        public IActionResult Login()
        {
            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
            }
            else
            {
                ViewBag.Error = null;
            }
            return View();
        }

        [HttpPost]
        [Route("/User/Login", Name = "LoginPost")]
        public IActionResult LoginPost(string username, string password)
        {
            User user = userRepository.FindByUsername(username);
            if (user == null)
            {
                TempData["Error"] = "Invalid username or password.";
                return RedirectToAction("Login");
            }
            if (user.Closed)
            {
                TempData["Error"] = "This account is closed.";
                return RedirectToAction("Login");
            }
            string salt = user.Salt;
            string hash = passwordHasher.HashPassword(password, salt);
            if (hash != user.PasswordHash)
            {
                TempData["Error"] = "Invalid username or password.";
                return RedirectToAction("Login");
            }
            loginHandler.RegisterLogin(user.ID, HttpContext);
            return RedirectToAction("Profile", new { id = user.ID });
        }

        [Route("/User/Logout")]
        [NoVerificationNeeded]
        public IActionResult Logout()
        {
            loginHandler.Logout(HttpContext);
            return RedirectToAction("Index", "Home");
        }

        [Route("/User/LogoutEverywhereElse")]
        [NoVerificationNeeded]
        public IActionResult LogoutEverywhereElse()
        {
            loginHandler.LogoutEverywhereExceptHere(HttpContext);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("/User/Edit")]
        [NoVerificationNeeded]
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
        [NoVerificationNeeded]
        public IActionResult Edit(string email, string about)
        {
            int? userId = loginHandler.LoggedInUserId(HttpContext);
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }
            User user = userRepository.FindById(userId.Value);
            user.Email = email;
            user.About = about;
            userRepository.Update(user);
            return RedirectToAction("Profile", new { name = user.Username });
        }

        [HttpGet]
        [Route("/User/ChartData/{type}/{user:int}/{range}/{show}")]
        public IActionResult ChartData(string type, int user, string range, string show)
        {
            if (!new string[] { "Rating", "TimedTraining" }.Contains(type))
            {
                return Json(new { success = false, error = "Invalid chart type." });
            }

            if (!new string[] { "each", "bestDay", type == "Rating" ? "endDay" : "avgDay" }.Contains(show))
            {
                return Json(new { success = false, error = "Invalid 'show' parameter." });
            }

            DateTime utcNow = DateTime.UtcNow;
            DateTime? from;
            DateTime? to;

            User u = userRepository.FindById(user);
            if (u == null)
            {
                return Json(new { success = false, error = "User not found." });
            }

            int userId = u.ID;

            if (range == "all")
            {
                from = to = null;
            }
            else if (range == "1d")
            {
                from = utcNow.Date;
                to = utcNow;
            }
            else if (range == "7d")
            {
                from = (utcNow - new TimeSpan(7, 0, 0, 0)).Date;
                to = utcNow;
            }
            else if (range == "30d")
            {
                from = (utcNow - new TimeSpan(30, 0, 0, 0)).Date;
                to = utcNow;
            }
            else if (range == "1y")
            {
                from = new DateTime(utcNow.Year - 1, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, utcNow.Second, utcNow.Millisecond);
                to = utcNow;
            }
            else if (range == "ytd")
            {
                from = new DateTime(utcNow.Year, 1, 1, 0, 0, 0, 0);
                to = utcNow;
            }
            else
            {
                return Json(new { success = false, error = "Invalid date range." });
            }

            if (type == "Rating")
            {
                List<RatingWithMetadata> ratings = ratingRepository.Get(userId, from, to, show);
                RatingChartData chart = new RatingChartData(ratings, show == "each");
                return Json(new { success = true, labels = chart.Labels, ratings = chart.Ratings });
            }
            else // type == "TimedTraining"
            {
                List<TimedTrainingScore> scores = timedTrainingScoreRepository.Get(userId, from, to, show);
                TimedTrainingChartData chart = new TimedTrainingChartData(scores, show == "each");
                return Json(new { success = true, labels = chart.Labels, scores = chart.Scores });
            }
        }

        [HttpPost]
        [NoVerificationNeeded]
        [Restricted(true, UserRole.NONE)]
        [Route("/User/Verify")]
        public IActionResult Verify(string verificationCode)
        {
            int verificationCodeI;
            if (!int.TryParse(verificationCode, out verificationCodeI))
            {
                return View("VerificationFailed");
            }

            if (userVerifier.Verify(loginHandler.LoggedInUserId(HttpContext).Value, verificationCodeI))
            {
                return View("Verified");
            }
            else
            {
                return View("VerificationFailed");
            }
        }

        [Route("/User/ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [Route("/User/SendPasswordReset")]
        public IActionResult SendPasswordReset(string email)
        {
            if (!validator.IsValidEmail(email))
            {
                return View("NoResetLinkSent");
            }

            User user = userRepository.FindByEmail(email);
            if (user == null)
            {
                return View("NoResetLinkSent");
            }

            PasswordRecoveryToken token = new PasswordRecoveryToken();
            user.PasswordRecoveryToken = token;
            userRepository.Update(user);
            emailSender.Send(user.Email, user.Username, "Chess Variants Training: Password Reset",
                string.Format("A password reset for your account was requested. Copy this link and paste it in your browser window to reset your password: {0}",
                Url.Action("ResetPassword", "User", new { token = token.TokenUnhashed }, Request.Scheme)));
            return View("ResetLinkSent");
        }

        [HttpGet]
        [Route("/User/ResetPassword")]
        public IActionResult ResetPassword([FromQuery] string token)
        {
            User associated = userRepository.FindByPasswordResetToken(token);
            if (associated == null)
            {
                return View("PasswordResetFailed");
            }
            ViewBag.Error = null;
            return View("ResetPassword", token);
        }

        [HttpPost]
        [Route("/User/ResetPassword")]
        public IActionResult ResetPasswordPost(string password, string confirm, string token)
        {
            User associated = userRepository.FindByPasswordResetToken(token);
            if (associated == null)
            {
                return View("PasswordResetFailed");
            }
            if (password != confirm)
            {
                ViewBag.Error = "The password and the password confirmation aren't equal. Please try again.";
                return View("ResetPassword", token);
            }
            Tuple<string, string> hashAndSalt = passwordHasher.HashPassword(password);
            associated.PasswordHash = hashAndSalt.Item1;
            associated.Salt = hashAndSalt.Item2;
            associated.PasswordRecoveryToken = null;
            userRepository.Update(associated);
            return View("PasswordUpdated");
        }
    }
}
