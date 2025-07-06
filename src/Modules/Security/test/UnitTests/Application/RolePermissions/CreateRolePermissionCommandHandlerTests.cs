using FluentAssertions;

using Moq;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.RolePermissions;
using Security.Application.RolePermissions;
using Security.Domain.RolePermissions;

using System.Data.Common;

namespace Security.test.UnitTests.Application.RolePermissions;

public class CreateRolePermissionCommandHandlerTests
{
    private readonly Mock<IRolePermissionRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<DbTransaction> _tx = new();

    private CreateRolePermissionCommandHandler BuildHandler()
    {
        _unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_tx.Object);
        return new CreateRolePermissionCommandHandler(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Valid()
    {
        // Arrange
        var command = new CreateRolePermissionCommand(1, "fvp:test:view");

        _repository.Setup(r => r.ExistsAsync(1, "fvp:test:view", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = BuildHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repository.Verify(r => r.Insert(It.IsAny<RolePermission>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_Permission_Already_Exists()
    {
        // Arrange
        var command = new CreateRolePermissionCommand(1, "fvp:test:view");

        _repository.Setup(r => r.ExistsAsync(1, "fvp:test:view", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = BuildHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("RolePermission.Exists");
        _repository.Verify(r => r.Insert(It.IsAny<RolePermission>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
    }
}

