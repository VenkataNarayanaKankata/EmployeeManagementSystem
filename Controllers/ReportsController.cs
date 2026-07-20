using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Super Admin,HR,Manager")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SalaryReport(
    int? departmentId,
    decimal? salaryFrom,
    decimal? salaryTo,
    string? sortBy,
    bool print = false)
        {
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
    }
}