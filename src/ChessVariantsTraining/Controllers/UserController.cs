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
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using ChessVariantsTraining.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace ChessVariantsTraining.Controllers
{
    public class UserController : CVTController
    {
        static HttpClient captchaClient = new HttpClient();

        IRatingRepository ratingRepository;
        IValidator validator;
        IPasswordHasher passwordHasher;
        ICounterRepository counterRepository;
        ITimedTrainingScoreRepository timedTrainingScoreRepository;
        IUserVerifier userVerifier;
        IEmailSender emailSender;
        IGameRepository gameRepository;
        IAttemptRepository attemptRepository;
        string recaptchaKey;
        bool requireEmailVerification;

        public UserController(IUserRepository _userRepository,
            IRatingRepository _ratingRepository,
            IValidator _validator,
            IPasswordHasher _passwordHasher,
            ICounterRepository _counterRepository,
            IPersistentLoginHandler _loginHandler,
            ITimedTrainingScoreRepository _timedTrainingScoreRepository,
            IUserVerifier _userVerifier,
            IEmailSender _emailSender,
            IGameRepository _gameRepository,
            IAttemptRepository _attemptRepository,
            IOptions<Settings> settings)
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
            attemptRepository = _attemptRepository;
            recaptchaKey = settings.Value.RecaptchaKey;
            requireEmailVerification = settings.Value.Email.RequireEmailVerification;
        }

        [HttpGet]
        [Route("/User/Register")]
        public async Task<IActionResult> Register()
        {
            if ((await loginHandler.LoggedInUserIdAsync(HttpContext)).HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = null;
            return View();
        }

        [HttpPost]
        [Route("/User/Register", Name = "NewUser")]
        public async Task<IActionResult> New(string username, string email, string password, string passwordConfirmation, [FromForm(Name = "g-recaptcha-response")] string gRecaptchaResponse)
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
            if (await userRepository.FindByUsernameAsync(username) != null)
            {
                ViewBag.Error.Add("The username is already taken.");
            }
            if (await userRepository.FindByEmailAsync(email) != null)
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

            if (!string.IsNullOrWhiteSpace(recaptchaKey))
            {
                Dictionary<string, string> captchaRequestValues = new Dictionary<string, string>()
                {
                    { "secret", recaptchaKey },
                    { "response", gRecaptchaResponse }
                };
                FormUrlEncodedContent content = new FormUrlEncodedContent(captchaRequestValues);
                HttpResponseMessage response = await captchaClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                string responseString = await response.Content.ReadAsStringAsync();
                Dictionary<string, dynamic> jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(responseString);
                if (!((bool)jsonResponse["success"]))
                {
                    ViewBag.Error.Add("Captcha verification failed.");
                }
            }

            if (ViewBag.Error.Count > 0)
            {
                return View("Register");
            }
            else
            {
                ViewBag.Error = null;
            }
            Task<int> userId = counterRepository.GetAndIncreaseAsync(Counter.USER_ID);
            Tuple<string, string> hashAndSalt = passwordHasher.HashPassword(password);
            string hash = hashAndSalt.Item1;
            string salt = hashAndSalt.Item2;
            User user = new User(await userId, username, email, hash, salt, "", 0, 0,
                new List<string>() { UserRole.NONE }, new Dictionary<string, Rating>()
                {
                    { "Atomic", new Rating(1500, 350, 0.06) },
                    { "ThreeCheck", new Rating(1500, 350, 0.06) },
                    { "KingOfTheHill", new Rating(1500, 350, 0.06) },
                    { "Antichess", new Rating(1500, 350, 0.06) },
                    { "Horde", new Rating(1500, 350, 0.06) },
                    { "RacingKings", new Rating(1500, 350, 0.06) },
                    { "Crazyhouse", new Rating(1500, 350, 0.06) }
                }, new List<int>());
            if (!requireEmailVerification)
            {
                user.VerificationCode = 0;
                user.Verified = true;
            }
            await userRepository.AddAsync(user);
            if (requireEmailVerification)
            {
                await userVerifier.SendVerificationEmailToAsync(user.ID);
            }
            await loginHandler.RegisterLoginAsync(user.ID, HttpContext);
            return RedirectToAction("Profile", new { id = user.ID });
        }

        [Route("/User/Profile/{id:int}")]
        public async Task<IActionResult> Profile(int id)
        {
            User user = await userRepository.FindByIdAsync(id);
            if (user == null)
            {
                return ViewResultForHttpError(HttpContext, new NotFound(string.Format("The user with ID '{0}' could not be found.", id)));
            }
            long gamesPlayed = await gameRepository.CountByPlayerIdAsync(id);
            ViewModels.User userViewModel = new ViewModels.User(user, gamesPlayed);
            return View(userViewModel);
        }

        [HttpGet]
        [Route("/User/Login", Name = "Login")]
        public async Task<IActionResult> Login()
        {
            if ((await loginHandler.LoggedInUserIdAsync(HttpContext)).HasValue)
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
        public async Task<IActionResult> LoginPost(string username, string password)
        {
            User user;
            if (!username.Contains("@"))
            {
                user = await userRepository.FindByUsernameAsync(username);
            }
            else
            {
                user = await userRepository.FindByEmailAsync(username);
            }
            if (user == null)
            {
                TempData["Error"] = "Invalid username/email or password.";
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
                TempData["Error"] = "Invalid username/email or password.";
                return RedirectToAction("Login");
            }
            await loginHandler.RegisterLoginAsync(user.ID, HttpContext);
            return RedirectToAction("Profile", new { id = user.ID });
        }

        [Route("/User/Logout")]
        [NoVerificationNeeded]
        public async Task<IActionResult> Logout()
        {
            await loginHandler.LogoutAsync(HttpContext);
            return RedirectToAction("Index", "Home");
        }

        [Route("/User/LogoutEverywhereElse")]
        [NoVerificationNeeded]
        public async Task<IActionResult> LogoutEverywhereElse()
        {
            await loginHandler.LogoutEverywhereExceptHereAsync(HttpContext);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("/User/Edit")]
        [NoVerificationNeeded]
        public async Task<IActionResult> Edit()
        {
            int? userId = await loginHandler.LoggedInUserIdAsync(HttpContext);
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }
            return View(await userRepository.FindByIdAsync(userId.Value));
        }

        [HttpPost]
        [Route("/User/Edit", Name = "EditPost")]
        [NoVerificationNeeded]
        public async Task<IActionResult> Edit(string username, string email, string about)
        {
            int? userId = await loginHandler.LoggedInUserIdAsync(HttpContext);
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }
            User user = await userRepository.FindByIdAsync(userId.Value);
            user.Username = username;
            user.Email = email;
            user.About = about;
            await userRepository.UpdateAsync(user);
            return RedirectToAction("Profile", new { id = user.ID });
        }

        [HttpGet]
        [Route("/User/ChartData/{type}/{user:int}/{range}/{show}")]
        public async Task<IActionResult> ChartData(string type, int user, string range, string show)
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

            User u = await userRepository.FindByIdAsync(user);
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
                List<RatingWithMetadata> ratings = await ratingRepository.GetAsync(userId, from, to, show);
                RatingChartData chart = new RatingChartData(ratings, show == "each");
                return Json(new { success = true, labels = chart.Labels, ratings = chart.Ratings });
            }
            else // type == "TimedTraining"
            {
                List<TimedTrainingScore> scores = await timedTrainingScoreRepository.GetAsync(userId, from, to, show);
                TimedTrainingChartData chart = new TimedTrainingChartData(scores, show == "each");
                return Json(new { success = true, labels = chart.Labels, scores = chart.Scores });
            }
        }

        [HttpPost]
        [NoVerificationNeeded]
        [Restricted(true, UserRole.NONE)]
        [Route("/User/Verify")]
        public async Task<IActionResult> Verify(string verificationCode)
        {
            if (!int.TryParse(verificationCode, out int verificationCodeI))
            {
                return View("VerificationFailed");
            }

            if (await userVerifier.VerifyAsync((await loginHandler.LoggedInUserIdAsync(HttpContext)).Value, verificationCodeI))
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
        public async Task<IActionResult> SendPasswordReset(string email)
        {
            if (!validator.IsValidEmail(email))
            {
                return View("NoResetLinkSent");
            }

            User user = await userRepository.FindByEmailAsync(email);
            if (user == null)
            {
                return View("NoResetLinkSent");
            }

            PasswordRecoveryToken token = new PasswordRecoveryToken();
            user.PasswordRecoveryToken = token;
            await userRepository.UpdateAsync(user);
            emailSender.Send(user.Email, user.Username, "Chess Variants Training: Password Reset",
                string.Format("A password reset for your account was requested. Copy this link and paste it in your browser window to reset your password: {0}",
                Url.Action("ResetPassword", "User", new { token = token.TokenUnhashed }, Request.Scheme)));
            return View("ResetLinkSent");
        }

        [HttpGet]
        [Route("/User/ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromQuery] string token)
        {
            User associated = await userRepository.FindByPasswordResetTokenAsync(token);
            if (associated == null)
            {
                return View("PasswordResetFailed");
            }
            ViewBag.Error = null;
            return View("ResetPassword", token);
        }

        [HttpPost]
        [Route("/User/ResetPassword")]
        public async Task<IActionResult> ResetPasswordPost(string password, string confirm, string token)
        {
            User associated = await userRepository.FindByPasswordResetTokenAsync(token);
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
            await userRepository.UpdateAsync(associated);
            await loginHandler.LogoutEverywhereAsync(associated.ID);
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
        public async Task<IActionResult> ChangePasswordPost(string currentPassword, string newPassword, string newPasswordConfirm)
        {
            User loggedIn = await loginHandler.LoggedInUserAsync(HttpContext);
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
            await userRepository.UpdateAsync(loggedIn);
            await loginHandler.LogoutEverywhereExceptHereAsync(HttpContext);
            return View("PasswordUpdated");
        }

        [HttpGet]
        [Route("/User/GameList/{id:int}")]
        public async Task<IActionResult> GameList(int id, [FromQuery] int page = 1)
        {
            int perPage = 25;
            if (page < 1)
            {
                return ViewResultForHttpError(HttpContext, new NotFound(string.Format("Page number too low.")));
            }
            User player = await userRepository.FindByIdAsync(id);
            if (player == null)
            {
                return ViewResultForHttpError(HttpContext, new NotFound(string.Format("The user with ID '{0}' could not be found.", id)));
            }
            long gameCount = await gameRepository.CountByPlayerIdAsync(id);
            if (gameCount == 0)
            {
                return ViewResultForHttpError(HttpContext, new NotFound(string.Format("This user hasn't played any games.")));
            }
            if ((page - 1) * perPage >= gameCount)
            {
                return ViewResultForHttpError(HttpContext, new NotFound(string.Format("Page number too high.")));
            }
            List<Game> gamesByPlayer = await gameRepository.GetByPlayerIdAsync(id, (page - 1) * perPage, perPage);
            IEnumerable<int> opponentsWhenWhite = gamesByPlayer.Where(x => (x.White as RegisteredPlayer)?.UserId == id && x.Black is RegisteredPlayer).Select(x => (x.Black as RegisteredPlayer).UserId);
            IEnumerable<int> opponentsWhenBlack = gamesByPlayer.Where(x => (x.Black as RegisteredPlayer)?.UserId == id && x.White is RegisteredPlayer).Select(x => (x.White as RegisteredPlayer).UserId);
            IEnumerable<int> allOpponentIds = opponentsWhenBlack.Concat(opponentsWhenWhite).Distinct();
            Dictionary<int, User> players = await userRepository.FindByIdsAsync(allOpponentIds);
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
            IEnumerable<int> pagesToShow = Enumerable.Range(1, (int)Math.Ceiling(gameCount / (float)perPage));
            ViewModels.GameListView model = new ViewModels.GameListView(player.Username, light, page, pagesToShow);
            return View(model);
        }

        [HttpGet]
        [Route("/User/History")]
        [Restricted(true, UserRole.NONE)]
        public async Task<IActionResult> History([FromQuery] int page = 1)
        {
            int perPage = 25;
            if (page < 1)
            {
                return ViewResultForHttpError(HttpContext, new NotFound("Page number too low."));
            }
            int user = (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value;
            long count = await attemptRepository.CountAsync(user);
            if (count == 0)
            {
                return ViewResultForHttpError(HttpContext, new NotFound("You haven't done any puzzles."));
            }
            if ((page - 1) * perPage >= count)
            {
                return ViewResultForHttpError(HttpContext, new NotFound("Page number too high."));
            }
            Task<List<Attempt>> attempts = attemptRepository.GetAsync(user, (page - 1) * perPage, perPage);
            IEnumerable<int> pagesToShow = Enumerable.Range(1, (int)Math.Ceiling(count / (float)perPage));
            ViewModels.PuzzleHistoryView model = new ViewModels.PuzzleHistoryView(pagesToShow, await attempts, page);
            return View(model);
        }
    }
}
