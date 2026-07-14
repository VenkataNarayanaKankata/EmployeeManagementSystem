using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }

        public int TotalDepartments { get; set; }

        public string AverageSalary { get; set; } = string.Empty;

        public string HighestSalary { get; set; } = string.Empty;

        public string LowestSalary { get; set; } = string.Empty;

        public int JoinedThisMonth { get; set; }

        public List<Employee> RecentEmployees { get; set; } = new();

        public List<string> DepartmentNames { get; set; } = new();

        public List<int> EmployeeCounts { get; set; } = new();
    }
}