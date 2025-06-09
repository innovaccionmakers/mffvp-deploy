using System.Data.Common;
using System.Text.Json;
using Common.SharedKernel.Domain;
using FluentAssertions;
using Moq;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Rules;
using Operations.Application.Contributions.CreateContribution;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.Origins;
using Operations.Integrations.Contributions.CreateContribution;
using RulesEngine.Models;

namespace Operations.test.UnitTests.Application.Contributions;

public class CreateContributionCommandHandlerTests
{
    private readonly Mock<IContributionCatalogResolver> _catalogResolver = new();
    private readonly Mock<IActivateLocator> _activateLocator = new();
    private readonly Mock<IContributionRemoteValidator> _remoteValidator = new();
    private readonly Mock<IPersonValidator> _personValidator = new();
    private readonly Mock<ITaxCalculator> _taxCalculator = new();
    private readonly Mock<IClientOperationRepository> _operationRepo = new();
    private readonly Mock<IAuxiliaryInformationRepository> _auxRepo = new();
    private readonly Mock<IRuleEvaluator<OperationsModuleMarker>> _ruleEvaluator = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ITrustCreator> _trustCreator = new();
    private readonly Mock<DbTransaction> _tx = new();

    private CreateContributionCommandHandler BuildHandler()
    {
        _uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_tx.Object);
        return new CreateContributionCommandHandler(
            _catalogResolver.Object,
            _activateLocator.Object,
            _remoteValidator.Object,
            _personValidator.Object,
            _taxCalculator.Object,
            _operationRepo.Object,
            _auxRepo.Object,
            _ruleEvaluator.Object,
            _uow.Object,
            _trustCreator.Object);
    }

    private static CreateContributionCommand BuildCommand(string? certified = "SI")
    {
        return new CreateContributionCommand(
            "CC",
            "123",
            10,
            "1",
            1000m,
            "SRC",
            "MOD",
            "CM",
            "PM",
            JsonDocument.Parse("{}"),
            "BANK",
            "ACC",
            certified,
            null,
            DateTime.Today,
            DateTime.Today,
            "sales",
            JsonDocument.Parse("{}"),
            null,
            "WEB",
            "1");
    }

    private static ContributionCatalogs BuildCatalogs()
    {
        var origin = Origin.Create("o", false, false, false, "A", "SRC").Value;
        var cfg = ConfigurationParameter.Create("cfg", "h");
        var channel = Operations.Domain.Channels.Channel.Create("c", "CH", false, "A").Value;
        var subtype = Domain.SubtransactionTypes.SubtransactionType.Create(
            "s",
            Guid.NewGuid(),
            "N",
            "A",
            "EXT",
            "SUB").Value;
        return new ContributionCatalogs(origin, cfg, cfg, cfg, channel, subtype,
            ConfigurationParameter.Create("cat", "h"));
    }

    private void SetupHappyPath(Result? trustResult = null)
    {
        trustResult ??= Result.Success();
        _catalogResolver
            .Setup(r => r.ResolveAsync(It.IsAny<CreateContributionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCatalogs());
        _activateLocator.Setup(l => l.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((true, 1, false)));
        _remoteValidator.Setup(v => v.ValidateAsync(1, 10, "1", It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1000m,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ContributionRemoteData(1, 10, 1, "port", 50m)));
        _operationRepo.Setup(r => r.ExistsContributionAsync(1, 10, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        _personValidator.Setup(p => p.ValidateAsync("CC", "123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _taxCalculator.Setup(t => t.ComputeAsync(false, true, 1000m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaxResult(1, 2, 3m, "name"));
        _trustCreator.Setup(t => t.CreateAsync(It.IsAny<TrustCreationDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(trustResult);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_All_Dependency_Succeed()
    {
        // arrange
        SetupHappyPath();
        var handler = BuildHandler();
        var command = BuildCommand();

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        _operationRepo.Verify(r => r.Insert(It.IsAny<ClientOperation>()), Times.Once);
        _auxRepo.Verify(r => r.Insert(It.IsAny<AuxiliaryInformation>()), Times.Once);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_ActivateLocator_Fails()
    {
        // arrange
        var error = Error.NotFound("A", "fail");
        _activateLocator.Setup(l => l.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<(bool, int, bool)>(error));
        var handler = BuildHandler();
        var command = BuildCommand();

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);
        _remoteValidator.Verify(
            v => v.ValidateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_RemoteValidator_Fails()
    {
        // arrange
        _activateLocator.Setup(l => l.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success((true, 1, false)));
        var error = Error.Problem("R", "bad");
        _remoteValidator.Setup(v => v.ValidateAsync(1, 10, "1", It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1000m,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ContributionRemoteData>(error));
        var handler = BuildHandler();
        var command = BuildCommand();

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);
        _operationRepo.Verify(r => r.Insert(It.IsAny<ClientOperation>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_Rules_Fail()
    {
        // arrange
        SetupHappyPath();
        var ruleError = new RuleValidationError("V", "invalid");
        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Array.Empty<RuleResultTree>(), new[] { ruleError }));
        var handler = BuildHandler();
        var command = BuildCommand();

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ruleError.Code);
        _personValidator.Verify(
            p => p.ValidateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_PersonValidator_Fails()
    {
        // arrange
        SetupHappyPath();
        var error = Error.Validation("P", "bad");
        _personValidator.Setup(p => p.ValidateAsync("CC", "123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));
        var handler = BuildHandler();
        var command = BuildCommand();

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(error);
        _trustCreator.Verify(t => t.CreateAsync(It.IsAny<TrustCreationDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Error_When_TrustCreator_Fails()
    {
        // arrange
        var trustErr = Error.Conflict("T", "fail");
        SetupHappyPath(Result.Failure(trustErr));
        var handler = BuildHandler();
        var command = BuildCommand();

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        AssertionExtensions.Should(result.Error).Be(trustErr);
        _tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("SI", true)]
    [InlineData("NO", true)]
    [InlineData(null, true)]
    [InlineData("   ", true)]
    [InlineData("XX", false)]
    public async Task Handle_Should_Validate_CertifiedContribution(string? value, bool expected)
    {
        // arrange
        SetupHappyPath();
        object? captured = null;
        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<string, object, CancellationToken>((_, ctx, _) => captured = ctx)
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        var handler = BuildHandler();
        var command = BuildCommand(value);

        // act
        await handler.Handle(command, CancellationToken.None);

        // assert
        captured.Should().NotBeNull();
        var prop = captured!.GetType().GetProperty("CertifiedContributionValid")!;
        prop.GetValue(captured).Should().Be(expected);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Handle_Should_Set_FirstContribution_Flag(bool exists, bool expected)
    {
        // arrange
        SetupHappyPath();
        _operationRepo.Setup(r =>
                r.ExistsContributionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
        object? captured = null;
        _ruleEvaluator
            .Setup(r => r.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<string, object, CancellationToken>((_, ctx, _) => captured = ctx)
            .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));
        var handler = BuildHandler();
        var command = BuildCommand();

        // act
        await handler.Handle(command, CancellationToken.None);

        // assert
        captured.Should().NotBeNull();
        var prop = captured!.GetType().GetProperty("IsFirstContribution")!;
        prop.GetValue(captured).Should().Be(expected);
    }
}