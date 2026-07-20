using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoleController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(string? search)
        {
            var roles = _context.Roles
                .Include(r => r.Employees)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                roles = roles.Where(r =>
                    r.RoleName.Contains(search));
            }


            var model = new RoleViewModel
            {
                Search = search,

                Roles = await roles
                    .OrderBy(r => r.RoleName)
                    .ToListAsync(),

                TotalRoles = await _context.Roles.CountAsync(),

                ActiveRoles = await _context.Roles
                    .CountAsync(r => r.IsActive),

                InactiveRoles = await _context.Roles
                    .CountAsync(r => !r.IsActive)
            };


            return View(model);
        }



        public IActionResult Create()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {

                bool exists = await _context.Roles
                    .AnyAsync(r => r.RoleName == role.RoleName);


                if (exists)
                {
                    ModelState.AddModelError(
                        "",
                        "Role already exists.");

                    return View(role);
                }


                _context.Roles.Add(role);

                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Role created successfully.";


                return RedirectToAction(nameof(Index));
            }


            return View(role);
        }



        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();


            var role = await _context.Roles
                .Include(r => r.Employees)
                .FirstOrDefaultAsync(r => r.RoleId == id);


            if (role == null)
                return NotFound();


            return View(role);
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();


            var role = await _context.Roles
                .FindAsync(id);


            if (role == null)
                return NotFound();


            return View(role);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Role role)
        {

            if (id != role.RoleId)
                return NotFound();


            if (ModelState.IsValid)
            {

                _context.Update(role);

                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Role updated successfully.";


                return RedirectToAction(nameof(Index));
            }


            return View(role);
        }




        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
                return NotFound();


            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == id);


            if (role == null)
                return NotFound();


            return View(role);
        }





        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var role = await _context.Roles
                .Include(r => r.Employees)
                .FirstOrDefaultAsync(r => r.RoleId == id);


            if (role == null)
                return NotFound();



            if (role.Employees.Any())
            {
                TempData["Error"] =
                    "Cannot delete role because employees are assigned.";

                return RedirectToAction(nameof(Index));
            }



            _context.Roles.Remove(role);


            await _context.SaveChangesAsync();



            TempData["Success"] =
                "Role deleted successfully.";


            return RedirectToAction(nameof(Index));
        }

    }
}