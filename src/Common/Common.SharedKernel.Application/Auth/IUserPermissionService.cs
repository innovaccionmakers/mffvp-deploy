namespace Common.SharedKernel.Application.Auth;

public interface IUserPermissionService
{
    Task<List<string>> GetPermissionsAsync(int userId);
}