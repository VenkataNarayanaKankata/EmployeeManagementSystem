using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class ActivityLog
    {
        [Key]
        public int ActivityLogId { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Action { get; set; } = string.Empty;

        public DateTime ActivityDate { get; set; } = DateTime.Now;
    }
}