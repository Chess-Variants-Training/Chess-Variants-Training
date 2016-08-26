using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.HttpErrors;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChessVariantsTraining.Controllers
{
    public class RestrictedController : ErrorCapableController
    {
        protected IUserRepository userRepository;
        protected IPersistentLoginHandler loginHandler;

        public RestrictedController(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler)
        {
            userRepository = _userRepository;
            loginHandler = _loginHandler;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
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