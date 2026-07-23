using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPermissionService _permissionService;


        public ProfileController(
            ApplicationDbContext context,
            IPermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }



        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.HasPermissionAsync("Profile.View"))
            {
                return Forbid();
            }


            var username = User.Identity?.Name;


            var admin = await _context.Admins
                .Include(a => a.Role)
                .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
                .Include(a => a.Employee)
                .ThenInclude(e => e.Designation)
                .FirstOrDefaultAsync(a => a.Username == username);



            if (admin == null)
            {
                return NotFound();
            }



            var model = new ProfileViewModel
            {
                Username = admin.Username,

                Role = admin.Role?.RoleName ?? "N/A",

                EmployeeName = admin.Employee?.FullName ?? "Not Assigned",

                EmployeeCode = admin.Employee?.EmployeeCode ?? "N/A",

                Department = admin.Employee?.Department?.DepartmentName ?? "N/A",

                Designation = admin.Employee?.Designation?.DesignationName ?? "N/A",

                LastLogin = admin.LastLogin,

                IsActive = admin.IsActive
            };


            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (!await _permissionService
                .HasPermissionAsync("Profile.Edit"))
            {
                return Forbid();
            }

            var username = User.Identity?.Name;

            var admin = await _context.Admins
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Username == username);


            if (admin == null)
            {
                return NotFound();
            }


            if (admin.Employee == null)
            {
                TempData["Error"] = "Employee profile is not linked.";
                return RedirectToAction(nameof(Index));
            }


            var model = new EditProfileViewModel
            {
                AdminId = admin.AdminId,

                EmployeeId = admin.Employee.EmployeeId,

                FirstName = admin.Employee.FirstName,

                LastName = admin.Employee.LastName,

                Email = admin.Employee.Email,

                Phone = admin.Employee.Phone,

                Gender = admin.Employee.Gender,

                ExistingPhoto = admin.Employee.PhotoPath
            };


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!await _permissionService.HasPermissionAsync("Profile.Edit"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == model.EmployeeId);

            if (employee == null)
            {
                return NotFound();
            }

            employee.FirstName = model.FirstName;

            employee.LastName = model.LastName;

            employee.Email = model.Email;

            employee.Phone = model.Phone;

            employee.Gender = model.Gender;

            if (model.Photo != null)
            {

                var uploadFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads");


                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(model.Photo.FileName);

                var filePath =
                    Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(stream);
                }
                employee.PhotoPath =
                    "/uploads/" + fileName;

            }
            await _context.SaveChangesAsync();
            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                "Updated profile information");
            TempData["Success"] =
                "Profile updated successfully.";
            return RedirectToAction(nameof(Index));

        }
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            if (!await _permissionService.HasPermissionAsync("Profile.ChangePassword"))
            {
                return Forbid();
            }


            return View();
        }

    }
}