using AtomicChessPuzzles.Attributes;
using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.HttpErrors;
using AtomicChessPuzzles.Models;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Http;
using System.Collections.Generic;

namespace AtomicChessPuzzles.Controllers
{
    public class RestrictedController : ErrorCapableController
    {
        protected IUserRepository userRepository;

        public RestrictedController(IUserRepository _userRepository)
        {
            userRepository = _userRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ControllerActionDescriptor descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            object[] actionAttrs = descriptor.MethodInfo.GetCustomAttributes(typeof(RestrictedAttribute), false);
            RestrictedAttribute attr = null;
            // 'Restricted' on an action overrides 'Restricted' on their controller
            if (actionAttrs == null || actionAttrs.Length == 0)
            {
                object[] controllerAttrs = descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RestrictedAttribute), true);
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
            int? userId = context.HttpContext.Session.GetInt32("userid");
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