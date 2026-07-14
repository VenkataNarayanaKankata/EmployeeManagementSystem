using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel();

            model.TotalEmployees = _context.Employees.Count();

            model.TotalDepartments = _context.Departments.Count();

            decimal avgSalary = _context.Employees.Any()
                ? _context.Employees.Average(e => e.Salary)
                : 0;

            decimal highestSalary = _context.Employees.Any()
                ? _context.Employees.Max(e => e.Salary)
                : 0;

            decimal lowestSalary = _context.Employees.Any()
                ? _context.Employees.Min(e => e.Salary)
                : 0;

            CultureInfo indianCulture = new CultureInfo("en-IN");

            model.AverageSalary = string.Format(indianCulture, "{0:N0}", avgSalary);

            model.HighestSalary = string.Format(indianCulture, "{0:N0}", highestSalary);

            model.LowestSalary = string.Format(indianCulture, "{0:N0}", lowestSalary);

            model.JoinedThisMonth = _context.Employees.Count(e =>
                e.JoiningDate.Month == DateTime.Now.Month &&
                e.JoiningDate.Year == DateTime.Now.Year);

            model.RecentEmployees = _context.Employees
                .Include(e => e.Department)
                .OrderByDescending(e => e.EmployeeId)
                .Take(5)
                .ToList();

            var departmentData = _context.Departments
                .Select(d => new
                {
                    Name = d.DepartmentName,
                    Count = _context.Employees.Count(e => e.DepartmentId == d.DepartmentId)
                })
                .ToList();

            model.DepartmentNames = departmentData
                .Select(x => x.Name)
                .ToList();

            model.EmployeeCounts = departmentData
                .Select(x => x.Count)
                .ToList();

            return View(model);
        }
    }
}