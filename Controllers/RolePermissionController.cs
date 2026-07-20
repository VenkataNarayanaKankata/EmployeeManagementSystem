using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class RolePermissionController : Controller
    {
        private readonly ApplicationDbContext _context;


        public RolePermissionController(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<IActionResult> Index(
            int? roleId,
            List<string>? selectedModules)
        {

            var roles = await _context.Roles
    .Where(r =>
        r.IsActive &&
        r.RoleName != "Super Admin")
    .OrderBy(r => r.RoleName)
    .ToListAsync();
                


            ViewBag.Roles = roles;



            var modules = await _context.Permissions
                .Where(p => p.IsActive)
                .Select(p => p.ModuleName)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();


            ViewBag.Modules = modules;



            if (roleId == null)
            {
                return View(new RolePermissionViewModel
                {
                    Modules = modules
                });
            }



            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == roleId);



            if (role == null)
            {
                return NotFound();
            }





            var query = _context.Permissions
                .Where(p => p.IsActive);



            if (selectedModules != null &&
                selectedModules.Any())
            {
                query = query.Where(p =>
                    selectedModules.Contains(p.ModuleName));
            }




            var permissions = await query
                .OrderBy(p => p.ModuleName)
                .ThenBy(p => p.PermissionName)
                .ToListAsync();





            var assignedPermissions =
                await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();






            var model = new RolePermissionViewModel
            {

                RoleId = role.RoleId,

                RoleName = role.RoleName,


                Modules = modules,


                SelectedModules =
                    selectedModules ?? new List<string>(),




                Permissions = permissions
                .Select(p => new PermissionItemViewModel
                {

                    PermissionId = p.PermissionId,

                    PermissionName = p.PermissionName,

                    ModuleName = p.ModuleName,


                    IsSelected =
                        assignedPermissions.Contains(p.PermissionId)

                })
                .ToList()

            };



            return View(model);

        }







        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            RolePermissionViewModel model)
        {


            var roleExists =
                await _context.Roles
                .AnyAsync(r => r.RoleId == model.RoleId);



            if (!roleExists)
            {
                return NotFound();
            }




            var oldPermissions =
                await _context.RolePermissions
                .Where(rp => rp.RoleId == model.RoleId)
                .ToListAsync();




            _context.RolePermissions
                .RemoveRange(oldPermissions);






            foreach (var permission in model.Permissions)
            {

                if (permission.IsSelected)
                {

                    var rolePermission = new RolePermission
                    {

                        RoleId = model.RoleId,

                        PermissionId =
                            permission.PermissionId,

                        IsAllowed = true

                    };


                    _context.RolePermissions
                        .Add(rolePermission);

                }

            }

            try
            {
                await _context.SaveChangesAsync();


                await ActivityLogger.LogAsync(
                    _context,
                    User.Identity?.Name,
                    $"Updated permissions for Role Id {model.RoleId}");


                TempData["Success"] =
                    "Role permissions updated successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Unable to update permissions.";
            }
            return RedirectToAction(
                nameof(Index),
                new
                {
                    roleId = model.RoleId
                });

        }

    }
}