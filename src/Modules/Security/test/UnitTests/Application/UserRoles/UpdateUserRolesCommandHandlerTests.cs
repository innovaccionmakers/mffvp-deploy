using FluentAssertions;

using Moq;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.UserRoles;
using Security.Application.UserRoles;
using Security.Domain.Roles;
using Security.Domain.UserRoles;
using Security.Domain.Users;

using Microsoft.EntityFrameworkCore.Storage;

namespace Security.test.UnitTests.Application.UserRoles;

public class UpdateUserRolesCommandHandlerTests
{
    private readonly Mock<IUserRoleRepository> _repo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IDbContextTransaction> _tx = new();

    private UpdateUserRolesCommandHandler BuildHandler()
    {
        _uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_tx.Object);

        return new UpdateUserRolesCommandHandler(
            _repo.Object,
            _roleRepo.Object,
            _userRepo.Object,
            _uow.Object
        );
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_RoleIds_Is_Empty()
    {
        var handler = BuildHandler();
        var command = new UpdateUserRolesCommand(1, []);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserRole.EmptyList");
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_User_Not_Found()
    {
        _userRepo.Setup(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

        var handler = BuildHandler();
        var command = new UpdateUserRolesCommand(1, [100]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_Role_Not_Found()
    {
        var userId = 1;
        var roleId = 999;

        _userRepo.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repo.Setup(r => r.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<UserRole>());
        _roleRepo.Setup(r => r.ExistsAsync(roleId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = BuildHandler();
        var command = new UpdateUserRolesCommand(userId, [roleId]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Successfully_Sync_UserRoles()
    {
        var userId = 1;
        var existing = new List<UserRole>
        {
            UserRole.Create(100, userId).Value,
            UserRole.Create(200, userId).Value
        };

        var incoming = new List<int> { 200, 300 };

        _userRepo.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        _repo.Setup(r => r.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(existing);

        _roleRepo.Setup(r => r.ExistsAsync(300, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        var handler = BuildHandler();
        var command = new UpdateUserRolesCommand(userId, incoming);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Description.Should().Be("User roles successfully synchronized.");

        _repo.Verify(r => r.Insert(It.Is<UserRole>(u => u.RoleId == 300)), Times.Once);
        _repo.Verify(r => r.Delete(It.Is<UserRole>(u => u.RoleId == 100)), Times.Once);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
