using Common.SharedKernel.Domain;

using Security.Domain.Users;

namespace Security.Domain.UserPermissions;

public sealed class UserPermission : Entity
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int PermitToken { get; private set; }
    public bool Granted { get; private set; }

    public User User { get; private set; } = null!;

    private UserPermission() { }

    public UserPermission(int userId, int permitToken, bool granted)
    {
        UserId = userId;
        PermitToken = permitToken;
        Granted = granted;
    }
}
