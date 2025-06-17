using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;
using FluentAssertions;
using Moq;
using Products.Application.Abstractions.Services.External;
using Products.Application.Abstractions.Services.Objectives;
using Products.Application.Abstractions.Services.Rules;
using Products.Application.Objectives.GetObjectives;
using Products.Domain.ConfigurationParameters;
using Products.Integrations.Objectives.GetObjectives;
using System.Linq;
using Common.SharedKernel.Application.Rules;
using Products.Application.Abstractions;
using RulesEngine.Models;

namespace Products.test.UnitTests.Application.Objectives;

public class GetObjectivesQueryHandlerTests
{
    private readonly Mock<IConfigurationParameterRepository> _configRepo = new();
    private readonly Mock<IAffiliateLocator> _affiliateLocator = new();
    private readonly Mock<IRuleEvaluator<ProductsModuleMarker>> _ruleEval  = new();
    private readonly Mock<IObjectiveReader> _objectiveReader = new();
    private readonly Mock<IGetObjectivesRules> _rules = new();

    private GetObjectivesQueryHandler BuildHandler()
    {
        return new GetObjectivesQueryHandler(_configRepo.Object, _affiliateLocator.Object, _ruleEval.Object, _objectiveReader.Object,
            _rules.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_DocumentType_Invalid()
    {
        // arrange
        var error = Error.Validation("DOC001", "Invalid type");

        _ruleEval
            .Setup(r => r.EvaluateAsync(
                "Products.Objective.RequiredFieldsGetObjectives",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync("CC",
                HomologScope.Of<GetObjectivesQuery>(c => c.TypeId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConfigurationParameter?)null);

        _affiliateLocator
            .Setup(a => a.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int?>(null));

        _objectiveReader
            .Setup(r => r.BuildValidationContextAsync(It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<StatusType>(), 
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ObjectiveValidationContext());

        _rules
            .Setup(r => r.EvaluateAsync(It.IsAny<ObjectiveValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

        var handler = BuildHandler();
        var query = new GetObjectivesQuery("CC", "123", StatusType.A);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);

        _configRepo.VerifyAll();
        _affiliateLocator.Verify(
            a => a.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _objectiveReader.Verify(r => r.BuildValidationContextAsync(It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<StatusType>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _rules.Verify(r => r.EvaluateAsync(It.IsAny<ObjectiveValidationContext>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _objectiveReader.Verify(
            r => r.ReadDtosAsync(It.IsAny<int>(), It.IsAny<StatusType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Affiliate_Locator_Fails()
    {
        // arrange
        _ruleEval
            .Setup(r => r.EvaluateAsync(
                "Products.Objective.RequiredFieldsGetObjectives",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(It.IsAny<string>(),
                HomologScope.Of<GetObjectivesQuery>(c => c.TypeId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("CC", "TipoDocumento"));

        var error = Error.NotFound("AFF001", "Affiliate not found");

        _affiliateLocator
            .Setup(a => a.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<int?>(error));

        var handler = BuildHandler();
        var query = new GetObjectivesQuery("CC", "123", StatusType.A);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);

        _objectiveReader.Verify(r => r.BuildValidationContextAsync(It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<StatusType>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _rules.Verify(r => r.EvaluateAsync(It.IsAny<ObjectiveValidationContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Rules_Evaluation_Fails()
    {
        // arrange
        _ruleEval
            .Setup(r => r.EvaluateAsync(
                "Products.Objective.RequiredFieldsGetObjectives",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(It.IsAny<string>(),
                HomologScope.Of<GetObjectivesQuery>(c => c.TypeId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("CC", "TipoDocumento"));

        _affiliateLocator
            .Setup(a => a.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int?>(99));

        var validationContext = new ObjectiveValidationContext { AffiliateExists = true };

        _objectiveReader
            .Setup(r => r.BuildValidationContextAsync(true,
                99,
                StatusType.A,
                /*docExists*/ true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationContext);

        var error = Error.Validation("RULE001", "rule failed");

        _rules
            .Setup(r => r.EvaluateAsync(validationContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

        var handler = BuildHandler();
        var query = new GetObjectivesQuery("CC", "123", StatusType.A);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);

        _objectiveReader.Verify(
            r => r.ReadDtosAsync(It.IsAny<int>(), It.IsAny<StatusType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Objectives_When_All_Dependency_Succeed()
    {
        // arrange
        _ruleEval
            .Setup(r => r.EvaluateAsync(
                "Products.Objective.RequiredFieldsGetObjectives",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(It.IsAny<string>(),
                HomologScope.Of<GetObjectivesQuery>(c => c.TypeId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("CC", "TipoDocumento"));

        _affiliateLocator
            .Setup(a => a.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int?>(99));

        var validationContext = new ObjectiveValidationContext { AffiliateExists = true };

        _objectiveReader
            .Setup(r => r.BuildValidationContextAsync(true,
                99,
                StatusType.A,
                /*docExists*/ true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationContext);

        _rules
            .Setup(r => r.EvaluateAsync(validationContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var objectives = new List<ObjectiveDto>
        {
            new(1, "T1", "Name1", "F1", "Plan1", "ALT1", "AltName", "Port", "Activo"),
            new(2, "T2", "Name2", "F2", "Plan2", "ALT2", "AltName2", "Port2", "Inactivo")
        };

        _objectiveReader
            .Setup(r => r.ReadDtosAsync(99, StatusType.A, It.IsAny<CancellationToken>()))
            .ReturnsAsync(objectives);

        var handler = BuildHandler();
        var query = new GetObjectivesQuery("CC", "123", StatusType.A);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(objectives.Select(o => new ObjectiveItem(o)));
    }
}