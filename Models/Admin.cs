using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class Admin
    {
        public int AdminId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}