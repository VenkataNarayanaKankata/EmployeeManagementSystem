using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Branch> Branches { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Designation> Designations { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<DepartmentRoleMapping> DepartmentRoleMappings { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }

        // ===========================
        // TRANSACTION TABLES
        // ===========================

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Branch)
                .WithMany(b => b.Employees)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Designation>()
                .HasOne(d => d.Department)
                .WithMany(dep => dep.Designations)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Designation)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<DepartmentRoleMapping>()
                .HasOne(m => m.Department)
                .WithMany(d => d.DepartmentRoleMappings)
                .HasForeignKey(m => m.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DepartmentRoleMapping>()
                .HasOne(m => m.Role)
                .WithMany(r => r.DepartmentRoleMappings)
                .HasForeignKey(m => m.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Admin>()
                .HasOne(a => a.Employee)
                .WithOne(e => e.Admin)
                .HasForeignKey<Admin>(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Admin>()
    .HasOne(a => a.Role)
    .WithMany()
    .HasForeignKey(a => a.RoleId)
    .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.EmployeeCode)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Username)
                .IsUnique();

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.DepartmentCode)
                .IsUnique();

            modelBuilder.Entity<Branch>()
                .HasIndex(b => b.BranchCode)
                .IsUnique();
            modelBuilder.Entity<Designation>()
    .HasIndex(d => d.DesignationCode)
    .IsUnique();

            modelBuilder.Entity<RolePermission>()
    .HasOne(rp => rp.Role)
    .WithMany(r => r.RolePermissions)
    .HasForeignKey(rp => rp.RoleId)
    .OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}