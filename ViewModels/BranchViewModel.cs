using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.ViewModels
{
	public class BranchViewModel
	{
		public int BranchId { get; set; }

		[Required]
		[Display(Name = "Branch Code")]
		[StringLength(20)]
		public string BranchCode { get; set; } = string.Empty;

		[Required]
		[Display(Name = "Branch Name")]
		[StringLength(100)]
		public string BranchName { get; set; } = string.Empty;

		[Display(Name = "Address")]
		public string? Address { get; set; }

		[Display(Name = "City")]
		public string? City { get; set; }

		[Display(Name = "State")]
		public string? State { get; set; }

		[Display(Name = "Country")]
		public string? Country { get; set; }

		[Display(Name = "Pincode")]
		public string? Pincode { get; set; }

		[Display(Name = "Phone")]
		public string? Phone { get; set; }

		[EmailAddress]
		[Display(Name = "Email")]
		public string? Email { get; set; }

		[Display(Name = "Head Office")]
		public bool IsHeadOffice { get; set; }

		[Display(Name = "Active")]
		public bool IsActive { get; set; } = true;

		// Dashboard & Reports
		public int EmployeeCount { get; set; }

		public int ActiveEmployeeCount { get; set; }

		public int InactiveEmployeeCount { get; set; }
	}
}