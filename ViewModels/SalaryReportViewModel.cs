using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagementSystem.ViewModels
{
    public class SalaryReportViewModel
    {
        public decimal HighestSalary { get; set; }

        public decimal LowestSalary { get; set; }

        public decimal AverageSalary { get; set; }

        public decimal TotalSalary { get; set; }

        public List<DepartmentSalaryViewModel> Departments { get; set; }
            = new();

        // Filters
        public SalaryReportFilterViewModel Filter { get; set; } = new();

        // Department Dropdown
        public List<SelectListItem> DepartmentList { get; set; } = new();
    }

    public class DepartmentSalaryViewModel
    {
        public string DepartmentName { get; set; } = "";

        public decimal TotalSalary { get; set; }
    }
}