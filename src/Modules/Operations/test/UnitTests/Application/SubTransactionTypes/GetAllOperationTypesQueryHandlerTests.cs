using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Domain.SubtransactionTypes;
using Operations.Application.SubTransactionTypes;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.SubtransactionTypes;
using Operations.Integrations.SubTransactionTypes;

namespace Operations.test.UnitTests.Application.SubTransactionTypes;

public class GetAllOperationTypesQueryHandlerTests
{
    private readonly Mock<ISubtransactionTypeRepository> _repo = new();
    private readonly Mock<IConfigurationParameterRepository> _paramRepo = new();
    private readonly IDistributedCache _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    private GetAllOperationTypesQueryHandler Build() => new(_repo.Object, _paramRepo.Object, _cache);

    private static SubtransactionType Type() =>
        SubtransactionType.Create(
            "T",
            Guid.NewGuid(),
            IncomeEgressNature.Income,
            Status.Active,
            "EXT",
            true,
            JsonDocument.Parse("{}"),
            "H").Value;

    [Fact]
    public async Task Handle_Should_Return_From_Cache_When_Available()
    {
        var cached = new[]
        {
            new { Id = 1L, Name = "T", Category = "Cat", Nature = IncomeEgressNature.Income, Status = Status.Active, External = "EXT", HomologatedCode = "H" }
        };
        await _cache.SetStringAsync("operations:subtransactiontypes:all", JsonSerializer.Serialize(cached));
        var expected = new[] { new SubtransactionTypeResponse(1, "T", "Cat", IncomeEgressNature.Income, Status.Active, "EXT", "H") };
        var handler = Build();

        var result = await handler.Handle(new GetAllOperationTypesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);
        _repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Query_Repository_When_Cache_Misses()
    {
        var type = Type();
        var types = new[] { type };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(types);
        var param = ConfigurationParameter.Create("Cat", "C");
        typeof(ConfigurationParameter).GetProperty("Uuid")!.SetValue(param, type.Category);
        _paramRepo.Setup(r => r.GetByUuidsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ConfigurationParameter> { { type.Category, param } });
        var handler = Build();

        var result = await handler.Handle(new GetAllOperationTypesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var expected = new[] { new SubtransactionTypeResponse(type.SubtransactionTypeId, type.Name, "Cat", type.Nature, type.Status, type.External, type.HomologatedCode) };
        result.Value.Should().BeEquivalentTo(expected);
        _repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        (await _cache.GetStringAsync("operations:subtransactiontypes:all")).Should().NotBeNull();
    }
}