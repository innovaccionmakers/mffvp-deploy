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
using Operations.Application.ClientOperations.GetOperationsVoid;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;
using Operations.Domain.OperationTypes;
using Operations.Integrations.ClientOperations.GetOperationsVoid;
using Xunit;

namespace Operations.test.UnitTests.Application.ClientOperations.GetOperationsVoid;

public class GetOperationsVoidQueryHandlerTests
{
    private static readonly Result<TrustInfoResult> SuccessfulTrustInfo =
        Result.Success(new TrustInfoResult(100));

    [Fact]
    public async Task Handle_ReturnsOrderedOperations_WhenAllConditionsAreMet()
    {
        // Arrange
        const int affiliateId = 100;
        const int objectiveId = 200;
        var operationType = CreateOperationType(1, "Anulación", null);
        var categorizedTypes = new List<OperationType>
        {
            CreateOperationType(4, "Tipo A", 1),
            CreateOperationType(5, "Tipo B", 1),
            CreateOperationType(6, "Tipo C", 1)
        };

        var operations = new List<ClientOperation>
        {
            CreateClientOperation(
                1,
                10,
                new DateTime(2024, 1, 10, 5, 30, 0, DateTimeKind.Utc),
                500m,
                categorizedTypes[0].OperationTypeId,
                contingentWithholding: 10m),
            CreateClientOperation(
                2,
                10,
                new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc),
                600m,
                categorizedTypes[1].OperationTypeId,
                contingentWithholding: 20m),
            CreateClientOperation(
                3,
                20,
                new DateTime(2024, 1, 12, 12, 0, 0, DateTimeKind.Utc),
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
            .Setup(repository => repository.GetByIdAsync(operationType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operationType);

        operationTypeRepositoryMock
            .Setup(repository => repository.GetTypesByCategoryAsync((int?)operationType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categorizedTypes);

        var portfolioLocatorMock = new Mock<IPortfolioLocator>();
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: 10L, Name: "Portafolio 10", CurrentDate: new DateTime(2024, 1, 15, 7, 0, 0, DateTimeKind.Utc))));
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: 20L, Name: "Portafolio 20", CurrentDate: new DateTime(2024, 1, 12, 9, 0, 0, DateTimeKind.Utc))));

        var trustInfoProviderMock = new Mock<ITrustInfoProvider>();
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(It.IsAny<long>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulTrustInfo);

        var handler = new GetOperationsVoidQueryHandler(
            clientOperationRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            trustInfoProviderMock.Object);

        var query = new GetOperationsVoidQuery(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 31),
            affiliateId,
            objectiveId,
            operationType.OperationTypeId,
            1,
            2);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(1);
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Select(item => item.ClientOperationId)
            .Should()
            .ContainInOrder(2, 3);
        result.Value.Items
            .Select(item => item.TransactionTypeName)
            .Should()
            .OnlyContain(name => name == operationType.Name);
        result.Value.Items
            .Select(item => item.OperationTypeId)
            .Should()
            .Contain(operations[1].OperationTypeId);
    }

    [Fact]
    public async Task Handle_OnlyIncludesOperations_WhenProcessDateMatchesPortfolioDate()
    {
        // Arrange
        const int affiliateId = 100;
        const int objectiveId = 200;
        var operationType = CreateOperationType(1, "Anulación", null);
        var categorizedTypes = new List<OperationType>
        {
            CreateOperationType(4, "Tipo A", 1)
        };

        var matchingProcessDate = new DateTime(2024, 1, 31, 23, 0, 0, DateTimeKind.Utc);
        var nonMatchingProcessDate = new DateTime(2024, 2, 1, 1, 0, 0, DateTimeKind.Utc);

        var matchingOperation = CreateClientOperation(
            1,
            10,
            matchingProcessDate,
            550m,
            categorizedTypes[0].OperationTypeId,
            contingentWithholding: 25m);

        var nonMatchingOperation = CreateClientOperation(
            2,
            10,
            nonMatchingProcessDate,
            650m,
            categorizedTypes[0].OperationTypeId,
            contingentWithholding: 30m);

        var clientOperationRepositoryMock = new Mock<IClientOperationRepository>();
        clientOperationRepositoryMock
            .Setup(repository => repository.GetContributionOperationsInRangeAsync(
                It.Is<IReadOnlyCollection<long>>(ids => ids.SequenceEqual(categorizedTypes.Select(type => type.OperationTypeId))),
                affiliateId,
                objectiveId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClientOperation> { matchingOperation, nonMatchingOperation });

        var operationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
        operationTypeRepositoryMock
            .Setup(repository => repository.GetByIdAsync(operationType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operationType);
        operationTypeRepositoryMock
            .Setup(repository => repository.GetTypesByCategoryAsync((int?)operationType.OperationTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categorizedTypes);

        var portfolioLocatorMock = new Mock<IPortfolioLocator>();
        portfolioLocatorMock
            .Setup(locator => locator.FindByPortfolioIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((PortfolioId: 10L, Name: "Portafolio 10", CurrentDate: new DateTime(2024, 1, 31, 8, 0, 0, DateTimeKind.Utc))));

        var trustInfoProviderMock = new Mock<ITrustInfoProvider>();
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(matchingOperation.ClientOperationId, matchingOperation.Amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulTrustInfo);
        trustInfoProviderMock
            .Setup(provider => provider.GetAsync(nonMatchingOperation.ClientOperationId, nonMatchingOperation.Amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulTrustInfo);

        var handler = new GetOperationsVoidQueryHandler(
            clientOperationRepositoryMock.Object,
            operationTypeRepositoryMock.Object,
            portfolioLocatorMock.Object,
            trustInfoProviderMock.Object);

        var query = new GetOperationsVoidQuery(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 2, 15),
            affiliateId,
            objectiveId,
            operationType.OperationTypeId,
            1,
            10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items.Should().ContainSingle();
        var item = result.Value.Items.Single();
        item.ClientOperationId.Should().Be(matchingOperation.ClientOperationId);
        item.ProcessDate.Should().Be(matchingOperation.ProcessDate);
        item.TransactionTypeName.Should().Be(operationType.Name);
        item.OperationTypeId.Should().Be(matchingOperation.OperationTypeId);
        item.ContingentWithholding.Should().Be(25m);

        trustInfoProviderMock.Verify(
            provider => provider.GetAsync(nonMatchingOperation.ClientOperationId, nonMatchingOperation.Amount, It.IsAny<CancellationToken>()),
            Times.Never);
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
