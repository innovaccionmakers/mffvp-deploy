using System.Text.Json;
using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Application.Closing.Services.Validation;
using Closing.Application.Closing.Services.Warnings;
using Closing.Application.PreClosing.Services.Commission.Dto;
using Closing.Application.PreClosing.Services.Commission.Interfaces;
using Closing.Application.PreClosing.Services.ExtraReturns.Dto;
using Closing.Application.PreClosing.Services.ExtraReturns.Interfaces;
using Closing.Application.PreClosing.Services.Orchestation;
using Closing.Application.PreClosing.Services.ProfitAndLoss;
using Closing.Application.PreClosing.Services.TreasuryConcepts;
using Closing.Application.PreClosing.Services.TreasuryConcepts.Dto;
using Closing.Application.PreClosing.Services.Validation;
using Closing.Application.PreClosing.Services.Validation.Dto;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Application.PreClosing.Services.Yield.Builders;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Dto;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.ProfitLosses;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Integrations.Common;
using Closing.Integrations.PreClosing.RunSimulation;

using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using FluentAssertions;

using Moq;

using Xunit;

namespace Closing.test.UnitTests.Application.PreClosing.Services.Orchestation;

public class SimulationOrchestratorTests
{
    [Fact]
    public async Task RunSimulationAsync_ShouldExecuteExtraReturnSimulationInParallelAndPersistImmediately()
    {
        // Arrange
        var portfolioId = 401;
        var closingDateUtc = new DateTime(2024, 9, 30, 0, 0, 0, DateTimeKind.Utc);
        var context = new SimulationOrchestratorTestContext();
        context.ConfigureClosingPortfolio(portfolioId, closingDateUtc, isFirstClosingDay: false);

        // 👇 Nuevo: siempre hay ND para este escenario
        context.ClosingBusinessRulesMock
            .Setup(rules => rules.HasDebitNotesAsync(
                portfolioId,
                closingDateUtc,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));

        var profitTaskSource = new TaskCompletionSource<IReadOnlyList<ProfitLossConceptSummary>>(TaskCreationOptions.RunContinuationsAsynchronously);
        context.ProfitLossServiceMock
            .Setup(service => service.GetProfitAndLossSummaryAsync(portfolioId, closingDateUtc, It.IsAny<CancellationToken>()))
            .Returns(profitTaskSource.Task);

        var commissionTaskSource = new TaskCompletionSource<IReadOnlyList<CommissionConceptSummary>>(TaskCreationOptions.RunContinuationsAsynchronously);
        context.CommissionServiceMock
            .Setup(service => service.CalculateAsync(portfolioId, closingDateUtc, It.IsAny<CancellationToken>()))
            .Returns(commissionTaskSource.Task);

        var treasuryTaskSource = new TaskCompletionSource<IReadOnlyList<TreasuryMovementSummary>>(TaskCreationOptions.RunContinuationsAsynchronously);
        context.MovementsServiceMock
            .Setup(service => service.GetMovementsSummaryAsync(portfolioId, closingDateUtc, It.IsAny<CancellationToken>()))
            .Returns(treasuryTaskSource.Task);

        var conceptJson = JsonSerializer.Serialize(new StringEntityDto("12", "Extraordinario"));
        var extraReturnSummary = new ExtraReturnSummary(
            portfolioId,
            new DateTime(2024, 9, 29, 14, 0, 0, DateTimeKind.Utc),
            12,
            "Extraordinario",
            2500m,
            conceptJson);

        var extraReturnsStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        context.ExtraReturnServiceMock
            .Setup(service => service.GetExtraReturnSummariesAsync(portfolioId, closingDateUtc, It.IsAny<CancellationToken>()))
            .Callback(() => extraReturnsStarted.TrySetResult(true))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<ExtraReturnSummary>>(new[] { extraReturnSummary }));

