
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
[Authorize(Roles = "Admin,HR,Manager")]
public class DepartmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public DepartmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: DEPARTMENTS
    public async Task<IActionResult> Index(string? search, bool? status)
    {
        var query = _context.Departments
            .Include(d => d.Employees)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d =>
                d.DepartmentCode.Contains(search) ||
                d.DepartmentName.Contains(search) ||
                (d.Description ?? "").Contains(search));
        }

        if (status.HasValue)
        {
            query = query.Where(d => d.IsActive == status.Value);
        }

        var departments = await query
            .OrderBy(d => d.DepartmentName)
            .Select(d => new DepartmentViewModel
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                IsActive = d.IsActive,
                EmployeeCount = d.Employees.Count(e => !e.IsDeleted),
                ActiveEmployeeCount = d.Employees.Count(e => !e.IsDeleted && e.IsActive),
                InactiveEmployeeCount = d.Employees.Count(e => !e.IsDeleted && !e.IsActive)
            })
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.TotalDepartments = departments.Count;
        ViewBag.ActiveDepartments = departments.Count(x => x.IsActive);
        ViewBag.InactiveDepartments = departments.Count(x => !x.IsActive);
        ViewBag.TotalEmployees = departments.Sum(x => x.EmployeeCount);

        return View(departments);
    }

    // GET: DEPARTMENTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await _context.Departments
            .FirstOrDefaultAsync(m => m.DepartmentId == id);

        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    // GET: DEPARTMENTS/Create
    [Authorize(Roles = "Admin,HR")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: DEPARTMENTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Department department)
    {
        if (!ModelState.IsValid)
            return View(department);

        bool codeExists = await _context.Departments
            .AnyAsync(d => d.DepartmentCode == department.DepartmentCode);

        if (codeExists)
        {
            ModelState.AddModelError(nameof(department.DepartmentCode),
                "Department Code already exists.");

            return View(department);
        }

        bool nameExists = await _context.Departments
            .AnyAsync(d => d.DepartmentName == department.DepartmentName);

        if (nameExists)
        {
            ModelState.AddModelError(nameof(department.DepartmentName),
                "Department Name already exists.");

            return View(department);
        }

        _context.Departments.Add(department);

        await _context.SaveChangesAsync();

        await ActivityLogger.LogAsync(
            _context,
            User.Identity?.Name,
            $"Created Department '{department.DepartmentName}'");

        TempData["Success"] = "Department created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: DEPARTMENTS/Edit/5
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await _context.Departments.FindAsync(id);

        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    // POST: DEPARTMENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Department department)
    {
        if (id != department.DepartmentId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
            return View(department);

        bool codeExists = await _context.Departments
            .AnyAsync(d =>
                d.DepartmentCode == department.DepartmentCode &&
                d.DepartmentId != department.DepartmentId);

        if (codeExists)
        {
            ModelState.AddModelError(nameof(department.DepartmentCode),
                "Department Code already exists.");

            return View(department);
        }

        bool nameExists = await _context.Departments
            .AnyAsync(d =>
                d.DepartmentName == department.DepartmentName &&
                d.DepartmentId != department.DepartmentId);

        if (nameExists)
        {
            ModelState.AddModelError(nameof(department.DepartmentName),
                "Department Name already exists.");

            return View(department);
        }

        try
        {
            _context.Update(department);

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Updated Department '{department.DepartmentName}'");

            TempData["Success"] = "Department updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DepartmentExists(department.DepartmentId))
            {
                return NotFound();
            }

            throw;
        }
    }

    // GET: DEPARTMENTS/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await _context.Departments
            .FirstOrDefaultAsync(m => m.DepartmentId == id);

        if (department == null)
        {
            return NotFound();
        }
        

        return View(department);
    }

    // POST: DEPARTMENTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department == null)
        {
            return NotFound();
        }

        // Check whether employees are assigned to this department
        bool hasEmployees = await _context.Employees
            .AnyAsync(e => e.DepartmentId == id && !e.IsDeleted);

        if (hasEmployees)
        {
            TempData["Error"] =
                "Cannot delete this department because employees are assigned to it.";

            return RedirectToAction(nameof(Index));
        }

        // No employees → delete department
        _context.Departments.Remove(department);

        await _context.SaveChangesAsync();

        await ActivityLogger.LogAsync(
            _context,
            User.Identity?.Name,
            $"Deleted Department: {department.DepartmentName}");
        TempData["Success"] =
    $"Department '{department.DepartmentName}' deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private bool DepartmentExists(int id)
    {
        return _context.Departments.Any(e => e.DepartmentId == id);
    }
}
