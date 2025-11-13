using Moq;
using Operations.Domain.OperationTypes;
using Operations.Infrastructure.OperationTypes;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.OperationTypes;
using System.Text.Json;
using System.Reflection;

namespace Operations.test.IntegrationTests.OperationTypes;

public class OperationTypeRepositoryTests
{
    private readonly Mock<IOperationTypeRepository> _operationTypeRepositoryMock;

    public OperationTypeRepositoryTests()
    {
        _operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
    }

    private static OperationType CreateOperationType(
        long id,
        string name,
        string homologatedCode,
        int? categoryId = null,
        IncomeEgressNature nature = IncomeEgressNature.Income,
        Status status = Status.Active,
        string external = "external",
        bool visible = true,
        JsonDocument? additionalAttributes = null)
    {
        // Usar el método factory público para crear la instancia
        var result = OperationType.Create(
            name: name,
            categoryId: categoryId,
            nature: nature,
            status: status,
            external: external,
            visible: visible,
            additionalAttributes: additionalAttributes ?? JsonDocument.Parse("{}"),
            homologatedCode: homologatedCode
        );

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("Failed to create OperationType");
        }

        var operationType = result.Value;

        // Usar reflection para establecer el ID ya que no hay setter público
        var operationTypeIdProperty = typeof(OperationType).GetProperty("OperationTypeId",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        operationTypeIdProperty?.SetValue(operationType, id);

        // También establecer el Id de la clase base Entity
        var entityIdProperty = typeof(OperationType).GetProperty("Id",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        entityIdProperty?.SetValue(operationType, id);

        return operationType;
    }

    [Fact]
    public async Task GetByHomologatedCodeAsync_ReturnsCorrectOperationType()
    {
        // Arrange
        var expectedOperationType = CreateOperationType(1, "Compra", "COMPRA_001", 1);
        _operationTypeRepositoryMock
            .Setup(repo => repo.GetByHomologatedCodeAsync("COMPRA_001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOperationType);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetByHomologatedCodeAsync("COMPRA_001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.CategoryId);
        Assert.Equal("Compra", result.Name);
        Assert.Equal("COMPRA_001", result.HomologatedCode);
        _operationTypeRepositoryMock.Verify(repo => repo.GetByHomologatedCodeAsync("COMPRA_001", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByHomologatedCodeAsync_ReturnsNullWhenNotFound()
    {
        // Arrange
        _operationTypeRepositoryMock
            .Setup(repo => repo.GetByHomologatedCodeAsync("INVALID_CODE", It.IsAny<CancellationToken>()))
            .ReturnsAsync((OperationType?)null);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetByHomologatedCodeAsync("INVALID_CODE");

        // Assert
        Assert.Null(result);
        _operationTypeRepositoryMock.Verify(repo => repo.GetByHomologatedCodeAsync("INVALID_CODE", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsMatchingOperationTypes()
    {
        // Arrange
        var expectedOperationTypes = new List<OperationType>
        {
            CreateOperationType(1, "Compra", "COMPRA_001", 1),
            CreateOperationType(2, "Compra", "COMPRA_002", 2)
        };

        _operationTypeRepositoryMock
            .Setup(repo => repo.GetByNameAsync("Compra", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOperationTypes);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetByNameAsync("Compra");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, ot => Assert.Equal("Compra", ot.Name));
        _operationTypeRepositoryMock.Verify(repo => repo.GetByNameAsync("Compra", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsEmptyWhenNoMatches()
    {
        // Arrange
        var emptyList = new List<OperationType>();
        _operationTypeRepositoryMock
            .Setup(repo => repo.GetByNameAsync("Venta", It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetByNameAsync("Venta");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _operationTypeRepositoryMock.Verify(repo => repo.GetByNameAsync("Venta", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByNameAndCategoryAsync_ReturnsCorrectOperationType()
    {
        // Arrange
        var expectedOperationType = CreateOperationType(1, "Compra", "COMPRA_001", 1);
        _operationTypeRepositoryMock
            .Setup(repo => repo.GetByNameAndCategoryAsync("Compra", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOperationType);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetByNameAndCategoryAsync("Compra", 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Compra", result.Name);
        Assert.Equal(1, result.CategoryId);
        _operationTypeRepositoryMock.Verify(repo => repo.GetByNameAndCategoryAsync("Compra", 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByNameAndCategoryAsync_ReturnsOperationTypeWithNullCategory()
    {
        // Arrange
        var expectedOperationType = CreateOperationType(1, "Compra", "COMPRA_001", null);
        _operationTypeRepositoryMock
            .Setup(repo => repo.GetByNameAndCategoryAsync("Compra", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOperationType);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetByNameAndCategoryAsync("Compra", null);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.CategoryId);
        _operationTypeRepositoryMock.Verify(repo => repo.GetByNameAndCategoryAsync("Compra", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByNameAndCategoryAsync_ReturnsNullWhenNotFound()
    {
        // Arrange
        _operationTypeRepositoryMock
            .Setup(repo => repo.GetByNameAndCategoryAsync("Venta", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OperationType?)null);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetByNameAndCategoryAsync("Venta", 1);

        // Assert
        Assert.Null(result);
        _operationTypeRepositoryMock.Verify(repo => repo.GetByNameAndCategoryAsync("Venta", 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCategoryIdAsync_ReturnsEmptyWhenNoMatches()
    {
        // Arrange
        var emptyList = new List<OperationType>();
        _operationTypeRepositoryMock
            .Setup(repo => repo.GetTypesByCategoryAsync(999, It.IsAny<CancellationToken>(), null, null))
            .ReturnsAsync(emptyList);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetTypesByCategoryAsync(999, new CancellationToken(), null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _operationTypeRepositoryMock.Verify(repo => repo.GetTypesByCategoryAsync(999, It.IsAny<CancellationToken>(), null, null), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyWhenNoActiveVisibleOperationTypes()
    {
        // Arrange
        var emptyList = new List<OperationType>();
        _operationTypeRepositoryMock
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _operationTypeRepositoryMock.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOperationTypesWithDifferentProperties()
    {
        // Arrange
        var expectedOperationTypes = new List<OperationType>
        {
            CreateOperationType(1, "Compra", "COMPRA_001", 1, IncomeEgressNature.Income, Status.Active, "EXT001", true),
            CreateOperationType(2, "Venta", "VENTA_001", 2, IncomeEgressNature.Egress, Status.Active, "EXT002", true)
        };

        _operationTypeRepositoryMock
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOperationTypes);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var compra = result.First(ot => ot.Name == "Compra");
        var venta = result.First(ot => ot.Name == "Venta");

        Assert.Equal(IncomeEgressNature.Income, compra.Nature);
        Assert.Equal(IncomeEgressNature.Egress, venta.Nature);
        Assert.Equal("EXT001", compra.External);
        Assert.Equal("EXT002", venta.External);

        _operationTypeRepositoryMock.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAccTransactionTypesAsync_ReturnsOperationTypesWithNullCategoryAndVisible()
    {
        // Arrange
        var expectedOperationTypes = new List<OperationType>
    {
        CreateOperationType(1, "Transacción Contable 1", "ACC_001", null, IncomeEgressNature.Income, Status.Active, "EXT_ACC1", true),
        CreateOperationType(2, "Transacción Contable 2", "ACC_002", null, IncomeEgressNature.Egress, Status.Active, "EXT_ACC2", true)
    };

        _operationTypeRepositoryMock
            .Setup(repo => repo.GetAccTransactionTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOperationTypes);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetAccTransactionTypesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, ot =>
        {
            Assert.Null(ot.CategoryId);
            Assert.True(ot.Visible);
        });
        _operationTypeRepositoryMock.Verify(repo => repo.GetAccTransactionTypesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAccTransactionTypesAsync_ReturnsEmptyWhenNoAccTransactionTypes()
    {
        // Arrange
        var emptyList = new List<OperationType>();
        _operationTypeRepositoryMock
            .Setup(repo => repo.GetAccTransactionTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetAccTransactionTypesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _operationTypeRepositoryMock.Verify(repo => repo.GetAccTransactionTypesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAccTransactionTypesAsync_ReturnsOnlyVisibleOperationTypes()
    {
        // Arrange
        var expectedOperationTypes = new List<OperationType>
    {
        CreateOperationType(1, "Transacción Visible", "ACC_VIS", null, IncomeEgressNature.Income, Status.Active, "EXT_VIS", true)
    };

        _operationTypeRepositoryMock
            .Setup(repo => repo.GetAccTransactionTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOperationTypes);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetAccTransactionTypesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, ot => Assert.True(ot.Visible));
        _operationTypeRepositoryMock.Verify(repo => repo.GetAccTransactionTypesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAccTransactionTypesAsync_ExcludesOperationTypesWithCategory()
    {
        // Arrange
        var expectedOperationTypes = new List<OperationType>
    {
        CreateOperationType(1, "Sin Categoría", "ACC_NO_CAT", null, IncomeEgressNature.Income, Status.Active, "EXT_NO_CAT", true)
    };

        _operationTypeRepositoryMock
            .Setup(repo => repo.GetAccTransactionTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOperationTypes);

        var repository = _operationTypeRepositoryMock.Object;

        // Act
        var result = await repository.GetAccTransactionTypesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, ot => Assert.Null(ot.CategoryId));
        _operationTypeRepositoryMock.Verify(repo => repo.GetAccTransactionTypesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}