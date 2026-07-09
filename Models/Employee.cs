using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public decimal Salary { get; set; }

        [Required]
        public DateTime JoiningDate { get; set; }
        public string? PhotoPath { get; set; }

        // Foreign Key
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        // Navigation Property
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}