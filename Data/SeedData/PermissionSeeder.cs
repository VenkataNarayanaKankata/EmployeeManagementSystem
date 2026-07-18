using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data.SeedData
{
    public static class PermissionSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            var permissions = new List<Permission>
            {

                new Permission
                {
                    PermissionName = "Dashboard.View",
                    ModuleName = "Dashboard",
                    Description = "View dashboard"
                },

                new Permission
                {
                    PermissionName = "Dashboard.EmployeeStats",
                    ModuleName = "Dashboard",
                    Description = "View employee statistics"
                },

                new Permission
                {
                    PermissionName = "Dashboard.SalaryStats",
                    ModuleName = "Dashboard",
                    Description = "View salary statistics"
                },

                new Permission
                {
                    PermissionName = "Dashboard.OrganizationStats",
                    ModuleName = "Dashboard",
                    Description = "View organization statistics"
                },


                new Permission
                {
                    PermissionName = "Employee.View",
                    ModuleName = "Employee",
                    Description = "View employees"
                },

                new Permission
                {
                    PermissionName = "Employee.Create",
                    ModuleName = "Employee",
                    Description = "Create employee"
                },

                new Permission
                {
                    PermissionName = "Employee.Edit",
                    ModuleName = "Employee",
                    Description = "Edit employee"
                },

                new Permission
                {
                    PermissionName = "Employee.Delete",
                    ModuleName = "Employee",
                    Description = "Delete employee"
                },

                new Permission
                {
                    PermissionName = "Employee.Restore",
                    ModuleName = "Employee",
                    Description = "Restore deleted employee"
                },

                new Permission
                {
                    PermissionName = "Employee.Import",
                    ModuleName = "Employee",
                    Description = "Import employees from Excel"
                },

                new Permission
                {
                    PermissionName = "Employee.Export",
                    ModuleName = "Employee",
                    Description = "Export employee data"
                },

                new Permission
                {
                    PermissionName = "Employee.ViewProfile",
                    ModuleName = "Employee",
                    Description = "View employee profile"
                },

                new Permission
                {
                    PermissionName = "Employee.ChangeStatus",
                    ModuleName = "Employee",
                    Description = "Activate or deactivate employee"
                },

                new Permission
                {
                    PermissionName = "Employee.AssignRole",
                    ModuleName = "Employee",
                    Description = "Assign role to employee"
                },
                new Permission
{
    PermissionName = "Employee.ViewSalary",
    ModuleName = "Employee",
    Description = "View employee salary details"
},
                new Permission
{
    PermissionName = "Employee.ChangeSalary",
    ModuleName = "Employee",
    Description = "Change employee salary"
},
                new Permission
{
    PermissionName = "Report.Print",
    ModuleName = "Report",
    Description = "Print reports"
},


                new Permission
                {
                    PermissionName = "Branch.View",
                    ModuleName = "Branch",
                    Description = "View branches"
                },

                new Permission
                {
                    PermissionName = "Branch.Create",
                    ModuleName = "Branch",
                    Description = "Create branch"
                },

                new Permission
                {
                    PermissionName = "Branch.Edit",
                    ModuleName = "Branch",
                    Description = "Edit branch"
                },

                new Permission
                {
                    PermissionName = "Branch.Delete",
                    ModuleName = "Branch",
                    Description = "Delete branch"
                },

                new Permission
                {
                    PermissionName = "Branch.AssignManager",
                    ModuleName = "Branch",
                    Description = "Assign branch manager"
                },


                new Permission
                {
                    PermissionName = "Department.View",
                    ModuleName = "Department",
                    Description = "View departments"
                },

                new Permission
                {
                    PermissionName = "Department.Create",
                    ModuleName = "Department",
                    Description = "Create department"
                },

                new Permission
                {
                    PermissionName = "Department.Edit",
                    ModuleName = "Department",
                    Description = "Edit department"
                },

                new Permission
                {
                    PermissionName = "Department.Delete",
                    ModuleName = "Department",
                    Description = "Delete department"
                },

                new Permission
                {
                    PermissionName = "Department.AssignHead",
                    ModuleName = "Department",
                    Description = "Assign department head"
                },


                new Permission
                {
                    PermissionName = "Designation.View",
                    ModuleName = "Designation",
                    Description = "View designations"
                },

                new Permission
                {
                    PermissionName = "Designation.Create",
                    ModuleName = "Designation",
                    Description = "Create designation"
                },

                new Permission
                {
                    PermissionName = "Designation.Edit",
                    ModuleName = "Designation",
                    Description = "Edit designation"
                },

                new Permission
                {
                    PermissionName = "Designation.Delete",
                    ModuleName = "Designation",
                    Description = "Delete designation"
                },


                new Permission
                {
                    PermissionName = "Role.View",
                    ModuleName = "Role",
                    Description = "View roles"
                },

                new Permission
                {
                    PermissionName = "Role.Create",
                    ModuleName = "Role",
                    Description = "Create role"
                },

                new Permission
                {
                    PermissionName = "Role.Edit",
                    ModuleName = "Role",
                    Description = "Edit role"
                },

                new Permission
                {
                    PermissionName = "Role.Delete",
                    ModuleName = "Role",
                    Description = "Delete role"
                },

                new Permission
                {
                    PermissionName = "Role.AssignPermission",
                    ModuleName = "Role",
                    Description = "Assign permissions to roles"
                },


                new Permission
                {
                    PermissionName = "User.View",
                    ModuleName = "User",
                    Description = "View users"
                },

                new Permission
                {
                    PermissionName = "User.Create",
                    ModuleName = "User",
                    Description = "Create user"
                },

                new Permission
                {
                    PermissionName = "User.Edit",
                    ModuleName = "User",
                    Description = "Edit user"
                },

                new Permission
                {
                    PermissionName = "User.Delete",
                    ModuleName = "User",
                    Description = "Delete user"
                },

                new Permission
                {
                    PermissionName = "User.ResetPassword",
                    ModuleName = "User",
                    Description = "Reset user password"
                },

                new Permission
                {
                    PermissionName = "User.ChangeStatus",
                    ModuleName = "User",
                    Description = "Change user status"
                },


                new Permission
                {
                    PermissionName = "Report.View",
                    ModuleName = "Report",
                    Description = "View reports"
                },

                new Permission
                {
                    PermissionName = "Report.Employee",
                    ModuleName = "Report",
                    Description = "Employee reports"
                },

                new Permission
                {
                    PermissionName = "Report.Department",
                    ModuleName = "Report",
                    Description = "Department reports"
                },

                new Permission
                {
                    PermissionName = "Report.Salary",
                    ModuleName = "Report",
                    Description = "Salary reports"
                },

                new Permission
                {
                    PermissionName = "Report.Export",
                    ModuleName = "Report",
                    Description = "Export reports"
                },


                new Permission
                {
                    PermissionName = "ActivityLog.View",
                    ModuleName = "Activity Log",
                    Description = "View activity logs"
                },

                new Permission
                {
                    PermissionName = "ActivityLog.Export",
                    ModuleName = "Activity Log",
                    Description = "Export activity logs"
                },


                new Permission
                {
                    PermissionName = "Profile.View",
                    ModuleName = "Profile",
                    Description = "View profile"
                },

                new Permission
                {
                    PermissionName = "Profile.Edit",
                    ModuleName = "Profile",
                    Description = "Edit profile"
                },

                new Permission
                {
                    PermissionName = "Profile.ChangePassword",
                    ModuleName = "Profile",
                    Description = "Change password"
                }


            };



            foreach (var permission in permissions)
            {
                bool exists = context.Permissions
                    .Any(p => p.PermissionName == permission.PermissionName);


                if (!exists)
                {
                    context.Permissions.Add(permission);
                }
            }


            context.SaveChanges();
        }
    }
}