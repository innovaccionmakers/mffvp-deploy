using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using Closing.Application.Closing.Services.ReturnsOperations;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Application.PreClosing.Services.Yield.Builders;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.YieldDetails;
using Closing.Domain.YieldsToDistribute;
using Closing.Domain.TrustYields;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Closing.test.UnitTests.Application.Closing.Services;

public sealed class ReturnsOperationsServiceTests
{
    private static ReturnsOperationsService CreateService(
        Mock<IYieldToDistributeRepository>? yieldToDistributeRepository = null,
        Mock<IConfigurationParameterRepository>? configurationParameterRepository = null,
        Mock<IYieldDetailCreationService>? yieldDetailCreationService = null,
        IYieldDetailBuilder? builder = null,
        Mock<ITimeControlService>? timeControlService = null,
        Mock<ITrustYieldRepository>? trustYieldRepository = null)
    {
        yieldToDistributeRepository ??= new Mock<IYieldToDistributeRepository>();
        configurationParameterRepository ??= new Mock<IConfigurationParameterRepository>();
        yieldDetailCreationService ??= new Mock<IYieldDetailCreationService>();
        timeControlService ??= new Mock<ITimeControlService>();
        trustYieldRepository ??= new Mock<ITrustYieldRepository>();

        builder ??= new AutomaticConceptYieldDetailBuilder();
        var yieldDetailBuilderService = new YieldDetailBuilderService(new[] { builder });

        var logger = Mock.Of<ILogger<ReturnsOperationsService>>();

        return new ReturnsOperationsService(
            yieldToDistributeRepository.Object,
            configurationParameterRepository.Object,
            yieldDetailCreationService.Object,
            yieldDetailBuilderService,
            timeControlService.Object,
            trustYieldRepository.Object,
            logger);
    }

    [Fact]
    public async Task RunAsync_NoRows_DoesNotCreateDetails()
    {
        var yieldRepo = new Mock<IYieldToDistributeRepository>();
        yieldRepo.Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<YieldToDistribute>());

        var configRepo = SetupConfiguration();

        var detailCreation = new Mock<IYieldDetailCreationService>();
        var timeControl = new Mock<ITimeControlService>();

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        var service = CreateService(yieldRepo, configRepo, detailCreation, timeControlService: timeControl, trustYieldRepository: trustYieldRepo);

        var result = await service.RunAsync(1, new DateTime(2024, 1, 15), true, CancellationToken.None);

