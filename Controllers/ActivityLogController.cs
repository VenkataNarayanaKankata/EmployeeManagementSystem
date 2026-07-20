using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Helpers;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class ActivityLogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPermissionService _permissionService;


        public ActivityLogController(
            ApplicationDbContext context,
            IPermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }



        // ==========================
        // Activity Log Report
        // ==========================

        public async Task<IActionResult> Index(
            string? username,
            string? activity,
            DateTime? fromDate,
            DateTime? toDate,
            bool print = false)
        {
            if (!await _permissionService
       .HasPermissionAsync("ActivityLog.View"))
            {
                return Forbid();
            }

            var query = _context.ActivityLogs
                .AsQueryable();



            // Username Filter

            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(x =>
                    x.Username.Contains(username));
            }




            // Activity Filter

            if (!string.IsNullOrWhiteSpace(activity))
            {
                query = query.Where(x =>
                    x.Action.Contains(activity));
            }




            // From Date Filter

            if (fromDate.HasValue)
            {
                query = query.Where(x =>
                    x.ActivityDate >= fromDate.Value.Date);
            }




            // To Date Filter

            if (toDate.HasValue)
            {
                query = query.Where(x =>
                    x.ActivityDate <
                    toDate.Value.Date.AddDays(1));
            }





            var activities = await query
                .OrderByDescending(x => x.ActivityDate)
                .ToListAsync();





            var model = new ActivityReportViewModel
            {

                Activities = activities,


                Filter = new ActivityReportFilterViewModel
                {
                    Username = username,
                    Activity = activity,
                    FromDate = fromDate,
                    ToDate = toDate
                },


                ActivityTypes =
                    DropdownHelper.GetActivityTypes()

            };






            // Dashboard Cards

            ViewBag.TotalActivities =
                await _context.ActivityLogs
                .CountAsync();



            ViewBag.TodayActivities =
                await _context.ActivityLogs
                .CountAsync(x =>
                    x.ActivityDate.Date ==
                    DateTime.Today);



            ViewBag.TotalUsers =
                await _context.ActivityLogs
                .Select(x => x.Username)
                .Distinct()
                .CountAsync();



            ViewBag.Print = print;







            // AJAX loading

            if (Request.Headers["X-Requested-With"]
                == "XMLHttpRequest")
            {
                return PartialView(
                    "_ActivityReportContent",
                    model);
            }






            return View(model);

        }







        // ==========================
        // Activity Details
        // ==========================


        public async Task<IActionResult> Details(int id)
        {
            if (!await _permissionService
       .HasPermissionAsync("ActivityLog.Details"))
            {
                return Forbid();
            }
            var activity =
                await _context.ActivityLogs
                .FirstOrDefaultAsync(x =>
                    x.ActivityLogId == id);



            if (activity == null)
            {
                return NotFound();
            }



            return View(activity);

        }








        // ==========================
        // Print Report
        // ==========================


        public async Task<IActionResult> Print(
            string? username,
            string? activity,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (!await _permissionService
    .HasPermissionAsync("ActivityLog.Print"))
            {
                return Forbid();
            }

            var query =
                _context.ActivityLogs
                .AsQueryable();




            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(x =>
                    x.Username.Contains(username));
            }



            if (!string.IsNullOrWhiteSpace(activity))
            {
                query = query.Where(x =>
                    x.Action.Contains(activity));
            }




            if (fromDate.HasValue)
            {
                query = query.Where(x =>
                    x.ActivityDate >= fromDate.Value.Date);
            }




            if (toDate.HasValue)
            {
                query = query.Where(x =>
                    x.ActivityDate <
                    toDate.Value.Date.AddDays(1));
            }






            var model = new ActivityReportViewModel
            {

                Activities =
                    await query
                    .OrderByDescending(x =>
                        x.ActivityDate)
                    .ToListAsync(),



                ActivityTypes =
                    DropdownHelper.GetActivityTypes()


            };




            return View(model);

        }

    }
}