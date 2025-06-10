using Products.Domain.ConfigurationParameters;
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
using Common.SharedKernel.Domain.ConfigurationParameters;
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

    private static Alternative BuildDummyAlternative(string homologatedCode)
    {
        var plan = Plan.Create("Plan", "Desc").Value;
        var pension = PensionFund.Create(1, 1, "Fund", "F", "ACT", "F-1").Value;
        var planFund = PlanFund.Create(plan, pension, "ACTIVO").Value;
        return Alternative.Create(planFund, 1, "Alt", "ACT", "Desc", homologatedCode).Value;
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
            "COM-123");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Document_Type_Invalid()
    {
        // arrange
        var request = BuildRequest();
        _alternativeRepo.Setup(r => r.GetByHomologatedCodeAsync(request.AlternativeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDummyAlternative(request.AlternativeId));
        _configRepo.Setup(r =>
                r.GetByCodeAndScopeAsync(request.ObjectiveType, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("type", request.ObjectiveType));
        var error = Error.Validation("DOC", "invalid");
        _docTypeValidator.Setup(v => v.EnsureExistsAsync(request.IdType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

        var handler = BuildHandler();

        // act
        var result = await handler.Handle(request, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);
        _affiliateLocator.Verify(
            l => l.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _ruleEvaluator.Verify(
            r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Affiliate_Locator_Fails()
    {
        // arrange
        var request = BuildRequest();
        _alternativeRepo.Setup(r => r.GetByHomologatedCodeAsync(request.AlternativeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDummyAlternative(request.AlternativeId));
        _configRepo.Setup(r =>
                r.GetByCodeAndScopeAsync(request.ObjectiveType, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("type", request.ObjectiveType));
        _docTypeValidator.Setup(v => v.EnsureExistsAsync(request.IdType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var error = Error.NotFound("AFF", "not found");
        _affiliateLocator.Setup(l => l.FindAsync(request.IdType, request.Identification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<int?>(error));

        var handler = BuildHandler();

        // act
        var result = await handler.Handle(request, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);
        _ruleEvaluator.Verify(
            r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Rules_Fail()
    {
        // arrange
        var request = BuildRequest();
        _alternativeRepo.Setup(r => r.GetByHomologatedCodeAsync(request.AlternativeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDummyAlternative(request.AlternativeId));
        _configRepo.Setup(r =>
                r.GetByCodeAndScopeAsync(request.ObjectiveType, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("type", request.ObjectiveType));
        _docTypeValidator.Setup(v => v.EnsureExistsAsync(request.IdType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _affiliateLocator.Setup(l => l.FindAsync(request.IdType, request.Identification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int?>(99));
        _officeRepo.Setup(r => r.GetByHomologatedCodesAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Office>
            {
                { request.OpeningOffice, Office.Create("Bogota", "ACTIVO", "BO", request.OpeningOffice, 1).Value },
                { request.CurrentOffice, Office.Create("Med", "ACTIVO", "ME", request.CurrentOffice, 2).Value }
            });
        _commercialRepo.Setup(r => r.GetByHomologatedCodeAsync(request.Commercial, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Commercial.Create("Com", "ACT", "C", request.Commercial).Value);
        var error = Error.Validation("RULE", "fail");
        _ruleEvaluator.Setup(r =>
                r.EvaluateAsync("Products.CreateObjective.Validation", It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(),
                new[] { new RuleValidationError(error.Code, error.Description) }));

        var handler = BuildHandler();

        // act
        var result = await handler.Handle(request, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(error.Code);
        _unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Create_Objective_When_All_Valid()
    {
        // arrange
        var request = BuildRequest();
        var alternative = BuildDummyAlternative(request.AlternativeId);
        _alternativeRepo.Setup(r => r.GetByHomologatedCodeAsync(request.AlternativeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alternative);
        _configRepo.Setup(r =>
                r.GetByCodeAndScopeAsync(request.ObjectiveType, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConfigurationParameter.Create("type", request.ObjectiveType));
        _docTypeValidator.Setup(v => v.EnsureExistsAsync(request.IdType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _affiliateLocator.Setup(l => l.FindAsync(request.IdType, request.Identification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int?>(99));
        var offices = new Dictionary<string, Office>
        {
            { request.OpeningOffice, Office.Create("Bogota", "ACTIVO", "BO", request.OpeningOffice, 1).Value },
            { request.CurrentOffice, Office.Create("Med", "ACTIVO", "ME", request.CurrentOffice, 2).Value }
        };
        _officeRepo.Setup(r => r.GetByHomologatedCodesAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(offices);
        var commercial = Commercial.Create("Com", "ACT", "C", request.Commercial).Value;
        _commercialRepo.Setup(r => r.GetByHomologatedCodeAsync(request.Commercial, It.IsAny<CancellationToken>()))
            .ReturnsAsync(commercial);
        _ruleEvaluator.Setup(r =>
                r.EvaluateAsync("Products.CreateObjective.Validation", It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        var handler = BuildHandler();

        // act
        var result = await handler.Handle(request, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        _ruleEvaluator.Verify(
            r => r.EvaluateAsync("Products.CreateObjective.Validation", It.Is<object>(ctx => IsValidContext(ctx)),
                It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}