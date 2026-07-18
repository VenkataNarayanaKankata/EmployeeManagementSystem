using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeManagementSystem.Authorization
{
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IPermissionService _permissionService;
        private readonly string _permissionName;


        public PermissionAuthorizationFilter(
            IPermissionService permissionService,
            string permissionName)
        {
            _permissionService = permissionService;
            _permissionName = permissionName;
        }



        public async Task OnAuthorizationAsync(
            AuthorizationFilterContext context)
        {

            bool hasPermission =
                await _permissionService
                .HasPermissionAsync(_permissionName);



            if (!hasPermission)
            {
                context.Result =
                    new RedirectToActionResult(
                        "AccessDenied",
                        "Account",
                        null);
            }

        }
    }
}