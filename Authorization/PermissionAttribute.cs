using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Authorization
{
    public class PermissionAttribute : TypeFilterAttribute
    {
        public PermissionAttribute(string permissionName)
            : base(typeof(PermissionAuthorizationFilter))
        {
            Arguments = new object[]
            {
                permissionName
            };
        }
    }
}