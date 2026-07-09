using EmployeeManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class ActivityLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.ActivityLogs
                .OrderByDescending(x => x.ActivityDate)
                .ToListAsync();

            return View(logs);
        }
    }
}