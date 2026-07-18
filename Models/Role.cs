using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Property
        public ICollection<Employee> Employees { get; set; }
            = new List<Employee>();
        public ICollection<DepartmentRoleMapping> DepartmentRoleMappings
        { get; set; } = new List<DepartmentRoleMapping>();
        public ICollection<RolePermission> RolePermissions
        {
            get; set;
        } = new List<RolePermission>();
    }
}