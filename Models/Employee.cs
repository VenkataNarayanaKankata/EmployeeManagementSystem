using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        // Employee Information

        [Required]
        [StringLength(20)]
        [Display(Name = "Employee Code")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        // Employment Details

        [Required]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        public string? PhotoPath { get; set; }

        // Department

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public Department? Department { get; set; }

        // Designation

        [Required]
        [Display(Name = "Designation")]
        public int DesignationId { get; set; }

        public Designation? Designation { get; set; }

        // System Role

        [Required]
        [Display(Name = "System Role")]
        public int RoleId { get; set; }

        public Role? Role { get; set; }

        // Employee Status

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation

        public Admin? Admin { get; set; }
        // Branch

        [Required]
        [Display(Name = "Branch")]
        public int BranchId { get; set; }

        public Branch? Branch { get; set; }
        public int? ReportingManagerId { get; set; }

        [ForeignKey(nameof(ReportingManagerId))]
        public Employee? ReportingManager { get; set; }


        public ICollection<Employee> TeamMembers { get; set; }
            = new List<Employee>();
    }
}