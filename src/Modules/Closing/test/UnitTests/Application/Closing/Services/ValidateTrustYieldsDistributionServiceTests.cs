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
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Closing.test.UnitTests.Application.Closing.Services;

public sealed class ValidateTrustYieldsDistributionServiceTests
{
    private static Yield CreateYield(
        int portfolioId,
        DateTime closingDateUtc,
        decimal income,
        decimal expenses,
        decimal commissions,
        decimal costs,
        decimal yieldToCredit,
        bool isClosed)
    {
        var y = (Yield)FormatterServices.GetUninitializedObject(typeof(Yield));

        SetProp(y, nameof(Yield.PortfolioId), portfolioId);
        SetProp(y, nameof(Yield.ClosingDate), closingDateUtc);
        SetProp(y, nameof(Yield.Income), income);
        SetProp(y, nameof(Yield.Expenses), expenses);
        SetProp(y, nameof(Yield.Commissions), commissions);
        SetProp(y, nameof(Yield.Costs), costs);
        SetProp(y, nameof(Yield.YieldToCredit), yieldToCredit);
        SetProp(y, nameof(Yield.IsClosed), isClosed);

        return y;

        static void SetProp(object obj, string name, object? value)
        {
            var p = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException($"Property {name} not found on {obj.GetType().Name}");
            p.SetValue(obj, value);
        }
    }

    private static TrustYield CreateTrustYield(
        int portfolioId,
        long trustId,
        DateTime closingDateUtc,
        decimal yieldAmount)
    {
        var ty = (TrustYield)FormatterServices.GetUninitializedObject(typeof(TrustYield));

        SetProp(ty, nameof(TrustYield.PortfolioId), portfolioId);
        SetProp(ty, nameof(TrustYield.TrustId), trustId);
        SetProp(ty, nameof(TrustYield.ClosingDate), closingDateUtc);
        SetProp(ty, nameof(TrustYield.YieldAmount), yieldAmount);

        return ty;

        static void SetProp(object obj, string name, object? value)
        {
            var p = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException($"Property {name} not found on {obj.GetType().Name}");
            p.SetValue(obj, value);
        }
    }

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
            yieldDetailBuilderService!, 
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
        yieldRepo.Setup(x => x.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Yield?)null);

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
    }

    [Fact]
    public async Task ReturnsFailureWhenNoTrustYields()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldEntity = CreateYield(portfolioId, closingDate, 1000m, 100m, 10m, 110m, 500m, isClosed: true);

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(yieldEntity);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Array.Empty<TrustYield>());

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var cfg = new Mock<IConfigurationParameterRepository>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess);
        yieldRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never); // we bail before SaveChanges
        timeCtrl.Verify(t => t.UpdateStepAsync(portfolioId, "ClosingAllocationCheck", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReturnsSuccessWhenDifferenceIsZero()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldEntity = CreateYield(portfolioId, closingDate, 1000m, 100m, 10m, 110m, 500m, isClosed: true);
        var t1 = CreateTrustYield(portfolioId, 101, closingDate, 200m);
        var t2 = CreateTrustYield(portfolioId, 102, closingDate, 300m);

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(yieldEntity);
        yieldRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new[] { t1, t2 });

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null; 
        var timeCtrl = new Mock<ITimeControlService>();
        var cfg = new Mock<IConfigurationParameterRepository>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        yieldRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cfg.Verify(x => x.GetByUuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Never);
        pvRepo.Verify(p => p.ApplyAllocationCheckDiffAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsFailureWhenToleranceMetadataMissing()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldEntity = CreateYield(portfolioId, closingDate, 0m, 0m, 0m, 0m, 500m, isClosed: true);
        var ty = CreateTrustYield(portfolioId, 201, closingDate, 450m); 

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(yieldEntity);
        yieldRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new[] { ty });

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParamWithNullMetadata());

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

        var yieldEntity = CreateYield(portfolioId, closingDate, 0m, 0m, 0m, 0m, 500m, isClosed: true);
        var ty = CreateTrustYield(portfolioId, 301, closingDate, 450m); 

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(yieldEntity);
        yieldRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new[] { ty });

        // tolerance = -0.01 (invalid)
        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"valor": -0.01}"""));

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

        var yieldEntity = CreateYield(portfolioId, closingDate, 0m, 0m, 0m, 0m, 500m, isClosed: true);
        var ty = CreateTrustYield(portfolioId, 401, closingDate, 495m); 

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(yieldEntity);
        yieldRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new[] { ty });

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"valor": 1}"""));
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentIncome, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParamWithNullMetadata());

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess); 
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Once);
    }

    [Fact]
    public async Task DoesNotAddWarningWhenDifferenceWithinTolerance()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldEntity = CreateYield(portfolioId, closingDate, 0m, 0m, 0m, 0m, 500m, isClosed: true);
        var ty = CreateTrustYield(portfolioId, 501, closingDate, 499.50m); 

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(yieldEntity);
        yieldRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new[] { ty });

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"valor": 1}"""));
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentIncome, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"id": 0, "nombre": "Ajuste Ingreso"}"""));

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess); 
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsFailureWhenAdjustmentMetadataMissing()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldEntity = CreateYield(portfolioId, closingDate, 0m, 0m, 0m, 0m, 500m, isClosed: true);
        var ty = CreateTrustYield(portfolioId, 601, closingDate, 450m); 

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(yieldEntity);
        yieldRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new[] { ty });

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"valor": 0.01}""")); 
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentIncome, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParamWithNullMetadata());

        var creation = new Mock<IYieldDetailCreationService>();
        YieldDetailBuilderService? builder = null;
        var timeCtrl = new Mock<ITimeControlService>();
        var warnings = new Mock<IWarningCollector>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();

        var svc = CreateService(yieldRepo, trustYieldRepo, creation, builder, timeCtrl, cfg, warnings, pvRepo);

        var result = await svc.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.False(result.IsSuccess);
        warnings.Verify(w => w.Add(It.IsAny<WarningItem>()), Times.Once); 
    }

    [Fact]
    public async Task ReturnsFailureWhenAdjustmentMetadataInvalid()
    {
        var portfolioId = 1;
        var closingDate = new DateTime(2025, 10, 7);

        var yieldEntity = CreateYield(portfolioId, closingDate, 0m, 0m, 0m, 0m, 500m, isClosed: true);
        var ty = CreateTrustYield(portfolioId, 701, closingDate, 450m); 

        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetForUpdateByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(yieldEntity);
        yieldRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new[] { ty });

        var cfg = new Mock<IConfigurationParameterRepository>();
        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"valor": 0.01}"""));

        cfg.Setup(x => x.GetByUuidAsync(ConfigurationParameterUuids.Closing.YieldAdjustmentIncome, It.IsAny<CancellationToken>()))
           .ReturnsAsync(CreateConfigParam("""{"id": 0, "nombre": ""}"""));

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