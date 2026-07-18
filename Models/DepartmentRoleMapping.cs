using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class DepartmentRoleMapping
    {
        public int DepartmentRoleMappingId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department? Department { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        public bool IsActive { get; set; } = true;
    }
}