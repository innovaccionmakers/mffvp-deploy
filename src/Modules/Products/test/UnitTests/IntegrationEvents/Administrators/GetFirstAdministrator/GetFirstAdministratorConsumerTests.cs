using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Products.IntegrationEvents.Administrators.GetFirstAdministrator;
using Products.Integrations.Administrators;
using Products.Integrations.Administrators.GetFirstAdministrator;

namespace Products.test.UnitTests.IntegrationEvents.Administrators.GetFirstAdministrator;

public sealed class GetFirstAdministratorConsumerTests
{
    private readonly Mock<MediatR.ISender> _senderMock;
    private readonly GetFirstAdministratorConsumer _consumer;

    public GetFirstAdministratorConsumerTests()
    {
        _senderMock = new Mock<MediatR.ISender>();
        _consumer = new GetFirstAdministratorConsumer(_senderMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Success_Response_When_Query_Succeeds_With_Administrator()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var administratorResponse = new AdministratorResponse(
            1,
            "123456789",
            1,
            0,
            "Test Administrator",
            Status.Active,
            "ENT001",
            1,
            "SFC001"
        );

        var queryResult = Result.Success<AdministratorResponse?>(administratorResponse);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetFirstAdministratorQuery>(), cancellationToken))
            .ReturnsAsync(queryResult);

        var request = new GetFirstAdministratorRequest();

        // Act
        var result = await _consumer.HandleAsync(request, cancellationToken);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Administrator.Should().NotBeNull();
        result.Administrator.Should().BeEquivalentTo(administratorResponse);
        result.Code.Should().BeNull();
        result.Message.Should().BeNull();

        _senderMock.Verify(
            s => s.Send(It.IsAny<GetFirstAdministratorQuery>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Success_Response_When_Query_Succeeds_With_Null()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var queryResult = Result.Success<AdministratorResponse?>(null);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetFirstAdministratorQuery>(), cancellationToken))
            .ReturnsAsync(queryResult);

        var request = new GetFirstAdministratorRequest();

        // Act
        var result = await _consumer.HandleAsync(request, cancellationToken);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Administrator.Should().BeNull();
        result.Code.Should().BeNull();
        result.Message.Should().BeNull();

        _senderMock.Verify(
            s => s.Send(It.IsAny<GetFirstAdministratorQuery>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Failure_Response_When_Query_Fails()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var error = Error.NotFound("ADMIN001", "Administrator not found");
        var queryResult = Result.Failure<AdministratorResponse?>(error);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetFirstAdministratorQuery>(), cancellationToken))
            .ReturnsAsync(queryResult);

        var request = new GetFirstAdministratorRequest();

        // Act
        var result = await _consumer.HandleAsync(request, cancellationToken);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Administrator.Should().BeNull();
        result.Code.Should().Be(error.Code);
        result.Message.Should().Be(error.Description);

        _senderMock.Verify(
            s => s.Send(It.IsAny<GetFirstAdministratorQuery>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Send_Correct_Query()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var queryResult = Result.Success<AdministratorResponse?>(null);
        GetFirstAdministratorQuery? capturedQuery = null;

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetFirstAdministratorQuery>(), cancellationToken))
            .Callback<object, CancellationToken>((query, _) => capturedQuery = query as GetFirstAdministratorQuery)
            .ReturnsAsync(queryResult);

        var request = new GetFirstAdministratorRequest();

        // Act
        _ = await _consumer.HandleAsync(request, cancellationToken);

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery.Should().BeOfType<GetFirstAdministratorQuery>();
    }

    [Fact]
    public async Task HandleAsync_Should_Honor_CancellationToken()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetFirstAdministratorQuery>(), It.IsAny<CancellationToken>()))
            .Returns<object, CancellationToken>((_, token) =>
            {
                token.ThrowIfCancellationRequested();
                return Task.FromResult(Result.Success<AdministratorResponse?>(null));
            });

        var request = new GetFirstAdministratorRequest();
        cancellationTokenSource.Cancel();

        // Act + Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            _ = await _consumer.HandleAsync(request, cancellationToken);
        });

        _senderMock.Verify(
            s => s.Send(It.IsAny<GetFirstAdministratorQuery>(), It.Is<CancellationToken>(t => t.IsCancellationRequested)),
            Times.Once);
    }
}

