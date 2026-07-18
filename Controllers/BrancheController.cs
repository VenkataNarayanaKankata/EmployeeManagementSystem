using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class BranchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BranchController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? search, bool? status)
        {
            var query = _context.Branches
                .Include(b => b.Employees)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.BranchCode.Contains(search) ||
                    b.BranchName.Contains(search) ||
                   (b.City ?? "").Contains(search));
            }


            if (status.HasValue)
            {
                query = query.Where(b => b.IsActive == status.Value);
            }


            var branches = await query
                .OrderBy(b => b.BranchName)
                .Select(b => new BranchViewModel
                {
                    BranchId = b.BranchId,
                    BranchCode = b.BranchCode,
                    BranchName = b.BranchName,
                    Address = b.Address,
                    City = b.City,
                    State = b.State,
                    Country = b.Country,
                    Pincode = b.Pincode,
                    Phone = b.Phone,
                    Email = b.Email,
                    IsHeadOffice = b.IsHeadOffice,
                    IsActive = b.IsActive,

                    EmployeeCount =
                        b.Employees.Count(e => !e.IsDeleted),

                    ActiveEmployeeCount =
                        b.Employees.Count(e =>
                            !e.IsDeleted &&
                            e.IsActive),

                    InactiveEmployeeCount =
                        b.Employees.Count(e =>
                            !e.IsDeleted &&
                            !e.IsActive)
                })
                .ToListAsync();


            ViewBag.Search = search;

            ViewBag.Status = status;


            ViewBag.TotalBranches = branches.Count;

            ViewBag.ActiveBranches =
                branches.Count(x => x.IsActive);

            ViewBag.InactiveBranches =
                branches.Count(x => !x.IsActive);

            ViewBag.HeadOfficeCount =
                branches.Count(x => x.IsHeadOffice);

            return View(branches);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new BranchViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Duplicate Branch Code
            bool codeExists = await _context.Branches
                .AnyAsync(b => b.BranchCode == model.BranchCode);

            if (codeExists)
            {
                ModelState.AddModelError(nameof(model.BranchCode),
                    "Branch Code already exists.");

                return View(model);
            }

            // Duplicate Branch Name
            bool nameExists = await _context.Branches
                .AnyAsync(b => b.BranchName == model.BranchName);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(model.BranchName),
                    "Branch Name already exists.");

                return View(model);
            }

            var branch = new Branch
            {
                BranchCode = model.BranchCode.Trim(),
                BranchName = model.BranchName.Trim(),
                Address = model.Address,
                City = model.City,
                State = model.State,
                Country = model.Country,
                Pincode = model.Pincode,
                Phone = model.Phone,
                Email = model.Email,
                IsHeadOffice = model.IsHeadOffice,
                IsActive = model.IsActive
            };

            _context.Branches.Add(branch);

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Created Branch '{branch.BranchName}'");

            TempData["Success"] =
                "Branch created successfully.";

            return RedirectToAction(nameof(Index));
        }
        //====================================================
        // EDIT
        //====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var branch = await _context.Branches.FindAsync(id);

            if (branch == null)
                return NotFound();

            var model = new BranchViewModel
            {
                BranchId = branch.BranchId,
                BranchCode = branch.BranchCode,
                BranchName = branch.BranchName,
                Address = branch.Address,
                City = branch.City,
                State = branch.State,
                Country = branch.Country,
                Pincode = branch.Pincode,
                Phone = branch.Phone,
                Email = branch.Email,
                IsHeadOffice = branch.IsHeadOffice,
                IsActive = branch.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BranchViewModel model)
        {
            if (id != model.BranchId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            bool codeExists = await _context.Branches
                .AnyAsync(b =>
                    b.BranchCode == model.BranchCode &&
                    b.BranchId != model.BranchId);

            if (codeExists)
            {
                ModelState.AddModelError(nameof(model.BranchCode),
                    "Branch Code already exists.");

                return View(model);
            }

            bool nameExists = await _context.Branches
                .AnyAsync(b =>
                    b.BranchName == model.BranchName &&
                    b.BranchId != model.BranchId);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(model.BranchName),
                    "Branch Name already exists.");

                return View(model);
            }

            var branch = await _context.Branches.FindAsync(id);

            if (branch == null)
                return NotFound();

            branch.BranchCode = model.BranchCode.Trim();
            branch.BranchName = model.BranchName.Trim();
            branch.Address = model.Address;
            branch.City = model.City;
            branch.State = model.State;
            branch.Country = model.Country;
            branch.Pincode = model.Pincode;
            branch.Phone = model.Phone;
            branch.Email = model.Email;
            branch.IsHeadOffice = model.IsHeadOffice;
            branch.IsActive = model.IsActive;

            _context.Update(branch);

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Updated Branch '{branch.BranchName}'");

            TempData["Success"] = "Branch updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        //====================================================
        // DETAILS
        //====================================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var branch = await _context.Branches
                .Include(b => b.Employees)
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (branch == null)
                return NotFound();

            var model = new BranchViewModel
            {
                BranchId = branch.BranchId,
                BranchCode = branch.BranchCode,
                BranchName = branch.BranchName,
                Address = branch.Address,
                City = branch.City,
                State = branch.State,
                Country = branch.Country,
                Pincode = branch.Pincode,
                Phone = branch.Phone,
                Email = branch.Email,
                IsHeadOffice = branch.IsHeadOffice,
                IsActive = branch.IsActive,

                EmployeeCount = branch.Employees.Count(e => !e.IsDeleted),

                ActiveEmployeeCount = branch.Employees.Count(e =>
                    !e.IsDeleted &&
                    e.IsActive),

                InactiveEmployeeCount = branch.Employees.Count(e =>
                    !e.IsDeleted &&
                    !e.IsActive)
            };

            return View(model);
        }
        //====================================================
        // DELETE
        //====================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var branch = await _context.Branches
                .Include(b => b.Employees)
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (branch == null)
                return NotFound();

            var model = new BranchViewModel
            {
                BranchId = branch.BranchId,
                BranchCode = branch.BranchCode,
                BranchName = branch.BranchName,
                Address = branch.Address,
                City = branch.City,
                State = branch.State,
                Country = branch.Country,
                Pincode = branch.Pincode,
                Phone = branch.Phone,
                Email = branch.Email,
                IsHeadOffice = branch.IsHeadOffice,
                IsActive = branch.IsActive,
                EmployeeCount = branch.Employees.Count(e => !e.IsDeleted)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var branch = await _context.Branches
                .Include(b => b.Employees)
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (branch == null)
                return NotFound();

            if (branch.Employees.Any(e => !e.IsDeleted))
            {
                TempData["Error"] =
                    "This branch contains employees. Transfer them before deleting the branch.";

                return RedirectToAction(nameof(Index));
            }

            _context.Branches.Remove(branch);

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Deleted Branch '{branch.BranchName}'");

            TempData["Success"] =
                "Branch deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}