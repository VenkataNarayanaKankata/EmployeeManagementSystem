using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagementSystem.ViewModels
{
    public class DepartmentReportViewModel
    {
        public int TotalDepartments { get; set; }

        public int TotalEmployees { get; set; }

        public List<DepartmentSummaryViewModel> Departments { get; set; }
            = new();

        // Filters
        public DepartmentReportFilterViewModel Filter { get; set; } = new();
    }

    public class DepartmentSummaryViewModel
    {
        public string DepartmentName { get; set; } = "";

        public int EmployeeCount { get; set; }

        public decimal AverageSalary { get; set; }

        public decimal HighestSalary { get; set; }

        public decimal LowestSalary { get; set; }
    }
}