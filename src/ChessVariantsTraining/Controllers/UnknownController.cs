using ChessVariantsTraining.HttpErrors;
using Microsoft.AspNet.Mvc;

namespace ChessVariantsTraining.Controllers
{
    public class UnknownController : ErrorCapableController
    {
        [Route("{*path}")]
        public IActionResult HandleUnknown(string path)
        {
            return ViewResultForHttpError(HttpContext,
                new NotFound(path + " could not be found on the server."));
        }
    }
}