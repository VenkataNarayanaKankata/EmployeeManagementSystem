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
                page.Margin(30);

                page.Header()
                    .Text("Employee Management Report")
                    .FontSize(22)
                    .Bold()
                    .FontColor(Colors.Blue.Medium);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Name
                        columns.RelativeColumn(3); // Email
                        columns.RelativeColumn(2); // Gender
                        columns.RelativeColumn(2); // Department
                        columns.RelativeColumn(2); // Salary
                        columns.RelativeColumn(1); // role
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Name").Bold();
                        header.Cell().Element(CellStyle).Text("Email").Bold();
                        header.Cell().Element(CellStyle).Text("Gender").Bold();
                        header.Cell().Element(CellStyle).Text("Department").Bold();
                        header.Cell().Element(CellStyle).Text("Salary").Bold();
                        header.Cell().Element(CellStyle).Text("Role").Bold();
                    });

                    foreach (var employee in _employees)
                    {
                        table.Cell().Element(CellStyle)
                            .Text($"{employee.FirstName} {employee.LastName}");

                        table.Cell().Element(CellStyle)
                            .Text(employee.Email);

                        table.Cell().Element(CellStyle)
                            .Text(employee.Gender);

                        table.Cell().Element(CellStyle)
                            .Text(employee.Department?.DepartmentName ?? "");

                        table.Cell().Element(CellStyle)
                            .Text($"₹ {employee.Salary:N0}");
                        table.Cell().Element(CellStyle)
                        .Text(employee.Role?.RoleName ?? "");
                    }

                    static IContainer CellStyle(IContainer container)
                    {
                        return container
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(5);
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