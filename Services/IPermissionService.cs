namespace EmployeeManagementSystem.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string permissionName);
    }
}