namespace EmployeeManagementSystem.ViewModels
{
    public class JoiningReportFilterViewModel
    {
        public int? DepartmentId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? SortBy { get; set; }
    }
}