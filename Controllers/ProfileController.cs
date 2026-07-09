using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            var model = new ProfileViewModel
            {
                Username = User.Identity?.Name ?? "",
                Role = "Administrator"
            };

            return View(model);
        }
    }
}