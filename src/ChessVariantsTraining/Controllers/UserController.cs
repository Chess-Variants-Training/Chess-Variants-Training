using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.HttpErrors;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using ChessVariantsTraining.Models;
using System.Linq;
using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.Models.Variant960;
using ChessVariantsTraining.DbRepositories.Variant960;

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
        IGameRepository gameRepository;

        public UserController(IUserRepository _userRepository,
            IRatingRepository _ratingRepository,
            IValidator _validator,
            IPasswordHasher _passwordHasher,
            ICounterRepository _counterRepository,
            IPersistentLoginHandler _loginHandler,
            ITimedTrainingScoreRepository _timedTrainingScoreRepository,
            IUserVerifier _userVerifier,
            IEmailSender _emailSender,
            IGameRepository _gameRepository)
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
            gameRepository = _gameRepository;
        }

        [HttpGet]
        [Route("/User/Register")]
        public IActionResult Register()
        {
            if (loginHandler.LoggedInUserId(HttpContext).HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = null;
            return View();
        }

        [HttpPost]
        [Route("/User/Register", Name = "NewUser")]
        public IActionResult New(string username, string email, string password, string passwordConfirmation)
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

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordConfirmation))
            {
                ViewBag.Error.Add("Your password or its confirmation cannot be empty.");
            }
            else if (!password.Equals(passwordConfirmation))
            {
                ViewBag.Error.Add("The password does not match its confirmation.");
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
            long gamesPlayed = gameRepository.CountByPlayerId(id);
            ViewModels.User userViewModel = new ViewModels.User(user, gamesPlayed);
            return View(userViewModel);
        }

        [HttpGet]
        [Route("/User/Login", Name = "Login")]
        public IActionResult Login()
        {
            if (loginHandler.LoggedInUserId(HttpContext).HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
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
            return RedirectToAction("Profile", new { id = user.ID });
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
            loginHandler.LogoutEverywhere(associated.ID);
            return View("PasswordUpdated");
        }

        [HttpGet]
        [Route("/User/ChangePassword")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult ChangePassword()
        {
            ViewBag.Error = TempData["Error"];
            return View();
        }

        [HttpPost]
        [Route("/User/ChangePassword")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult ChangePasswordPost(string currentPassword, string newPassword, string newPasswordConfirm)
        {
            User loggedIn = loginHandler.LoggedInUser(HttpContext);
            string salt = loggedIn.Salt;
            string hash = loggedIn.PasswordHash;
            if (string.IsNullOrEmpty(currentPassword) || hash != passwordHasher.HashPassword(currentPassword, salt))
            {
                TempData["Error"] = "Invalid current password.";
                return RedirectToAction("ChangePassword", "User");
            }
            if (newPassword != newPasswordConfirm)
            {
                TempData["Error"] = "The password and its confirmation don't match.";
                return RedirectToAction("ChangePassword", "User");
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                TempData["Error"] = "Your new password cannot be empty.";
                return RedirectToAction("ChangePassword", "User");
            }
            Tuple<string, string> hashAndSalt = passwordHasher.HashPassword(newPassword);
            loggedIn.PasswordHash = hashAndSalt.Item1;
            loggedIn.Salt = hashAndSalt.Item2;
            userRepository.Update(loggedIn);
            loginHandler.LogoutEverywhereExceptHere(HttpContext);
            return View("PasswordUpdated");
        }

        [HttpGet]
        [Route("/User/GameList/{id:int}")]
        public IActionResult GameList(int id, [FromQuery] int page = 1)
        {
            int perPage = 25;
            if (page < 1)
            {
                return ViewResultForHttpError(HttpContext, new NotFound(string.Format("Page number too low.")));
            }
            User player = userRepository.FindById(id);
            if (player == null)
            {
                return ViewResultForHttpError(HttpContext, new NotFound(string.Format("The user with ID '{0}' could not be found.", id)));
            }
            long gameCount = gameRepository.CountByPlayerId(id);
            if (gameCount == 0)
            {
                return ViewResultForHttpError(HttpContext, new NotFound(string.Format("This user hasn't played any games.")));
            }
            if ((page - 1) * perPage >= gameCount)
            {
                return ViewResultForHttpError(HttpContext, new NotFound(string.Format("Page number too high.")));
            }
            List<Game> gamesByPlayer = gameRepository.GetByPlayerId(id, (page - 1) * perPage, perPage);
            IEnumerable<int> opponentsWhenWhite = gamesByPlayer.Where(x => (x.White as RegisteredPlayer)?.UserId == id && x.Black is RegisteredPlayer).Select(x => (x.Black as RegisteredPlayer).UserId);
            IEnumerable<int> opponentsWhenBlack = gamesByPlayer.Where(x => (x.Black as RegisteredPlayer)?.UserId == id && x.White is RegisteredPlayer).Select(x => (x.White as RegisteredPlayer).UserId);
            IEnumerable<int> allOpponentIds = opponentsWhenBlack.Concat(opponentsWhenWhite).Distinct();
            Dictionary<int, User> players = userRepository.FindByIds(allOpponentIds);
            players[id] = player;
            List<ViewModels.LightGame> light = new List<ViewModels.LightGame>();
            foreach (Game game in gamesByPlayer)
            {
                string white;
                string black;
                
                if (game.White is AnonymousPlayer)
                {
                    white = "(Anonymous)";
                }
                else
                {
                    white = players[(game.White as RegisteredPlayer).UserId].Username;
                }

                if (game.Black is AnonymousPlayer)
                {
                    black = "(Anonymous)";
                }
                else
                {
                    black = players[(game.Black as RegisteredPlayer).UserId].Username;
                }

                string result = Game.Results.ToFriendlyString(game.Result);
                string url = Url.Action("Game", "Variant960", new { id = game.ID });
                light.Add(new ViewModels.LightGame(white, black, result, url, game.StartedUtc.ToString("dd/MM/yyyy HH:mm")));
            }
            IEnumerable<int> pagesToShow = Enumerable.Range(1, (int)Math.Ceiling(gameCount / 20F));
            ViewModels.GameListView model = new ViewModels.GameListView(player.Username, light, page, pagesToShow);
            return View(model);
        }
    }
}
