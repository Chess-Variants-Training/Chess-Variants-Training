using ChessVariantsTraining.HttpErrors;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;

namespace ChessVariantsTraining.Controllers
{
    public class ErrorCapableController : Controller
    {
         public ViewResult ViewResultForHttpError(HttpContext context, HttpError err)
         {
             context.Response.StatusCode = err.StatusCode;
             return View("Error", err);
         }
    }
}