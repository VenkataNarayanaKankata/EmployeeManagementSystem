using ClosedXML.Excel;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPermissionService _permissionService;


        public ReportsController(
            ApplicationDbContext context,
            IPermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            if (!await HasPermission("Report.View"))
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }

            return View();
        }

        public async Task<IActionResult> SalaryReport(
    int? departmentId,
    decimal? salaryFrom,
    decimal? salaryTo,
    string? sortBy,
    bool print = false)
        {
            if (!await HasPermission("Report.Salary"))
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }
            var query = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            // Department Filter
            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            // Salary From
            if (salaryFrom.HasValue)
            {
                query = query.Where(e => e.Salary >= salaryFrom.Value);
            }

            // Salary To
            if (salaryTo.HasValue)
            {
                query = query.Where(e => e.Salary <= salaryTo.Value);
            }

            var employees = await query.ToListAsync();

            var departmentSalary = employees
                .GroupBy(e => e.Department!.DepartmentName)
                .Select(g => new DepartmentSalaryViewModel
                {
                    DepartmentName = g.Key,
                    TotalSalary = g.Sum(x => x.Salary)
                });

            // Sorting
            departmentSalary = sortBy switch
            {
                "HighestSalary" => departmentSalary.OrderByDescending(d => d.TotalSalary),
                "LowestSalary" => departmentSalary.OrderBy(d => d.TotalSalary),
                _ => departmentSalary.OrderBy(d => d.DepartmentName)
            };

            var model = new SalaryReportViewModel
            {
                HighestSalary = employees.Any() ? employees.Max(e => e.Salary) : 0,
                LowestSalary = employees.Any() ? employees.Min(e => e.Salary) : 0,
                AverageSalary = employees.Any() ? employees.Average(e => e.Salary) : 0,
                TotalSalary = employees.Sum(e => e.Salary),

                Departments = departmentSalary.ToList(),

                Filter = new SalaryReportFilterViewModel
                {
                    DepartmentId = departmentId,
                    SalaryFrom = salaryFrom,
                    SalaryTo = salaryTo,
                    SortBy = sortBy
                },

                DepartmentList = await _context.Departments
                    .Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = d.DepartmentId.ToString(),
                        Text = d.DepartmentName
                    })
                    .ToListAsync()
            };

            ViewBag.Print = print;

            // If AJAX request, return only report content
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_SalaryReportContent", model);
            }
            await ActivityLogger.LogAsync(
    _context,
    User.Identity?.Name,
    "Viewed Salary Report");
            return View(model);
        }
        public async Task<IActionResult> JoiningReport(
    int? departmentId,
    DateTime? fromDate,
    DateTime? toDate,
    string? sortBy,
    bool print = false)
        {
            if (!await HasPermission("Report.Employee"))
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }
            var query = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            // Department Filter
            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            // From Date Filter
            if (fromDate.HasValue)
            {
                query = query.Where(e => e.JoiningDate.Date >= fromDate.Value.Date);
            }

            // To Date Filter
            if (toDate.HasValue)
            {
                query = query.Where(e => e.JoiningDate.Date <= toDate.Value.Date);
            }

            // Sorting
            query = sortBy switch
            {
                "Oldest" => query.OrderBy(e => e.JoiningDate),

                "EmployeeName" => query.OrderBy(e => e.FirstName),

                _ => query.OrderByDescending(e => e.JoiningDate)
            };

            var employees = await query.ToListAsync();

            var model = new JoiningReportViewModel
            {
                TotalEmployees = employees.Count,

                TodayJoining = employees.Count(e =>
                    e.JoiningDate.Date == DateTime.Today),

                ThisMonthJoining = employees.Count(e =>
                    e.JoiningDate.Month == DateTime.Today.Month &&
                    e.JoiningDate.Year == DateTime.Today.Year),

                ThisYearJoining = employees.Count(e =>
                    e.JoiningDate.Year == DateTime.Today.Year),

                EarliestJoiningDate = employees.Any()
         ? employees.Min(e => e.JoiningDate)
         : null,

                LatestJoiningDate = employees.Any()
         ? employees.Max(e => e.JoiningDate)
         : null,

                Employees = employees,

                RecentEmployees = employees
         .OrderByDescending(e => e.JoiningDate)
         .Take(10)
         .ToList(),

                Filter = new JoiningReportFilterViewModel
                {
                    DepartmentId = departmentId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    SortBy = sortBy
                },

                DepartmentList = await _context.Departments
         .OrderBy(d => d.DepartmentName)
         .Select(d => new SelectListItem
         {
             Value = d.DepartmentId.ToString(),
             Text = d.DepartmentName
         })
         .ToListAsync()
            };

            ViewBag.Print = print;

            // Return only report content for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_JoiningReportContent", model);
            }
            await ActivityLogger.LogAsync(
    _context,
    User.Identity?.Name,
    "Viewed Joining Report");
            return View(model);
        }
        public async Task<IActionResult> DepartmentReport(
    string? departmentName,
    string? sortBy,
    bool print = false)
        {
            if (!await HasPermission("Report.Department"))
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }
            var employees = await _context.Employees
                .Include(e => e.Department)
                .ToListAsync();

            var departments = employees
                .GroupBy(e => e.Department!.DepartmentName)
                .Select(g => new DepartmentSummaryViewModel
                {
                    DepartmentName = g.Key,
                    EmployeeCount = g.Count(),
                    AverageSalary = g.Average(x => x.Salary),
                    HighestSalary = g.Max(x => x.Salary),
                    LowestSalary = g.Min(x => x.Salary)
                });

            // Search Department
            if (!string.IsNullOrWhiteSpace(departmentName))
            {
                departments = departments.Where(d =>
                    d.DepartmentName.Contains(departmentName));
            }

            // Sorting
            departments = sortBy switch
            {
                "Employees" => departments.OrderByDescending(d => d.EmployeeCount),

                "AverageSalary" => departments.OrderByDescending(d => d.AverageSalary),

                _ => departments.OrderBy(d => d.DepartmentName)
            };

            var departmentList = departments.ToList();

            var model = new DepartmentReportViewModel
            {
                TotalDepartments = departmentList.Count,

                TotalEmployees = departmentList.Sum(d => d.EmployeeCount),

                Departments = departmentList,

                Filter = new DepartmentReportFilterViewModel
                {
                    DepartmentName = departmentName,
                    SortBy = sortBy
                }
            };

            ViewBag.Print = print;
            await ActivityLogger.LogAsync(
    _context,
    User.Identity?.Name,
    "Viewed Department Report");
            return View(model);
        }
        public async Task<IActionResult> EmployeeReport(
    string? searchName,
    int? departmentId,
    DateTime? fromDate,
    DateTime? toDate,
    bool print = false)
        {
            if (!await HasPermission("Report.Employee"))
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }
            var query = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            // Search by Employee Name
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(e =>
                    e.FirstName.Contains(searchName) ||
                    e.LastName.Contains(searchName));
            }

            // Filter by Department
            if (departmentId.HasValue)
            {
                query = query.Where(e =>
                    e.DepartmentId == departmentId.Value);
            }

            // Joining Date From
            if (fromDate.HasValue)
            {
                query = query.Where(e =>
                    e.JoiningDate >= fromDate.Value);
            }

            // Joining Date To
            if (toDate.HasValue)
            {
                query = query.Where(e =>
                    e.JoiningDate <= toDate.Value);
            }

            var employees = await query
                .OrderBy(e => e.EmployeeId)
                .ToListAsync();

            var model = new EmployeeReportViewModel
            {
                TotalEmployees = employees.Count,

                AverageSalary = employees.Any()
                    ? employees.Average(e => e.Salary)
                    : 0,

                HighestSalary = employees.Any()
                    ? employees.Max(e => e.Salary)
                    : 0,

                LowestSalary = employees.Any()
                    ? employees.Min(e => e.Salary)
                    : 0,

                Employees = employees,

                Filter = new EmployeeReportFilterViewModel
                {
                    SearchName = searchName,
                    DepartmentId = departmentId,
                    FromDate = fromDate,
                    ToDate = toDate
                },

                Departments = await _context.Departments
                    .Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = d.DepartmentId.ToString(),
                        Text = d.DepartmentName
                    })
                    .ToListAsync()
            };

            ViewBag.Print = print;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EmployeeReportContent", model);
            }
            await ActivityLogger.LogAsync(
    _context,
    User.Identity?.Name,
    "Viewed Employee Report");
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ExportEmployeeExcel()
        {
            if (!await HasPermission("Report.Export"))
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }


            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Role)
                .Where(e => !e.IsDeleted)
                .ToListAsync();


            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Employees");


            worksheet.Cell(1, 1).Value = "Employee Name";
            worksheet.Cell(1, 2).Value = "Department";
            worksheet.Cell(1, 3).Value = "Designation";
            worksheet.Cell(1, 4).Value = "Role";
            worksheet.Cell(1, 5).Value = "Salary";


            int row = 2;


            foreach (var employee in employees)
            {
                worksheet.Cell(row, 1).Value = employee.FullName;
                worksheet.Cell(row, 2).Value = employee.Department?.DepartmentName;
                worksheet.Cell(row, 3).Value = employee.Designation?.DesignationName;
                worksheet.Cell(row, 4).Value = employee.Role?.RoleName;
                worksheet.Cell(row, 5).Value = employee.Salary;

                row++;
            }


            using var stream = new MemoryStream();

            workbook.SaveAs(stream);


            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Employee_Report.xlsx");
        }



        [HttpGet]
        public async Task<IActionResult> ExportDepartmentExcel()
        {
            if (!await HasPermission("Report.Export"))
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }


            var departments = await _context.Departments
                .Include(d => d.Employees)
                .ToListAsync();


            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Departments");


            worksheet.Cell(1, 1).Value = "Department";
            worksheet.Cell(1, 2).Value = "Employees";


            int row = 2;


            foreach (var department in departments)
            {
                worksheet.Cell(row, 1).Value =
                    department.DepartmentName;

                worksheet.Cell(row, 2).Value =
                    department.Employees.Count;

                row++;
            }


            using var stream = new MemoryStream();

            workbook.SaveAs(stream);


            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Department_Report.xlsx");
        }



        [HttpGet]
        public async Task<IActionResult> ExportSalaryExcel()
        {
            if (!await HasPermission("Report.Export"))
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }


            var employees = await _context.Employees
                .Include(e => e.Department)
                .Where(e => !e.IsDeleted)
                .ToListAsync();


            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Salary");


            worksheet.Cell(1, 1).Value = "Employee";
            worksheet.Cell(1, 2).Value = "Department";
            worksheet.Cell(1, 3).Value = "Salary";


            int row = 2;


            foreach (var employee in employees)
            {
                worksheet.Cell(row, 1)
                    .Value = employee.FullName;

                worksheet.Cell(row, 2)
                    .Value = employee.Department?.DepartmentName;

                worksheet.Cell(row, 3)
                    .Value = employee.Salary;

                row++;
            }


            using var stream = new MemoryStream();

            workbook.SaveAs(stream);


            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Salary_Report.xlsx");
        }



        [HttpGet]
        public async Task<IActionResult> ExportJoiningExcel()
        {
            if (!await HasPermission("Report.Export"))
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }


            var employees = await _context.Employees
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.JoiningDate)
                .ToListAsync();


            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Joining");


            worksheet.Cell(1, 1).Value = "Employee";
            worksheet.Cell(1, 2).Value = "Joining Date";


            int row = 2;


            foreach (var employee in employees)
            {
                worksheet.Cell(row, 1)
                    .Value = employee.FullName;

                worksheet.Cell(row, 2)
                    .Value = employee.JoiningDate
                    .ToString("dd-MM-yyyy");

                row++;
            }


            using var stream = new MemoryStream();

            workbook.SaveAs(stream);


            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Joining_Report.xlsx");
        }



        private async Task<bool> HasPermission(string permission)
        {
            return await _permissionService
                .HasPermissionAsync(permission);
        }
    }
}