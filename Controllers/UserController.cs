using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Admins.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Username.Contains(search));
            }

            var users = await query
                .OrderBy(x => x.Username)
                .ToListAsync();

            var vm = new UserManagementViewModel
            {
                Users = users,
                Search = search,
                TotalUsers = await _context.Admins.CountAsync(),
                ActiveUsers = await _context.Admins.CountAsync(x => x.IsActive),
                InactiveUsers = await _context.Admins.CountAsync(x => !x.IsActive)
            };

            return View(vm);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool exists = await _context.Admins
                .AnyAsync(x => x.Username == model.Username);

            if (exists)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(model);
            }

            var admin = new Admin
            {
                Username = model.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                IsActive = model.IsActive
            };

            _context.Admins.Add(admin);

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Created user '{admin.Username}' ({admin.Role})");

            TempData["Success"] = "User created successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Admins.FindAsync(id);

            if (user == null)
                return NotFound();

            var vm = new EditUserViewModel
            {
                AdminId = user.AdminId,
                Username = user.Username,
                Role = user.Role,
                IsActive = user.IsActive,
                LastLogin = user.LastLogin
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Admins.FindAsync(model.AdminId);

            if (user == null)
                return NotFound();

            // Prevent deactivating yourself
            if (user.Username == User.Identity?.Name && !model.IsActive)
            {
                ModelState.AddModelError("", "You cannot deactivate your own account.");
                return View(model);
            }

            user.Username = model.Username;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            _context.Update(user);

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Updated user '{user.Username}'");

            TempData["Success"] = "User updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Admins.FindAsync(id);

            if (user == null)
                return NotFound();

            var model = new ResetPasswordViewModel
            {
                AdminId = user.AdminId,
                Username = user.Username
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Admins.FindAsync(model.AdminId);

            if (user == null)
                return NotFound();

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            _context.Update(user);

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Reset password for '{user.Username}'");

            TempData["Success"] =
    $"Password reset successfully for '{user.Username}'.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Admins.FindAsync(id);

            if (user == null)
                return NotFound();

            return View(user);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Admins.FindAsync(id);

            if (user == null)
                return NotFound();

            // Prevent deleting yourself
            if (user.Username == User.Identity?.Name)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent deleting the last Admin
            if (user.Role == "Admin")
            {
                int adminCount = await _context.Admins
                    .CountAsync(a => a.Role == "Admin");

                if (adminCount <= 1)
                {
                    TempData["Error"] =
                        "Cannot delete the last Admin account.";

                    return RedirectToAction(nameof(Index));
                }
            }

            _context.Admins.Remove(user);

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Deleted user '{user.Username}'");

            TempData["Success"] =
                $"User '{user.Username}' deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}