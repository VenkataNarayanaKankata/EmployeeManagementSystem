using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class Permission
    {
        public int PermissionId { get; set; }


        [Required]
        [StringLength(100)]
        public string PermissionName { get; set; } = string.Empty;


        [StringLength(250)]
        public string? Description { get; set; }


        public string ModuleName { get; set; } = string.Empty;


        public bool IsActive { get; set; } = true;


        public ICollection<RolePermission> RolePermissions { get; set; }
            = new List<RolePermission>();
    }
}