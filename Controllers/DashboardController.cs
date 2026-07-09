using EmployeeManagementSystem.Data;
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
            ViewBag.TotalEmployees = _context.Employees.Count();

            ViewBag.TotalDepartments = _context.Departments.Count();
            // Highest Salary
            ViewBag.HighestSalary = _context.Employees.Any()
                ? _context.Employees.Max(e => e.Salary)
                : 0;

            // Lowest Salary
            ViewBag.LowestSalary = _context.Employees.Any()
                ? _context.Employees.Min(e => e.Salary)
                : 0;

            // Employees Joined This Month
            ViewBag.JoinedThisMonth = _context.Employees.Count(e =>
                e.JoiningDate.Month == DateTime.Now.Month &&
                e.JoiningDate.Year == DateTime.Now.Year);

            decimal avgSalary = _context.Employees.Any()
       ? _context.Employees.Average(e => e.Salary)
       : 0;

            CultureInfo indianCulture = new CultureInfo("en-IN");

            ViewBag.AverageSalary = string.Format(indianCulture, "{0:N0}", avgSalary);
            ViewBag.HighestSalary = string.Format(indianCulture, "{0:N0}", ViewBag.HighestSalary);

            ViewBag.LowestSalary = string.Format(indianCulture, "{0:N0}", ViewBag.LowestSalary);

            var recentEmployees = _context.Employees
                .Include(e => e.Department)
                .OrderByDescending(e => e.EmployeeId)
                .Take(5)
                .ToList();
            // Employee count by department
            var departmentData = _context.Departments
                .Select(d => new
                {
                    Department = d.DepartmentName,
                    Count = _context.Employees.Count(e => e.DepartmentId == d.DepartmentId)
                })
                .ToList();

            ViewBag.DepartmentNames = departmentData.Select(x => x.Department).ToList();
            ViewBag.EmployeeCounts = departmentData.Select(x => x.Count).ToList();

            return View(recentEmployees);
        }
    }
}