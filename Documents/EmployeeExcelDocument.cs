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

                // Header
                worksheet.Cell(1, 1).Value = "Employee ID";
                worksheet.Cell(1, 2).Value = "First Name";
                worksheet.Cell(1, 3).Value = "Last Name";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Phone";
                worksheet.Cell(1, 6).Value = "Salary";
                worksheet.Cell(1, 7).Value = "Joining Date";
                worksheet.Cell(1, 8).Value = "Department";

                var header = worksheet.Range("A1:H1");

                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.SteelBlue;
                header.Style.Font.FontColor = XLColor.White;

                int row = 2;

                foreach (var employee in _employees)
                {
                    worksheet.Cell(row, 1).Value = employee.EmployeeId;
                    worksheet.Cell(row, 2).Value = employee.FirstName;
                    worksheet.Cell(row, 3).Value = employee.LastName;
                    worksheet.Cell(row, 4).Value = employee.Email;
                    worksheet.Cell(row, 5).Value = employee.Phone;
                    worksheet.Cell(row, 6).Value = employee.Salary;
                    worksheet.Cell(row, 7).Value = employee.JoiningDate.ToString("dd-MMM-yyyy");
                    worksheet.Cell(row, 8).Value = employee.Department?.DepartmentName;

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