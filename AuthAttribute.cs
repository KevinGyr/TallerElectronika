using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TallerElectronika
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(userId))
            {
                // No autenticado, redirige a login
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}