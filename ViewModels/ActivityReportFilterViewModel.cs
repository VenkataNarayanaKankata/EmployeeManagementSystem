using System;

namespace EmployeeManagementSystem.ViewModels
{
    public class ActivityReportFilterViewModel
    {
        public string? Username { get; set; }

        public string? Activity { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}