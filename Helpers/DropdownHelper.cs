using EmployeeManagementSystem.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagementSystem.Helpers
{
    public static class DropdownHelper
    {
        public static List<SelectListItem> GetActivityTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "All Activities",
                    Value = ""
                },

                new SelectListItem
                {
                    Text = ActivityTypes.Login,
                    Value = ActivityTypes.Login
                },

                new SelectListItem
                {
                    Text = ActivityTypes.Logout,
                    Value = ActivityTypes.Logout
                },

                new SelectListItem
                {
                    Text = ActivityTypes.EmployeeAdded,
                    Value = ActivityTypes.EmployeeAdded
                },

                new SelectListItem
                {
                    Text = ActivityTypes.EmployeeUpdated,
                    Value = ActivityTypes.EmployeeUpdated
                },

                new SelectListItem
                {
                    Text = ActivityTypes.EmployeeDeleted,
                    Value = ActivityTypes.EmployeeDeleted
                },

                new SelectListItem
                {
                    Text = ActivityTypes.DepartmentAdded,
                    Value = ActivityTypes.DepartmentAdded
                },

                new SelectListItem
                {
                    Text = ActivityTypes.DepartmentUpdated,
                    Value = ActivityTypes.DepartmentUpdated
                },

                new SelectListItem
                {
                    Text = ActivityTypes.DepartmentDeleted,
                    Value = ActivityTypes.DepartmentDeleted
                },

                new SelectListItem
                {
                    Text = ActivityTypes.EmployeeImported,
                    Value = ActivityTypes.EmployeeImported
                }
            };
        }
    }
}