using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.OperationTypes;

using FluentAssertions;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Moq;

using Operations.Application.OperationTypes;
using Operations.Domain.OperationTypes;
using Operations.Integrations.OperationTypes;

using System.Text.Json;

namespace Operations.test.UnitTests.Application.OperationTypes;

public class GetAllOperationTypesQueryHandlerTests
{
    private readonly Mock<IOperationTypeRepository> _repo = new();
    private readonly IDistributedCache _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    private GetAllOperationTypesQueryHandler Build() => new(_repo.Object, _cache);

    private static OperationType Type(long id, int? categoryId = null)
    {
        var type = OperationType.Create(
            "T",
            categoryId,
            IncomeEgressNature.Income,
            Status.Active,
            "EXT",
            true,
            JsonDocument.Parse("{}"),
            "H").Value;
        typeof(OperationType).GetProperty("OperationTypeId")!.SetValue(type, id);
        return type;
    }

    [Fact]
    public async Task Handle_Should_Return_From_Cache_When_Available()
    {
        var cached = new[]
        {
            new { Id = 1L, Name = "T", Category = (string?)null, Nature = IncomeEgressNature.Income, Status = Status.Active, External = "EXT", HomologatedCode = "H" }
        };
        await _cache.SetStringAsync("operations:operationtypes:all", JsonSerializer.Serialize(cached));
        var expected = new[] { new OperationTypeResponse(1, "T", null, IncomeEgressNature.Income, Status.Active, "EXT", "H") };
        var handler = Build();

        var result = await handler.Handle(new GetAllOperationTypesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);
        _repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Query_Repository_When_Cache_Misses()
    {
        var type = Type(1);
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { type });
        var handler = Build();

        var result = await handler.Handle(new GetAllOperationTypesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var expected = new[] { new OperationTypeResponse(type.OperationTypeId, type.Name, null, type.Nature, type.Status, type.External, type.HomologatedCode) };
        result.Value.Should().BeEquivalentTo(expected);
        _repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        (await _cache.GetStringAsync("operations:operationtypes:all")).Should().NotBeNull();
    }
}
