using ClosedXML.Excel;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Documents
{
    public class EmployeeExcelDocument
    {
        private readonly List<Employee> _employees;

        public EmployeeExcelDocument(List<Employee> employees)
        {
            _employees = employees;
        }

        public byte[] GenerateExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Employees");


                worksheet.Cell(1, 1).Value = "Employee Code";
                worksheet.Cell(1, 2).Value = "Employee Name";
                worksheet.Cell(1, 3).Value = "Email";
                worksheet.Cell(1, 4).Value = "Phone";
                worksheet.Cell(1, 5).Value = "Gender";
                worksheet.Cell(1, 6).Value = "Branch";
                worksheet.Cell(1, 7).Value = "Department";
                worksheet.Cell(1, 8).Value = "Designation";
                worksheet.Cell(1, 9).Value = "Role";
                worksheet.Cell(1, 10).Value = "Salary";
                worksheet.Cell(1, 11).Value = "Joining Date";
                worksheet.Cell(1, 12).Value = "Status";


                var header = worksheet.Range("A1:L1");

                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.SteelBlue;
                header.Style.Font.FontColor = XLColor.White;


                int row = 2;


                foreach (var employee in _employees)
                {
                    worksheet.Cell(row, 1).Value =
                        employee.EmployeeCode;


                    worksheet.Cell(row, 2).Value =
                        $"{employee.FirstName} {employee.LastName}";


                    worksheet.Cell(row, 3).Value =
                        employee.Email;


                    worksheet.Cell(row, 4).Value =
                        employee.Phone;


                    worksheet.Cell(row, 5).Value =
                        employee.Gender;


                    worksheet.Cell(row, 6).Value =
                        employee.Branch?.BranchName;


                    worksheet.Cell(row, 7).Value =
                        employee.Department?.DepartmentName;


                    worksheet.Cell(row, 8).Value =
                        employee.Designation?.DesignationName;


                    worksheet.Cell(row, 9).Value =
                        employee.Role?.RoleName;


                    worksheet.Cell(row, 10).Value =
                        employee.Salary;


                    worksheet.Cell(row, 11).Value =
                        employee.JoiningDate.ToString("dd-MMM-yyyy");


                    worksheet.Cell(row, 12).Value =
                        employee.IsActive ? "Active" : "Inactive";


                    row++;
                }


                worksheet.Columns().AdjustToContents();


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return stream.ToArray();
                }
            }
        }
    }
}