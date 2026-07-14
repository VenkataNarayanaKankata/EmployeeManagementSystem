using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagementSystem.ViewModels
{
    public class JoiningReportViewModel
    {
        // Summary Cards
        public int TotalEmployees { get; set; }

        public int TodayJoining { get; set; }

        public int ThisMonthJoining { get; set; }

        public int ThisYearJoining { get; set; }

        public DateTime? EarliestJoiningDate { get; set; }

        public DateTime? LatestJoiningDate { get; set; }

        // Employee List
        public List<Employee> Employees { get; set; } = new();

        // Recent Employees
        public List<Employee> RecentEmployees { get; set; } = new();

        // Filters
        public JoiningReportFilterViewModel Filter { get; set; } = new();

        // Department Dropdown
        public List<SelectListItem> DepartmentList { get; set; } = new();
    }
}