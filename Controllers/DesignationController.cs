using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers;

[Authorize(Roles = "Admin,HR,Manager")]
public class DesignationController : Controller
{
    private readonly ApplicationDbContext _context;

    public DesignationController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, int? departmentId, bool? status)
    {
        var query = _context.Designations
            .Include(d => d.Department)
            .Include(d => d.Employees)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d =>
                d.DesignationCode.Contains(search) ||
                d.DesignationName.Contains(search) ||
                (d.Description ?? "").Contains(search));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId == departmentId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(d => d.IsActive == status.Value);
        }

        var designations = await query
            .OrderBy(d => d.DesignationName)
            .Select(d => new DesignationViewModel
            {
                DesignationId = d.DesignationId,
                DesignationCode = d.DesignationCode,
                DesignationName = d.DesignationName,
                Description = d.Description,
                DepartmentId = d.DepartmentId,
                DepartmentName = d.Department!.DepartmentName,
                IsActive = d.IsActive,
                EmployeeCount = d.Employees.Count(e => !e.IsDeleted),
                ActiveEmployeeCount = d.Employees.Count(e => !e.IsDeleted && e.IsActive),
                InactiveEmployeeCount = d.Employees.Count(e => !e.IsDeleted && !e.IsActive)
            })
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.DepartmentId = departmentId;

        ViewBag.Departments = new SelectList(
            await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync(),
            "DepartmentId",
            "DepartmentName",
            departmentId);

        ViewBag.TotalDesignations = designations.Count;
        ViewBag.ActiveDesignations = designations.Count(x => x.IsActive);
        ViewBag.InactiveDesignations = designations.Count(x => !x.IsActive);
        ViewBag.TotalEmployees = designations.Sum(x => x.EmployeeCount);

        return View(designations);
    }
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = new SelectList(
            await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync(),
            "DepartmentId",
            "DepartmentName");

        return View();
    }
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Designation designation)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = new SelectList(
                await _context.Departments
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync(),
                "DepartmentId",
                "DepartmentName",
                designation.DepartmentId);

            return View(designation);
        }

        bool exists = await _context.Designations
            .AnyAsync(d =>
                d.DesignationCode == designation.DesignationCode);

        if (exists)
        {
            ModelState.AddModelError(
                nameof(designation.DesignationCode),
                "Designation Code already exists.");

            ViewBag.Departments = new SelectList(
                await _context.Departments
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync(),
                "DepartmentId",
                "DepartmentName",
                designation.DepartmentId);

            return View(designation);
        }

        _context.Designations.Add(designation);

        await _context.SaveChangesAsync();

        await ActivityLogger.LogAsync(
            _context,
            User.Identity?.Name,
            $"Created Designation '{designation.DesignationName}'");

        TempData["Success"] = "Designation created successfully.";

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var designation = await _context.Designations
            .Include(d => d.Department)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.DesignationId == id);

        if (designation == null)
        {
            return NotFound();
        }

        return View(designation);
    }
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var designation = await _context.Designations
            .FindAsync(id);

        if (designation == null)
        {
            return NotFound();
        }

        ViewBag.Departments = new SelectList(
            await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync(),
            "DepartmentId",
            "DepartmentName",
            designation.DepartmentId);

        return View(designation);
    }
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Designation designation)
    {
        if (id != designation.DesignationId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Departments = new SelectList(
                await _context.Departments
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync(),
                "DepartmentId",
                "DepartmentName",
                designation.DepartmentId);

            return View(designation);
        }

        try
        {
            _context.Update(designation);

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Updated Designation: {designation.DesignationName}");

            TempData["Success"] =
                "Designation updated successfully.";

        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DesignationExists(designation.DesignationId))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }
    private bool DesignationExists(int id)
    {
        return _context.Designations
            .Any(e => e.DesignationId == id);
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var designation = await _context.Designations
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.DesignationId == id);

        if (designation == null)
        {
            return NotFound();
        }

        return View(designation);
    }
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var designation = await _context.Designations
            .FirstOrDefaultAsync(d => d.DesignationId == id);

        if (designation == null)
        {
            return NotFound();
        }


        bool hasEmployees = await _context.Employees
            .AnyAsync(e =>
                e.DesignationId == id &&
                !e.IsDeleted);


        if (hasEmployees)
        {
            TempData["Error"] =
                "Cannot delete this designation because employees are assigned to it.";

            return RedirectToAction(nameof(Index));
        }


        _context.Designations.Remove(designation);

        await _context.SaveChangesAsync();


        await ActivityLogger.LogAsync(
            _context,
            User.Identity?.Name,
            $"Deleted Designation: {designation.DesignationName}");


        TempData["Success"] =
            $"Designation '{designation.DesignationName}' deleted successfully.";


        return RedirectToAction(nameof(Index));
    }
}