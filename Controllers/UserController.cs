using DocumentFormat.OpenXml.Spreadsheet;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public UserController(
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Admins
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Department)
                .AsQueryable();

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
        public async Task<IActionResult> Create()
        {
            var model = new CreateUserViewModel();

            model.Roles = await _context.Roles
                .Where(r => r.IsActive)
                .Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName
                })
                .ToListAsync();

            model.Employees = await _context.Employees
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeId.ToString(),
                    Text = e.FirstName + " " + e.LastName
                })
                .ToListAsync();

            return View(model);
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

            // Generate temporary password
            string temporaryPassword = PasswordGenerator.Generate();

            var admin = new Admin
            {
                Username = model.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
                RoleId = model.RoleId,
                IsActive = model.IsActive,
                EmployeeId = model.EmployeeId,
                MustChangePassword = true
            };

            _context.Admins.Add(admin);

            var employee = await _context.Employees
    .FirstOrDefaultAsync(e => e.EmployeeId == model.EmployeeId);
            if (employee != null)
            {
                string subject = "Welcome to Employee Management System";
                string body = $@"
<h2>Welcome {employee.FirstName} {employee.LastName}</h2>

<p>Your login account has been created successfully.</p>

<hr>

<p><b>Employee Code:</b> {employee.EmployeeCode}</p>

<p><b>Username:</b> {admin.Username}</p>

<p><b>Temporary Password:</b> {temporaryPassword}</p>

<hr>

<p>
Please login and change your password after your first login.
</p>

<p>
Thank you,<br/>
Employee Management System
</p>";
                await _emailService.SendEmailAsync(
    employee.Email,
    subject,
    body);
            }

            var roleName = await _context.Roles
    .Where(r => r.RoleId == model.RoleId)
    .Select(r => r.RoleName)
    .FirstOrDefaultAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Created user '{admin.Username}' ({roleName})"
            );

            TempData["Success"] = "User created successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Admins
    .Include(a => a.Role)
    .FirstOrDefaultAsync(a => a.AdminId == id);

            if (user == null)
                return NotFound();

            var vm = new EditUserViewModel
            {
                AdminId = user.AdminId,
                Username = user.Username,
                RoleId = user.RoleId,
                IsActive = user.IsActive,
                LastLogin = user.LastLogin,
                EmployeeId = user.EmployeeId,

                Employees = await _context.Employees
                    .Where(e => !e.IsDeleted)
                    .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = e.EmployeeId.ToString(),
                        Text = e.EmployeeCode + " - " +
       e.FirstName + " " +
       e.LastName
                    })
                    .ToListAsync()
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Employees = await _context.Employees
                    .Where(e => !e.IsDeleted)
                    .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = e.EmployeeId.ToString(),
                        Text = e.EmployeeCode + " - " + e.FirstName + " " + e.LastName
                    })
                    .ToListAsync();

                return View(model);
            }

            var user = await _context.Admins.FindAsync(model.AdminId);

            if (user == null)
                return NotFound();

            // Prevent deactivating your own account
            if (user.Username == User.Identity?.Name && !model.IsActive)
            {
                ModelState.AddModelError("", "You cannot deactivate your own account.");

                model.Employees = await _context.Employees
                    .Where(e => !e.IsDeleted)
                    .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = e.EmployeeId.ToString(),
                        Text = e.EmployeeCode + " - " + e.FirstName + " " + e.LastName
                    })
                    .ToListAsync();

                return View(model);
            }

            user.Username = model.Username;
            user.RoleId = model.RoleId;
            user.IsActive = model.IsActive;
            user.EmployeeId = model.EmployeeId;

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
            if (user.Role.RoleName == "Admin")
            {
                int adminCount = await _context.Admins
                   .CountAsync(a => a.Role.RoleName == "Admin");

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
        [HttpGet]
        public async Task<IActionResult> GenerateUsers()
        {
            // Employees without login accounts
            var employees = await _context.Employees
                .Where(e => !e.IsDeleted &&
                            !_context.Admins.Any(a => a.EmployeeId == e.EmployeeId))
                .ToListAsync();

            int createdUsers = 0;

            foreach (var employee in employees)
            {
                // Generate username
                string username = employee.FirstName.ToLower();

                // If username already exists, append Employee Code
                if (await _context.Admins.AnyAsync(a => a.Username == username))
                {
                    username += employee.EmployeeCode.ToLower();
                }

                // Generate temporary password
                string temporaryPassword = PasswordGenerator.Generate();

                var user = new Admin
                {
                    Username = username,
                    Password = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
                    RoleId = employee.RoleId,
                    EmployeeId = employee.EmployeeId,
                    IsActive = true,
                    MustChangePassword = true
                };

                _context.Admins.Add(user);

                createdUsers++;

                // Send Welcome Email
                string subject = "Welcome to Employee Management System";

                string body = $@"
<!DOCTYPE html>
<html>

<head>
    <meta charset='UTF-8'>
</head>

<body style='font-family:Arial,Helvetica,sans-serif;background:#f4f6f9;padding:30px;'>

    <table width='650'
           align='center'
           cellpadding='0'
           cellspacing='0'
           style='background:#ffffff;
                  border-radius:10px;
                  box-shadow:0 3px 12px rgba(0,0,0,.15);
                  overflow:hidden;'>

        <tr style='background:#0d6efd;color:white;'>

            <td style='padding:25px;text-align:center;'>

                <h1 style='margin:0;'>
                    Employee Management System
                </h1>

                <p style='margin-top:8px;'>
                    Welcome to the Organization
                </p>

            </td>

        </tr>

        <tr>

            <td style='padding:30px;'>

                <h2 style='color:#0d6efd;'>

                    Hello {employee.FirstName} {employee.LastName},

                </h2>

                <p>

                    Your employee login account has been created successfully.

                    Below are your account details.

                </p>

                <table width='100%'
                       cellpadding='10'
                       style='border-collapse:collapse;
                              margin-top:20px;'>

                    <tr style='background:#f8f9fa;'>

                        <td><b>Employee Code</b></td>

                        <td>{employee.EmployeeCode}</td>

                    </tr>

                    <tr>

                        <td><b>Employee Name</b></td>

                        <td>{employee.FirstName} {employee.LastName}</td>

                    </tr>

                    <tr style='background:#f8f9fa;'>

                        <td><b>Email</b></td>

                        <td>{employee.Email}</td>

                    </tr>

                    <tr>

                        <td><b>Department</b></td>

                        <td>{employee.Department?.DepartmentName}</td>

                    </tr>

                    <tr style='background:#f8f9fa;'>

                        <td><b>Role</b></td>

                        <td>{employee.Role?.RoleName}</td>

                    </tr>

                    <tr>

                        <td><b>Username</b></td>

                        <td>{username}</td>

                    </tr>

                    <tr style='background:#fff3cd;'>

                        <td><b>Temporary Password</b></td>

                        <td style='font-size:18px;
                                   color:#dc3545;
                                   font-weight:bold;'>

                            {temporaryPassword}

                        </td>

                    </tr>

                </table>

                <div style='text-align:center;margin-top:35px;'>

                    <a href='https://localhost:5001/Account/Login'
                       style='background:#0d6efd;
                              color:white;
                              text-decoration:none;
                              padding:14px 35px;
                              border-radius:6px;
                              font-size:16px;
                              font-weight:bold;'>

                        Login to Employee Portal

                    </a>

                </div>

                <div style='margin-top:35px;
                            padding:18px;
                            background:#fff3cd;
                            border-left:5px solid #ffc107;'>

                    <h4 style='margin-top:0;color:#856404;'>

                        Security Notice

                    </h4>

                    <p style='margin-bottom:0;'>

                        This password is temporary.

                        For security reasons, you must log in using this temporary password and immediately change it.

                        You will not be allowed to access the system until your password has been changed.

                    </p>

                </div>

                <hr style='margin:35px 0;'>

                <p>

                    If you have any questions, please contact your HR or System Administrator.

                </p>

                <p>

                    Regards,

                    <br>

                    <strong>Employee Management System</strong>

                </p>

            </td>

        </tr>

        <tr style='background:#f8f9fa;'>

            <td style='padding:20px;
                       text-align:center;
                       color:#777;'>

                © 2026 Employee Management System

                <br>

                This is an automated email. Please do not reply.

            </td>

        </tr>

    </table>

</body>

</html>";

                await _emailService.SendEmailAsync(
                    employee.Email,
                    subject,
                    body);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"{createdUsers} user account(s) generated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}