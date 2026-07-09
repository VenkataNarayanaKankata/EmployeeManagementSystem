using ClosedXML.Excel;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    public class ImportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================
        // GET : Import Page
        // ================================
        [HttpGet]
        public IActionResult EmployeeImport()
        {
            return View();
        }

        // ================================
        // Preview Excel
        // ================================
        [HttpPost]
        public async Task<IActionResult> PreviewImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please select an Excel file."
                });
            }

            using var stream = new MemoryStream();

            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheet(1);

            int totalEmployees = worksheet.LastRowUsed().RowNumber() - 1;

            HashSet<string> existingDepartments = new();
            HashSet<string> newDepartments = new();

            for (int row = 2; row <= worksheet.LastRowUsed().RowNumber(); row++)
            {
                string departmentName = worksheet.Cell(row, 7)
                                                 .GetString()
                                                 .Trim();

                if (string.IsNullOrWhiteSpace(departmentName))
                    continue;

                bool exists = await _context.Departments
                    .AnyAsync(d => d.DepartmentName == departmentName);

                if (exists)
                    existingDepartments.Add(departmentName);
                else
                    newDepartments.Add(departmentName);
            }

            return Json(new
            {
                success = true,
                fileName = file.FileName,
                totalEmployees = totalEmployees,
                totalDepartments = existingDepartments.Count + newDepartments.Count,
                existingDepartments = existingDepartments,
                newDepartments = newDepartments
            });
        }

        // ================================
        // Upload Employees
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadEmployees(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select an Excel file.";
                return RedirectToAction(nameof(EmployeeImport));
            }

            using var stream = new MemoryStream();

            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheet(1);

            int lastRow = worksheet.LastRowUsed().RowNumber();

            int importedCount = 0;
            int skippedCount = 0;
            int newDepartmentCount = 0;

            for (int row = 2; row <= lastRow; row++)
            {
                string firstName = worksheet.Cell(row, 1).GetString().Trim();
                string lastName = worksheet.Cell(row, 2).GetString().Trim();
                string email = worksheet.Cell(row, 3).GetString().Trim();
                string phone = worksheet.Cell(row, 4).GetString().Trim();
                decimal salary = worksheet.Cell(row, 5).GetValue<decimal>();
                DateTime joiningDate = worksheet.Cell(row, 6).GetDateTime();
                string departmentName = worksheet.Cell(row, 7).GetString().Trim();

                // Skip duplicate employee
                bool employeeExists = await _context.Employees
                    .AnyAsync(e => e.Email == email);

                if (employeeExists)
                {
                    skippedCount++;
                    continue;
                }

                // Find Department
                var department = await _context.Departments
                    .FirstOrDefaultAsync(d => d.DepartmentName == departmentName);

                // Create Department if not exists
                if (department == null)
                {
                    department = new Department
                    {
                        DepartmentName = departmentName
                    };

                    _context.Departments.Add(department);
                    await _context.SaveChangesAsync();

                    newDepartmentCount++;
                }

                // Create Employee
                var employee = new Employee
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = phone,
                    Salary = salary,
                    JoiningDate = joiningDate,
                    DepartmentId = department.DepartmentId
                };

                _context.Employees.Add(employee);

                importedCount++;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Import Completed Successfully! " +
                $"Imported: {importedCount}, " +
                $"Skipped: {skippedCount}, " +
                $"New Departments: {newDepartmentCount}";

            return RedirectToAction(nameof(EmployeeImport));
        }
    }
}