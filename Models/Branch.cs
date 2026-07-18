using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class Branch
    {
        public int BranchId { get; set; }

        [Required]
        [StringLength(20)]
        public string BranchCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string BranchName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? Pincode { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsHeadOffice { get; set; }

        public ICollection<Employee> Employees { get; set; }
            = new List<Employee>();
    }
}