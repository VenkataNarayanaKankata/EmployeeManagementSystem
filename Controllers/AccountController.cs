using BCrypt.Net;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmployeeManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.ShowRegister = !_context.Admins.Any();

            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        // POST
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var admin = await _context.Admins
    .Include(a => a.Employee)
    .Include(a => a.Role)
    .FirstOrDefaultAsync(a => a.Username == model.Username);

            if (admin == null)
            {
                ModelState.AddModelError("", "Invalid Username or Password");
                return View(model);
            }

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(model.Password, admin.Password);

            if (!isPasswordCorrect)
            {
                ModelState.AddModelError("", "Invalid Username or Password");
                return View(model);
            }
            if (!admin.IsActive)
            {
                ModelState.AddModelError("", "Your account has been deactivated.");
                return View(model);
            }

            admin.LastLogin = DateTime.Now;

            _context.Update(admin);

            await _context.SaveChangesAsync();

            // Create claims
            var claims = new List<Claim>
{
    new Claim(
        ClaimTypes.Name,
        admin.Username),

    new Claim(
        ClaimTypes.Role,
        admin.Role?.RoleName ?? "Employee"),

    new Claim(
        "RoleId",
        admin.RoleId.ToString())
};

            // Store EmployeeId for Employee users
            if (admin.EmployeeId.HasValue)
            {
                claims.Add(new Claim(
                    "EmployeeId",
                    admin.EmployeeId.Value.ToString()));
            }

            // Create identity
            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            // Create principal
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign in
            await HttpContext.SignInAsync(
     CookieAuthenticationDefaults.AuthenticationScheme,
     claimsPrincipal);

            await ActivityLogger.LogAsync(
                _context,
                admin.Username,
                "Logged In");

            if (admin.MustChangePassword)
            {
                return RedirectToAction("ChangePassword");
            }


            // Role based redirect
            if (admin.Role != null)
            {
                switch (admin.Role.RoleName)
                {
                    case "Super Admin":
                        return RedirectToAction(
                            "Index",
                            "Dashboard");


                    case "HR":
                        return RedirectToAction(
                            "Index",
                            "HRDashboard");


                    case "Manager":
                        return RedirectToAction(
                            "Index",
                            "ManagerDashboard");


                    case "Employee":
                        return RedirectToAction(
                            "Index",
                            "EmployeeDashboard");
                }
            }


            // Default
            return RedirectToAction(
                "Index",
                "Home");
        }
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var username = User.Identity?.Name;

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == username);

            if (admin == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ChangePasswordViewModel
            {
                AdminId = admin.AdminId,
                Username = admin.Username
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.AdminId == model.AdminId);

            if (admin == null)
            {
                return RedirectToAction("Login");
            }

            // Verify old password
            bool isOldPasswordCorrect =
                BCrypt.Net.BCrypt.Verify(model.OldPassword, admin.Password);

            if (!isOldPasswordCorrect)
            {
                ModelState.AddModelError("", "Old password is incorrect.");
                return View(model);
            }

            // Hash new password
            admin.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            admin.MustChangePassword = false;

            admin.PasswordChangedOn = DateTime.Now;

            _context.Update(admin);

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                admin.Username,
                "Changed Password");

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Success"] =
                "Password changed successfully. Please login with your new password.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                "Logged Out");

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Register()
        {
            // If at least one user already exists,
            // disable public registration.
            if (_context.Admins.Any())
            {
                TempData["Error"] = "Registration is disabled. Please contact the Administrator.";

                return RedirectToAction("Login");
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Allow registration only when there are no users
            if (_context.Admins.Any())
            {
                TempData["Error"] = "Registration is disabled.";

                return RedirectToAction("Login");
            }

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
                RoleId = 1,
                IsActive = true
            };

            _context.Admins.Add(admin);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Administrator account created successfully. Please login.";

            return RedirectToAction("Login");
        }
    }
}