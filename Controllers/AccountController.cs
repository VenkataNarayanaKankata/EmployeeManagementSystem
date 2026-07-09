using BCrypt.Net;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using EmployeeManagementSystem.Helpers;

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
            return View();
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var admin = await _context.Admins
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

            // Create claims
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, admin.Username),
    new Claim(ClaimTypes.Role, "Admin")
};

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

            return RedirectToAction("Index", "Dashboard");
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var username = User.Identity?.Name;

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == username);

            if (admin == null)
            {
                return RedirectToAction("Login");
            }

            bool isCurrentPasswordCorrect =
                BCrypt.Net.BCrypt.Verify(model.CurrentPassword, admin.Password);

            if (!isCurrentPasswordCorrect)
            {
                ModelState.AddModelError("", "Current password is incorrect.");
                return View(model);
            }

            admin.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            _context.Update(admin);
            await ActivityLogger.LogAsync(
    _context,
    User.Identity?.Name,
    "Changed Password");
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Success"] = "Password changed successfully. Please login again.";

            return RedirectToAction("Login");
        }
        [HttpGet]
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
    }
}