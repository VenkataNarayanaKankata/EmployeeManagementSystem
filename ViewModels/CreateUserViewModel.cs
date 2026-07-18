using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;


        [Required]
        public int RoleId { get; set; }


        public bool IsActive { get; set; } = true;


        public int? EmployeeId { get; set; }


        public IEnumerable<SelectListItem>? Employees { get; set; }


        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}