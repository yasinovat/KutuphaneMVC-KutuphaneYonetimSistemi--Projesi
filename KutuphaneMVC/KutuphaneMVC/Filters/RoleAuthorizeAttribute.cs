using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KutuphaneMVC.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        public string[]? Roles { get; set; }

        public AuthorizeRoleAttribute(params string[] roles)
        {
            Roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var username = session.GetString("username");
            var userRole = session.GetString("role");

            // Oturum kontrolü
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            // Rol kontrolü (eğer belirtilmişse)
            if (Roles != null && Roles.Length > 0)
            {
                if (string.IsNullOrEmpty(userRole) || !Roles.Contains(userRole))
                {
                    // Yetkisiz erişim - Login'e yönlendir veya 403 göster
                    context.Result = new RedirectToActionResult("Index", "Login", null);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
