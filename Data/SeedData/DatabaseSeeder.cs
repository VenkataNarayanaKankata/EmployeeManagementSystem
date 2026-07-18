using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Data.SeedData
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            await context.Database.MigrateAsync();

            // Branches
            if (!await context.Branches.AnyAsync())
            {
                context.Branches.Add(new Branch
                {
                    BranchCode = "HYD",
                    BranchName = "Hyderabad Head Office",
                    City = "Hyderabad",
                    State = "Telangana",
                    Country = "India",
                    IsHeadOffice = true,
                    IsActive = true
                });

                await context.SaveChangesAsync();
            }

            // Departments
            if (!await context.Departments.AnyAsync())
            {
                context.Departments.AddRange(

                    new Department
                    {
                        DepartmentCode = "ADMIN",
                        DepartmentName = "Administration"
                    },

                    new Department
                    {
                        DepartmentCode = "HR",
                        DepartmentName = "Human Resources"
                    },

                    new Department
                    {
                        DepartmentCode = "IT",
                        DepartmentName = "Information Technology"
                    },

                    new Department
                    {
                        DepartmentCode = "FIN",
                        DepartmentName = "Finance"
                    },

                    new Department
                    {
                        DepartmentCode = "SALES",
                        DepartmentName = "Sales"
                    },

                    new Department
                    {
                        DepartmentCode = "MKT",
                        DepartmentName = "Marketing"
                    },

                    new Department
                    {
                        DepartmentCode = "OPS",
                        DepartmentName = "Operations"
                    },

                    new Department
                    {
                        DepartmentCode = "CS",
                        DepartmentName = "Customer Support"
                    }

                );

                await context.SaveChangesAsync();
            }

            // Roles
            if (!await context.Roles.AnyAsync())
            {
                context.Roles.AddRange(

                    new Role
                    {
                        RoleName = "Admin",
                        Description = "System Administrator"
                    },

                    new Role
                    {
                        RoleName = "HR",
                        Description = "Human Resource"
                    },

                    new Role
                    {
                        RoleName = "Manager",
                        Description = "Department Manager"
                    },

                    new Role
                    {
                        RoleName = "Employee",
                        Description = "Standard Employee"
                    }

                );

                await context.SaveChangesAsync();
                // Designations
                if (!await context.Designations.AnyAsync())
                {
                    var adminDept = await context.Departments
                        .FirstAsync(d => d.DepartmentCode == "ADMIN");

                    var hrDept = await context.Departments
                        .FirstAsync(d => d.DepartmentCode == "HR");

                    var itDept = await context.Departments
                        .FirstAsync(d => d.DepartmentCode == "IT");

                    var financeDept = await context.Departments
                        .FirstAsync(d => d.DepartmentCode == "FIN");

                    var salesDept = await context.Departments
                        .FirstAsync(d => d.DepartmentCode == "SALES");

                    var marketingDept = await context.Departments
                        .FirstAsync(d => d.DepartmentCode == "MKT");

                    var operationsDept = await context.Departments
                        .FirstAsync(d => d.DepartmentCode == "OPS");

                    var supportDept = await context.Departments
                        .FirstAsync(d => d.DepartmentCode == "CS");

                    context.Designations.AddRange(

                        // Administration
                        new Designation
                        {
                            DesignationName = "System Administrator",
                            DepartmentId = adminDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "Office Administrator",
                            DepartmentId = adminDept.DepartmentId
                        },

                        // HR
                        new Designation
                        {
                            DesignationName = "HR Executive",
                            DepartmentId = hrDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "Recruiter",
                            DepartmentId = hrDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "HR Manager",
                            DepartmentId = hrDept.DepartmentId
                        },

                        // IT
                        new Designation
                        {
                            DesignationName = "Software Developer",
                            DepartmentId = itDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "Senior Software Developer",
                            DepartmentId = itDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "QA Engineer",
                            DepartmentId = itDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "DevOps Engineer",
                            DepartmentId = itDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "Technical Lead",
                            DepartmentId = itDept.DepartmentId
                        },

                        // Finance
                        new Designation
                        {
                            DesignationName = "Accountant",
                            DepartmentId = financeDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "Finance Executive",
                            DepartmentId = financeDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "Finance Manager",
                            DepartmentId = financeDept.DepartmentId
                        },

                        // Sales
                        new Designation
                        {
                            DesignationName = "Sales Executive",
                            DepartmentId = salesDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "Sales Manager",
                            DepartmentId = salesDept.DepartmentId
                        },

                        // Marketing
                        new Designation
                        {
                            DesignationName = "Marketing Executive",
                            DepartmentId = marketingDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "Digital Marketing Specialist",
                            DepartmentId = marketingDept.DepartmentId
                        },

                        // Operations
                        new Designation
                        {
                            DesignationName = "Operations Executive",
                            DepartmentId = operationsDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "Operations Manager",
                            DepartmentId = operationsDept.DepartmentId
                        },

                        // Customer Support
                        new Designation
                        {
                            DesignationName = "Support Executive",
                            DepartmentId = supportDept.DepartmentId
                        },
                        new Designation
                        {
                            DesignationName = "Support Manager",
                            DepartmentId = supportDept.DepartmentId
                        }

                    );

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}