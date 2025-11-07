using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution;
using Closing.Application.Closing.Services.Warnings;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Closing.Domain.YieldsToDistribute;
using Closing.Integrations.Common;
using Closing.Integrations.PreClosing.RunSimulation;
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
        Mock<IYieldToDistributeRepository> yieldToDistributeRepository,
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
            yieldToDistributeRepository.Object,
            yieldDetailCreationService.Object,
            yieldDetailBuilderService!, 
            timeControlService.Object,
            configRepo.Object,
            warnings.Object,
            portfolioValuationRepository.Object
            );
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
        var yieldToDistributeRepo = new Mock<IYieldToDistributeRepository>();
        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var cfg = new Mock<IConfigurationParameterRepository>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, yieldToDistributeRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

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

        // UpdateCreditedYieldsAsync se llama con lo distribuido (500)
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 500m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(500m);

        // Para que difference sea 0: pending = 0
        var yieldToDistributeRepo = new Mock<IYieldToDistributeRepository>();
        yieldToDistributeRepo.Setup(x => x.GetTotalYieldAmountRoundedAsync(portfolioId, closingDate, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(0m);

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"id": 1, "nombre": "Ajuste Rendimiento Nota Contable"}"""));

        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, yieldToDistributeRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        timeCtrl.Verify(t => t.UpdateStepAsync(portfolioId, "ClosingAllocationCheck", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        yieldRepo.Verify(y => y.UpdateCreditedYieldsAsync(portfolioId, closingDate, 500m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        // No entra al bloque de diferencia ≠ 0
        cfg.Verify(x => x.GetReadOnlyByUuidsAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()), Times.Never);
        // Sí consulta el parámetro para filtrar los "por distribuir"
        cfg.Verify(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, It.IsAny<CancellationToken>()), Times.Once);
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
        yieldRepo.Setup(x => x.GetCreditedYieldsAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(200m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 450m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(450m);

        var yieldToDistributeRepo = new Mock<IYieldToDistributeRepository>();
        yieldToDistributeRepo.Setup(x => x.GetTotalYieldAmountRoundedAsync(portfolioId, closingDate, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(250m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"id": 1, "nombre": "Ajuste Rendimiento Nota Contable"}"""));
        var map = Map(
            (ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, CreateConfigParamWithNullMetadata())
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

        var svc = CreateService(yieldRepo, trustYieldRepo, yieldToDistributeRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

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
        yieldRepo.Setup(x => x.GetCreditedYieldsAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(200m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 450m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(450m);

        var yieldToDistributeRepo = new Mock<IYieldToDistributeRepository>();
        yieldToDistributeRepo.Setup(x => x.GetTotalYieldAmountRoundedAsync(portfolioId, closingDate, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(250m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"id": 1, "nombre": "Ajuste Rendimiento Nota Contable"}"""));
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

        var svc = CreateService(yieldRepo, trustYieldRepo, yieldToDistributeRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

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
        yieldRepo.Setup(x => x.GetCreditedYieldsAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(200m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 495m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(495m);

        var yieldToDistributeRepo = new Mock<IYieldToDistributeRepository>();
        yieldToDistributeRepo.Setup(x => x.GetTotalYieldAmountRoundedAsync(portfolioId, closingDate, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(295m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"id": 1, "nombre": "Ajuste Rendimiento Nota Contable"}"""));
        // Solo tolerancia = 1; se omite metadata de concepto para que falle después del warning
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

        var svc = CreateService(yieldRepo, trustYieldRepo, yieldToDistributeRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

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

        // Se actualiza con lo distribuido (499.5)
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 499.5m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(499.5m);

        // Para estar dentro de tolerancia (1): difference = 500 - (499.5 + 0) = +0.5
        var yieldToDistributeRepo = new Mock<IYieldToDistributeRepository>();
        yieldToDistributeRepo.Setup(x => x.GetTotalYieldAmountRoundedAsync(portfolioId, closingDate, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(0m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"id": 1, "nombre": "Ajuste Rendimiento Nota Contable"}"""));

        // Tolerancia = 1 y concepto ingreso inválido para forzar Failure sin warning
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

        var svc = CreateService(yieldRepo, trustYieldRepo, yieldToDistributeRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        // Falla por metadata inválida del concepto, pero SIN warning (porque |difference| <= tolerance)
        Assert.False(result.IsSuccess);
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
        yieldRepo.Setup(x => x.GetCreditedYieldsAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(200m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 450m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(450m);

        var yieldToDistributeRepo = new Mock<IYieldToDistributeRepository>();
        yieldToDistributeRepo.Setup(x => x.GetTotalYieldAmountRoundedAsync(portfolioId, closingDate, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(250m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"id": 1, "nombre": "Ajuste Rendimiento Nota Contable"}"""));
        // Tolerancia chica para forzar warning; se omite concepto para que falle por metadata faltante
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

        var svc = CreateService(yieldRepo, trustYieldRepo, yieldToDistributeRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

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
        yieldRepo.Setup(x => x.GetCreditedYieldsAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(200m);
        yieldRepo.Setup(x => x.UpdateCreditedYieldsAsync(portfolioId, closingDate, 450m, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetDistributedTotalRoundedAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(450m);

        var yieldToDistributeRepo = new Mock<IYieldToDistributeRepository>();
        yieldToDistributeRepo.Setup(x => x.GetTotalYieldAmountRoundedAsync(portfolioId, closingDate, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(250m);

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"id": 1, "nombre": "Ajuste Rendimiento Nota Contable"}"""));
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

        var svc = CreateService(yieldRepo, trustYieldRepo, yieldToDistributeRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess);
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Once);
        pvRepo.Verify(p => p.ApplyAllocationCheckDiffAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CallsNewRepositoryMethodsForCreditedYieldsAndPendingDistribution()
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

        // difference = 500 - (500 + 0) = 0 → Success
        var yieldToDistributeRepo = new Mock<IYieldToDistributeRepository>();
        yieldToDistributeRepo.Setup(x => x.GetTotalYieldAmountRoundedAsync(portfolioId, closingDate, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(0m);

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"id": 1, "nombre": "Ajuste Rendimiento Nota Contable"}"""));

        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, yieldToDistributeRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        // Se llama al nuevo repo con el conceptJson (puede ser null si la metadata fuese inválida)
        yieldToDistributeRepo.Verify(y => y.GetTotalYieldAmountRoundedAsync(portfolioId, closingDate, It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        // Siempre se consulta el parámetro para filtrar "por distribuir"
        cfg.Verify(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, It.IsAny<CancellationToken>()), Times.Once);
        // No hay ajuste de valoración porque difference == 0
        pvRepo.Verify(p => p.ApplyAllocationCheckDiffAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
