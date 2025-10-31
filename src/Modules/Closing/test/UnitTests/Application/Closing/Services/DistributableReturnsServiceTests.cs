using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Closing.Application.Closing.Services.DistributableReturns;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Domain.ClientOperations;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Closing.Domain.YieldsToDistribute;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Closing.test.UnitTests.Application.Closing.Services;

public sealed class DistributableReturnsServiceTests
{
    private static DistributableReturnsService CreateService(
        Mock<ITrustYieldRepository> trustYieldRepository,
        Mock<IClientOperationRepository> clientOperationRepository,
        Mock<IYieldRepository> yieldRepository,
        Mock<IPortfolioValuationRepository> portfolioValuationRepository,
        Mock<IConfigurationParameterRepository> configurationParameterRepository,
        Mock<IYieldToDistributeRepository> yieldToDistributeRepository,
        Mock<ITimeControlService> timeControlService)
    {
        var logger = Mock.Of<ILogger<DistributableReturnsService>>();
        return new DistributableReturnsService(
            trustYieldRepository.Object,
            clientOperationRepository.Object,
            yieldRepository.Object,
            portfolioValuationRepository.Object,
            configurationParameterRepository.Object,
            yieldToDistributeRepository.Object,
            timeControlService.Object,
            logger);
    }

    private static ConfigurationParameter CreateParameter(string json)
    {
        var parameter = (ConfigurationParameter)FormatterServices.GetUninitializedObject(typeof(ConfigurationParameter));
        var metadataProp = typeof(ConfigurationParameter).GetProperty(nameof(ConfigurationParameter.Metadata), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ConfigurationParameter.Metadata not found");
        metadataProp.SetValue(parameter, JsonDocument.Parse(json));
        return parameter;
    }

    private static TrustYield CreateTrustYield(
        long id,
        long trustId,
        int portfolioId,
        DateTime closingDateUtc,
        decimal closingBalance,
        decimal preClosingBalance = 0m)
    {
        var result = TrustYield.Create(
            trustId,
            portfolioId,
            closingDateUtc,
            participation: 0m,
            units: 0m,
            yieldAmount: 0m,
            preClosingBalance: preClosingBalance,
            closingBalance: closingBalance,
            income: 0m,
            expenses: 0m,
            commissions: 0m,
            cost: 0m,
            capital: 0m,
            processDate: closingDateUtc,
            contingentRetention: 0m,
            yieldRetention: 0m);

        var entity = result.Value;
        typeof(TrustYield).GetProperty("TrustYieldId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(entity, id);
        return entity;
    }

    [Fact]
    public async Task ReturnsFailureWhenYieldIsMissing()
    {
        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        var clientOpRepo = new Mock<IClientOperationRepository>();
        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetReadOnlyToDistributeByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((YieldToDistributeDto?)null);
        var pvRepo = new Mock<IPortfolioValuationRepository>();
        var configRepo = new Mock<IConfigurationParameterRepository>();
        var ytdRepo = new Mock<IYieldToDistributeRepository>();
        var timeCtrl = new Mock<ITimeControlService>();

        var service = CreateService(trustYieldRepo, clientOpRepo, yieldRepo, pvRepo, configRepo, ytdRepo, timeCtrl);

        var result = await service.RunAsync(1, new DateTime(2025, 10, 7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        timeCtrl.Verify(t => t.UpdateStepAsync(1, "Closing/DistributableReturns", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReturnsFailureWhenPortfolioValuationMissing()
    {
        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        var clientOpRepo = new Mock<IClientOperationRepository>();
        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetReadOnlyToDistributeByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new YieldToDistributeDto(100m, 0m, 0m, 0m, 0m));

        var pvRepo = new Mock<IPortfolioValuationRepository>();
        pvRepo.Setup(x => x.GetReadOnlyToDistributePortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioValuationClosing?)null);

        var configRepo = new Mock<IConfigurationParameterRepository>();
        var ytdRepo = new Mock<IYieldToDistributeRepository>();
        var timeCtrl = new Mock<ITimeControlService>();

        var service = CreateService(trustYieldRepo, clientOpRepo, yieldRepo, pvRepo, configRepo, ytdRepo, timeCtrl);

        var result = await service.RunAsync(2, new DateTime(2025, 10, 7), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ReturnsSuccessWhenNoCandidatesFound()
    {
        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(x => x.GetReadOnlyByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TrustYield>());

        var clientOpRepo = new Mock<IClientOperationRepository>();
        var yieldRepo = new Mock<IYieldRepository>();
        yieldRepo.Setup(x => x.GetReadOnlyToDistributeByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new YieldToDistributeDto(100m, 0m, 0m, 0m, 0m));

        var pvRepo = new Mock<IPortfolioValuationRepository>();
        pvRepo.Setup(x => x.GetReadOnlyToDistributePortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioValuationClosing(1000m, 10m));

        var configRepo = new Mock<IConfigurationParameterRepository>();
        var ytdRepo = new Mock<IYieldToDistributeRepository>();
        var timeCtrl = new Mock<ITimeControlService>();

        var service = CreateService(trustYieldRepo, clientOpRepo, yieldRepo, pvRepo, configRepo, ytdRepo, timeCtrl);

        var result = await service.RunAsync(3, new DateTime(2025, 10, 7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        ytdRepo.Verify(x => x.InsertRangeAsync(It.IsAny<IEnumerable<YieldToDistribute>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatesYieldToDistributeRecordsWhenConditionsMatch()
    {
        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        var clientOpRepo = new Mock<IClientOperationRepository>();
        var yieldRepo = new Mock<IYieldRepository>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();
        var configRepo = new Mock<IConfigurationParameterRepository>();
        var ytdRepo = new Mock<IYieldToDistributeRepository>();
        var timeCtrl = new Mock<ITimeControlService>();

        var portfolioId = 5;
        var closingDate = new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc);

        var trustYields = new[]
        {
            CreateTrustYield(100, 2001, portfolioId, closingDate, closingBalance: 100m, preClosingBalance: 0m),
            CreateTrustYield(101, 2002, portfolioId, closingDate, closingBalance: 150m, preClosingBalance: 10m)
        };

        trustYieldRepo.Setup(x => x.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trustYields);

        List<long>? deletedIds = null;
        trustYieldRepo.Setup(x => x.DeleteByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<long>, CancellationToken>((ids, _) => deletedIds = ids.ToList())
            .ReturnsAsync(1);

        yieldRepo.Setup(x => x.GetReadOnlyToDistributeByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new YieldToDistributeDto(200m, 0m, 0m, 0m, 0m));

        pvRepo.Setup(x => x.GetReadOnlyToDistributePortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioValuationClosing(1000m, 10m));

        clientOpRepo.Setup(x => x.GetTrustIdsByStatusAndProcessDateAsync(It.IsAny<IEnumerable<long>>(), closingDate, LifecycleStatus.AnnulledByDebitNote, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new long[] { 2001 });

        configRepo.Setup(x => x.GetReadOnlyByUuidsAsync(
                It.Is<IEnumerable<Guid>>(uuids => uuids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteIncome)
                    && uuids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteExpense)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>
            {
                [ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteIncome] = CreateParameter("{\"id\": 3, \"nombre\": \"Ajuste Rendimiento NC Ingreso\"}"),
                [ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteExpense] = CreateParameter("{\"id\": 4, \"nombre\": \"Ajuste Rendimiento NC Gasto\"}")
            });

        List<YieldToDistribute>? captured = null;
        ytdRepo.Setup(x => x.InsertRangeAsync(It.IsAny<IEnumerable<YieldToDistribute>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<YieldToDistribute>, CancellationToken>((rows, _) => captured = rows.ToList())
            .Returns(Task.CompletedTask);
        ytdRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = CreateService(trustYieldRepo, clientOpRepo, yieldRepo, pvRepo, configRepo, ytdRepo, timeCtrl);

        var result = await service.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        var item = Assert.Single(captured!);
        Assert.Equal(2001, item.TrustId);
        Assert.Equal(portfolioId, item.PortfolioId);
        Assert.Equal(closingDate, item.ClosingDate);
        Assert.Equal(closingDate, item.ApplicationDate);
        Assert.Equal(0.1m, item.Participation);
        Assert.Equal(20m, item.YieldAmount);
        Assert.Equal(3, item.Concept.RootElement.GetProperty("id").GetInt32());
        Assert.Equal("Ajuste Rendimiento NC Ingreso", item.Concept.RootElement.GetProperty("nombre").GetString());
        Assert.NotNull(deletedIds);
        Assert.Equal(new[] { 100L }, deletedIds!);
        ytdRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UsesExpenseConceptWhenYieldAmountIsNegative()
    {
        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        var clientOpRepo = new Mock<IClientOperationRepository>();
        var yieldRepo = new Mock<IYieldRepository>();
        var pvRepo = new Mock<IPortfolioValuationRepository>();
        var configRepo = new Mock<IConfigurationParameterRepository>();
        var ytdRepo = new Mock<IYieldToDistributeRepository>();
        var timeCtrl = new Mock<ITimeControlService>();

        var portfolioId = 6;
        var closingDate = new DateTime(2025, 10, 8, 0, 0, 0, DateTimeKind.Utc);

        var trustYields = new[]
        {
            CreateTrustYield(200, 3001, portfolioId, closingDate, closingBalance: 50m, preClosingBalance: 0m)
        };

        trustYieldRepo.Setup(x => x.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trustYields);

        yieldRepo.Setup(x => x.GetReadOnlyToDistributeByPortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new YieldToDistributeDto(-100m, 0m, 0m, 0m, 0m));

        pvRepo.Setup(x => x.GetReadOnlyToDistributePortfolioAndDateAsync(portfolioId, closingDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioValuationClosing(200m, 10m));

        clientOpRepo.Setup(x => x.GetTrustIdsByStatusAndProcessDateAsync(It.IsAny<IEnumerable<long>>(), closingDate, LifecycleStatus.AnnulledByDebitNote, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new long[] { 3001 });

        configRepo.Setup(x => x.GetReadOnlyByUuidsAsync(
                It.Is<IEnumerable<Guid>>(uuids => uuids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteIncome)
                    && uuids.Contains(ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteExpense)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>
            {
                [ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteIncome] = CreateParameter("{\"id\": 3, \"nombre\": \"Ajuste Rendimiento NC Ingreso\"}"),
                [ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteExpense] = CreateParameter("{\"id\": 4, \"nombre\": \"Ajuste Rendimiento NC Gasto\"}")
            });

        List<YieldToDistribute>? captured = null;
        ytdRepo.Setup(x => x.InsertRangeAsync(It.IsAny<IEnumerable<YieldToDistribute>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<YieldToDistribute>, CancellationToken>((rows, _) => captured = rows.ToList())
            .Returns(Task.CompletedTask);
        ytdRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        trustYieldRepo.Setup(x => x.DeleteByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService(trustYieldRepo, clientOpRepo, yieldRepo, pvRepo, configRepo, ytdRepo, timeCtrl);

        var result = await service.RunAsync(portfolioId, closingDate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        var item = Assert.Single(captured!);
        Assert.Equal(4, item.Concept.RootElement.GetProperty("id").GetInt32());
        Assert.Equal("Ajuste Rendimiento NC Gasto", item.Concept.RootElement.GetProperty("nombre").GetString());
        Assert.True(item.YieldAmount < 0m);
    }
}
