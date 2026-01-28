using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClientServerCommunication.Security
{
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            var role = httpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role) || !_roles.Contains(role))
            {
                context.Result = new RedirectToPageResult("/AccessDenied");
            }
        }
    }
}
