using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ActivityLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? username,
            string? activity,
            DateTime? fromDate,
            DateTime? toDate,
            bool print = false)
        {
            var query = _context.ActivityLogs.AsQueryable();

            // Username Filter
            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(a => a.Username.Contains(username));
            }

            // Activity Filter
            if (!string.IsNullOrWhiteSpace(activity))
            {
                query = query.Where(a => a.Action.Contains(activity));
            }

            // From Date Filter
            if (fromDate.HasValue)
            {
                query = query.Where(a => a.ActivityDate.Date >= fromDate.Value.Date);
            }

            // To Date Filter
            if (toDate.HasValue)
            {
                query = query.Where(a => a.ActivityDate.Date <= toDate.Value.Date);
            }

            var model = new ActivityReportViewModel
            {
                Activities = await query
                    .OrderByDescending(a => a.ActivityDate)
                    .ToListAsync(),

                Filter = new ActivityReportFilterViewModel
                {
                    Username = username,
                    Activity = activity,
                    FromDate = fromDate,
                    ToDate = toDate
                },

                ActivityTypes = DropdownHelper.GetActivityTypes()
            };

            ViewBag.Print = print;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ActivityReportContent", model);
            }

            return View(model);
        }

        public async Task<IActionResult> Print()
        {
            var model = new ActivityReportViewModel
            {
                Activities = await _context.ActivityLogs
                    .OrderByDescending(x => x.ActivityDate)
                    .ToListAsync(),

                ActivityTypes = DropdownHelper.GetActivityTypes()
            };

            return View(model);
        }
    }
}