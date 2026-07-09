using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Helpers
{
    public static class ActivityLogger
    {
        public static async Task LogAsync(
            ApplicationDbContext context,
            string? username,
            string action)
        {
            if (string.IsNullOrEmpty(username))
                username = "Unknown";

            var log = new ActivityLog
            {
                Username = username,
                Action = action,
                ActivityDate = DateTime.Now
            };

            context.ActivityLogs.Add(log);

            await context.SaveChangesAsync();
        }
    }
}