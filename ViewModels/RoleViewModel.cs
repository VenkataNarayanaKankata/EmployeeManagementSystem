using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.ViewModels
{
    public class RoleViewModel
    {
        public List<Role> Roles { get; set; } = new();

        public string? Search { get; set; }

        public int TotalRoles { get; set; }

        public int ActiveRoles { get; set; }

        public int InactiveRoles { get; set; }
    }
}