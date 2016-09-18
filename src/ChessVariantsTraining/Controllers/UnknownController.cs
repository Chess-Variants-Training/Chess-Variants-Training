using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.HttpErrors;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChessVariantsTraining.Controllers
{
    public class UnknownController : CVTController
    {
        public UnknownController(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler) : base(_userRepository, _loginHandler) { }

        [Route("{*path}")]
        public IActionResult HandleUnknown(string path)
        {
            return ViewResultForHttpError(HttpContext,
                new NotFound(path + " could not be found on the server."));
        }
    }
}