using Closing.Application.Abstractions;
using Closing.Application.PortfolioValuations.Queries;
using Closing.Domain.PortfolioValuations;
using Closing.Integrations.PortfolioValuations.Queries;
using Closing.Integrations.PortfolioValuations.Response;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using RulesEngine.Models;

namespace Closing.test.UnitTests.Application.PortfolioValuations;

public class GetPortfolioValuationInfoQueryHandlerTests
{
    private readonly Mock<IPortfolioValuationRepository> _portfolioValuationRepository = new();
    private readonly Mock<IInternalRuleEvaluator<ClosingModuleMarker>> _ruleEvaluator = new();

    private GetPortfolioValuationInfoQueryHandler CreateHandler()
    {
        return new GetPortfolioValuationInfoQueryHandler(
            _portfolioValuationRepository.Object,
            _ruleEvaluator.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Valuation_Info_When_Rules_Pass()
    {
        // Arrange
        var valuation = PortfolioValuation.Create(
            1,
            new DateTime(2024, 05, 15),
            1000m,
            1000m,
            100m,
            10m,
            0.5m,
            0.2m,
            0.01m,
            0m,
            0m,
            DateTime.UtcNow,
            true).Value;

        _portfolioValuationRepository
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(valuation.PortfolioId, valuation.ClosingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(valuation);

        object? capturedContext = null;

        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(
                "Closing.GetPortfolioValuationInfo.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Callback((string _, object context, CancellationToken _) => capturedContext = context)
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

        var handler = CreateHandler();
        var query = new GetPortfolioValuationInfoQuery(valuation.PortfolioId, valuation.ClosingDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new PortfolioValuationInfoResponse(valuation.PortfolioId, valuation.ClosingDate, valuation.UnitValue));
        _ruleEvaluator.Verify(r => r.EvaluateAsync(
            "Closing.GetPortfolioValuationInfo.Validation",
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Once);

        capturedContext.Should().NotBeNull();
        var contextType = capturedContext!.GetType();
        contextType.GetProperty("ValuationExists")!.GetValue(capturedContext).Should().Be(true);
        contextType.GetProperty("UnitValueIsPositive")!.GetValue(capturedContext).Should().Be(true);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_Valuation_Is_Not_Found()
    {
        // Arrange
        var error = Error.Validation("CLOSING_013", "No hay valoración disponible para los datos ingresados.");

        _portfolioValuationRepository
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioValuation?)null);

        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(
                "Closing.GetPortfolioValuationInfo.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { new RuleValidationError(error.Code, error.Description) }));

        var handler = CreateHandler();
        var query = new GetPortfolioValuationInfoQuery(1, DateTime.UtcNow);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(error.Code);
        result.Error.Description.Should().Be(error.Description);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_Unit_Value_Is_Not_Positive()
    {
        // Arrange
        var valuation = PortfolioValuation.Create(
            1,
            new DateTime(2024, 05, 15),
            1000m,
            1000m,
            100m,
            0m,
            0.5m,
            0.2m,
            0.01m,
            0m,
            0m,
            DateTime.UtcNow,
            true).Value;

        _portfolioValuationRepository
            .Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(valuation.PortfolioId, valuation.ClosingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(valuation);

        var error = Error.Validation("CLOSING_014", "El valor de la unidad debe ser mayor a 0.");

        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(
                "Closing.GetPortfolioValuationInfo.Validation",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { new RuleValidationError(error.Code, error.Description) }));

        var handler = CreateHandler();
        var query = new GetPortfolioValuationInfoQuery(valuation.PortfolioId, valuation.ClosingDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(error.Code);
        result.Error.Description.Should().Be(error.Description);
    }
}