        var persistenceCapture = new TaskCompletionSource<(IReadOnlyList<YieldDetail> Details, PersistenceMode Mode)>(TaskCreationOptions.RunContinuationsAsynchronously);
        context.YieldDetailCreationServiceMock
            .Setup(service => service.CreateYieldDetailsAsync(
                It.IsAny<IEnumerable<YieldDetail>>(),
                It.IsAny<PersistenceMode>(),
                It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<YieldDetail>, PersistenceMode, CancellationToken>((details, mode, _) =>
            {
                persistenceCapture.TrySetResult((details.ToList(), mode));
                return Task.CompletedTask;
            });

        var sut = context.BuildSut();
        var runTask = sut.RunSimulationAsync(new RunSimulationCommand(portfolioId, closingDateUtc, true), CancellationToken.None);

        // 👇 Nuevo: evitamos que la prueba se quede colgada para siempre
        await extraReturnsStarted.Task.WaitAsync(System.TimeSpan.FromSeconds(5));

        var persistenceResult = await persistenceCapture.Task;
        persistenceResult.Mode.Should().Be(PersistenceMode.Immediate);
        persistenceResult.Details.Should().ContainSingle().Which.Source.Should().Be(YieldsSources.ExtraReturn);

        runTask.IsCompleted.Should().BeFalse("other simulation tasks are still pending");

        profitTaskSource.SetResult(Array.Empty<ProfitLossConceptSummary>());
        commissionTaskSource.SetResult(Array.Empty<CommissionConceptSummary>());
        treasuryTaskSource.SetResult(Array.Empty<TreasuryMovementSummary>());

        var result = await runTask;
        result.IsSuccess.Should().BeTrue();

        context.YieldDetailCreationServiceMock.Verify(service => service.CreateYieldDetailsAsync(
                It.IsAny<IEnumerable<YieldDetail>>(),
                PersistenceMode.Immediate,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RunSimulationAsync_ShouldMarkExtraReturnDetailsAccordingToClosingFlag(bool isClosing)
    {
        // Arrange
        var portfolioId = 77;
        var closingDateUtc = new DateTime(2024, 10, 15, 0, 0, 0, DateTimeKind.Utc);
        var context = new SimulationOrchestratorTestContext();

        if (isClosing)
        {
            context.ConfigureClosingPortfolio(portfolioId, closingDateUtc, isFirstClosingDay: false);
        }
        else
        {
            context.ConfigureFrontValidation(isFirstClosingDay: false);
        }

        // 👇 Nuevo: aseguramos que haya ND para que se ejecute ExtraReturns
        context.ClosingBusinessRulesMock
            .Setup(rules => rules.HasDebitNotesAsync(
                portfolioId,
                closingDateUtc,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));

        context.ProfitLossServiceMock
            .Setup(service => service.GetProfitAndLossSummaryAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ProfitLossConceptSummary>());
        context.CommissionServiceMock
            .Setup(service => service.CalculateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CommissionConceptSummary>());
        context.MovementsServiceMock
            .Setup(service => service.GetMovementsSummaryAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TreasuryMovementSummary>());

        var summary = new ExtraReturnSummary(
            portfolioId,
            new DateTime(2024, 10, 14, 12, 0, 0, DateTimeKind.Utc),
            44,
            "Distribución",
            1000m,
            JsonSerializer.Serialize(new StringEntityDto("44", "Distribución")));

        context.ExtraReturnServiceMock
            .Setup(service => service.GetExtraReturnSummariesAsync(portfolioId, closingDateUtc, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<ExtraReturnSummary>>(new[] { summary }));

        IReadOnlyList<YieldDetail>? capturedDetails = null;
        context.YieldDetailCreationServiceMock
            .Setup(service => service.CreateYieldDetailsAsync(
                It.IsAny<IEnumerable<YieldDetail>>(),
                PersistenceMode.Immediate,
                It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<YieldDetail>, PersistenceMode, CancellationToken>((details, _, _) => capturedDetails = details.ToList())
            .Returns(Task.CompletedTask);

        var sut = context.BuildSut();

        // Act
        var result = await sut.RunSimulationAsync(new RunSimulationCommand(portfolioId, closingDateUtc, isClosing), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedDetails.Should().NotBeNull();
        var detail = capturedDetails!.Should().ContainSingle().Which;
        detail.IsClosed.Should().Be(isClosing);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RunSimulationAsync_ShouldSkipExtraReturnPersistenceWhenConsolidationFailsOrIsEmpty(bool consolidationFailure)
    {
        // Arrange
        var portfolioId = 202;
        var closingDateUtc = new DateTime(2024, 11, 20, 0, 0, 0, DateTimeKind.Utc);
        var context = new SimulationOrchestratorTestContext();
        context.ConfigureClosingPortfolio(portfolioId, closingDateUtc, isFirstClosingDay: false);

        // 👇 Nuevo: hay ND, así que se ejecuta el flujo de ExtraReturns y se genera la advertencia
        context.ClosingBusinessRulesMock
            .Setup(rules => rules.HasDebitNotesAsync(
                portfolioId,
                closingDateUtc,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));

        context.ProfitLossServiceMock
            .Setup(service => service.GetProfitAndLossSummaryAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ProfitLossConceptSummary>());
        context.CommissionServiceMock
            .Setup(service => service.CalculateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CommissionConceptSummary>());
        context.MovementsServiceMock
            .Setup(service => service.GetMovementsSummaryAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TreasuryMovementSummary>());

        if (consolidationFailure)
        {
            context.ExtraReturnServiceMock
                .Setup(service => service.GetExtraReturnSummariesAsync(portfolioId, closingDateUtc, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<IReadOnlyCollection<ExtraReturnSummary>>(Error.Validation("EXTRA_FAIL", "fallo")));
        }
        else
        {
            context.ExtraReturnServiceMock
                .Setup(service => service.GetExtraReturnSummariesAsync(portfolioId, closingDateUtc, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<IReadOnlyCollection<ExtraReturnSummary>>(Array.Empty<ExtraReturnSummary>()));
        }

        var sut = context.BuildSut();

        // Act
        var result = await sut.RunSimulationAsync(new RunSimulationCommand(portfolioId, closingDateUtc, true), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.YieldDetailCreationServiceMock.Verify(service => service.CreateYieldDetailsAsync(
                It.IsAny<IEnumerable<YieldDetail>>(),
                PersistenceMode.Immediate,
                It.IsAny<CancellationToken>()),
            Times.Never);
        context.WarningCollectorMock.Verify(collector => collector.Add(
                It.Is<WarningItem>(w => w.Code == "ADV006")),
            Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RunSimulationAsyncHasDebitNotesFalseSkipsExtraReturnSimulation(bool isClosing)
    {
        // Arrange
        var portfolioId = 303;
        var closingDateUtc = new DateTime(2024, 12, 5, 0, 0, 0, DateTimeKind.Utc);
        var context = new SimulationOrchestratorTestContext();

        if (isClosing)
        {
            // Escenario cierre: usa el validador de portafolio + existencia de valoración
            context.ConfigureClosingPortfolio(portfolioId, closingDateUtc, isFirstClosingDay: false);
        }
        else
        {
            // Escenario front: usa el validador de front con IsFirstClosingDay = false
            context.ConfigureFrontValidation(isFirstClosingDay: false);
        }

        // HasDebitNotes = false -> no debe ejecutarse la simulación de ExtraReturns
        context.ClosingBusinessRulesMock
            .Setup(rules => rules.HasDebitNotesAsync(
                portfolioId,
                closingDateUtc,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(false));

        // El resto de consolidaciones no nos interesan en este test, las dejamos vacías
        context.ProfitLossServiceMock
            .Setup(service => service.GetProfitAndLossSummaryAsync(
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ProfitLossConceptSummary>());

        context.CommissionServiceMock
            .Setup(service => service.CalculateAsync(
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CommissionConceptSummary>());

        context.MovementsServiceMock
            .Setup(service => service.GetMovementsSummaryAsync(
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TreasuryMovementSummary>());

        var sut = context.BuildSut();

        // Act
        var result = await sut.RunSimulationAsync(
            new RunSimulationCommand(portfolioId, closingDateUtc, isClosing),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // No debe llamarse el servicio de ExtraReturn porque HasDebitNotes = false
        context.ExtraReturnServiceMock.Verify(service => service.GetExtraReturnSummariesAsync(
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        // No debe persistirse ningún detalle de ExtraReturn
        context.YieldDetailCreationServiceMock.Verify(service => service.CreateYieldDetailsAsync(
                It.IsAny<IEnumerable<YieldDetail>>(),
                PersistenceMode.Immediate,
                It.IsAny<CancellationToken>()),
            Times.Never);

        // Tampoco debe generarse la advertencia ADV006 (solo aplica cuando sí esperamos ExtraReturns)
        context.WarningCollectorMock.Verify(collector => collector.Add(
                It.Is<WarningItem>(w => w.Code == "ADV006")),
            Times.Never);
    }

    private sealed class SimulationOrchestratorTestContext
    {
        public Mock<IUnitOfWork> UnitOfWorkMock { get; } = new();
        public Mock<IProfitAndLossConsolidationService> ProfitLossServiceMock { get; } = new();
        public Mock<ICommissionCalculationService> CommissionServiceMock { get; } = new();
        public Mock<IMovementsConsolidationService> MovementsServiceMock { get; } = new();
        public Mock<IExtraReturnConsolidationService> ExtraReturnServiceMock { get; } = new();
        public Mock<IYieldDetailCreationService> YieldDetailCreationServiceMock { get; } = new();
        public Mock<IYieldPersistenceService> YieldPersistenceServiceMock { get; } = new();
        public Mock<IPortfolioValuationRepository> PortfolioValuationRepositoryMock { get; } = new();
        public Mock<IRunSimulationValidationReader> ValidationReaderMock { get; } = new();
        public Mock<IYieldDetailRepository> YieldDetailRepositoryMock { get; } = new();
        public Mock<IYieldRepository> YieldRepositoryMock { get; } = new();
        public Mock<IPortfolioValidator> PortfolioValidatorMock { get; } = new();
        public Mock<IWarningCollector> WarningCollectorMock { get; } = new();
        public Mock<IPreclosingCleanupService> PreclosingCleanupServiceMock { get; } = new();
        public YieldDetailBuilderService YieldDetailBuilderService { get; }

        public Mock<IClosingBusinessRules> ClosingBusinessRulesMock { get; } = new();

        public SimulationOrchestratorTestContext()
        {
            YieldDetailBuilderService = new YieldDetailBuilderService(new IYieldDetailBuilder[]
            {
                new ExtraReturnYieldDetailBuilder()
            });

            UnitOfWorkMock
                .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            YieldPersistenceServiceMock
                .Setup(service => service.ConsolidateAsync(It.IsAny<RunSimulationParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SimulatedYieldResult());

            WarningCollectorMock
                .Setup(collector => collector.GetAll())
                .Returns(Array.Empty<WarningItem>());

            PreclosingCleanupServiceMock
                .Setup(service => service.CleanAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            YieldDetailCreationServiceMock
                .Setup(service => service.CreateYieldDetailsAsync(
                    It.IsAny<IEnumerable<YieldDetail>>(),
                    It.IsAny<PersistenceMode>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            ValidationReaderMock
                .Setup(reader => reader.ValidateAndDescribeAsync(It.IsAny<RunSimulationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new RunSimulationValidationInfo(
                    false,
                    new FirstDayStateResult(false, false, false, false, false, false, null, Result.Success())
                )));

            PortfolioValidatorMock
                .Setup(validator => validator.GetPortfolioDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new PortfolioData(0, DateTime.UtcNow)));

            PortfolioValuationRepositoryMock
                .Setup(repository => repository.ExistsByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        public SimulationOrchestrator BuildSut() => new(
            UnitOfWorkMock.Object,
            ProfitLossServiceMock.Object,
            CommissionServiceMock.Object,
            MovementsServiceMock.Object,
            ExtraReturnServiceMock.Object,
            YieldDetailCreationServiceMock.Object,
            YieldDetailBuilderService,
            YieldPersistenceServiceMock.Object,
            PortfolioValuationRepositoryMock.Object,
            ValidationReaderMock.Object,
            YieldDetailRepositoryMock.Object,
            YieldRepositoryMock.Object,
            PortfolioValidatorMock.Object,
            WarningCollectorMock.Object,
            PreclosingCleanupServiceMock.Object,
            ClosingBusinessRulesMock.Object
            );

        public void ConfigureClosingPortfolio(int portfolioId, DateTime currentDateUtc, bool isFirstClosingDay)
        {
            PortfolioValidatorMock
                .Setup(validator => validator.GetPortfolioDataAsync(portfolioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new PortfolioData(portfolioId, currentDateUtc)));

            PortfolioValuationRepositoryMock
                .Setup(repository => repository.ExistsByPortfolioAndDateAsync(portfolioId, currentDateUtc, It.IsAny<CancellationToken>()))
                .ReturnsAsync(!isFirstClosingDay);
        }

        public void ConfigureFrontValidation(bool isFirstClosingDay)
        {
            ValidationReaderMock
                .Setup(reader => reader.ValidateAndDescribeAsync(It.IsAny<RunSimulationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new RunSimulationValidationInfo(
                    isFirstClosingDay,
                    new FirstDayStateResult(isFirstClosingDay, false, false, false, false, false, null, Result.Success())
                )));
        }
    }
}
