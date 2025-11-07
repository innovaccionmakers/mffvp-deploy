using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.OperationTypes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Operations.Application.TrustOperations.Queries;
using Operations.Domain.OperationTypes;
using Operations.Domain.TrustOperations;
using Operations.Integrations.TrustOperations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Operations.test.UnitTests.Application.TrustOperations.Queries;

public class GetTrustOperationsByPortfolioProcessDateAndTypeQueryHandlerTests
{
    private readonly Mock<ITrustOperationRepository> trustOperationRepository = new();
    private readonly Mock<IOperationTypeRepository> operationTypeRepository = new();
    private readonly Mock<ILogger<GetTrustOperationsByPortfolioProcessDateAndTypeQueryHandler>> logger = new();

    private GetTrustOperationsByPortfolioProcessDateAndTypeQueryHandler CreateSut()
        => new(trustOperationRepository.Object, operationTypeRepository.Object, logger.Object);

    [Fact]
    public async Task Handle_ShouldReturnOperations_WhenRepositoryReturnsData()
    {
        // Arrange
        var operationType = CreateOperationType(77, "Rendimiento acumulado");
        operationTypeRepository
            .Setup(repo => repo.GetByIdAsync(operationType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operationType);

        var operations = new List<TrustOperation>
        {
            CreateTrustOperation(10, 1000m, 1, operationType.OperationTypeId, new DateTime(2024, 1, 15)),
            CreateTrustOperation(10, -250m, 2, operationType.OperationTypeId, new DateTime(2024, 1, 15))
        };

        trustOperationRepository
            .Setup(repo => repo.GetByPortfolioProcessDateAndOperationTypeAsync(10, new DateTime(2024, 1, 15), operationType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operations);

        var sut = CreateSut();
        var query = new GetTrustOperationsByPortfolioProcessDateAndTypeQuery(10, new DateTime(2024, 1, 15), operationType.OperationTypeId);

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().ContainEquivalentOf(new TrustOperationResponse(
            10,
            new DateTime(2024, 1, 15),
            new TrustOperationTypeSummary(operationType.OperationTypeId, operationType.Name),
            1000m));
        result.Value.Should().ContainEquivalentOf(new TrustOperationResponse(
            10,
            new DateTime(2024, 1, 15),
            new TrustOperationTypeSummary(operationType.OperationTypeId, operationType.Name),
            -250m));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyCollection_WhenNoOperationsFound()
    {
        // Arrange
        var operationType = CreateOperationType(99, "Otro");
        trustOperationRepository
            .Setup(repo => repo.GetByPortfolioProcessDateAndOperationTypeAsync(5, It.IsAny<DateTime>(), operationType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TrustOperation>());

        var sut = CreateSut();
        var query = new GetTrustOperationsByPortfolioProcessDateAndTypeQuery(5, new DateTime(2024, 5, 10), operationType.OperationTypeId);

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        operationTypeRepository.Verify(
            repo => repo.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnOperations_WhenOperationTypeIsNotFound()
    {
        // Arrange
        operationTypeRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OperationType?)null);

        var operations = new List<TrustOperation>
        {
            CreateTrustOperation(10, 1000m, 1, 77, new DateTime(2024, 1, 15))
        };

        trustOperationRepository
            .Setup(repo => repo.GetByPortfolioProcessDateAndOperationTypeAsync(10, new DateTime(2024, 1, 15), 77, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operations);

        var sut = CreateSut();
        var query = new GetTrustOperationsByPortfolioProcessDateAndTypeQuery(10, new DateTime(2024, 1, 15), 77);

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value.Should().ContainEquivalentOf(new TrustOperationResponse(
            10,
            new DateTime(2024, 1, 15),
            new TrustOperationTypeSummary(77, string.Empty),
            1000m));
    }

    private static OperationType CreateOperationType(long id, string name)
    {
        var json = System.Text.Json.JsonDocument.Parse("{}");
        return OperationType.Create(
            name,
            null,
            IncomeEgressNature.Income,
            Status.Active,
            string.Empty,
            true,
            json,
            string.Empty).Value;
    }

    private static TrustOperation CreateTrustOperation(
        int portfolioId,
        decimal amount,
        long trustId,
        long operationTypeId,
        DateTime processDate)
    {
        var result = TrustOperation.Create(
            1,
            trustId,
            amount,
            0m,
            operationTypeId,
            portfolioId,
            processDate,
            processDate,
            processDate);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
