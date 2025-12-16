using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.Data;
using Accounting.Application.ConcecutivesSetup;
using Accounting.Domain.Consecutives;
using Accounting.Integrations.ConsecutivesSetup;
using Accounting.Infrastructure;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Infrastructure.RulesEngine;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Accounting.test.IntegrationTests.ConsecutivesSetup;

public class ConsecutivesSetupFlowTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly InMemoryConsecutiveRepository _repository = new();
    private readonly InMemoryUnitOfWork _unitOfWork = new();

    public ConsecutivesSetupFlowTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole());
        services.AddRulesEngine<AccountingModuleMarker>(typeof(AccountingModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Get_Handler_Should_Return_Seeded_Consecutives()
    {
        // Arrange
        var handler = new GetConsecutivesSetupQueryHandler(_repository);
        await SeedConsecutiveAsync(1, "ING", "DOC-100", 10);
        await SeedConsecutiveAsync(2, "EGR", "DOC-200", 20);

        // Act
        var result = await handler.Handle(new GetConsecutivesSetupQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().ContainEquivalentOf(new { Id = 1L, Nature = "ING", SourceDocument = "DOC-100", Consecutive = 10 });
        result.Value.Should().ContainEquivalentOf(new { Id = 2L, Nature = "EGR", SourceDocument = "DOC-200", Consecutive = 20 });
    }

    [Fact]
    public async Task Update_Handler_Should_Return_Error_When_Consecutive_Is_Missing()
    {
        // Arrange
        var ruleEvaluator = _serviceProvider.GetRequiredService<IInternalRuleEvaluator<AccountingModuleMarker>>();
        var handler = new UpdateConsecutiveSetupCommandHandler(_repository, _unitOfWork, ruleEvaluator);

        var command = new UpdateConsecutiveSetupCommand(999, "DOC-300", 30);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("ACCOUNTING_CONSECUTIVE_001");
    }

    [Fact]
    public async Task Update_Handler_Should_Return_Error_When_SourceDocument_Is_Duplicated()
    {
        // Arrange
        var ruleEvaluator = _serviceProvider.GetRequiredService<IInternalRuleEvaluator<AccountingModuleMarker>>();
        var handler = new UpdateConsecutiveSetupCommandHandler(_repository, _unitOfWork, ruleEvaluator);

        await SeedConsecutiveAsync(1, "ING", "DOC-100", 10);
        await SeedConsecutiveAsync(2, "EGR", "DOC-200", 20);

        var command = new UpdateConsecutiveSetupCommand(2, "DOC-100", 30);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("ACCOUNTING_CONSECUTIVE_002");
    }

    [Fact]
    public async Task Update_Handler_Should_Update_When_Rules_Pass()
    {
        // Arrange
        var ruleEvaluator = _serviceProvider.GetRequiredService<IInternalRuleEvaluator<AccountingModuleMarker>>();
        var handler = new UpdateConsecutiveSetupCommandHandler(_repository, _unitOfWork, ruleEvaluator);

        await SeedConsecutiveAsync(1, "ING", "DOC-100", 10);
        await SeedConsecutiveAsync(2, "EGR", "DOC-200", 20);

        var command = new UpdateConsecutiveSetupCommand(2, "DOC-201", 25);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new
        {
            Id = 2L,
            Nature = "EGR",
            SourceDocument = "DOC-201",
            Consecutive = 25
        });

        var updated = _repository.Items.Single(c => c.ConsecutiveId == 2);
        updated.SourceDocument.Should().Be("DOC-201");
        updated.Number.Should().Be(25);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    private async Task SeedConsecutiveAsync(long id, string nature, string sourceDocument, int number)
    {
        var consecutive = Consecutive.Create(nature, sourceDocument, number).Value;
        typeof(Consecutive).GetProperty(nameof(Consecutive.ConsecutiveId))!
            .SetValue(consecutive, id);

        await _repository.UpdateAsync(consecutive, CancellationToken.None);
    }
}

internal sealed class InMemoryConsecutiveRepository : IConsecutiveRepository
{
    public List<Consecutive> Items { get; } = new();

    public Task<Consecutive?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult<Consecutive?>(Items.FirstOrDefault(c => c.ConsecutiveId == id));

    public Task<Consecutive?> GetConsecutiveByNatureAsync(string nature)
        => Task.FromResult<Consecutive?>(Items.FirstOrDefault(c => c.Nature == nature));

    public Task<IReadOnlyCollection<Consecutive>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<Consecutive>>(Items.ToList());

    public Task UpdateAsync(Consecutive consecutive, CancellationToken cancellationToken = default)
    {
        var existing = Items.FirstOrDefault(c => c.ConsecutiveId == consecutive.ConsecutiveId);
        if (existing is null)
        {
            Items.Add(consecutive);
            return Task.CompletedTask;
        }

        existing.UpdateDetails(consecutive.Nature, consecutive.SourceDocument, consecutive.Number);
        return Task.CompletedTask;
    }

    public Task UpdateIncomeConsecutiveAsync(int newConsecutiveNumber, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task UpdateEgressConsecutiveAsync(int newConsecutiveNumber, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<bool> IsSourceDocumentInUseAsync(string sourceDocument, long excludedConsecutiveId, CancellationToken cancellationToken = default)
    {
        var inUse = Items.Any(c => c.SourceDocument == sourceDocument && c.ConsecutiveId != excludedConsecutiveId);
        return Task.FromResult(inUse);
    }
}

internal sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction = new FakeDbContextTransaction();
        return Task.FromResult(transaction);
    }
}

internal sealed class FakeDbContextTransaction : IDbContextTransaction
{
    public Guid TransactionId { get; } = Guid.NewGuid();

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Commit()
    {
    }

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Rollback()
    {
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
