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

            // Kullanıcı login değilse Login sayfasına yönlendir
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
