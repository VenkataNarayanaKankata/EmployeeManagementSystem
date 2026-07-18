using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PermissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PermissionController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(
            string? search,
            string? module)
        {

            var permissions = _context.Permissions
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                permissions = permissions.Where(p =>
                    p.PermissionName.Contains(search));
            }


            if (!string.IsNullOrWhiteSpace(module))
            {
                permissions = permissions.Where(p =>
                    p.ModuleName == module);
            }


            var model = new PermissionViewModel
            {
                Search = search,
                Module = module,

                Permissions = await permissions
                    .OrderBy(p => p.ModuleName)
                    .ThenBy(p => p.PermissionName)
                    .ToListAsync(),

                TotalPermissions =
                    await _context.Permissions.CountAsync(),

                ActivePermissions =
                    await _context.Permissions
                    .CountAsync(p => p.IsActive),

                InactivePermissions =
                    await _context.Permissions
                    .CountAsync(p => !p.IsActive),

                Modules =
                    await _context.Permissions
                    .Select(p => p.ModuleName)
                    .Distinct()
                    .ToListAsync()
            };


            return View(model);
        }



        public IActionResult Create()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Permission permission)
        {

            if (ModelState.IsValid)
            {

                bool exists =
                    await _context.Permissions
                    .AnyAsync(p =>
                    p.PermissionName ==
                    permission.PermissionName);


                if (exists)
                {
                    ModelState.AddModelError(
                        "",
                        "Permission already exists.");

                    return View(permission);
                }


                _context.Permissions.Add(permission);

                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Permission created successfully.";


                return RedirectToAction(nameof(Index));
            }


            return View(permission);
        }



        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
                return NotFound();


            var permission =
                await _context.Permissions
                .FirstOrDefaultAsync(
                    p => p.PermissionId == id);


            if (permission == null)
                return NotFound();


            return View(permission);
        }



        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
                return NotFound();


            var permission =
                await _context.Permissions
                .FindAsync(id);


            if (permission == null)
                return NotFound();


            return View(permission);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Permission permission)
        {

            if (id != permission.PermissionId)
                return NotFound();


            if (ModelState.IsValid)
            {

                _context.Update(permission);

                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Permission updated successfully.";


                return RedirectToAction(nameof(Index));
            }


            return View(permission);
        }



        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
                return NotFound();


            var permission =
                await _context.Permissions
                .FirstOrDefaultAsync(
                    p => p.PermissionId == id);


            if (permission == null)
                return NotFound();


            return View(permission);
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {

            var permission =
                await _context.Permissions
                .FindAsync(id);


            if (permission != null)
            {

                _context.Permissions.Remove(permission);

                await _context.SaveChangesAsync();

            }


            TempData["Success"] =
                "Permission deleted successfully.";


            return RedirectToAction(nameof(Index));
        }

    }
}