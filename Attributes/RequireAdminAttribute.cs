using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EcommercePWA.Services;

namespace EcommercePWA.Attributes
{
    public class RequireAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            
            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", new { 
                    returnUrl = context.HttpContext.Request.Path,
                    message = "Você precisa estar logado para acessar esta área."
                });
                return;
            }

            var authService = context.HttpContext.RequestServices.GetService<IAuthService>();
            var user = authService?.GetUserByIdAsync(userId.Value).Result;
            
            if (user == null || user.Role != "Admin")
            {
                context.Result = new RedirectToActionResult("Index", "Home", new { 
                    message = "Acesso negado. Você não tem permissão para acessar a área administrativa."
                });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
