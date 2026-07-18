using ClosedXML.Excel;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class ImportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult EmployeeImport()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> PreviewImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please select Excel file."
                });
            }


            using var stream = new MemoryStream();

            await file.CopyToAsync(stream);


            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheet(1);


            int lastRow = worksheet.LastRowUsed().RowNumber();


            HashSet<string> existingDepartments = new();
            HashSet<string> newDepartments = new();



            for (int row = 2; row <= lastRow; row++)
            {

                string departmentCode =
                    worksheet.Cell(row, 10)
                    .GetString()
                    .Trim();


                if (string.IsNullOrEmpty(departmentCode))
                    continue;



                bool exists =
                    await _context.Departments
                    .AnyAsync(x =>
                    x.DepartmentCode == departmentCode);



                if (exists)
                {
                    existingDepartments.Add(departmentCode);
                }
                else
                {
                    newDepartments.Add(departmentCode);
                }

            }



            return Json(new
            {
                success = true,
                fileName = file.FileName,
                totalEmployees = lastRow - 1,
                totalDepartments =
                    existingDepartments.Count +
                    newDepartments.Count,

                existingDepartments,
                newDepartments
            });

        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadEmployees(IFormFile file)
        {

            if (file == null || file.Length == 0)
            {
                TempData["Error"] =
                    "Please select Excel file.";

                return RedirectToAction(nameof(EmployeeImport));
            }



            using var stream = new MemoryStream();

            await file.CopyToAsync(stream);



            using var workbook = new XLWorkbook(stream);


            var worksheet = workbook.Worksheet(1);


            int lastRow =
                worksheet.LastRowUsed().RowNumber();



            int imported = 0;
            int skipped = 0;



            for (int row = 2; row <= lastRow; row++)
            {

                string employeeCode =
                    worksheet.Cell(row, 1)
                    .GetString()
                    .Trim();


                string firstName =
                    worksheet.Cell(row, 2)
                    .GetString()
                    .Trim();


                string lastName =
                    worksheet.Cell(row, 3)
                    .GetString()
                    .Trim();


                string email =
                    worksheet.Cell(row, 4)
                    .GetString()
                    .Trim();


                string phone =
                    worksheet.Cell(row, 5)
                    .GetString()
                    .Trim();


                string gender =
                    worksheet.Cell(row, 6)
                    .GetString()
                    .Trim();



                decimal salary =
                    worksheet.Cell(row, 7)
                    .GetValue<decimal>();


                DateTime joiningDate =
                    worksheet.Cell(row, 8)
                    .GetDateTime();



                string branchCode =
                    worksheet.Cell(row, 9)
                    .GetString()
                    .Trim();



                string departmentCode =
                    worksheet.Cell(row, 10)
                    .GetString()
                    .Trim();



                string designationCode =
                    worksheet.Cell(row, 11)
                    .GetString()
                    .Trim();



                string roleName =
                    worksheet.Cell(row, 12)
                    .GetString()
                    .Trim();




                bool exists =
                    await _context.Employees
                    .AnyAsync(x =>
                    x.Email == email);



                if (exists)
                {
                    skipped++;
                    continue;
                }





                var branch =
                    await _context.Branches
                    .FirstOrDefaultAsync(x =>
                    x.BranchCode == branchCode);



                var department =
                    await _context.Departments
                    .FirstOrDefaultAsync(x =>
                    x.DepartmentCode == departmentCode);



                var designation =
                    await _context.Designations
                    .FirstOrDefaultAsync(x =>
                    x.DesignationCode == designationCode);



                var role =
                    await _context.Roles
                    .FirstOrDefaultAsync(x =>
                    x.RoleName == roleName);





                if (branch == null ||
                   department == null ||
                   designation == null ||
                   role == null)
                {
                    skipped++;
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


                imported++;

            }




            await _context.SaveChangesAsync();



            await ActivityLogger.LogAsync(
                _context,
                User.Identity?.Name,
                $"Imported Employees: {imported}, Skipped: {skipped}"
            );




            TempData["Success"] =
                $"Import Completed. Imported: {imported}, Skipped: {skipped}";



            return RedirectToAction(nameof(EmployeeImport));

        }

    }
}