using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using FluentAssertions;
using Moq;
using Operations.Application.Abstractions.External;
using Operations.Application.ClientOperations.GetOperationsND;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;
using Operations.Domain.OperationTypes;
using Operations.Integrations.ClientOperations.GetOperationsND;

namespace Operations.test.UnitTests.Application.ClientOperations.GetOperationsND;

public class GetOperationsNDHandlerTests
{
    private static readonly Result<TrustInfoResult> SuccessfulTrustInfo =
        Result.Success(new TrustInfoResult(100));

    [Fact]
    public async Task Handle_ReturnsOrderedOperations_WhenAllConditionsAreMet()
    {
        // Arrange
        const int affiliateId = 100;
        const int objectiveId = 200;
        var contributionType = CreateOperationType(1, "Aporte", null);
        var categorizedTypes = new List<OperationType>
        {
            CreateOperationType(4, "Ninguno", 1),
            CreateOperationType(5, "Descuento nómina", 1),
            CreateOperationType(6, "Débito Automático", 1)
        };
        var operations = new List<ClientOperation>
        {
            CreateClientOperation(
                1,
                10,
                new DateTime(2024, 1, 10),
                500m,
                categorizedTypes[0].OperationTypeId,
                contingentWithholding: 10m),
            CreateClientOperation(
                2,
                10,
                new DateTime(2024, 1, 15),
                600m,
                categorizedTypes[1].OperationTypeId,
                contingentWithholding: 20m),
            CreateClientOperation(
                3,
                20,
                new DateTime(2024, 1, 12),
                700m,
                categorizedTypes[2].OperationTypeId,
                contingentWithholding: 30m)
        };

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetContributionOperationsInRangeAsync(
                It.Is<IReadOnlyCollection<long>>(ids => ids.SequenceEqual(categorizedTypes.Select(type => type.OperationTypeId))),
                affiliateId,
                objectiveId,
                It.Is<DateTime>(date => date == new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                It.Is<DateTime>(date => date == new DateTime(2024, 1, 31, 0, 0, 0, DateTimeKind.Utc)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(operations);

        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);

        operationTypeRepositoryMock
            .Setup(repository => repository.GetTypesByCategoryAsync((int?)contributionType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categorizedTypes);

        var portfolioLocatorMock = new Mock<IPortfolioLocator>();
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: 10L, Name: "Portafolio 10", CurrentDate: new DateTime(2024, 1, 31))));
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: 20L, Name: "Portafolio 20", CurrentDate: new DateTime(2024, 1, 30))));

        var trustInfoProviderMock = new Mock<ITrustInfoProvider>();
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(It.IsAny<long>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulTrustInfo);

        var handler = new GetOperationsNDHandler(
            clientOperationRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            trustInfoProviderMock.Object);

        var query = new GetOperationsNDQuery(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 31),
            affiliateId,
            objectiveId,
            1,
            2);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(2);
        result.Value.TotalCount.Should().Be(3);
        result.Value.TotalPages.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Select(item => item.ClientOperationId)
            .Should()
            .ContainInOrder(2, 3);
        result.Value.Items
            .Select(item => item.TransactionTypeName)
            .Should()
            .OnlyContain(name => name == contributionType.Name);
        result.Value.Items
            .Select(item => item.ContingentWithholding)
            .Should()
            .ContainInOrder(20m, 30m);

        portfolioLocatorMock.Verify(
            locator => locator.FindByPortfolioIdAsync(10, It.IsAny<CancellationToken>()),
            Times.Once);
        portfolioLocatorMock.Verify(
            locator => locator.FindByPortfolioIdAsync(20, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_IncludesOperations_WhenProcessDateEqualsPortfolioCurrentDate()
    {
        // Arrange
        const int affiliateId = 100;
        const int objectiveId = 200;
        var contributionType = CreateOperationType(1, "Aporte", null);
        var categorizedTypes = new List<OperationType>
        {
            CreateOperationType(4, "Ninguno", 1)
        };

        var operationProcessDate = new DateTime(2024, 1, 31, 23, 30, 0, DateTimeKind.Utc);
        var operation = CreateClientOperation(
            1,
            10,
            operationProcessDate,
            550m,
            categorizedTypes[0].OperationTypeId,
            contingentWithholding: 25m);

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetContributionOperationsInRangeAsync(
                It.Is<IReadOnlyCollection<long>>(ids => ids.SequenceEqual(categorizedTypes.Select(type => type.OperationTypeId))),
                affiliateId,
                objectiveId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClientOperation> { operation });

        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);

        operationTypeRepositoryMock
            .Setup(repository => repository.GetTypesByCategoryAsync((int?)contributionType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categorizedTypes);

        var portfolioLocatorMock = new Mock<IPortfolioLocator>();
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: 10L, Name: "Portafolio 10", CurrentDate: new DateTime(2024, 1, 31, 8, 15, 0, DateTimeKind.Utc))));

        var trustInfoProviderMock = new Mock<ITrustInfoProvider>();
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(operation.ClientOperationId, operation.Amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulTrustInfo);

        var handler = new GetOperationsNDHandler(
            clientOperationRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            trustInfoProviderMock.Object);

        var query = new GetOperationsNDQuery(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 31),
            affiliateId,
            objectiveId,
            1,
            10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.TotalPages.Should().Be(1);
        result.Value.Items.Should().ContainSingle();

        var item = result.Value.Items.Single();
        item.ClientOperationId.Should().Be(operation.ClientOperationId);
        item.ProcessDate.Should().Be(operationProcessDate);
        item.TransactionTypeName.Should().Be(contributionType.Name);
        item.Amount.Should().Be(operation.Amount);
        item.ContingentWithholding.Should().Be(25m);

        portfolioLocatorMock.Verify(
            locator => locator.FindByPortfolioIdAsync(10, It.IsAny<CancellationToken>()),
            Times.Once);
        trustInfoProviderMock.Verify(
            provider => provider.GetAsync(operation.ClientOperationId, operation.Amount, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_IgnoresTimeComponent_WhenComparingProcessDateWithPortfolioCurrentDate()
    {
        // Arrange
        const int affiliateId = 100;
        const int objectiveId = 200;
        var contributionType = CreateOperationType(1, "Aporte", null);
        var categorizedTypes = new List<OperationType>
        {
            CreateOperationType(4, "Ninguno", 1)
        };

        var eligibleProcessDate = new DateTime(2024, 1, 31, 23, 59, 0, DateTimeKind.Utc);
        var boundaryProcessDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        var eligibleOperation = CreateClientOperation(
            1,
            10,
            eligibleProcessDate,
            500m,
            categorizedTypes[0].OperationTypeId,
            contingentWithholding: 10m);

        var boundaryOperation = CreateClientOperation(
            2,
            10,
            boundaryProcessDate,
            600m,
            categorizedTypes[0].OperationTypeId,
            contingentWithholding: 20m);

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetContributionOperationsInRangeAsync(
                It.Is<IReadOnlyCollection<long>>(ids => ids.SequenceEqual(categorizedTypes.Select(type => type.OperationTypeId))),
                affiliateId,
                objectiveId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClientOperation> { eligibleOperation, boundaryOperation });

        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);

        operationTypeRepositoryMock
            .Setup(repository => repository.GetTypesByCategoryAsync((int?)contributionType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categorizedTypes);

        var portfolioLocatorMock = new Mock<IPortfolioLocator>();
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: 10L, Name: "Portafolio 10", CurrentDate: new DateTime(2024, 1, 31, 12, 0, 0, DateTimeKind.Utc))));

        var trustInfoProviderMock = new Mock<ITrustInfoProvider>();
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(eligibleOperation.ClientOperationId, eligibleOperation.Amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulTrustInfo);
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(boundaryOperation.ClientOperationId, boundaryOperation.Amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulTrustInfo);

        var handler = new GetOperationsNDHandler(
            clientOperationRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            trustInfoProviderMock.Object);

        var query = new GetOperationsNDQuery(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 2, 28),
            affiliateId,
            objectiveId,
            1,
            10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items.Should().ContainSingle();

        var item = result.Value.Items.Single();
        item.ClientOperationId.Should().Be(eligibleOperation.ClientOperationId);
        item.ProcessDate.Should().Be(eligibleProcessDate);
        item.TransactionTypeName.Should().Be(contributionType.Name);
        item.Amount.Should().Be(eligibleOperation.Amount);
        item.ContingentWithholding.Should().Be(10m);

        trustInfoProviderMock.Verify(
            provider => provider.GetAsync(eligibleOperation.ClientOperationId, eligibleOperation.Amount, It.IsAny<CancellationToken>()),
            Times.Once);
        trustInfoProviderMock.Verify(
            provider => provider.GetAsync(boundaryOperation.ClientOperationId, boundaryOperation.Amount, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyResult_WhenContributionTypeDoesNotExist()
    {
        // Arrange
        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>(MockBehavior.Strict);
        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
            .ReturnsAsync((OperationType?)null);

        var portfolioLocatorMock = new Mock<IPortfolioLocator>(MockBehavior.Strict);
        var trustInfoProviderMock = new Mock<ITrustInfoProvider>(MockBehavior.Strict);

        var handler = new GetOperationsNDHandler(
            clientOperationRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            trustInfoProviderMock.Object);

        var query = new GetOperationsNDQuery(
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow,
            100,
            200,
            1,
            10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);

        clientOperationRepositoryMock.VerifyNoOtherCalls();
        portfolioLocatorMock.VerifyNoOtherCalls();
        trustInfoProviderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsEmptyResult_WhenCategorizedContributionTypesDoNotExist()
    {
        // Arrange
        var contributionType = CreateOperationType(1, "Aporte", null);

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>(MockBehavior.Strict);
        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);
        operationTypeRepositoryMock
            .Setup(repository => repository.GetTypesByCategoryAsync((int?)contributionType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<OperationType>());

        var portfolioLocatorMock = new Mock<IPortfolioLocator>(MockBehavior.Strict);
        var trustInfoProviderMock = new Mock<ITrustInfoProvider>(MockBehavior.Strict);

        var handler = new GetOperationsNDHandler(
            clientOperationRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            trustInfoProviderMock.Object);

        var query = new GetOperationsNDQuery(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            100,
            200,
            2,
            5);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(5);

        clientOperationRepositoryMock.VerifyNoOtherCalls();
        portfolioLocatorMock.VerifyNoOtherCalls();
        trustInfoProviderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ExcludesOperations_WhenTrustValidationFailsOrProcessDateIsNotEligible()
    {
        // Arrange
        const int affiliateId = 100;
        const int objectiveId = 200;
        var contributionType = CreateOperationType(1, "Aporte", null);
        var categorizedTypes = new List<OperationType>
        {
            CreateOperationType(4, "Ninguno", 1),
            CreateOperationType(5, "Descuento nómina", 1)
        };
        var eligibleOperation = CreateClientOperation(1, 10, new DateTime(2024, 1, 10), 250m, categorizedTypes[0].OperationTypeId);
        var futureProcessDateOperation = CreateClientOperation(2, 10, new DateTime(2024, 2, 1), 300m, categorizedTypes[1].OperationTypeId);

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetContributionOperationsInRangeAsync(
                It.Is<IReadOnlyCollection<long>>(ids => ids.SequenceEqual(categorizedTypes.Select(type => type.OperationTypeId))),
                affiliateId,
                objectiveId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClientOperation> { eligibleOperation, futureProcessDateOperation });

        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);

        operationTypeRepositoryMock
            .Setup(repository => repository.GetTypesByCategoryAsync((int?)contributionType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categorizedTypes);

        var portfolioLocatorMock = new Mock<IPortfolioLocator>();
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: 10L, Name: "Portafolio 10", CurrentDate: new DateTime(2024, 1, 15))));

        var trustInfoProviderMock = new Mock<ITrustInfoProvider>();
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(eligibleOperation.ClientOperationId, eligibleOperation.Amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<TrustInfoResult>(Error.Validation("TRUST", "invalid")));
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(futureProcessDateOperation.ClientOperationId, futureProcessDateOperation.Amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulTrustInfo);

        var handler = new GetOperationsNDHandler(
            clientOperationRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            trustInfoProviderMock.Object);

        var query = new GetOperationsNDQuery(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 2, 15),
            affiliateId,
            objectiveId,
            1,
            10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyResult_WhenAffiliateOrObjectiveDoesNotMatch()
    {
        // Arrange
        const int affiliateId = 100;
        const int objectiveId = 200;
        var contributionType = CreateOperationType(1, "Aporte", null);
        var categorizedTypes = new List<OperationType>
        {
            CreateOperationType(4, "Ninguno", 1),
            CreateOperationType(5, "Descuento nómina", 1)
        };

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetContributionOperationsInRangeAsync(
                It.Is<IReadOnlyCollection<long>>(ids => ids.SequenceEqual(categorizedTypes.Select(type => type.OperationTypeId))),
                affiliateId,
                objectiveId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ClientOperation>());

        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contributionType);

        operationTypeRepositoryMock
            .Setup(repository => repository.GetTypesByCategoryAsync((int?)contributionType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categorizedTypes);

        var portfolioLocatorMock = new Mock<IPortfolioLocator>(MockBehavior.Strict);
        var trustInfoProviderMock = new Mock<ITrustInfoProvider>(MockBehavior.Strict);

        var handler = new GetOperationsNDHandler(
            clientOperationRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            trustInfoProviderMock.Object);

        var query = new GetOperationsNDQuery(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 31),
            affiliateId,
            objectiveId,
            3,
            25);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.PageNumber.Should().Be(3);
        result.Value.PageSize.Should().Be(25);

        portfolioLocatorMock.VerifyNoOtherCalls();
        trustInfoProviderMock.VerifyNoOtherCalls();
    }

    private static ClientOperation CreateClientOperation(
        long clientOperationId,
        int portfolioId,
        DateTime processDate,
        decimal amount,
        long operationTypeId,
        int affiliateId = 100,
        int objectiveId = 200,
        decimal contingentWithholding = 0m)
    {
        var operation = ClientOperation.Create(
            DateTime.UtcNow,
            affiliateId: affiliateId,
            objectiveId: objectiveId,
            portfolioId,
            amount,
            processDate,
            operationTypeId,
            DateTime.UtcNow,
            LifecycleStatus.Active).Value;

        typeof(ClientOperation)
            .GetProperty(nameof(ClientOperation.ClientOperationId), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(operation, clientOperationId);

        var auxiliaryInformation = AuxiliaryInformation.Create(
            clientOperationId,
            originId: 1,
            collectionMethodId: 1,
            paymentMethodId: 1,
            collectionAccount: "ACC",
            paymentMethodDetail: JsonDocumentFactory.Empty,
            certificationStatusId: 1,
            taxConditionId: 1,
            contingentWithholding: contingentWithholding,
            verifiableMedium: JsonDocumentFactory.Empty,
            collectionBank: 1,
            depositDate: DateTime.UtcNow,
            salesUser: "user",
            originModalityId: 1,
            cityId: 1,
            channelId: 1,
            userId: "user").Value;

        typeof(ClientOperation)
            .GetProperty(nameof(ClientOperation.AuxiliaryInformation), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(operation, auxiliaryInformation);

        return operation;
    }

    private static OperationType CreateOperationType(
        long operationTypeId,
        string name,
        int? categoryId)
    {
        var operationType = OperationType.Create(
            name,
            categoryId,
            IncomeEgressNature.Income,
            Status.Active,
            string.Empty,
            visible: true,
            additionalAttributes: JsonDocumentFactory.Empty,
            homologatedCode: "AP").Value;

        typeof(OperationType)
            .GetProperty(nameof(OperationType.OperationTypeId), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(operationType, operationTypeId);

        return operationType;
    }
}

internal static class JsonDocumentFactory
{
    public static System.Text.Json.JsonDocument Empty =>
        System.Text.Json.JsonDocument.Parse("{}");
}
