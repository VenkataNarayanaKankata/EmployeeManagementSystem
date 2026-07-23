using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data.SeedData
{
    public static class RolePermissionSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {


            var roles = context.Roles.ToList();

            var permissions = context.Permissions.ToList();



            foreach (var role in roles)
            {

                List<string> permissionNames = new();



                switch (role.RoleName)
                {

                    case "Super Admin":

                        permissionNames =
                            permissions
                            .Select(p => p.PermissionName)
                            .ToList();

                        break;



                    case "Admin":

                        permissionNames =
                            permissions
                            .Where(p =>
                                p.ModuleName != "Profile" ||
                                p.PermissionName != "Profile.ChangePassword")
                            .Select(p => p.PermissionName)
                            .ToList();

                        break;



                    case "HR":

                        permissionNames = new List<string>
                        {
                            "Dashboard.View",

                            "Employee.View",
                            "Employee.Create",
                            "Employee.Edit",
                            "Employee.Import",
                            "Employee.Export",
                            "Employee.ViewProfile",
                            "Employee.ViewSalary",

                            "Report.View",
                            "Report.Employee",
                            "Report.Export",

                            "Profile.View",
                            "Profile.Edit",
                            "Profile.ChangePassword"
                        };

                        break;



                    case "Branch Manager":

                        permissionNames = new List<string>
                        {
                            "Dashboard.View",

                            "Employee.View",
                            "Employee.ViewProfile",

                            "Branch.View",

                            "Department.View",

                            "Report.View",
                            "Report.Employee",

                            "Profile.View",
"Profile.Edit",
"Profile.ChangePassword"
                        };

                        break;



                    case "Department Manager":

                        permissionNames = new List<string>
                        {
                            "Dashboard.View",

                            "Employee.View",
                            "Employee.ViewProfile",

                            "Department.View",

                            "Report.View",
"Profile.View",
"Profile.Edit",
"Profile.ChangePassword",
                        };

                        break;



                    case "Employee":

                        permissionNames = new List<string>
                        {
                            "Dashboard.View",

                            "Employee.ViewProfile",

                            "Profile.View",
                            "Profile.Edit",
                            "Profile.ChangePassword"
                        };

                        break;

                }





                foreach (var permissionName in permissionNames)
                {

                    var permission =
                        permissions
                        .FirstOrDefault(p =>
                            p.PermissionName == permissionName);



                    if (permission != null)
                    {

                        bool exists =
                            context.RolePermissions
                            .Any(rp =>
                                rp.RoleId == role.RoleId &&
                                rp.PermissionId == permission.PermissionId);



                        if (!exists)
                        {

                            context.RolePermissions.Add(
                                new RolePermission
                                {
                                    RoleId = role.RoleId,

                                    PermissionId =
                                        permission.PermissionId,

                                    IsAllowed = true
                                });

                        }

                    }

                }

            }



            context.SaveChanges();

        }
    }
}