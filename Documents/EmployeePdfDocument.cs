using EmployeeManagementSystem.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EmployeeManagementSystem.Documents
{
    public class EmployeePdfDocument : IDocument
    {
        private readonly List<Employee> _employees;

        public EmployeePdfDocument(List<Employee> employees)
        {
            _employees = employees;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;


        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);


                page.Header()
                    .Text("Employee Management Report")
                    .FontSize(20)
                    .Bold()
                    .FontColor(Colors.Blue.Medium);



                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Code
                        columns.RelativeColumn(3); // Name
                        columns.RelativeColumn(3); // Branch
                        columns.RelativeColumn(3); // Department
                        columns.RelativeColumn(3); // Designation
                        columns.RelativeColumn(2); // Role
                        columns.RelativeColumn(2); // Salary
                        columns.RelativeColumn(2); // Status
                    });



                    table.Header(header =>
                    {
                        header.Cell()
                            .Element(CellStyle)
                            .Text("Code")
                            .Bold();


                        header.Cell()
                            .Element(CellStyle)
                            .Text("Name")
                            .Bold();


                        header.Cell()
                            .Element(CellStyle)
                            .Text("Branch")
                            .Bold();


                        header.Cell()
                            .Element(CellStyle)
                            .Text("Department")
                            .Bold();


                        header.Cell()
                            .Element(CellStyle)
                            .Text("Designation")
                            .Bold();


                        header.Cell()
                            .Element(CellStyle)
                            .Text("Role")
                            .Bold();


                        header.Cell()
                            .Element(CellStyle)
                            .Text("Salary")
                            .Bold();


                        header.Cell()
                            .Element(CellStyle)
                            .Text("Status")
                            .Bold();
                    });



                    foreach (var employee in _employees)
                    {
                        table.Cell()
                            .Element(CellStyle)
                            .Text(employee.EmployeeCode ?? "");


                        table.Cell()
                            .Element(CellStyle)
                            .Text($"{employee.FirstName} {employee.LastName}");


                        table.Cell()
                            .Element(CellStyle)
                            .Text(employee.Branch?.BranchName ?? "");


                        table.Cell()
                            .Element(CellStyle)
                            .Text(employee.Department?.DepartmentName ?? "");


                        table.Cell()
                            .Element(CellStyle)
                            .Text(employee.Designation?.DesignationName ?? "");


                        table.Cell()
                            .Element(CellStyle)
                            .Text(employee.Role?.RoleName ?? "");


                        table.Cell()
                            .Element(CellStyle)
                            .Text($"₹ {employee.Salary:N0}");


                        table.Cell()
                            .Element(CellStyle)
                            .Text(employee.IsActive ? "Active" : "Inactive");
                    }



                    static IContainer CellStyle(IContainer container)
                    {
                        return container
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(4);
                    }
                });



                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.Now.ToString("dd MMM yyyy"));
                    });
            });
        }
    }
}