using System.Data.Common;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Products.Application.Abstractions;
using Products.Application.Abstractions.Data;
using Products.Application.Abstractions.Rules;
using Products.Application.Abstractions.Services.External;
using Products.Application.Objectives.CreateObjective;
using Products.Domain.Alternatives;
using Products.Domain.Commercials;
using Products.Domain.ConfigurationParameters;
using Products.Domain.Offices;
using Products.Domain.PensionFunds;
using Products.Domain.PlanFunds;
using Products.Domain.Plans;
using Products.Integrations.Objectives.CreateObjective;
using RulesEngine.Models;

namespace Products.test.UnitTests.Application.Objectives;

public class CreateObjectiveCommandHandlerTests
{
    private readonly Mock<IConfigurationParameterRepository> _configRepo = new();
    private readonly Mock<IDocumentTypeValidator> _docTypeValidator = new();
    private readonly Mock<IAlternativeRepository> _alternativeRepo = new();
    private readonly Mock<IAffiliateLocator> _affiliateLocator = new();
    private readonly Mock<ICommercialRepository> _commercialRepo = new();
    private readonly Mock<IOfficeRepository> _officeRepo = new();
    private readonly Mock<IRuleEvaluator<ProductsModuleMarker>> _ruleEvaluator = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<DbTransaction> _tx = new();

    private CreateObjectiveCommandHandler BuildHandler()
    {
        _unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_tx.Object);

        return new CreateObjectiveCommandHandler(
            _configRepo.Object,
            _docTypeValidator.Object,
            _alternativeRepo.Object,
            _affiliateLocator.Object,
            _commercialRepo.Object,
            _officeRepo.Object,
            _ruleEvaluator.Object,
            _unitOfWork.Object
        );
    }

    private Alternative BuildDummyAlternative(string homologatedCode)
    {
        var planResult = Plan.Create("Plan Prueba", "Descripción prueba");
        if (!planResult.IsSuccess)
            throw new Exception("No se pudo crear el Plan de prueba");
        var dummyPlan = planResult.Value;

        var pensionResult = PensionFund.Create(
            1,
            99999,
            "Fondo Prueba",
            "FP",
            "ACTIVO",
            "FP-01"
        );
        if (!pensionResult.IsSuccess)
            throw new Exception("No se pudo crear el PensionFund de prueba");
        var dummyPensionFund = pensionResult.Value;

        var planFundResult = PlanFund.Create(dummyPlan, dummyPensionFund, "ACTIVO");
        if (!planFundResult.IsSuccess)
            throw new Exception("No se pudo crear el PlanFund de prueba");
        var dummyPlanFund = planFundResult.Value;

        var alternativeResult = Alternative.Create(
            dummyPlanFund,
            1,
            "Alt Prueba",
            "ACTIVO",
            "Descripción alt",
            homologatedCode
        );
        if (!alternativeResult.IsSuccess)
            throw new Exception("No se pudo crear la Alternative de prueba");
        return alternativeResult.Value;
    }

    [Fact]
    public async Task Handle_Should_Create_Objective_When_Request_Is_Valid()
    {
        // arrange
        var request = new CreateObjectiveCommand(
            "CC",
            "123456",
            "ALT-01",
            "OBJ-PLAN",
            "Jubilarme",
            "OF-BOG",
            "OF-MED",
            "COM-123"
        );

        var alternativeEntity = BuildDummyAlternative(request.AlternativeId);
        _alternativeRepo
            .Setup(r => r.GetByHomologatedCodeAsync(request.AlternativeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alternativeEntity);

        _configRepo.Setup(r => r.GetByCodeAndScopeAsync(request.ObjectiveType,
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("type", request.ObjectiveType));

        _docTypeValidator.Setup(v => v.EnsureExistsAsync(request.IdType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _affiliateLocator.Setup(l => l.FindAsync(request.IdType, request.Identification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int?>(99));

        var offices = new Dictionary<string, Office>
        {
            { request.OpeningOffice, Office.Create("Bogotá", "ACTIVO", "BO", request.OpeningOffice, 1).Value },
            { request.CurrentOffice, Office.Create("Medellín", "ACTIVO", "ME", request.CurrentOffice, 2).Value }
        };
        _officeRepo.Setup(r => r.GetByHomologatedCodesAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(offices);

        _commercialRepo.Setup(r => r.GetByHomologatedCodeAsync(request.Commercial, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Commercial.Create("Pedro", "ACTIVO", "PE", request.Commercial).Value);

        _ruleEvaluator.Setup(r => r.EvaluateAsync(
                "Products.CreateObjective.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                true,
                Array.Empty<RuleResultTree>() as IReadOnlyCollection<RuleResultTree>,
                Array.Empty<RuleValidationError>() as IReadOnlyCollection<RuleValidationError>
            ));

        var handler = BuildHandler();

        // act
        var result = await handler.Handle(request, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ObjectiveId.Should().NotBeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Rule_Evaluator_Fails()
    {
        // arrange
        var request = new CreateObjectiveCommand(
            "CC", "123", "ALT-01", "OBJ-PLAN",
            "Jubilarme", "OF-BOG", "OF-MED", "COM-123"
        );

        var alternativeEntity = BuildDummyAlternative(request.AlternativeId);
        _alternativeRepo
            .Setup(r => r.GetByHomologatedCodeAsync(request.AlternativeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alternativeEntity);

        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("type", request.ObjectiveType));

        _docTypeValidator
            .Setup(v => v.EnsureExistsAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _affiliateLocator
            .Setup(l => l.FindAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int?>(null));

        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(
                "Products.CreateObjective.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                false,
                Array.Empty<RuleResultTree>() as IReadOnlyCollection<RuleResultTree>,
                new[]
                {
                    new RuleValidationError("OBJ001", "Invalid office")
                } as IReadOnlyCollection<RuleValidationError>
            ));

        var oficinasSimuladas = new Dictionary<string, Office>
        {
            { "OF-BOG", Office.Create("Bogotá", "ACTIVO", "BO", "OF-BOG", 1).Value },
            { "OF-MED", Office.Create("Medellín", "ACTIVO", "ME", "OF-MED", 2).Value }
        };
        _officeRepo
            .Setup(r => r.GetByHomologatedCodesAsync(
                It.IsAny<string[]>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(oficinasSimuladas);

        _commercialRepo
            .Setup(r => r.GetByHomologatedCodeAsync(
                request.Commercial,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Commercial?)null);

        var handler = BuildHandler();

        // act
        var result = await handler.Handle(request, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("OBJ001");
        result.Error.Description.Should().Be("Invalid office");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}