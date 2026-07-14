using System;

namespace EmployeeManagementSystem.ViewModels
{
    public class EmployeeReportFilterViewModel
    {
        public string? SearchName { get; set; }

        public int? DepartmentId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}