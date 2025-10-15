using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Associate.Application.Abstractions;
using Associate.Application.Balances.GetBalancesByObjective;
using Associate.Domain.Activates;
using Associate.Domain.ConfigurationParameters;
using Associate.Integrations.Balances.GetBalancesByObjective;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Customers.IntegrationEvents.PersonInformation;
using Moq;
using Products.IntegrationEvents.AdditionalInformation;
using Products.IntegrationEvents.EntityValidation;
using Products.Integrations.AdditionalInformation;
using RulesEngine.Models;
using Trusts.IntegrationEvents.GetBalances;
using Trusts.Integrations.Trusts.GetBalances;

namespace Associate.test.UnitTests.Application.Balances;

public class GetBalancesByObjectiveHandlerTests
{
    private const string PaginationWorkflow = "Associate.BalancesByObjective.PaginationValidation";
    private const string ValidationWorkflow = "Associate.BalancesByObjective.Validation";

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenBalancesAndAdditionalInformationAreValid()
    {
        // Arrange
        var context = CreateHandlerContext();
        var documentTypeCode = "CC";
        var documentTypeUuid = Guid.NewGuid();
        var configurationParameter = CreateConfigurationParameter(documentTypeCode, documentTypeUuid);
        var activation = CreateActivate(configurationParameter.Uuid, "123456789", 712);

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                It.Is<string>(w => w == PaginationWorkflow),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                It.Is<string>(w => w == ValidationWorkflow),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.ConfigurationRepository.Setup(r => r.GetByCodeAndScopeAsync(
                documentTypeCode,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configurationParameter);

        context.ActivateRepository.Setup(r => r.GetByIdTypeAndNumber(
                configurationParameter.Uuid,
                activation.Identification,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(activation);

        context.RpcClient.Setup(r => r.CallAsync<GetPersonInformationRequest, GetPersonInformationResponse>(
                It.Is<GetPersonInformationRequest>(req =>
                    req.DocumentType == documentTypeCode &&
                    req.Identification == activation.Identification),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPersonInformationResponse(true, null, null, CreatePersonInformation("John Doe")));

        context.RpcClient.Setup(r => r.CallAsync<ValidateEntityRequest, ValidateEntityResponse>(
                It.Is<ValidateEntityRequest>(req => req.Entity == "KIT"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidateEntityResponse(true, null, null));

        var balances = new[]
        {
            new BalanceResponse(1, 10, 150.45m, 50.11m),
            new BalanceResponse(1, 11, 25.20m, 10.05m),
            new BalanceResponse(2, 20, 0m, 0m)
        };

        context.RpcClient.Setup(r => r.CallAsync<GetBalancesRequest, GetBalancesResponse>(
                It.Is<GetBalancesRequest>(req => req.AffiliateId == activation.ActivateId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetBalancesResponse(true, null, null, balances));

        var expectedPairs = new[] { (1, 10), (1, 11) };
        var additionalInformationItems = new[]
        {
            new AdditionalInformationItem(10, "Portfolio Ten", 1, "Objective One", 101, "Alternative A", 201, "Fund Alpha", "PC10", "OC1", "ALT1", "FC1"),
            new AdditionalInformationItem(11, "Portfolio Eleven", 1, "Objective One B", 102, "Alternative B", 202, "Fund Beta", "PC11", "OC1B", "ALT2", "FC2")
        };

        context.RpcClient.Setup(r => r.CallAsync<GetAdditionalInformationRequest, GetAdditionalInformationResponse>(
                It.Is<GetAdditionalInformationRequest>(req =>
                    req.AffiliateId == activation.ActivateId &&
                    req.Pairs.SequenceEqual(expectedPairs)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAdditionalInformationResponse(true, null, null, additionalInformationItems));

        var handler = context.Handler;
        var query = new GetBalancesByObjectiveQuery("KIT", documentTypeCode, activation.Identification, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);

        var item = result.Value.Items.First();
        Assert.Equal("KIT", item.Entity);
        Assert.Equal("FC1", item.FundCode);
        Assert.Equal("PC10", item.ParticipationCode);
        Assert.Equal("Fund Alpha", item.FundName);
        Assert.Equal("Objective One", item.ParticipationName);
        Assert.Equal(1, item.ProductNumber);
        Assert.Equal(activation.Identification, item.AffiliateDocument);
        Assert.Equal("John Doe", item.HolderName);
        Assert.Equal(60.16m, item.AvailableBalance);
        Assert.Equal(175.65m, item.TotalBalance);

        Assert.Equal(1, result.Value.PageInfo.TotalRecords);
        Assert.Equal(1, result.Value.PageInfo.TotalPages);
        Assert.Equal(10, result.Value.PageInfo.RecordsPerPage);

        context.ConfigurationRepository.Verify(r => r.GetByCodeAndScopeAsync(
            documentTypeCode,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);

        context.ActivateRepository.Verify(r => r.GetByIdTypeAndNumber(
            configurationParameter.Uuid,
            activation.Identification,
            It.IsAny<CancellationToken>()), Times.Once);

        context.RuleEvaluator.Verify(r => r.EvaluateAsync(
            PaginationWorkflow,
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        context.RuleEvaluator.Verify(r => r.EvaluateAsync(
            ValidationWorkflow,
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Once);

        context.RpcClient.Verify(r => r.CallAsync<GetPersonInformationRequest, GetPersonInformationResponse>(
            It.IsAny<GetPersonInformationRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);

        context.RpcClient.Verify(r => r.CallAsync<ValidateEntityRequest, ValidateEntityResponse>(
            It.IsAny<ValidateEntityRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);

        context.RpcClient.Verify(r => r.CallAsync<GetBalancesRequest, GetBalancesResponse>(
            It.IsAny<GetBalancesRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);

        context.RpcClient.Verify(r => r.CallAsync<GetAdditionalInformationRequest, GetAdditionalInformationResponse>(
            It.IsAny<GetAdditionalInformationRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenNoBalancesAreAvailable()
    {
        // Arrange
        var context = CreateHandlerContext();
        var documentTypeCode = "CC";
        var configurationParameter = CreateConfigurationParameter(documentTypeCode, Guid.NewGuid());
        var activation = CreateActivate(configurationParameter.Uuid, "123456789", 912);

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                It.Is<string>(w => w == PaginationWorkflow),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                It.Is<string>(w => w == ValidationWorkflow),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.ConfigurationRepository.Setup(r => r.GetByCodeAndScopeAsync(
                documentTypeCode,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configurationParameter);

        context.ActivateRepository.Setup(r => r.GetByIdTypeAndNumber(
                configurationParameter.Uuid,
                activation.Identification,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(activation);

        context.RpcClient.Setup(r => r.CallAsync<GetPersonInformationRequest, GetPersonInformationResponse>(
                It.IsAny<GetPersonInformationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPersonInformationResponse(true, null, null, CreatePersonInformation("Jane Doe")));

        context.RpcClient.Setup(r => r.CallAsync<ValidateEntityRequest, ValidateEntityResponse>(
                It.IsAny<ValidateEntityRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidateEntityResponse(true, null, null));

        context.RpcClient.Setup(r => r.CallAsync<GetBalancesRequest, GetBalancesResponse>(
                It.IsAny<GetBalancesRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetBalancesResponse(true, null, null, Array.Empty<BalanceResponse>()));

        var handler = context.Handler;
        var query = new GetBalancesByObjectiveQuery("KIT", documentTypeCode, activation.Identification, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);

        var pageInfo = result.Value.PageInfo;
        Assert.Equal(0, pageInfo.TotalRecords);
        Assert.Equal(0, pageInfo.TotalPages);
        Assert.Equal(0, pageInfo.RecordsPerPage);

        context.RpcClient.Verify(r => r.CallAsync<GetAdditionalInformationRequest, GetAdditionalInformationResponse>(
            It.IsAny<GetAdditionalInformationRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenEarlyPaginationValidationFails()
    {
        // Arrange
        var context = CreateHandlerContext();
        var validationError = new RuleValidationError("PAG_001", "Paginacion invalida");

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                PaginationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));

        var handler = context.Handler;
        var query = new GetBalancesByObjectiveQuery("KIT", "CC", "123456789", 0, -1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("PAG_001", result.Error.Code);
        Assert.Equal(Common.SharedKernel.Core.Primitives.ErrorType.Validation, result.Error.Type);

        context.ConfigurationRepository.Verify(r => r.GetByCodeAndScopeAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenRequestValidationFails()
    {
        // Arrange
        var context = CreateHandlerContext();
        var validationError = new RuleValidationError("VAL_001", "Datos incompletos");

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                PaginationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                ValidationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { validationError }));

        context.ConfigurationRepository.Setup(r => r.GetByCodeAndScopeAsync(
                "CC",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConfigurationParameter?)null);

        var handler = context.Handler;
        var query = new GetBalancesByObjectiveQuery("KIT", "CC", null, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("VAL_001", result.Error.Code);
        context.ActivateRepository.Verify(r => r.GetByIdTypeAndNumber(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenPersonValidationFails()
    {
        // Arrange
        var context = CreateHandlerContext();
        var documentTypeCode = "CC";
        var configurationParameter = CreateConfigurationParameter(documentTypeCode, Guid.NewGuid());
        var activation = CreateActivate(configurationParameter.Uuid, "123456789", 99);

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                PaginationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                ValidationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.ConfigurationRepository.Setup(r => r.GetByCodeAndScopeAsync(
                documentTypeCode,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configurationParameter);

        context.ActivateRepository.Setup(r => r.GetByIdTypeAndNumber(
                configurationParameter.Uuid,
                activation.Identification,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(activation);

        context.RpcClient.Setup(r => r.CallAsync<GetPersonInformationRequest, GetPersonInformationResponse>(
                It.IsAny<GetPersonInformationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPersonInformationResponse(false, "PER_001", "Persona invalida", null));

        var handler = context.Handler;
        var query = new GetBalancesByObjectiveQuery("KIT", documentTypeCode, activation.Identification, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("PER_001", result.Error.Code);

        context.RpcClient.Verify(r => r.CallAsync<ValidateEntityRequest, ValidateEntityResponse>(
            It.IsAny<ValidateEntityRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenEntityValidationFails()
    {
        // Arrange
        var context = CreateHandlerContext();
        var documentTypeCode = "CC";
        var configurationParameter = CreateConfigurationParameter(documentTypeCode, Guid.NewGuid());
        var activation = CreateActivate(configurationParameter.Uuid, "123456789", 101);

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                PaginationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                ValidationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.ConfigurationRepository.Setup(r => r.GetByCodeAndScopeAsync(
                documentTypeCode,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configurationParameter);

        context.ActivateRepository.Setup(r => r.GetByIdTypeAndNumber(
                configurationParameter.Uuid,
                activation.Identification,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(activation);

        context.RpcClient.Setup(r => r.CallAsync<GetPersonInformationRequest, GetPersonInformationResponse>(
                It.IsAny<GetPersonInformationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPersonInformationResponse(true, null, null, CreatePersonInformation("Jose Perez")));

        context.RpcClient.Setup(r => r.CallAsync<ValidateEntityRequest, ValidateEntityResponse>(
                It.IsAny<ValidateEntityRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidateEntityResponse(false, "ENT_001", "Entidad invalida"));

        var handler = context.Handler;
        var query = new GetBalancesByObjectiveQuery("KIT", documentTypeCode, activation.Identification, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ENT_001", result.Error.Code);

        context.RpcClient.Verify(r => r.CallAsync<GetBalancesRequest, GetBalancesResponse>(
            It.IsAny<GetBalancesRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenBalancesRpcFails()
    {
        // Arrange
        var context = CreateHandlerContext();
        var documentTypeCode = "CC";
        var configurationParameter = CreateConfigurationParameter(documentTypeCode, Guid.NewGuid());
        var activation = CreateActivate(configurationParameter.Uuid, "123456789", 202);

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                PaginationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                ValidationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.ConfigurationRepository.Setup(r => r.GetByCodeAndScopeAsync(
                documentTypeCode,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configurationParameter);

        context.ActivateRepository.Setup(r => r.GetByIdTypeAndNumber(
                configurationParameter.Uuid,
                activation.Identification,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(activation);

        context.RpcClient.Setup(r => r.CallAsync<GetPersonInformationRequest, GetPersonInformationResponse>(
                It.IsAny<GetPersonInformationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPersonInformationResponse(true, null, null, CreatePersonInformation("Jose Perez")));

        context.RpcClient.Setup(r => r.CallAsync<ValidateEntityRequest, ValidateEntityResponse>(
                It.IsAny<ValidateEntityRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidateEntityResponse(true, null, null));

        context.RpcClient.Setup(r => r.CallAsync<GetBalancesRequest, GetBalancesResponse>(
                It.IsAny<GetBalancesRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetBalancesResponse(false, "BAL_001", "No hay balances", Array.Empty<BalanceResponse>()));

        var handler = context.Handler;
        var query = new GetBalancesByObjectiveQuery("KIT", documentTypeCode, activation.Identification, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("BAL_001", result.Error.Code);

        context.RpcClient.Verify(r => r.CallAsync<GetAdditionalInformationRequest, GetAdditionalInformationResponse>(
            It.IsAny<GetAdditionalInformationRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenAdditionalInformationRpcFails()
    {
        // Arrange
        var context = CreateHandlerContext();
        var documentTypeCode = "CC";
        var configurationParameter = CreateConfigurationParameter(documentTypeCode, Guid.NewGuid());
        var activation = CreateActivate(configurationParameter.Uuid, "123456789", 320);

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                PaginationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                ValidationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.ConfigurationRepository.Setup(r => r.GetByCodeAndScopeAsync(
                documentTypeCode,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configurationParameter);

        context.ActivateRepository.Setup(r => r.GetByIdTypeAndNumber(
                configurationParameter.Uuid,
                activation.Identification,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(activation);

        context.RpcClient.Setup(r => r.CallAsync<GetPersonInformationRequest, GetPersonInformationResponse>(
                It.IsAny<GetPersonInformationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPersonInformationResponse(true, null, null, CreatePersonInformation("Jose Perez")));

        context.RpcClient.Setup(r => r.CallAsync<ValidateEntityRequest, ValidateEntityResponse>(
                It.IsAny<ValidateEntityRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidateEntityResponse(true, null, null));

        var balances = new[]
        {
            new BalanceResponse(1, 10, 100m, 40m)
        };

        context.RpcClient.Setup(r => r.CallAsync<GetBalancesRequest, GetBalancesResponse>(
                It.IsAny<GetBalancesRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetBalancesResponse(true, null, null, balances));

        context.RpcClient.Setup(r => r.CallAsync<GetAdditionalInformationRequest, GetAdditionalInformationResponse>(
                It.IsAny<GetAdditionalInformationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAdditionalInformationResponse(false, "OBJ_404", "Objetivo no encontrado", Array.Empty<AdditionalInformationItem>()));

        var handler = context.Handler;
        var query = new GetBalancesByObjectiveQuery("KIT", documentTypeCode, activation.Identification, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("OBJ_404", result.Error.Code);
    }

    [Fact]
    public async Task Handle_PropagatesExceptions_FromRepositories()
    {
        // Arrange
        var context = CreateHandlerContext();
        var documentTypeCode = "CC";
        var configurationParameter = CreateConfigurationParameter(documentTypeCode, Guid.NewGuid());
        var activationIdentification = "123456789";

        context.RuleEvaluator.Setup(r => r.EvaluateAsync(
                PaginationWorkflow,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessfulRules());

        context.ConfigurationRepository.Setup(r => r.GetByCodeAndScopeAsync(
                documentTypeCode,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configurationParameter);

        context.ActivateRepository.Setup(r => r.GetByIdTypeAndNumber(
                configurationParameter.Uuid,
                activationIdentification,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository failure"));

        var handler = context.Handler;
        var query = new GetBalancesByObjectiveQuery("KIT", documentTypeCode, activationIdentification, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query, CancellationToken.None));
    }

    private static HandlerContext CreateHandlerContext()
    {
        var activateRepository = new Mock<IActivateRepository>();
        var configurationRepository = new Mock<IConfigurationParameterRepository>();
        var ruleEvaluator = new Mock<IRuleEvaluator<AssociateModuleMarker>>();
        var rpcClient = new Mock<IRpcClient>();

        var handler = new GetBalancesByObjectiveHandler(
            activateRepository.Object,
            configurationRepository.Object,
            ruleEvaluator.Object,
            rpcClient.Object);

        return new HandlerContext(handler, activateRepository, configurationRepository, ruleEvaluator, rpcClient);
    }

    private static ConfigurationParameter CreateConfigurationParameter(string code, Guid uuid)
    {
        var parameter = ConfigurationParameter.Create("DocumentType", code);
        typeof(ConfigurationParameter)
            .GetProperty(nameof(ConfigurationParameter.Uuid), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(parameter, uuid);

        return parameter;
    }

    private static Activate CreateActivate(Guid documentType, string identification, int activateId)
    {
        var activation = Activate.Create(documentType, identification, false, false, DateTime.UtcNow).Value;
        typeof(Activate)
            .GetProperty(nameof(Activate.ActivateId), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(activation, activateId);
        return activation;
    }

    private static (bool Success, IReadOnlyCollection<RuleResultTree> Results, IReadOnlyCollection<RuleValidationError> Errors) SuccessfulRules()
        => (true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>());
    
    private static PersonInformation CreatePersonInformation(string fullName)
    {
        return new PersonInformation(
            1,
            Guid.NewGuid(),
            "CC",
            "1234567890",
            "John",
            "Middle",
            "Doe",
            "Smith",
            new DateTime(1990, 1, 1),
            "3001234567",
            fullName,
            1,
            57,
            11,
            111,
            "john.doe@example.com",
            42,
            Status.Active,
            "123 Main Street",
            true,
            7,
            9);
    }
    
    private sealed record HandlerContext(
        GetBalancesByObjectiveHandler Handler,
        Mock<IActivateRepository> ActivateRepository,
        Mock<IConfigurationParameterRepository> ConfigurationRepository,
        Mock<IRuleEvaluator<AssociateModuleMarker>> RuleEvaluator,
        Mock<IRpcClient> RpcClient);
}
