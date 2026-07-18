using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class Admin
    {
        public int AdminId { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;


        [Required]
        public string Password { get; set; } = string.Empty;


        public int RoleId { get; set; }


        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; } = null!;


        public bool IsActive { get; set; } = true;

        public bool MustChangePassword { get; set; } = true;

        public DateTime? LastLogin { get; set; }

        public DateTime? PasswordChangedOn { get; set; }


        public int? EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public Employee? Employee { get; set; }
    }
}