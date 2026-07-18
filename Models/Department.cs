using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(20)]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties

        public ICollection<Employee> Employees { get; set; }
            = new List<Employee>();

        public ICollection<Designation> Designations { get; set; }
            = new List<Designation>();
        public ICollection<DepartmentRoleMapping> DepartmentRoleMappings
        { get; set; } = new List<DepartmentRoleMapping>();
    }
}