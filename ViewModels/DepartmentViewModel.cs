using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.ViewModels
{
    public class DepartmentViewModel
    {
        public int DepartmentId { get; set; }

        [Required]
        [Display(Name = "Department Code")]
        [StringLength(20)]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Department Name")]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(250)]
        public string? Description { get; set; }

        [Display(Name = "Status")]
        public bool IsActive { get; set; } = true;

        // Statistics

        public int EmployeeCount { get; set; }

        public int ActiveEmployeeCount { get; set; }

        public int InactiveEmployeeCount { get; set; }
    }
}