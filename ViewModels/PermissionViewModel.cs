using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.ViewModels
{
    public class PermissionViewModel
    {
        public List<Permission> Permissions { get; set; } = new();

        public string? Search { get; set; }

        public string? Module { get; set; }

        public int TotalPermissions { get; set; }

        public int ActivePermissions { get; set; }

        public int InactivePermissions { get; set; }

        public List<string> Modules { get; set; } = new();
    }
}