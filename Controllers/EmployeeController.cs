using ClosedXML.Excel;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Documents;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
    [Permission("Employee.View")]
    public async Task<IActionResult> Index(
        string searchString,
        int? branchId,
        int? departmentId,
        int? designationId,
        int? roleId)
    {
        ViewData["BranchId"] = new SelectList(
            _context.Branches
                .Where(b => b.IsActive),
            "BranchId",
            "BranchName",
            branchId);


        ViewData["DepartmentId"] = new SelectList(
            _context.Departments
                .Where(d => d.IsActive),
            "DepartmentId",
            "DepartmentName",
            departmentId);


        ViewData["DesignationId"] = new SelectList(
            _context.Designations
                .Where(d => d.IsActive),
            "DesignationId",
            "DesignationName",
            designationId);


        ViewData["RoleId"] = new SelectList(
            _context.Roles
                .Where(r => r.IsActive),
            "RoleId",
            "RoleName",
            roleId);


        ViewBag.SearchString = searchString;

        var employees = _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Role)
            .Where(e => !e.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            employees = employees.Where(e =>
                e.EmployeeCode.Contains(searchString) ||
                e.FirstName.Contains(searchString) ||
                e.LastName.Contains(searchString) ||
                e.Email.Contains(searchString));
        }

        if (branchId.HasValue)
        {
            employees = employees.Where(e =>
                e.BranchId == branchId);
        }

        if (departmentId.HasValue)
        {
            employees = employees.Where(e =>
                e.DepartmentId == departmentId);
        }


        if (designationId.HasValue)
        {
            employees = employees.Where(e =>
                e.DesignationId == designationId);
        }

        if (roleId.HasValue)
        {
            employees = employees.Where(e =>
                e.RoleId == roleId);
        }
        var employeeList = await employees
            .OrderBy(e => e.EmployeeId)
            .ToListAsync();


        return View(employeeList);
    }
    // GET: EMPLOYEES/Details/5
    [Permission("Employee.ViewProfile")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }
    [HttpGet]
    [Permission("Employee.Import")]
    public IActionResult Import()
    {
        return View();
    }


    [HttpPost]
    [Permission("Employee.Import")]
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

        for (int row = 2; row <= lastRow; row++)
        {
            string employeeCode = worksheet.Cell(row, 1).Value.ToString();
            string firstName = worksheet.Cell(row, 2).Value.ToString();
            string lastName = worksheet.Cell(row, 3).Value.ToString();
            string email = worksheet.Cell(row, 4).Value.ToString();
            string phone = worksheet.Cell(row, 5).Value.ToString();
            string gender = worksheet.Cell(row, 6).Value.ToString();
            decimal salary = Convert.ToDecimal(
                worksheet.Cell(row, 7).Value);

            DateTime joiningDate = Convert.ToDateTime(
                worksheet.Cell(row, 8).Value);

            string branchName = worksheet.Cell(row, 9).Value.ToString();
            string departmentName = worksheet.Cell(row, 10).Value.ToString();
            string designationName = worksheet.Cell(row, 11).Value.ToString();
            string roleName = worksheet.Cell(row, 12).Value.ToString();


            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.BranchName == branchName);


            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName == departmentName);


            var designation = await _context.Designations
                .FirstOrDefaultAsync(d =>
                    d.DesignationName == designationName &&
                    d.DepartmentId == department.DepartmentId);


            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);


            if (branch == null ||
                department == null ||
                designation == null ||
                role == null)
            {
                continue;
            }


            Employee employee = new Employee
            {
                EmployeeCode = employeeCode,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                Gender = gender,
                Salary = salary,
                JoiningDate = joiningDate,

                BranchId = branch.BranchId,
                DepartmentId = department.DepartmentId,
                DesignationId = designation.DesignationId,
                RoleId = role.RoleId,

                IsActive = true,
                IsDeleted = false
            };


            _context.Employees.Add(employee);
        }


        await _context.SaveChangesAsync();


        await ActivityLogger.LogAsync(
            _context,
            User.Identity?.Name,
            "Imported Employees from Excel");


        TempData["Success"] =
            $"Excel imported successfully. Total rows: {lastRow - 1}";


        return RedirectToAction(nameof(Index));
    }
    [Permission("Employee.Import")]
    public IActionResult DownloadTemplate()
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Employees");

            worksheet.Cell(1, 1).Value = "EmployeeCode";
            worksheet.Cell(1, 2).Value = "FirstName";
            worksheet.Cell(1, 3).Value = "LastName";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "Phone";
            worksheet.Cell(1, 6).Value = "Gender";
            worksheet.Cell(1, 7).Value = "Salary";
            worksheet.Cell(1, 8).Value = "JoiningDate";
            worksheet.Cell(1, 9).Value = "Branch";
            worksheet.Cell(1, 10).Value = "Department";
            worksheet.Cell(1, 11).Value = "Designation";
            worksheet.Cell(1, 12).Value = "Role";

            worksheet.Cell(2, 1).Value = "EMP001";
            worksheet.Cell(2, 2).Value = "Venkat";
            worksheet.Cell(2, 3).Value = "Narayana";
            worksheet.Cell(2, 4).Value = "venkat@gmail.com";
            worksheet.Cell(2, 5).Value = "9876543210";
            worksheet.Cell(2, 6).Value = "Male";
            worksheet.Cell(2, 7).Value = 50000;
            worksheet.Cell(2, 8).Value = DateTime.Today;
            worksheet.Cell(2, 9).Value = "Hyderabad Head Office";
            worksheet.Cell(2, 10).Value = "Information Technology";
            worksheet.Cell(2, 11).Value = "Software Engineer";
            worksheet.Cell(2, 12).Value = "Employee";

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
    [Permission("Employee.Create")]
    public IActionResult Create()
    {
        var lastEmployee = _context.Employees
            .OrderByDescending(e => e.EmployeeId)
            .FirstOrDefault();

        string nextEmployeeCode = "EMP001";

        if (lastEmployee != null &&
            !string.IsNullOrEmpty(lastEmployee.EmployeeCode))
        {
            int number = int.Parse(
                lastEmployee.EmployeeCode.Replace("EMP", ""));

            nextEmployeeCode = $"EMP{(number + 1):D3}";
        }

        var employee = new Employee
        {
            EmployeeCode = nextEmployeeCode
        };


        ViewData["BranchId"] = new SelectList(
            _context.Branches
                .Where(b => b.IsActive),
            "BranchId",
            "BranchName");


        ViewData["DepartmentId"] = new SelectList(
            _context.Departments
                .Where(d => d.IsActive),
            "DepartmentId",
            "DepartmentName");


        ViewData["DesignationId"] = new SelectList(
            _context.Designations
                .Where(d => d.IsActive),
            "DesignationId",
            "DesignationName");


        ViewData["RoleId"] = new SelectList(
            _context.Roles
                .Where(r => r.IsActive),
            "RoleId",
            "RoleName");
        ViewData["ReportingManagerId"] =
    new SelectList(
        _context.Employees
        .Include(e => e.Role)
        .Where(e => e.Role.RoleName == "Manager"
                    && e.IsActive
                    && !e.IsDeleted),
        "EmployeeId",
        "FullName");


        return View(employee);
    }



    [HttpPost]
    [Permission("Employee.Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("EmployeeId,EmployeeCode,FirstName,LastName,Email,Phone,Gender,Salary,JoiningDate,BranchId,DepartmentId,DesignationId,RoleId,ReportingManagerId")]
    Employee employee,
        IFormFile? PhotoFile)
    {
        if (ModelState.IsValid)
        {
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() +
                                  Path.GetExtension(PhotoFile.FileName);

                string uploadFolder = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "uploads");


                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }


                string filePath = Path.Combine(
                    uploadFolder,
                    fileName);


                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await PhotoFile.CopyToAsync(stream);
                }


                employee.PhotoPath = fileName;
            }


            employee.IsActive = true;
            employee.IsDeleted = false;


            _context.Add(employee);

            await _context.SaveChangesAsync();


            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Added Employee: {employee.FirstName} {employee.LastName}");


            TempData["Success"] = "Employee created successfully.";

            return RedirectToAction(nameof(Index));
        }


        ViewData["BranchId"] = new SelectList(
            _context.Branches
                .Where(b => b.IsActive),
            "BranchId",
            "BranchName",
            employee.BranchId);


        ViewData["DepartmentId"] = new SelectList(
            _context.Departments
                .Where(d => d.IsActive),
            "DepartmentId",
            "DepartmentName",
            employee.DepartmentId);


        ViewData["DesignationId"] = new SelectList(
            _context.Designations
                .Where(d => d.IsActive),
            "DesignationId",
            "DesignationName",
            employee.DesignationId);


        ViewData["RoleId"] = new SelectList(
            _context.Roles
                .Where(r => r.IsActive),
            "RoleId",
            "RoleName",
            employee.RoleId);
        ViewData["ReportingManagerId"] =
    new SelectList(
        _context.Employees
        .Include(e => e.Role)
        .Where(e => e.Role.RoleName == "Manager"
                    && e.IsActive
                    && !e.IsDeleted),
        "EmployeeId",
        "FullName",
        employee.ReportingManagerId);

        return View(employee);
    }

    [Permission("Employee.Edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Role)
            .Include(e => e.ReportingManager)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);

        if (employee == null)
        {
            return NotFound();
        }


        ViewData["BranchId"] = new SelectList(
            _context.Branches.Where(b => b.IsActive),
            "BranchId",
            "BranchName",
            employee.BranchId);


        ViewData["DepartmentId"] = new SelectList(
            _context.Departments.Where(d => d.IsActive),
            "DepartmentId",
            "DepartmentName",
            employee.DepartmentId);


        ViewData["DesignationId"] = new SelectList(
            _context.Designations.Where(d => d.IsActive),
            "DesignationId",
            "DesignationName",
            employee.DesignationId);


        ViewData["RoleId"] = new SelectList(
            _context.Roles.Where(r => r.IsActive),
            "RoleId",
            "RoleName",
            employee.RoleId);
        ViewData["ReportingManagerId"] =
    new SelectList(
        _context.Employees
        .Include(e => e.Role)
        .Where(e => e.Role.RoleName == "Manager"
                    && e.EmployeeId != employee.EmployeeId
                    && e.IsActive
                    && !e.IsDeleted),
        "EmployeeId",
        "FullName",
        employee.ReportingManagerId);

        return View(employee);
    }



    [HttpPost]
    [Permission("Employee.Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("EmployeeId,FirstName,LastName,Email,Phone,Gender,Salary,JoiningDate,BranchId,DepartmentId,DesignationId,RoleId,ReportingManagerId,PhotoPath")]
    Employee employee,
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
                var existingEmployee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeId == id);


                if (existingEmployee == null)
                {
                    return NotFound();
                }


                existingEmployee.FirstName = employee.FirstName;
                existingEmployee.LastName = employee.LastName;
                existingEmployee.Email = employee.Email;
                existingEmployee.Phone = employee.Phone;
                existingEmployee.Gender = employee.Gender;
                existingEmployee.Salary = employee.Salary;
                existingEmployee.JoiningDate = employee.JoiningDate;

                existingEmployee.BranchId = employee.BranchId;
                existingEmployee.DepartmentId = employee.DepartmentId;
                existingEmployee.DesignationId = employee.DesignationId;
                existingEmployee.RoleId = employee.RoleId;
                existingEmployee.ReportingManagerId =
    employee.ReportingManagerId;


                if (PhotoFile != null && PhotoFile.Length > 0)
                {
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


                    string fileName = Guid.NewGuid().ToString() +
                                      Path.GetExtension(PhotoFile.FileName);


                    string uploadFolder = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "uploads");


                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }


                    string filePath = Path.Combine(
                        uploadFolder,
                        fileName);


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


                TempData["Success"] = "Employee updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.EmployeeId))
                {
                    return NotFound();
                }

                throw;
            }
        }


        ViewData["BranchId"] = new SelectList(
            _context.Branches.Where(b => b.IsActive),
            "BranchId",
            "BranchName",
            employee.BranchId);


        ViewData["DepartmentId"] = new SelectList(
            _context.Departments.Where(d => d.IsActive),
            "DepartmentId",
            "DepartmentName",
            employee.DepartmentId);


        ViewData["DesignationId"] = new SelectList(
            _context.Designations.Where(d => d.IsActive),
            "DesignationId",
            "DesignationName",
            employee.DesignationId);


        ViewData["RoleId"] = new SelectList(
            _context.Roles.Where(r => r.IsActive),
            "RoleId",
            "RoleName",
            employee.RoleId);


        return View(employee);
    }
    [Permission("Employee.Delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }



    [HttpPost, ActionName("Delete")]
    [Permission("Employee.Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == id);

        if (employee == null)
        {
            return NotFound();
        }


        employee.IsDeleted = true;

        await _context.SaveChangesAsync();


        await ActivityLogger.LogAsync(
            _context,
            User.Identity?.Name,
            $"Moved Employee to Recycle Bin: {employee.FirstName} {employee.LastName}");


        TempData["Success"] = "Employee moved to Recycle Bin successfully.";

        return RedirectToAction(nameof(Index));
    }
    [Permission("Employee.Export")]
    public async Task<IActionResult> ExportToExcel()
    {
        var employees = await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Role)
            .Where(e => !e.IsDeleted)
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



    [Permission("Employee.Export")]
    public async Task<IActionResult> ExportToPdf()
    {
        var employees = await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Role)
            .Where(e => !e.IsDeleted)
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
    [Permission("Employee.Restore")]
    public async Task<IActionResult> RecycleBin()
    {
        var deletedEmployees = await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Role)
            .Where(e => e.IsDeleted)
            .ToListAsync();

        return View(deletedEmployees);
    }


    [HttpPost]
    [Permission("Employee.Restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == id);

        if (employee == null)
        {
            return NotFound();
        }

        employee.IsDeleted = false;

        await _context.SaveChangesAsync();


        await ActivityLogger.LogAsync(
            _context,
            User.Identity?.Name,
            $"Restored Employee: {employee.FirstName} {employee.LastName}");


        TempData["Success"] = "Employee restored successfully.";

        return RedirectToAction(nameof(RecycleBin));
    }



    [HttpPost]
    [Permission("Employee.Delete")]
    public async Task<IActionResult> PermanentDelete(int id)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == id);


        if (employee == null)
        {
            return NotFound();
        }


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


        string employeeName =
            $"{employee.FirstName} {employee.LastName}";


        await ActivityLogger.LogAsync(
            _context,
            User.Identity?.Name,
            $"Permanently Deleted Employee: {employeeName}");


        _context.Employees.Remove(employee);

        await _context.SaveChangesAsync();


        TempData["Success"] =
            "Employee permanently deleted successfully.";


        return RedirectToAction(nameof(RecycleBin));
    }


    private bool EmployeeExists(int? id)
    {
        return _context.Employees.Any(e => e.EmployeeId == id);
    }
}