        Assert.True(result.IsSuccess);
        detailCreation.Verify(d => d.CreateYieldDetailsAsync(It.IsAny<IEnumerable<YieldDetail>>(), It.IsAny<PersistenceMode>(), It.IsAny<CancellationToken>()), Times.Never);
        timeControl.Verify(t => t.UpdateStepAsync(1, "ReturnsOperations", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        trustYieldRepo.Verify(r => r.DeleteByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_PositiveRows_CreatesIncomeDetail()
    {
        var conceptJson = JsonDocument.Parse("{\"id\":5,\"nombre\":\"Ajuste Rendimiento Nota Contable\"}");
        var yield = YieldToDistribute.Create(1, 10, DateTime.UtcNow.Date, DateTime.UtcNow.Date, 1m, 200m, conceptJson, DateTime.UtcNow).Value;

        var yieldRepo = new Mock<IYieldToDistributeRepository>();
        yieldRepo.Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(10, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { yield });

        var configRepo = SetupConfiguration();

        List<YieldDetail>? captured = null;
        var detailCreation = new Mock<IYieldDetailCreationService>();
        detailCreation.Setup(d => d.CreateYieldDetailsAsync(It.IsAny<IEnumerable<YieldDetail>>(), PersistenceMode.Transactional, It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<YieldDetail>, PersistenceMode, CancellationToken>((details, _, _) => captured = details.ToList())
            .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(r => r.GetReadOnlyByTrustIdsAndDateAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<long, TrustYield>
            {
                [yield.TrustId] = CreateTrustYield(100, yield.TrustId, yield.PortfolioId, yield.ClosingDate)
            });
        trustYieldRepo.Setup(r => r.DeleteByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService(yieldRepo, configRepo, detailCreation, trustYieldRepository: trustYieldRepo);

        var result = await service.RunAsync(10, new DateTime(2024, 1, 31), true, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        var detail = Assert.Single(captured!);
        Assert.Equal(200m, detail.Income);
        Assert.Equal(0m, detail.Expenses);
        Assert.Equal("Concepto Automático", detail.Source);
        trustYieldRepo.Verify(r => r.GetReadOnlyByTrustIdsAndDateAsync(It.Is<IEnumerable<long>>(ids => ids.Single() == yield.TrustId), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        trustYieldRepo.Verify(r => r.DeleteByIdsAsync(It.Is<IEnumerable<long>>(ids => ids.Single() == 100L), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_PositiveAndNegativeRows_CreatesBothConcepts()
    {
        var conceptJson = JsonDocument.Parse("{\"id\":5,\"nombre\":\"Ajuste Rendimiento Nota Contable\"}");
        var positive = YieldToDistribute.Create(1, 10, DateTime.UtcNow.Date, DateTime.UtcNow.Date, 1m, 300m, conceptJson, DateTime.UtcNow).Value;
        var negative = YieldToDistribute.Create(2, 10, DateTime.UtcNow.Date, DateTime.UtcNow.Date, 1m, -120m, conceptJson, DateTime.UtcNow).Value;

        var yieldRepo = new Mock<IYieldToDistributeRepository>();
        yieldRepo.Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(10, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { positive, negative });

        var configRepo = SetupConfiguration();

        List<YieldDetail>? captured = null;
        var detailCreation = new Mock<IYieldDetailCreationService>();
        detailCreation.Setup(d => d.CreateYieldDetailsAsync(It.IsAny<IEnumerable<YieldDetail>>(), PersistenceMode.Transactional, It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<YieldDetail>, PersistenceMode, CancellationToken>((details, _, _) => captured = details.ToList())
            .Returns(Task.CompletedTask);

        var trustYieldRepo = new Mock<ITrustYieldRepository>();
        trustYieldRepo.Setup(r => r.GetReadOnlyByTrustIdsAndDateAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<long, TrustYield>
            {
                [positive.TrustId] = CreateTrustYield(101, positive.TrustId, positive.PortfolioId, positive.ClosingDate),
                [negative.TrustId] = CreateTrustYield(102, negative.TrustId, negative.PortfolioId, negative.ClosingDate)
            });
        trustYieldRepo.Setup(r => r.DeleteByIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var service = CreateService(yieldRepo, configRepo, detailCreation, trustYieldRepository: trustYieldRepo);

        var result = await service.RunAsync(10, new DateTime(2024, 1, 31), true, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        Assert.Equal(2, captured!.Count);

        var income = captured.Single(d => d.Income > 0);
        var expense = captured.Single(d => d.Expenses > 0);

        Assert.Equal(300m, income.Income);
        Assert.Equal(120m, expense.Expenses);
        trustYieldRepo.Verify(r => r.DeleteByIdsAsync(It.Is<IEnumerable<long>>(ids => ids.Count() == 2 && ids.Contains(101L) && ids.Contains(102L)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_MissingConfiguration_ReturnsFailure()
    {
        var yieldRepo = new Mock<IYieldToDistributeRepository>();
        yieldRepo.Setup(r => r.GetReadOnlyByPortfolioAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<YieldToDistribute>());

        var configRepo = new Mock<IConfigurationParameterRepository>();
        configRepo.Setup(r => r.GetReadOnlyByUuidsAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>());

        var service = CreateService(yieldRepo, configRepo);

        var result = await service.RunAsync(1, new DateTime(2024, 1, 15), true, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RO001", result.Error.Code);
    }

    private static Mock<IConfigurationParameterRepository> SetupConfiguration()
    {
        var configRepo = new Mock<IConfigurationParameterRepository>();
        configRepo.Setup(r => r.GetReadOnlyByUuidsAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter>
            {
                [ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote] = CreateParameter("{\"id\":5,\"nombre\":\"Ajuste Rendimiento Nota Contable\"}"),
                [ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteIncome] = CreateParameter("{\"id\":10,\"nombre\":\"Ajuste Rendimiento NC Ingreso\"}"),
                [ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteExpense] = CreateParameter("{\"id\":11,\"nombre\":\"Ajuste Rendimiento NC Gasto\"}")
            });

        return configRepo;
    }

    private static ConfigurationParameter CreateParameter(string json)
    {
        var parameter = (ConfigurationParameter)FormatterServices.GetUninitializedObject(typeof(ConfigurationParameter));
        var metadataProp = typeof(ConfigurationParameter).GetProperty(nameof(ConfigurationParameter.Metadata), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ConfigurationParameter.Metadata not found");
        metadataProp.SetValue(parameter, JsonDocument.Parse(json));
        return parameter;
    }

    private static TrustYield CreateTrustYield(long id, long trustId, int portfolioId, DateTime closingDateUtc)
    {
        var result = TrustYield.Create(
            trustId,
            portfolioId,
            closingDateUtc,
            participation: 0m,
            units: 0m,
            yieldAmount: 0m,
            preClosingBalance: 0m,
            closingBalance: 0m,
            income: 0m,
            expenses: 0m,
            commissions: 0m,
            cost: 0m,
            capital: 0m,
            processDate: closingDateUtc,
            contingentRetention: 0m,
            yieldRetention: 0m);

        var entity = result.Value;
        typeof(TrustYield).GetProperty(nameof(TrustYield.TrustYieldId), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(entity, id);
        return entity;
    }
}
