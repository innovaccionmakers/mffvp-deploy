using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Products.Application.Abstractions.Services.External;
using Products.Application.Abstractions.Services.Objectives;
using Products.Application.Abstractions.Services.Rules;
using Products.Application.Objectives.GetObjectives;
using Products.Integrations.Objectives.GetObjectives;

namespace Products.test.UnitTests.Application.Objectives;

public class GetObjectivesQueryHandlerTests
{
    private readonly Mock<IDocumentTypeValidator> _docValidator = new();
    private readonly Mock<IAffiliateLocator> _affiliateLocator = new();
    private readonly Mock<IObjectiveReader> _objectiveReader = new();
    private readonly Mock<IGetObjectivesRules> _rules = new();

    private GetObjectivesQueryHandler BuildHandler()
    {
        return new GetObjectivesQueryHandler(
            _docValidator.Object,
            _affiliateLocator.Object,
            _objectiveReader.Object,
            _rules.Object
        );
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_DocumentType_Invalid()
    {
        // arrange
        var error = Error.Validation("DOC001", "Invalid type");
        _docValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

        var handler = BuildHandler();
        var query = new GetObjectivesQuery("CC", "123", StatusType.A);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);
        _affiliateLocator.Verify(
            l => l.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _objectiveReader.Verify(
            r => r.BuildValidationContextAsync(It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<StatusType>(),
                It.IsAny<CancellationToken>()), Times.Never);
        _rules.Verify(r => r.EvaluateAsync(It.IsAny<ObjectiveValidationContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Affiliate_Locator_Fails()
    {
        // arrange
        _docValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var error = Error.NotFound("AFF001", "Affiliate not found");
        _affiliateLocator.Setup(a => a.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<int?>(error));

        var handler = BuildHandler();
        var query = new GetObjectivesQuery("CC", "123", StatusType.A);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);
        _objectiveReader.Verify(
            r => r.BuildValidationContextAsync(It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<StatusType>(),
                It.IsAny<CancellationToken>()), Times.Never);
        _rules.Verify(r => r.EvaluateAsync(It.IsAny<ObjectiveValidationContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Rules_Evaluation_Fails()
    {
        // arrange
        _docValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _affiliateLocator.Setup(a => a.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int?>(99));
        var validationContext = new ObjectiveValidationContext { AffiliateExists = true };
        _objectiveReader
            .Setup(r => r.BuildValidationContextAsync(true, 99, StatusType.A, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationContext);
        var error = Error.Validation("RULE001", "rule failed");
        _rules.Setup(r => r.EvaluateAsync(validationContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

        var handler = BuildHandler();
        var query = new GetObjectivesQuery("CC", "123", StatusType.A);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);
        _objectiveReader.Verify(
            r => r.ReadDtosAsync(It.IsAny<int>(), It.IsAny<StatusType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Objectives_When_All_Dependency_Succeed()
    {
        // arrange
        _docValidator.Setup(v => v.EnsureExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _affiliateLocator.Setup(a => a.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int?>(99));
        var validationContext = new ObjectiveValidationContext { AffiliateExists = true };
        _objectiveReader
            .Setup(r => r.BuildValidationContextAsync(true, 99, StatusType.A, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationContext);
        _rules.Setup(r => r.EvaluateAsync(validationContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var objectives = new List<ObjectiveDto>
        {
            new(1, "T1", "Name1", "F1", "Plan1", "ALT1", "AltName", "Port", Status.Active),
            new(2, "T2", "Name2", "F2", "Plan2", "ALT2", "AltName2", "Port2", Status.Inactive)
        };
        _objectiveReader.Setup(r => r.ReadDtosAsync(99, StatusType.A, It.IsAny<CancellationToken>()))
            .ReturnsAsync(objectives);

        var handler = BuildHandler();
        var query = new GetObjectivesQuery("CC", "123", StatusType.A);

        // act
        var result = await handler.Handle(query, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Objectives.Should().BeEquivalentTo(objectives);
    }
}