using BCrypt.Net;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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

            // Redirect after successful login
            return RedirectToAction("Index", "Dashboard");
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}