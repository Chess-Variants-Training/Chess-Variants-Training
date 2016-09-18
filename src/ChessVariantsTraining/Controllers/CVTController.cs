using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.HttpErrors;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChessVariantsTraining.Controllers
{
    public class CVTController : Controller
    {
        protected IUserRepository userRepository;
        protected IPersistentLoginHandler loginHandler;

        public CVTController(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler)
        {
            userRepository = _userRepository;
            loginHandler = _loginHandler;
        }

        public ViewResult ViewResultForHttpError(HttpContext context, HttpError err)
        {
            context.Response.StatusCode = err.StatusCode;
            return View("Error", err);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Same-origin check for non-GET requests:
            if (context.HttpContext.Request.Method.ToUpperInvariant() != "GET")
            {
                string originHost = null;
                if (context.HttpContext.Request.Headers.ContainsKey("Origin"))
                {
                    originHost = new Uri(context.HttpContext.Request.Headers["Origin"]).Host;
                }
                else if (context.HttpContext.Request.Headers.ContainsKey("Referer"))
                {
                    originHost = new Uri(context.HttpContext.Request.Headers["Referer"]).Host;
                }

                string expectedOriginHost = context.HttpContext.Request.Host.Host;
                if (originHost != expectedOriginHost)
                {
                    context.Result = ViewResultForHttpError(context.HttpContext, new BadRequest("This request was not trusted."));
                    return;
                }
            }

            // Role check:
            ControllerActionDescriptor descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            RestrictedAttribute[] actionAttrs = descriptor.MethodInfo.GetCustomAttributes<RestrictedAttribute>(false)?.ToArray();
            RestrictedAttribute attr = null;
            // 'Restricted' on an action overrides 'Restricted' on their controller
            if (actionAttrs == null || actionAttrs.Length == 0)
            {
                RestrictedAttribute[] controllerAttrs = descriptor.ControllerTypeInfo.GetCustomAttributes<RestrictedAttribute>(true)?.ToArray();
                if (controllerAttrs == null || controllerAttrs.Length == 0)
                {
                    base.OnActionExecuting(context);
                    return;
                }
                attr = controllerAttrs[0] as RestrictedAttribute;
            }
            else
            {
                attr = actionAttrs[0] as RestrictedAttribute;        
            }
            int? userId = loginHandler.LoggedInUserId(context.HttpContext);
            bool loggedIn = userId.HasValue;
            if (attr.LoginRequired && !loggedIn)
            {
                context.Result = ViewResultForHttpError(context.HttpContext, new Forbidden("You need to be logged in."));
                return;
            }
            List<string> roles = loggedIn ? userRepository.FindById(userId.Value).Roles : new List<string>() { UserRole.NONE };
            if (!UserRole.HasAtLeastThePrivilegesOf(roles, attr.Roles))
            {
                context.Result = ViewResultForHttpError(context.HttpContext, new Forbidden("You don't have enough privileges to do this."));
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}