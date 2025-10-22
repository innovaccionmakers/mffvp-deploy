using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution;
using Closing.Application.Closing.Services.Warnings;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Closing.Integrations.Common;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Closing.test.UnitTests.Application.Closing.Services;

public sealed class ValidateTrustYieldsDistributionServiceTests
{
    private static ConfigurationParameter CreateConfigParam(string jsonMetadata)
    {
        var param = (ConfigurationParameter)FormatterServices.GetUninitializedObject(typeof(ConfigurationParameter));
        var metaProp = typeof(ConfigurationParameter).GetProperty(nameof(ConfigurationParameter.Metadata),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ConfigurationParameter.Metadata not found");

        metaProp.SetValue(param, JsonDocument.Parse(jsonMetadata));
        return param;
    }

    private static ConfigurationParameter CreateConfigParamWithNullMetadata()
    {
        var param = (ConfigurationParameter)FormatterServices.GetUninitializedObject(typeof(ConfigurationParameter));
        return param;
    }

    private static ReadOnlyDictionary<Guid, ConfigurationParameter> Map(params (Guid id, ConfigurationParameter p)[] items)
        => new(new Dictionary<Guid, ConfigurationParameter>(items.ToDictionary(x => x.id, x => x.p)));

    private static ValidateTrustYieldsDistributionService CreateService(
        Mock<IYieldRepository> yieldRepository,
        Mock<ITrustYieldRepository> trustYieldRepository,
        Mock<IYieldDetailCreationService> yieldDetailCreationService,
        YieldDetailBuilderService? yieldDetailBuilderService,
        Mock<ITimeControlService> timeControlService,
        Mock<IConfigurationParameterRepository> configRepo,
        Mock<IWarningCollector> warnings,
        Mock<IPortfolioValuationRepository> portfolioValuationRepository)
    {
        var logger = Mock.Of<ILogger<ValidateTrustYieldsDistributionService>>();
        return new ValidateTrustYieldsDistributionService(
            yieldRepository.Object,
            trustYieldRepository.Object,
            yieldDetailCreationService.Object,
            yieldDetailBuilderService!, // null-forgiving: en estos tests no se invoca cuando falta metadata
            timeControlService.Object,
            configRepo.Object,
            warnings.Object,
            portfolioValuationRepository.Object,
            logger);
    }

    [Fact]
    public async Task ReturnsFailureWhenYieldNotFound()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetYieldToCreditAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((decimal?)null);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var cfg = new Mock<IConfigurationParameterRepository>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess);
        timeCtrl.Verify(t => t.UpdateStepAsync(portfolioId, "ClosingAllocationCheck", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        yieldRepo.Verify(y => y.UpdateCreditedYieldsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        cfg.Verify(x => x.GetReadOnlyByUuidsAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsSuccessWhenDifferenceIsZero()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetYieldToCreditAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(500m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 500m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(500m);

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var cfg = new Mock<IConfigurationParameterRepository>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        timeCtrl.Verify(t => t.UpdateStepAsync(portfolioId, "ClosingAllocationCheck", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        yieldRepo.Verify(y => y.UpdateCreditedYieldsAsync(portfolioId, closingDate, 500m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        cfg.Verify(x => x.GetReadOnlyByUuidsAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()), Times.Never);
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Never);
        pvRepo.Verify(p => p.ApplyAllocationCheckDiffAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsFailureWhenToleranceMetadataMissing()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetYieldToCreditAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(500m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 450m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(450m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        var map = Map(
            (ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, CreateConfigParamWithNullMetadata())
        // Income/Expense no incluidos: bastará con fallar antes por metadata null de tolerancia
        );
        cfg.Setup(x => x.GetReadOnlyByUuidsAsync(
                It.Is<Guid[]>(ids => ids.Contains(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentIncome)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentExpense)),
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(map);

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess);
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Never);
        pvRepo.Verify(p => p.ApplyAllocationCheckDiffAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsFailureWhenToleranceIsNegative()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetYieldToCreditAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(500m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 450m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(450m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        var map = Map(
            (ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, CreateConfigParam("""{"valor": -0.01}"""))
        );
        cfg.Setup(x => x.GetReadOnlyByUuidsAsync(
                It.Is<Guid[]>(ids => ids.Contains(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentIncome)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentExpense)),
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(map);

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess);
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Never);
        pvRepo.Verify(p => p.ApplyAllocationCheckDiffAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddsWarningWhenDifferenceExceedsTolerance()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetYieldToCreditAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(500m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 495m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(495m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        // Solo tolerancia = 1; omitimos metadata de concepto para que falle después del warning
        var map = Map(
            (ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, CreateConfigParam("""{"valor": 1}"""))
        );
        cfg.Setup(x => x.GetReadOnlyByUuidsAsync(
                It.Is<Guid[]>(ids => ids.Contains(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentIncome)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentExpense)),
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(map);

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess); // falla por metadata faltante del concepto
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Once);
        timeCtrl.Verify(t => t.UpdateStepAsync(portfolioId, "ClosingAllocationCheck", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        yieldRepo.Verify(y => y.UpdateCreditedYieldsAsync(portfolioId, closingDate, 495m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DoesNotAddWarningWhenDifferenceWithinTolerance()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetYieldToCreditAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(500m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 499.5m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(499.5m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        // Tolerancia 1 y concepto con metadata inválida para provocar Failure sin warning
        var map = Map(
            (ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, CreateConfigParam("""{"valor": 1}""")),
            (ConfigurationParameterUuids.Closing.YieldAdjustmentIncome, CreateConfigParam("""{"id": 0, "nombre": "Ajuste Ingreso"}"""))
        );
        cfg.Setup(x => x.GetReadOnlyByUuidsAsync(
                It.Is<Guid[]>(ids => ids.Contains(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentIncome)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentExpense)),
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(map);

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess); // metadata inválida del concepto
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Never);
        pvRepo.Verify(p => p.ApplyAllocationCheckDiffAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsFailureWhenAdjustmentMetadataMissing()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetYieldToCreditAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(500m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 450m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(450m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        // Tolerancia chica para forzar warning; omitimos concepto para que falle por metadata faltante
        var map = Map(
            (ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, CreateConfigParam("""{"valor": 0.01}"""))
        );
        cfg.Setup(x => x.GetReadOnlyByUuidsAsync(
                It.Is<Guid[]>(ids => ids.Contains(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentIncome)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentExpense)),
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(map);

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess);
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Once);
        pvRepo.Verify(p => p.ApplyAllocationCheckDiffAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsFailureWhenAdjustmentMetadataInvalid()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetYieldToCreditAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(500m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 450m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(450m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        // Tolerancia pequeña y concepto inválido para disparar warning + failure
        var map = Map(
            (ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, CreateConfigParam("""{"valor": 0.01}""")),
            (ConfigurationParameterUuids.Closing.YieldAdjustmentIncome, CreateConfigParam("""{"id": 0, "nombre": ""}"""))
        );
        cfg.Setup(x => x.GetReadOnlyByUuidsAsync(
                It.Is<Guid[]>(ids => ids.Contains(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentIncome)
                                   && ids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentExpense)),
                It.IsAny<CancellationToken>()))
           .ReturnsAsync(map);

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess);
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Once);
        pvRepo.Verify(p => p.ApplyAllocationCheckDiffAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
