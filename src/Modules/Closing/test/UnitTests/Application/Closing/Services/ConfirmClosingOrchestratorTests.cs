using System;
using System.Threading;
using System.Threading.Tasks;
using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Abort;
using Closing.Application.Closing.Services.DistributableReturns.Interfaces;
using Closing.Application.Closing.Services.Orchestration;
using Closing.Application.Closing.Services.ReturnsOperations.Interfaces;
using Closing.Application.Closing.Services.Telemetry;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Application.Closing.Services.Validation;
using Closing.Application.Closing.Services.Warnings;
using Closing.Integrations.Common;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Closing.test.UnitTests.Application.Closing.Services;

public sealed class ConfirmClosingOrchestratorTests
{
    private static ConfirmClosingOrchestrator CreateOrchestrator(
        Mock<IDistributeTrustYieldsService> distributeTrustYieldsService,
        Mock<IDistributableReturnsService> distributableReturnsService,
        Mock<IValidateTrustYieldsDistributionService> validateTrustYieldsDistributionService,
        Mock<IReturnsOperationsService> returnsOperationsService,
        Mock<IWarningCollector> warningCollector,
        Mock<IAbortClosingService> abortClosingService,
        Mock<IUnitOfWork> unitOfWork,
        Mock<IClosingStepTimer> stepTimer,
        Mock<IClosingBusinessRules> rules)
    {
        stepTimer
            .Setup(timer => timer.Track(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<string?>()))
            .Returns(Mock.Of<IDisposable>());

        warningCollector.Setup(collector => collector.GetAll()).Returns(Array.Empty<WarningItem>());

        unitOfWork
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        abortClosingService
            .Setup(service => service.AbortAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Default: NO es primer día de cierre a menos que el test lo sobreescriba.
        rules
            .Setup(r => r.IsFirstClosingDayAsync(It.IsAny<int>(),It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        rules
        .Setup(r => r.HasDebitNotesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(true));

        return new ConfirmClosingOrchestrator(
            distributeTrustYieldsService.Object,
            distributableReturnsService.Object,
            validateTrustYieldsDistributionService.Object,
            returnsOperationsService.Object,
            warningCollector.Object,
            abortClosingService.Object,
            unitOfWork.Object,
            stepTimer.Object,
            Mock.Of<ILogger<ConfirmClosingOrchestrator>>(),
            rules.Object);
    }

    [Fact]
    public async Task ConfirmAsync_AllStepsSuccessful_ReturnsSuccess()
    {
        const int portfolioId = 11;
        var closingDate = new DateTime(2024, 1, 31);

        var distributeTrustYieldsService = new Mock<IDistributeTrustYieldsService>();
        distributeTrustYieldsService
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var distributableReturnsService = new Mock<IDistributableReturnsService>();
        distributableReturnsService
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var validateTrustYieldsDistributionService = new Mock<IValidateTrustYieldsDistributionService>();
        validateTrustYieldsDistributionService
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var returnsOperationsService = new Mock<IReturnsOperationsService>();
        returnsOperationsService
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var warningCollector = new Mock<IWarningCollector>();
        warningCollector.Setup(collector => collector.GetAll()).Returns(Array.Empty<WarningItem>());

        var abortClosingService = new Mock<IAbortClosingService>();
        abortClosingService
            .Setup(service => service.AbortAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var stepTimer = new Mock<IClosingStepTimer>();
        stepTimer
            .Setup(timer => timer.Track(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<string?>()))
            .Returns(Mock.Of<IDisposable>());

        var rules = new Mock<IClosingBusinessRules>();
        // Asegurar que NO es primer día (ejecuta todos los pasos)

        rules
           .Setup(r => r.IsFirstClosingDayAsync(portfolioId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(false);

        rules
           .Setup(r => r.HasDebitNotesAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(Result.Success(true));

        var orchestrator = new ConfirmClosingOrchestrator(
            distributeTrustYieldsService.Object,
            distributableReturnsService.Object,
            validateTrustYieldsDistributionService.Object,
            returnsOperationsService.Object,
            warningCollector.Object,
            abortClosingService.Object,
            unitOfWork.Object,
            stepTimer.Object,
            Mock.Of<ILogger<ConfirmClosingOrchestrator>>(),
            rules.Object);

        var result = await orchestrator.ConfirmAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(portfolioId, result.Value.PortfolioId);
        Assert.False(result.Value.HasWarnings);

        distributeTrustYieldsService.Verify(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        distributableReturnsService.Verify(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        validateTrustYieldsDistributionService.Verify(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        returnsOperationsService.Verify(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        abortClosingService.Verify(service => service.AbortAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_DistributableReturnsFails_ReturnsFailure()
    {
        const int portfolioId = 21;
        var closingDate = new DateTime(2024, 2, 29);

        var distribution = new Mock<IDistributeTrustYieldsService>();
        distribution
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var distributableReturnsService = new Mock<IDistributableReturnsService>();
        var failure = new Error("DR-FAIL", "error", ErrorType.Failure);
        distributableReturnsService
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(failure));

        var validation = new Mock<IValidateTrustYieldsDistributionService>();
        var returnsOperations = new Mock<IReturnsOperationsService>();
        var warningCollector = new Mock<IWarningCollector>();
        warningCollector.Setup(collector => collector.GetAll()).Returns(Array.Empty<WarningItem>());
        var abort = new Mock<IAbortClosingService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var timer = new Mock<IClosingStepTimer>();
        var rules = new Mock<IClosingBusinessRules>();

        var orchestrator = CreateOrchestrator(
            distribution,
            distributableReturnsService,
            validation,
            returnsOperations,
            warningCollector,
            abort,
            unitOfWork,
            timer,
            rules);

        var result = await orchestrator.ConfirmAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(failure.Code, result.Error.Code);

        returnsOperations.Verify(service => service.RunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        validation.Verify(service => service.RunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_ReturnsOperationsFails_ReturnsFailure()
    {
        const int portfolioId = 31;
        var closingDate = new DateTime(2024, 3, 15);

        var distribution = new Mock<IDistributeTrustYieldsService>();
        distribution
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var distributableReturns = new Mock<IDistributableReturnsService>();
        distributableReturns
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var validation = new Mock<IValidateTrustYieldsDistributionService>();
        validation
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var returnsOperations = new Mock<IReturnsOperationsService>();
        var failure = new Error("RO-FAIL", "error", ErrorType.Failure);
        returnsOperations
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(failure));

        var warningCollector = new Mock<IWarningCollector>();
        warningCollector.Setup(collector => collector.GetAll()).Returns(Array.Empty<WarningItem>());
        var abort = new Mock<IAbortClosingService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var timer = new Mock<IClosingStepTimer>();
        var rules = new Mock<IClosingBusinessRules>();

        var orchestrator = CreateOrchestrator(
            distribution,
            distributableReturns,
            validation,
            returnsOperations,
            warningCollector,
            abort,
            unitOfWork,
            timer,
            rules);

        var result = await orchestrator.ConfirmAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(failure.Code, result.Error.Code);

        validation.Verify(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        returnsOperations.Verify(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), true, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_ValidationFails_ReturnsFailure()
    {
        const int portfolioId = 41;
        var closingDate = new DateTime(2024, 4, 30);

        var distribution = new Mock<IDistributeTrustYieldsService>();
        distribution
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var distributableReturns = new Mock<IDistributableReturnsService>();
        distributableReturns
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var validationFailure = new Error("VAL-FAIL", "error", ErrorType.Failure);
        var validation = new Mock<IValidateTrustYieldsDistributionService>();
        validation
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(validationFailure));

        var returnsOperations = new Mock<IReturnsOperationsService>();
        returnsOperations
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var warningCollector = new Mock<IWarningCollector>();
        warningCollector.Setup(collector => collector.GetAll()).Returns(Array.Empty<WarningItem>());
        var abort = new Mock<IAbortClosingService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var timer = new Mock<IClosingStepTimer>();
        var rules = new Mock<IClosingBusinessRules>();

        var orchestrator = CreateOrchestrator(
            distribution,
            distributableReturns,
            validation,
            returnsOperations,
            warningCollector,
            abort,
            unitOfWork,
            timer,
            rules);

        var result = await orchestrator.ConfirmAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(validationFailure.Code, result.Error.Code);

        // Si validation falla, returnsOperations no se ejecuta
        returnsOperations.Verify(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), true, It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_OnUnexpectedException_AbortsClosing()
    {
        const int portfolioId = 51;
        var closingDate = new DateTime(2024, 5, 31);

        var distribution = new Mock<IDistributeTrustYieldsService>();
        distribution
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var distributableReturns = new Mock<IDistributableReturnsService>();
        var validation = new Mock<IValidateTrustYieldsDistributionService>();
        var returnsOperations = new Mock<IReturnsOperationsService>();
        var warningCollector = new Mock<IWarningCollector>();
        warningCollector.Setup(collector => collector.GetAll()).Returns(Array.Empty<WarningItem>());

        var abort = new Mock<IAbortClosingService>();
        abort
            .Setup(service => service.AbortAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var unitOfWork = new Mock<IUnitOfWork>();
        var timer = new Mock<IClosingStepTimer>();
        var rules = new Mock<IClosingBusinessRules>();

        var orchestrator = CreateOrchestrator(
            distribution,
            distributableReturns,
            validation,
            returnsOperations,
            warningCollector,
            abort,
            unitOfWork,
            timer,
            rules);

        var result = await orchestrator.ConfirmAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsFailure);
        // No asumimos un código fijo: basta con que sea fallo y que se aborte
        Assert.False(result.IsSuccess);

        abort.Verify(service => service.AbortAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        returnsOperations.Verify(service => service.RunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        validation.Verify(service => service.RunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_WhenIsFirstClosingDay_SkipsDistributableReturnsAndReturnsOperations()
    {
        const int portfolioId = 61;
        var closingDate = new DateTime(2024, 6, 30);

        var distributeTrustYieldsService = new Mock<IDistributeTrustYieldsService>();
        distributeTrustYieldsService
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var distributableReturnsService = new Mock<IDistributableReturnsService>();
        var validateTrustYieldsDistributionService = new Mock<IValidateTrustYieldsDistributionService>();
        validateTrustYieldsDistributionService
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var returnsOperationsService = new Mock<IReturnsOperationsService>();
        var warningCollector = new Mock<IWarningCollector>();
        warningCollector.Setup(collector => collector.GetAll()).Returns(Array.Empty<WarningItem>());

        var abortClosingService = new Mock<IAbortClosingService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var stepTimer = new Mock<IClosingStepTimer>();
        stepTimer
            .Setup(timer => timer.Track(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<string?>()))
            .Returns(Mock.Of<IDisposable>());

        var rules = new Mock<IClosingBusinessRules>();
        // Aquí sí debe ser primer día para que se salte DistributableReturns y ReturnsOperations
        rules
            .Setup(r => r.IsFirstClosingDayAsync(portfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var orchestrator = new ConfirmClosingOrchestrator(
            distributeTrustYieldsService.Object,
            distributableReturnsService.Object,
            validateTrustYieldsDistributionService.Object,
            returnsOperationsService.Object,
            warningCollector.Object,
            abortClosingService.Object,
            unitOfWork.Object,
            stepTimer.Object,
            Mock.Of<ILogger<ConfirmClosingOrchestrator>>(),
            rules.Object);

        var result = await orchestrator.ConfirmAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(portfolioId, result.Value.PortfolioId);

        // Verificar que se ejecutan los servicios esperados
        distributeTrustYieldsService.Verify(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        validateTrustYieldsDistributionService.Verify(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verificar que NO se ejecutan DistributableReturns y ReturnsOperations
        distributableReturnsService.Verify(service => service.RunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        returnsOperationsService.Verify(service => service.RunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_WhenHasDebitNotesIsFalse_SkipsDistributableReturnsAndReturnsOperations()
    {
        const int portfolioId = 71;
        var closingDate = new DateTime(2024, 7, 31);

        var distribution = new Mock<IDistributeTrustYieldsService>();
        distribution
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var distributableReturnsService = new Mock<IDistributableReturnsService>();
        distributableReturnsService
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var validation = new Mock<IValidateTrustYieldsDistributionService>();
        validation
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var returnsOperations = new Mock<IReturnsOperationsService>();
        returnsOperations
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var warningCollector = new Mock<IWarningCollector>();
        warningCollector.Setup(collector => collector.GetAll()).Returns(Array.Empty<WarningItem>());

        var abort = new Mock<IAbortClosingService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var timer = new Mock<IClosingStepTimer>();
        var rules = new Mock<IClosingBusinessRules>();

        var orchestrator = CreateOrchestrator(
            distribution,
            distributableReturnsService,
            validation,
            returnsOperations,
            warningCollector,
            abort,
            unitOfWork,
            timer,
            rules);

        // No es primer día
        rules
            .Setup(r => r.IsFirstClosingDayAsync(portfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // HasDebitNotes = false
        rules
            .Setup(r => r.HasDebitNotesAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(false));

        var result = await orchestrator.ConfirmAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(portfolioId, result.Value.PortfolioId);

        // Se ejecutan siempre
        distribution.Verify(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        validation.Verify(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Deben quedar saltados cuando HasDebitNotes = false
        distributableReturnsService.Verify(service => service.RunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        returnsOperations.Verify(service => service.RunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_WhenHasDebitNotesIsFalse_AndValidationFails_ReturnsFailureWithoutDistributableReturnsOrReturnsOperations()
    {
        const int portfolioId = 81;
        var closingDate = new DateTime(2024, 8, 31);

        var distribution = new Mock<IDistributeTrustYieldsService>();
        distribution
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var distributableReturnsService = new Mock<IDistributableReturnsService>();
        distributableReturnsService
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var validationError = new Error("VAL-ND-FAIL", "error", ErrorType.Failure);
        var validation = new Mock<IValidateTrustYieldsDistributionService>();
        validation
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(validationError));

        var returnsOperations = new Mock<IReturnsOperationsService>();
        returnsOperations
            .Setup(service => service.RunAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var warningCollector = new Mock<IWarningCollector>();
        warningCollector.Setup(collector => collector.GetAll()).Returns(Array.Empty<WarningItem>());

        var abort = new Mock<IAbortClosingService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var timer = new Mock<IClosingStepTimer>();
        var rules = new Mock<IClosingBusinessRules>();

        var orchestrator = CreateOrchestrator(
            distribution,
            distributableReturnsService,
            validation,
            returnsOperations,
            warningCollector,
            abort,
            unitOfWork,
            timer,
            rules);

        // No es primer día
        rules
            .Setup(r => r.IsFirstClosingDayAsync(portfolioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // HasDebitNotes = false
        rules
            .Setup(r => r.HasDebitNotesAsync(portfolioId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(false));

        var result = await orchestrator.ConfirmAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(validationError.Code, result.Error.Code);

        // Nunca deben ejecutarse cuando falla la validación y HasDebitNotes = false
        distributableReturnsService.Verify(service => service.RunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        returnsOperations.Verify(service => service.RunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);

        // Tampoco se persiste nada
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


}
