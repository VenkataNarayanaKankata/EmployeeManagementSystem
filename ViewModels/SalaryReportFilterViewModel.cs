namespace EmployeeManagementSystem.ViewModels
{
    public class SalaryReportFilterViewModel
    {
        public int? DepartmentId { get; set; }

        public decimal? SalaryFrom { get; set; }

        public decimal? SalaryTo { get; set; }

        public string? SortBy { get; set; }
    }
}