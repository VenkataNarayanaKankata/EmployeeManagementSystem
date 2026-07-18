using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        public int AdminId { get; set; }

        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Old Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}