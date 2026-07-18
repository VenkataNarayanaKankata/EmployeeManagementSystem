using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.ViewModels
{
    public class DesignationViewModel
    {
        public int DesignationId { get; set; }

        [Required]
        [Display(Name = "Designation Code")]
        public string DesignationCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Designation Name")]
        public string DesignationName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int EmployeeCount { get; set; }

        public int ActiveEmployeeCount { get; set; }

        public int InactiveEmployeeCount { get; set; }
    }
}