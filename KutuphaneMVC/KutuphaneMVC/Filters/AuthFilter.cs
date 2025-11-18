using Microsoft.AspNetCore.Http;              // Session erişimi için 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KutuphaneMVC.Filters
{
    public class AuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var username = session.GetString("username");

            // Eğer giriş yapılmamışsa Login sayfasına yönlendir
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
