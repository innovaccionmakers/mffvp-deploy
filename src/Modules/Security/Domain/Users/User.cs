using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Security.Domain.UserPermissions;
using Security.Domain.UserRoles;

namespace Security.Domain.Users;

public sealed class User : Entity
{
    public int Id { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string MiddleName { get; private set; } = string.Empty;
    public string Identification { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<UserPermission> UserPermissions { get; private set; } = new List<UserPermission>();

    private User() { }

    public User(int id, string userName, string name, string middleName, string identification, string email, string displayName)
    {
        Id = id;
        UserName = userName;
        Name = name;
        MiddleName = middleName;
        Identification = identification;
        Email = email;
        DisplayName = displayName;
    }

    public static Result<User> Create(
        int id,
        string userName,
        string name,
        string middleName,
        string identification,
        string email,
        string displayName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return Result.Failure<User>(Error.Validation("User.UserName.Required", "Username is required."));

        var user = new User
        {
            Id = id,
            UserName = userName,
            Name = name,
            MiddleName = middleName,
            Identification = identification,
            Email = email,
            DisplayName = displayName
        };

        return Result.Success(user);
    }

}
