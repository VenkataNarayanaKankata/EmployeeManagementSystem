using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagementSystem.ViewModels
{
    public class ActivityReportViewModel
    {
        public List<ActivityLog> Activities { get; set; } = new();

        public ActivityReportFilterViewModel Filter { get; set; } = new();

        public List<SelectListItem> ActivityTypes { get; set; } = new();
    }
}