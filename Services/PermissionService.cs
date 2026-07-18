using EmployeeManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmployeeManagementSystem.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public PermissionService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }



        public async Task<bool> HasPermissionAsync(string permissionName)
        {

            var user = _httpContextAccessor
                .HttpContext?
                .User;


            if (user == null ||
                !user.Identity!.IsAuthenticated)
            {
                return false;
            }



            var roleIdClaim =
                user.FindFirst("RoleId");



            if (roleIdClaim == null)
            {
                return false;
            }



            int roleId =
                int.Parse(roleIdClaim.Value);




            bool hasPermission =
                await _context.RolePermissions
                .Include(rp => rp.Permission)
                .AnyAsync(rp =>
                    rp.RoleId == roleId &&
                    rp.Permission.PermissionName == permissionName &&
                    rp.IsAllowed);



            return hasPermission;

        }

    }
}