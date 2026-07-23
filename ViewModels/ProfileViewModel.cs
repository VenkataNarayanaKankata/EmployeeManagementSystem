namespace EmployeeManagementSystem.ViewModels
{
    public class ProfileViewModel
    {
        public int AdminId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;


        public string? EmployeeName { get; set; }

        public string? EmployeeCode { get; set; }


        public string? Department { get; set; }

        public string? Designation { get; set; }


        public string? Email { get; set; }

        public string? Phone { get; set; }


        public bool IsActive { get; set; }


        public DateTime? LastLogin { get; set; }
    }
}