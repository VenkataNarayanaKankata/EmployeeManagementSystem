using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagementSystem.ViewModels
{
    public class EmployeeReportViewModel
    {
        // Summary Cards
        public int TotalEmployees { get; set; }

        public decimal AverageSalary { get; set; }

        public decimal HighestSalary { get; set; }

        public decimal LowestSalary { get; set; }

        // Employee List
        public List<Employee> Employees { get; set; } = new();

        // Filters
        public EmployeeReportFilterViewModel Filter { get; set; } = new();

        // Department Dropdown
        public List<SelectListItem> Departments { get; set; } = new();
    }
}