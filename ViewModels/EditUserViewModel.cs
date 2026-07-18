using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.ViewModels
{
    public class EditUserViewModel
    {
        public int AdminId { get; set; }


        [Required]
        public string Username { get; set; } = string.Empty;


        [Required]
        public int RoleId { get; set; }


        public bool IsActive { get; set; }


        public DateTime? LastLogin { get; set; }


        public int? EmployeeId { get; set; }


        public IEnumerable<SelectListItem>? Employees { get; set; }


        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}