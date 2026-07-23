using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            model.TotalEmployees = await _context.Employees
                .CountAsync(e => !e.IsDeleted);

            model.TotalBranches = await _context.Branches.CountAsync();

            model.TotalDepartments = await _context.Departments.CountAsync();

            model.TotalDesignations = await _context.Designations.CountAsync();

            model.TotalRoles = await _context.Roles.CountAsync();


            model.ActiveEmployees = await _context.Employees
                .CountAsync(e => e.IsActive && !e.IsDeleted);


            model.InactiveEmployees = await _context.Employees
                .CountAsync(e => !e.IsActive && !e.IsDeleted);


            if (await _context.Employees.AnyAsync(e => !e.IsDeleted))
            {
                model.TotalSalary = await _context.Employees
                    .Where(e => !e.IsDeleted)
                    .SumAsync(e => e.Salary);


                model.AverageSalary = await _context.Employees
                    .Where(e => !e.IsDeleted)
                    .AverageAsync(e => e.Salary);


                model.HighestSalary = await _context.Employees
                    .Where(e => !e.IsDeleted)
                    .MaxAsync(e => e.Salary);


                model.LowestSalary = await _context.Employees
                    .Where(e => !e.IsDeleted)
                    .MinAsync(e => e.Salary);
            }


            model.JoinedThisMonth =
                await _context.Employees.CountAsync(e =>
                    e.JoiningDate.Month == DateTime.Now.Month &&
                    e.JoiningDate.Year == DateTime.Now.Year &&
                    !e.IsDeleted);



            model.RecentEmployees =
                await _context.Employees
                .Include(e => e.Branch)
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Role)
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.EmployeeId)
                .Take(5)
                .ToListAsync();



            var branchData =
                await _context.Branches
                .Select(b => new
                {
                    Name = b.BranchName,
                    Count = _context.Employees.Count(e =>
                        e.BranchId == b.BranchId &&
                        !e.IsDeleted)
                })
                .ToListAsync();


            model.BranchNames =
                branchData.Select(x => x.Name).ToList();


            model.BranchEmployeeCounts =
                branchData.Select(x => x.Count).ToList();





            var departmentData =
                await _context.Departments
                .Select(d => new
                {
                    Name = d.DepartmentName,
                    Count = _context.Employees.Count(e =>
                        e.DepartmentId == d.DepartmentId &&
                        !e.IsDeleted)
                })
                .ToListAsync();


            model.DepartmentNames =
                departmentData.Select(x => x.Name).ToList();


            model.DepartmentEmployeeCounts =
                departmentData.Select(x => x.Count).ToList();



            var topDepartments = departmentData
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();


            model.TopDepartmentNames =
                topDepartments.Select(x => x.Name).ToList();


            model.TopDepartmentCounts =
                topDepartments.Select(x => x.Count).ToList();






            var designationData =
                await _context.Designations
                .Select(d => new
                {
                    Name = d.DesignationName,

                    Count = _context.Employees.Count(e =>
                        e.DesignationId == d.DesignationId &&
                        !e.IsDeleted)

                })
                .ToListAsync();


            model.DesignationNames =
                designationData.Select(x => x.Name).ToList();


            model.DesignationEmployeeCounts =
                designationData.Select(x => x.Count).ToList();



            var topDesignations = designationData
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();


            model.TopDesignationNames =
                topDesignations.Select(x => x.Name).ToList();


            model.TopDesignationCounts =
                topDesignations.Select(x => x.Count).ToList();







            var roleData =
     await _context.Roles
     .Select(r => new
     {
         Name = r.RoleName,
         Count = _context.Employees
             .Where(e => e.RoleId == r.RoleId && !e.IsDeleted)
             .Count()
     })
     .Where(x => x.Count > 0)
     .ToListAsync();


            model.RoleNames =
                roleData.Select(x => x.Name).ToList();


            model.RoleEmployeeCounts =
                roleData.Select(x => x.Count).ToList();




            ViewBag.CurrentRole =
     User.FindFirstValue(
         ClaimTypes.Role);


            var role = User.FindFirstValue(
                ClaimTypes.Role);

            if (role == "Employee")
            {
                var employeeIdClaim =
                    User.FindFirst("EmployeeId");


                if (employeeIdClaim != null)
                {
                    int employeeId =
                        int.Parse(employeeIdClaim.Value);


                    model.LoggedInEmployee =
                        await _context.Employees
                        .Include(e => e.Branch)
                        .Include(e => e.Department)
                        .Include(e => e.Designation)
                        .Include(e => e.Role)
                        .FirstOrDefaultAsync(
                            e => e.EmployeeId == employeeId);
                }


                return View(
                    "EmployeeDashboard",
                    model);
            }

            if (role == "HR")
            {
                return View(
                    "HRDashboard",
                    model);
            }

            if (role == "Manager")
            {
                var employeeIdClaim =
                    User.FindFirst("EmployeeId");


                if (employeeIdClaim != null)
                {
                    int managerId =
                        int.Parse(employeeIdClaim.Value);


                    var manager =
                        await _context.Employees
                        .Include(e => e.Department)
                        .FirstOrDefaultAsync(
                            e => e.EmployeeId == managerId);



                    model.LoggedInEmployee = manager;



                    model.MyTeamEmployees =
                        await _context.Employees
                        .Include(e => e.Department)
                        .Include(e => e.Designation)
                        .Include(e => e.Role)
                        .Where(e =>
                            e.ReportingManagerId == managerId
                            &&
                            !e.IsDeleted)
                        .ToListAsync();



                    model.MyTeamCount =
                        model.MyTeamEmployees.Count;



                    model.MyActiveTeamCount =
                        model.MyTeamEmployees
                        .Count(e => e.IsActive);



                    model.MyDepartment =
                        manager?.Department?.DepartmentName
                        ?? "N/A";



                    if (model.MyTeamEmployees.Any())
                    {
                        model.MyTeamAverageSalary =
                            model.MyTeamEmployees
                            .Average(e => e.Salary);
                    }
                }


                return View(
                    "ManagerDashboard",
                    model);
            }

            return View(model);
        }
    }
}