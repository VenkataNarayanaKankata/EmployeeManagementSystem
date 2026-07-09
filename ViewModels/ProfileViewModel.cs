using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.ViewModels
{
    public class ProfileViewModel
    {
        public string Username { get; set; }

        public string Role { get; set; } = "Administrator";
    }
}