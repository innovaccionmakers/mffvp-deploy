using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Products.Application.Administrators.GetFirstAdministrator;
using Products.Domain.Administrators;
using Products.Integrations.Administrators;
using Products.Integrations.Administrators.GetFirstAdministrator;

namespace Products.test.UnitTests.Application.Administrators.GetFirstAdministrator;

public sealed class GetFirstAdministratorQueryHandlerTests
{
    private readonly Mock<IAdministratorRepository> _repositoryMock;
    private readonly GetFirstAdministratorQueryHandler _handler;

    public GetFirstAdministratorQueryHandlerTests()
    {
        _repositoryMock = new Mock<IAdministratorRepository>();
        _handler = new GetFirstAdministratorQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_No_Administrator_Exists()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        _repositoryMock
            .Setup(r => r.GetFirstOrderedByIdAsync(cancellationToken))
            .ReturnsAsync((Administrator?)null);

        var query = new GetFirstAdministratorQuery();

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();

        _repositoryMock.Verify(
            r => r.GetFirstOrderedByIdAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Administrator_Response_When_Administrator_Exists()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var administrator = Administrator.Create(
            "123456789",
            1,
            "Test Administrator",
            Status.Active,
            "ENT001",
            0,
            1,
            "SFC001"
        ).Value;

        _repositoryMock
            .Setup(r => r.GetFirstOrderedByIdAsync(cancellationToken))
            .ReturnsAsync(administrator);

        var query = new GetFirstAdministratorQuery();

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AdministratorId.Should().Be(administrator.AdministratorId);
        result.Value.Identification.Should().Be(administrator.Identification);
        result.Value.IdentificationTypeId.Should().Be(administrator.IdentificationTypeId);
        result.Value.Digit.Should().Be(administrator.Digit);
        result.Value.Name.Should().Be(administrator.Name);
        result.Value.Status.Should().Be(administrator.Status);
        result.Value.EntityCode.Should().Be(administrator.EntityCode);
        result.Value.EntityType.Should().Be(administrator.EntityType);
        result.Value.SfcEntityCode.Should().Be(administrator.SfcEntityCode);

        _repositoryMock.Verify(
            r => r.GetFirstOrderedByIdAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Honor_CancellationToken()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _repositoryMock
            .Setup(r => r.GetFirstOrderedByIdAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(token =>
            {
                token.ThrowIfCancellationRequested();
                return Task.FromResult<Administrator?>(null);
            });

        var query = new GetFirstAdministratorQuery();
        cancellationTokenSource.Cancel();

        // Act + Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            _ = await _handler.Handle(query, cancellationToken);
        });

        _repositoryMock.Verify(
            r => r.GetFirstOrderedByIdAsync(It.Is<CancellationToken>(t => t.IsCancellationRequested)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Map_All_Administrator_Properties_Correctly()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var administrator = Administrator.Create(
            "987654321",
            2,
            "Another Administrator",
            Status.Inactive,
            "ENT002",
            5,
            2,
            "SFC002"
        ).Value;

        _repositoryMock
            .Setup(r => r.GetFirstOrderedByIdAsync(cancellationToken))
            .ReturnsAsync(administrator);

        var query = new GetFirstAdministratorQuery();

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var expectedResponse = new AdministratorResponse(
            administrator.AdministratorId,
            administrator.Identification,
            administrator.IdentificationTypeId,
            administrator.Digit,
            administrator.Name,
            administrator.Status,
            administrator.EntityCode,
            administrator.EntityType,
            administrator.SfcEntityCode
        );

        result.Value.Should().BeEquivalentTo(expectedResponse);
    }
}

