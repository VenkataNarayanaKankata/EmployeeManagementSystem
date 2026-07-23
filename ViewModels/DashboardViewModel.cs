using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }

        public int TotalBranches { get; set; }

        public int TotalDepartments { get; set; }

        public int TotalDesignations { get; set; }

        public int TotalRoles { get; set; }


        public int ActiveEmployees { get; set; }

        public int InactiveEmployees { get; set; }


        public decimal TotalSalary { get; set; }

        public decimal AverageSalary { get; set; }

        public decimal HighestSalary { get; set; }

        public decimal LowestSalary { get; set; }


        public int JoinedThisMonth { get; set; }


        public List<Employee> RecentEmployees { get; set; } = new();



        public List<string> BranchNames { get; set; } = new();

        public List<int> BranchEmployeeCounts { get; set; } = new();



        public List<string> DepartmentNames { get; set; } = new();

        public List<int> DepartmentEmployeeCounts { get; set; } = new();



        public List<string> DesignationNames { get; set; } = new();

        public List<int> DesignationEmployeeCounts { get; set; } = new();



        public List<string> RoleNames { get; set; } = new();

        public List<int> RoleEmployeeCounts { get; set; } = new();



        public List<string> TopDepartmentNames { get; set; } = new();

        public List<int> TopDepartmentCounts { get; set; } = new();



        public List<string> TopDesignationNames { get; set; } = new();

        public List<int> TopDesignationCounts { get; set; } = new();



        public Employee? LoggedInEmployee { get; set; }
        public int MyTeamCount { get; set; }

        public int MyActiveTeamCount { get; set; }

        public string? MyDepartment { get; set; }

        public decimal MyTeamAverageSalary { get; set; }


        public List<Employee> MyTeamEmployees { get; set; } = new();
    }
}