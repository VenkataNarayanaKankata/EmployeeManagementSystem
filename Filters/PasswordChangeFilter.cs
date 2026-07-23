using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeManagementSystem.Filters
{
    public class PasswordChangeFilter : IActionFilter
    {
        public void OnActionExecuting(
            ActionExecutingContext context)
        {

            var user =
                context.HttpContext.User;


            if (user.Identity != null &&
               user.Identity.IsAuthenticated)
            {

                var mustChange =
                    user.FindFirst("MustChangePassword");


                if (mustChange != null &&
                   mustChange.Value == "true")
                {

                    var controller =
                        context.RouteData.Values["controller"]
                        ?.ToString();


                    var action =
                        context.RouteData.Values["action"]
                        ?.ToString();



                    if (controller != "Account" ||
                       action != "ChangePassword")
                    {

                        context.Result =
                            new RedirectToActionResult(
                                "ChangePassword",
                                "Account",
                                null);

                    }

                }

            }

        }



        public void OnActionExecuted(
            ActionExecutedContext context)
        {

        }
    }
}