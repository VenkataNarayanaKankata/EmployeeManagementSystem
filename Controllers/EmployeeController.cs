
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using EmployeeManagementSystem.Documents;
using EmployeeManagementSystem.Helpers;
using System.IO;

[Authorize]
public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    
    public EmployeeController(
     ApplicationDbContext context,
     IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: EMPLOYEES
    public async Task<IActionResult> Index(string searchString, int? departmentId, int? page)
    {
        ViewData["DepartmentId"] = new SelectList(
            _context.Departments,
            "DepartmentId",
            "DepartmentName",
            departmentId);

        ViewBag.SearchString = searchString;

        var employees = _context.Employees
     .Include(e => e.Department)
     .Where(e => !e.IsDeleted)
     .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            employees = employees.Where(e =>
                e.FirstName.Contains(searchString) ||
                e.LastName.Contains(searchString));
        }

        if (departmentId.HasValue)
        {
            employees = employees.Where(e => e.DepartmentId == departmentId);
        }

        var employeeList = await employees
    .OrderBy(e => e.EmployeeId)
    .ToListAsync();

        return View(await employees
     .OrderBy(e => e.EmployeeId)
     .ToListAsync());
    }
    // GET: EMPLOYEES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(m => m.EmployeeId == id);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }
    // GET: Employee/Import
  
    [HttpGet]
    public IActionResult Import()
    {
        return View();
    }
    // POST: Employee/Import
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select an Excel file.";
            return RedirectToAction(nameof(Import));
        }

        using var stream = new MemoryStream();

        await file.CopyToAsync(stream);

        using var workbook = new XLWorkbook(stream);

        var worksheet = workbook.Worksheet(1);

        int lastRow = worksheet.LastRowUsed().RowNumber();

        TempData["Success"] = $"Excel contains {lastRow - 1} employee(s).";

        return RedirectToAction(nameof(Import));
    }
    // GET: Employee/DownloadTemplate
    public IActionResult DownloadTemplate()
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Employees");

            // Headers
            worksheet.Cell(1, 1).Value = "FirstName";
            worksheet.Cell(1, 2).Value = "LastName";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Phone";
            worksheet.Cell(1, 5).Value = "Salary";
            worksheet.Cell(1, 6).Value = "JoiningDate";
            worksheet.Cell(1, 7).Value = "Department";

            // Sample Data
            worksheet.Cell(2, 1).Value = "Venkat";
            worksheet.Cell(2, 2).Value = "Narayana";
            worksheet.Cell(2, 3).Value = "venkat@gmail.com";
            worksheet.Cell(2, 4).Value = "9876543210";
            worksheet.Cell(2, 5).Value = 50000;
            worksheet.Cell(2, 6).Value = DateTime.Today;
            worksheet.Cell(2, 7).Value = "IT";

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "EmployeeTemplate.xlsx");
            }
        }
    }
 
    
    // GET: EMPLOYEES/Create
    // GET: EMPLOYEES/Create
    public IActionResult Create()
    {
        ViewData["DepartmentId"] = new SelectList(
            _context.Departments,
            "DepartmentId",
            "DepartmentName");

        return View();
    }

    // POST: EMPLOYEES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    [Bind("EmployeeId,FirstName,LastName,Email,Phone,Salary,JoiningDate,DepartmentId")] Employee employee,
    IFormFile? PhotoFile)
    {
        if (ModelState.IsValid)
        {
            // Check if a photo was uploaded
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                // Create a unique file name
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(PhotoFile.FileName);

                // Full path to wwwroot/uploads
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                // Create folder if it doesn't exist
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Complete file path
                string filePath = Path.Combine(uploadFolder, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await PhotoFile.CopyToAsync(stream);
                }

                // Save file name in database
                employee.PhotoPath = fileName;
            }

            _context.Add(employee);
            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Added Employee: {employee.FirstName} {employee.LastName}");

            return RedirectToAction(nameof(Index));
        }

        ViewData["DepartmentId"] = new SelectList(
            _context.Departments,
            "DepartmentId",
            "DepartmentName",
            employee.DepartmentId);

        return View(employee);
    }

    // GET: EMPLOYEES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        ViewData["DepartmentId"] = new SelectList(
    _context.Departments,
    "DepartmentId",
    "DepartmentName",
    employee.DepartmentId);

        return View(employee);
    }

    // POST: EMPLOYEES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    int id,
    [Bind("EmployeeId,FirstName,LastName,Email,Phone,Salary,JoiningDate,DepartmentId,PhotoPath")] Employee employee,
    IFormFile? PhotoFile)
    {
        if (id != employee.EmployeeId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Get existing employee from database
                var existingEmployee = await _context.Employees.FindAsync(id);

                if (existingEmployee == null)
                    return NotFound();

                // Update employee details
                existingEmployee.FirstName = employee.FirstName;
                existingEmployee.LastName = employee.LastName;
                existingEmployee.Email = employee.Email;
                existingEmployee.Phone = employee.Phone;
                existingEmployee.Salary = employee.Salary;
                existingEmployee.JoiningDate = employee.JoiningDate;
                existingEmployee.DepartmentId = employee.DepartmentId;

                // Upload new photo if selected
                if (PhotoFile != null && PhotoFile.Length > 0)
                {
                    // Delete old photo
                    if (!string.IsNullOrEmpty(existingEmployee.PhotoPath))
                    {
                        string oldFile = Path.Combine(
                            _webHostEnvironment.WebRootPath,
                            "uploads",
                            existingEmployee.PhotoPath);

                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    }

                    // Create new filename
                    string fileName = Guid.NewGuid().ToString() +
                                      Path.GetExtension(PhotoFile.FileName);

                    string uploadFolder = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "uploads");

                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await PhotoFile.CopyToAsync(stream);
                    }

                    existingEmployee.PhotoPath = fileName;
                }

                await _context.SaveChangesAsync();
                await ActivityLogger.LogAsync(
    _context,
    User.Identity?.Name,
    $"Updated Employee: {existingEmployee.FirstName} {existingEmployee.LastName}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.EmployeeId))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        ViewData["DepartmentId"] = new SelectList(
            _context.Departments,
            "DepartmentId",
            "DepartmentName",
            employee.DepartmentId);

        return View(employee);
    }

    // GET: EMPLOYEES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(m => m.EmployeeId == id);
        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }


    // POST: EMPLOYEES/Delete/5
    // POST: EMPLOYEES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee != null)
        {
            // Soft Delete
            employee.IsDeleted = true;

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Moved Employee to Recycle Bin: {employee.FirstName} {employee.LastName}");
        }

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> ExportToExcel()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .ToListAsync();

        var document = new EmployeeExcelDocument(employees);

        byte[] excel = document.GenerateExcel();
        await ActivityLogger.LogAsync(
    _context,
    User.Identity?.Name,
    "Exported Employees to Excel");

        return File(
            excel,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Employees.xlsx");
    }
    public async Task<IActionResult> ExportToPdf()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .ToListAsync();

        var document = new EmployeePdfDocument(employees);

        byte[] pdf = document.GeneratePdf();
        await ActivityLogger.LogAsync(
    _context,
    User.Identity?.Name,
    "Exported Employees to PDF");

        return File(
            pdf,
            "application/pdf",
            "EmployeeReport.pdf");
    }
    public async Task<IActionResult> RecycleBin()
    {
        var deletedEmployees = await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.IsDeleted)
            .OrderByDescending(e => e.EmployeeId)
            .ToListAsync();

        return View(deletedEmployees);
    }
    [HttpPost]
    public async Task<IActionResult> Restore(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee != null)
        {
            employee.IsDeleted = false;

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Restored Employee: {employee.FirstName} {employee.LastName}");
        }

        return RedirectToAction(nameof(RecycleBin));
    }
    [HttpPost]
    public async Task<IActionResult> PermanentDelete(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee != null)
        {
            // Delete photo
            if (!string.IsNullOrEmpty(employee.PhotoPath))
            {
                string imagePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "uploads",
                    employee.PhotoPath);

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            string employeeName = employee.FirstName + " " + employee.LastName;

            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Permanently Deleted Employee: {employeeName}");

            _context.Employees.Remove(employee);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(RecycleBin));
    }

    private bool EmployeeExists(int? id)
    {
        return _context.Employees.Any(e => e.EmployeeId == id);
    }
}
