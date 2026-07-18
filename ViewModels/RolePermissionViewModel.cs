namespace EmployeeManagementSystem.ViewModels
{
    public class RolePermissionViewModel
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;


        public List<string> Modules { get; set; } = new();


        public List<string> SelectedModules { get; set; } = new();


        public List<PermissionItemViewModel> Permissions { get; set; }
            = new();
    }


    public class PermissionItemViewModel
    {
        public int PermissionId { get; set; }

        public string PermissionName { get; set; } = string.Empty;


        public string ModuleName { get; set; } = string.Empty;


        public bool IsSelected { get; set; }
    }
}