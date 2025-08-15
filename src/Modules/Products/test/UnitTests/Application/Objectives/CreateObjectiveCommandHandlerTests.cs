using System.Data.Common;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;
using FluentAssertions;
using Moq;
using Products.Application.Abstractions;
using Products.Application.Abstractions.Data;
using Products.Application.Abstractions.Services.External;
using Products.Application.Objectives.CreateObjective;
using Products.Domain.Alternatives;
using Products.Domain.Commercials;
using Products.Domain.ConfigurationParameters;
using Products.Domain.Objectives;
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
    private readonly Mock<IAlternativeRepository> _alternativeRepo = new();
    private readonly Mock<IObjectiveRepository> _objectiveRepo = new();
    private readonly Mock<IAffiliateLocator> _affiliateLocator = new();
    private readonly Mock<ICommercialRepository> _commercialRepo = new();
    private readonly Mock<IOfficeRepository> _officeRepo = new();
    private readonly Mock<IRuleEvaluator<ProductsModuleMarker>> _ruleEvaluator = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<DbTransaction> _tx = new();

    private CreateObjectiveCommandHandler BuildHandler()
    {
        _unitOfWork
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_tx.Object);

        return new CreateObjectiveCommandHandler(
            _configRepo.Object,
            _alternativeRepo.Object,
            _objectiveRepo.Object,
            _affiliateLocator.Object,
            _commercialRepo.Object,
            _officeRepo.Object,
            _ruleEvaluator.Object,
            _unitOfWork.Object
        );
    }

    private static Alternative BuildDummyAlternative(string homologatedCode)
    {
        var plan = Plan.Create("Plan", "Desc", "2").Value;
        var pension = PensionFund.Create(1, 1, "Fund", "F", Status.Active, "F-1").Value;
        var planFund = PlanFund.Create(plan, pension, Status.Active).Value;
        return Alternative.Create(planFund, 1, "Alt", Status.Active, "Desc", homologatedCode).Value;
    }

    private static bool IsValidContext(object ctx)
    {
        var t = ctx.GetType();
        return (bool)t.GetProperty("AlternativeIdExists")!.GetValue(ctx)! &&
               (bool)t.GetProperty("ObjectiveTypeExists")!.GetValue(ctx)! &&
               (bool)t.GetProperty("ClientAffiliated")!.GetValue(ctx)! &&
               (bool)t.GetProperty("OpeningOfficeExists")!.GetValue(ctx)! &&
               (bool)t.GetProperty("CurrentOfficeExists")!.GetValue(ctx)! &&
               (bool)t.GetProperty("CommercialExists")!.GetValue(ctx)!;
    }

    private CreateObjectiveCommand BuildRequest()
    {
        return new CreateObjectiveCommand(
            "CC",
            "123",
            "ALT-01",
            "OBJ-PLAN",
            "Jubilarme",
            "OF-BOG",
            "OF-MED",
            "COM-123"
        );
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Document_Type_Invalid()
    {
        var request = BuildRequest();

        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(
                "Products.CreateObjective.RequiredFields",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        _alternativeRepo
            .Setup(r => r.GetByHomologatedCodeAsync(request.AlternativeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDummyAlternative(request.AlternativeId));

        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(
                request.ObjectiveType,
                HomologScope.Of<CreateObjectiveCommand>(c => c.ObjectiveType),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("type", request.ObjectiveType));

        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(
                request.IdType,
                HomologScope.Of<CreateObjectiveCommand>(c => c.IdType),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConfigurationParameter?)null);

        _officeRepo
            .Setup(r => r.GetByHomologatedCodesAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Office>
            {
                { request.OpeningOffice, Office.Create("Bogotá", Status.Active, "BO", request.OpeningOffice,  1,"CC-BO").Value },
                {
                    request.CurrentOffice,
                    Office.Create("Medellín", Status.Active, "ME", request.CurrentOffice,2, "CC-ME").Value
                }
            });

        _commercialRepo
            .Setup(r => r.GetByHomologatedCodeAsync(request.Commercial, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Commercial.Create("Com", Status.Active, "C", request.Commercial).Value);

        var ruleError = new RuleValidationError("DOC", "invalid");
        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(
                "Products.CreateObjective.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { ruleError }));

        var handler = BuildHandler();

        var result = await handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ruleError.Code);
        result.Error.Description.Should().Be(ruleError.Message);

        _affiliateLocator.Verify(
            l => l.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _ruleEvaluator.Verify(
            r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _objectiveRepo.Verify(r => r.AddAsync(It.IsAny<Objective>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Affiliate_Locator_Fails()
    {
        var request = BuildRequest();

        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(
                "Products.CreateObjective.RequiredFields",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        _alternativeRepo
            .Setup(r => r.GetByHomologatedCodeAsync(request.AlternativeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDummyAlternative(request.AlternativeId));

        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(request.ObjectiveType, It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("type", request.ObjectiveType));

        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(request.IdType,
                HomologScope.Of<CreateObjectiveCommand>(c => c.IdType), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("CC", "TipoDocumento"));

        var error = Error.NotFound("AFF", "not found");
        _affiliateLocator
            .Setup(l => l.FindAsync(request.IdType, request.Identification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<int?>(error));

        var handler = BuildHandler();

        var result = await handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);

        _ruleEvaluator.Verify(
            r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _objectiveRepo.Verify(r => r.AddAsync(It.IsAny<Objective>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Rules_Fail()
    {
        var request = BuildRequest();

        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(
                "Products.CreateObjective.RequiredFields",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        _alternativeRepo
            .Setup(r => r.GetByHomologatedCodeAsync(request.AlternativeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDummyAlternative(request.AlternativeId));

        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(request.ObjectiveType, It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("type", request.ObjectiveType));

        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(request.IdType,
                HomologScope.Of<CreateObjectiveCommand>(c => c.IdType), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("CC", "TipoDocumento"));

        _affiliateLocator
            .Setup(l => l.FindAsync(request.IdType, request.Identification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int?>(99));

        _officeRepo
            .Setup(r => r.GetByHomologatedCodesAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Office>
            {
                { request.OpeningOffice, Office.Create("Bogota", Status.Active, "BO", request.OpeningOffice,  1,"CC-BO").Value },
                { request.CurrentOffice, Office.Create("Med", Status.Active, "ME", request.CurrentOffice, 2, "CC-ME").Value }
            });

        _commercialRepo
            .Setup(r => r.GetByHomologatedCodeAsync(request.Commercial, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Commercial.Create("Com", Status.Active, "C", request.Commercial).Value);

        var ruleError = new RuleValidationError("RULE", "fail");
        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(
                "Products.CreateObjective.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { ruleError }));

        var handler = BuildHandler();

        var result = await handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ruleError.Code);

        _objectiveRepo.Verify(r => r.AddAsync(It.IsAny<Objective>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Create_Objective_When_All_Valid()
    {
        var request = BuildRequest();
        var alternative = BuildDummyAlternative(request.AlternativeId);

        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(
                "Products.CreateObjective.RequiredFields",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        _alternativeRepo
            .Setup(r => r.GetByHomologatedCodeAsync(request.AlternativeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alternative);

        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(request.ObjectiveType, It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("type", request.ObjectiveType));

        _configRepo
            .Setup(r => r.GetByCodeAndScopeAsync(request.IdType,
                HomologScope.Of<CreateObjectiveCommand>(c => c.IdType), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("CC", "TipoDocumento"));

        _affiliateLocator
            .Setup(l => l.FindAsync(request.IdType, request.Identification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int?>(99));

        _officeRepo
            .Setup(r => r.GetByHomologatedCodesAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Office>
            {
                { request.OpeningOffice, Office.Create("Bogota", Status.Active, "BO", request.OpeningOffice,  1,"CC-BO").Value },
                { request.CurrentOffice, Office.Create("Med", Status.Active, "ME", request.CurrentOffice, 2, "CC-ME").Value }
            });

        _commercialRepo
            .Setup(r => r.GetByHomologatedCodeAsync(request.Commercial, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Commercial.Create("Com", Status.Active, "C", request.Commercial).Value);

        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(
                "Products.CreateObjective.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        _objectiveRepo
            .Setup(r => r.AddAsync(It.IsAny<Objective>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = BuildHandler();

        var result = await handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _ruleEvaluator.Verify(
            r => r.EvaluateAsync("Products.CreateObjective.Validation", It.Is<object>(ctx => IsValidContext(ctx)),
                It.IsAny<CancellationToken>()), Times.Once);

        _objectiveRepo.Verify(
            r => r.AddAsync(
                It.Is<Objective>(o =>
                    o.Name == request.ObjectiveName &&
                    o.AffiliateId == 99 &&
                    o.AlternativeId == alternative.AlternativeId
                ),
                It.IsAny<CancellationToken>()
            ), Times.Once);

        _unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}