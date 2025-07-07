using FluentAssertions;

using Moq;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.RolePermissions;
using Security.Application.RolePermissions;
using Security.Domain.RolePermissions;
using Security.Domain.Roles;

using System.Data.Common;

namespace Security.test.UnitTests.Application.RolePermissions;

public class CreateRolePermissionCommandHandlerTests
{
    private readonly Mock<IRolePermissionRepository> _rolePermissionRepository = new();
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<DbTransaction> _transaction = new();

    private CreateRolePermissionCommandHandler BuildHandler()
    {
        _unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_transaction.Object);

        return new CreateRolePermissionCommandHandler(
            _rolePermissionRepository.Object,
            _roleRepository.Object,
            _unitOfWork.Object
        );
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Valid()
    {
        // Arrange
        var command = new CreateRolePermissionCommand(1, "fvp:test:view");

        _roleRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        _rolePermissionRepository.Setup(r => r.ExistsAsync(1, "fvp:test:view", It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(false);

        var handler = BuildHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _rolePermissionRepository.Verify(r => r.Insert(It.IsAny<RolePermission>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_Permission_Already_Exists()
    {
        // Arrange
        var command = new CreateRolePermissionCommand(1, "fvp:test:view");

        _roleRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        _rolePermissionRepository.Setup(r => r.ExistsAsync(1, "fvp:test:view", It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(true);

        var handler = BuildHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("RolePermission.Exists");
        _rolePermissionRepository.Verify(r => r.Insert(It.IsAny<RolePermission>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_ScopePermission_Is_Empty()
    {
        // Arrange
        var command = new CreateRolePermissionCommand(1, "");

        var handler = BuildHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Permission.Required");
        _roleRepository.Verify(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_Role_Does_Not_Exist()
    {
        // Arrange
        var command = new CreateRolePermissionCommand(1, "fvp:test:view");

        _roleRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);

        var handler = BuildHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.NotFound");
        _rolePermissionRepository.Verify(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
