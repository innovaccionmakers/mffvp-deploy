namespace Security.Application.Contracts.Auth;

public interface IUserPermissionService
{
    Task<List<string>> GetPermissionsAsync(int userId);
}
