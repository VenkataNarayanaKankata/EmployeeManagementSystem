using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.ViewModels
{
    public class UserManagementViewModel
    {
        public List<Admin> Users { get; set; } = new();

        public string? Search { get; set; }

        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int InactiveUsers { get; set; }
    }
}