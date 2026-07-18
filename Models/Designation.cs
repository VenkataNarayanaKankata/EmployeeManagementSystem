using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class Designation
    {
        public int DesignationId { get; set; }

        [Required]
        [StringLength(20)]
        public string DesignationCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DesignationName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department? Department { get; set; }

        public ICollection<Employee> Employees { get; set; }
            = new List<Employee>();
    }
}