using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.ViewModels
{
    public class EditProfileViewModel
    {

        public int AdminId { get; set; }

        public int EmployeeId { get; set; }



        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;



        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;



        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;



        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;



        [Required]
        public string Gender { get; set; } = string.Empty;



        public string? ExistingPhoto { get; set; }



        public IFormFile? Photo { get; set; }

    }
}