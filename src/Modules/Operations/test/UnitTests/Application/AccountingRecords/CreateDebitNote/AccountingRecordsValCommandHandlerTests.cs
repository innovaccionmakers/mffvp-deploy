using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Domain.OperationTypes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.AccountingRecords;
using Operations.Application.Abstractions.Services.Closing;
using Operations.Application.AccountingRecords.CreateDebitNote;
using Operations.Domain.ClientOperations;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.OperationTypes;
using Operations.Integrations.AccountingRecords.CreateDebitNote;
using RulesEngine.Models;
using CoreErrorType = Common.SharedKernel.Core.Primitives.ErrorType;

namespace Operations.test.UnitTests.Application.AccountingRecords.CreateDebitNote;

public class AccountingRecordsValCommandHandlerTests
{
    private static readonly (bool Success, IReadOnlyCollection<RuleResultTree> Results, IReadOnlyCollection<RuleValidationError> Errors)
        SuccessfulRules = (true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>());

    [Fact]
    public async Task Handle_WithExistingTrustId_WhenTrustValidationFails_ReturnsFailure()
    {
        var trustValidationError = Error.Validation("Trust.Mismatch", "Valor diferente al saldo del fideicomiso");

        var fixture = TestFixture.Create()
            .WithTrustInfo(Result.Failure<TrustInfoResult>(trustValidationError));

        var result = await fixture.HandleAsync();

        result.IsFailure.Should().BeTrue();
        ((object)result.Error).Should().BeEquivalentTo(trustValidationError);
        fixture.TrustInfoProviderMock.Verify(
            provider => provider.GetAsync(
                fixture.ClientOperationId,
                fixture.Amount,
                It.IsAny<CancellationToken>()),
            Times.Once);
        fixture.AccountingRecordsOperMock.Verify(
            service => service.ExecuteAsync(
                It.IsAny<AccountingRecordsOperRequest>(),
                It.IsAny<AccountingRecordsValidationResult>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenClosingIsActive_ReturnsFailure()
    {
        var validationError = new RuleValidationError(
            "OPS_ACC_CLOSING_ACTIVE",
            "No es posible realizar nota débito: proceso de cierre en ejecución.");

        var fixture = TestFixture.Create()
            .WithClosingAvailable(false)
            .WithValidationFailure(validationError);

        var result = await fixture.HandleAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        fixture.GetValidationContext<bool>("ClosingAvailable").Should().BeFalse();
        fixture.GetValidationContext<bool>("OperationAffiliateMatches").Should().BeTrue();
        fixture.GetValidationContext<bool>("OperationObjectiveMatches").Should().BeTrue();
        fixture.GetValidationContext<bool>("TrustAdjustmentTypeExists").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenProcessDateIsNotLessThanPortfolioDate_ReturnsFailure()
    {
        var validationError = new RuleValidationError(
            "OPS_ACC_PROCESSDATE_IS_VALID",
            "No es posible anular: la fecha de proceso de la operación debe ser menor a la fecha de proceso del portafolio.");

        var fixture = TestFixture.Create()
            .WithProcessDate(fixture => fixture.PortfolioCurrentDate.AddDays(1))
            .WithValidationFailure(validationError);

        var result = await fixture.HandleAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        fixture.GetValidationContext<bool>("ProcessDateIsValid").Should().BeFalse();
        fixture.GetValidationContext<bool>("OperationAffiliateMatches").Should().BeTrue();
        fixture.GetValidationContext<bool>("OperationObjectiveMatches").Should().BeTrue();
        fixture.GetValidationContext<bool>("TrustAdjustmentTypeExists").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAnnulmentAlreadyExists_ReturnsFailure()
    {
        var validationError = new RuleValidationError(
            "OPS_ACC_PENDING_ANNULMENT",
            "Solo se permite anular una transacción a la vez; hay una anulación en curso.");

        var fixture = TestFixture.Create()
            .WithPendingAnnulment(true)
            .WithValidationFailure(validationError);

        var result = await fixture.HandleAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        fixture.GetValidationContext<bool>("NoPendingAnnulment").Should().BeFalse();
        fixture.GetValidationContext<bool>("TrustAdjustmentTypeExists").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenOperationIsNotActive_ReturnsFailure()
    {
        var validationError = new RuleValidationError(
            "OPS_ACC_STATUS_INVALID",
            "Operación no válida: solo se pueden anular operaciones en estado Activo.");

        var fixture = TestFixture.Create()
            .WithOperationStatus(LifecycleStatus.Annulled)
            .WithValidationFailure(validationError);

        var result = await fixture.HandleAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        fixture.GetValidationContext<bool>("OperationIsActive").Should().BeFalse();
        fixture.GetValidationContext<bool>("TrustAdjustmentTypeExists").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAffiliateDoesNotMatchOperation_ReturnsFailure()
    {
        var validationError = new RuleValidationError(
            "OPS_ACC_AFFILIATE_MISMATCH",
            "Operación no válida: la operación no pertenece al afiliado indicado.");

        var fixture = TestFixture.Create()
            .WithValidationFailure(validationError);

        var command = fixture.CreateCommand(affiliateId: fixture.AffiliateId + 1);

        var result = await fixture.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        fixture.GetValidationContext<bool>("OperationAffiliateMatches").Should().BeFalse();
        fixture.GetValidationContext<bool>("OperationObjectiveMatches").Should().BeTrue();
        fixture.GetValidationContext<bool>("TrustAdjustmentTypeExists").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenObjectiveDoesNotMatchOperation_ReturnsFailure()
    {
        var validationError = new RuleValidationError(
            "OPS_ACC_OBJECTIVE_MISMATCH",
            "Operación no válida: la operación no pertenece al objetivo indicado.");

        var fixture = TestFixture.Create()
            .WithValidationFailure(validationError);

        var command = fixture.CreateCommand(objectiveId: fixture.ObjectiveId + 1);

        var result = await fixture.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        fixture.GetValidationContext<bool>("OperationAffiliateMatches").Should().BeTrue();
        fixture.GetValidationContext<bool>("OperationObjectiveMatches").Should().BeFalse();
        fixture.GetValidationContext<bool>("TrustAdjustmentTypeExists").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCauseConfigurationParameterNotFound_ReturnsFailure()
    {
        var validationError = new RuleValidationError(
            "OPS_ACC_CAUSE_NOT_FOUND",
            "No es posible registrar la nota débito: la causal no está configurada.");

        var fixture = TestFixture.Create()
            .WithoutCauseConfigurationParameter()
            .WithValidationFailure(validationError);

        var result = await fixture.HandleAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(CoreErrorType.Validation);
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        fixture.ConfigurationParameterRepositoryMock.Verify(
            repository => repository.GetByIdAsync(
                fixture.CauseId,
                It.IsAny<CancellationToken>()),
            Times.Once);
        fixture.AccountingRecordsOperMock.Verify(
            service => service.ExecuteAsync(
                It.IsAny<AccountingRecordsOperRequest>(),
                It.IsAny<AccountingRecordsValidationResult>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOperationTypeIsNotContribution_ReturnsFailure()
    {
        var validationError = new RuleValidationError(
            "OPS_ACC_INVALID_OPERATION_TYPE",
            "La nota débito solo aplica a aportes.");

        var fixture = TestFixture.Create()
            .WithOperationType(CreateOperationType(900, "Rescate", null))
            .WithValidationFailure(validationError);

        var result = await fixture.HandleAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(CoreErrorType.Validation);
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        fixture.GetValidationContext<bool>("OperationExists").Should().BeTrue();
        fixture.GetValidationContext<bool>("OperationTypeExists").Should().BeTrue();
        fixture.GetValidationContext<bool>("ContributionTypeExists").Should().BeTrue();
        fixture.GetValidationContext<bool>("OperationIsContribution").Should().BeFalse();
        fixture.GetValidationContext<bool>("PortfolioFound").Should().BeTrue();
        fixture.GetValidationContext<bool>("DebitNoteTypeExists").Should().BeTrue();
        fixture.GetValidationContext<bool>("NoPendingAnnulment").Should().BeTrue();
        fixture.GetValidationContext<bool>("TrustAdjustmentTypeExists").Should().BeTrue();
        fixture.AccountingRecordsOperMock.Verify(
            service => service.ExecuteAsync(
                It.IsAny<AccountingRecordsOperRequest>(),
                It.IsAny<AccountingRecordsValidationResult>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOperationTypeNotFound_ReturnsFailure()
    {
        var validationError = new RuleValidationError(
            "OPS_ACC_OPERATION_TYPE_NOT_FOUND",
            "No es posible registrar la nota débito: el tipo de operación no está configurado.");

        var fixture = TestFixture.Create()
            .WithoutOperationType()
            .WithValidationFailure(validationError);

        var result = await fixture.HandleAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(CoreErrorType.Validation);
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        fixture.GetValidationContext<bool>("OperationExists").Should().BeTrue();
        fixture.GetValidationContext<bool>("OperationTypeExists").Should().BeFalse();
        fixture.GetValidationContext<bool>("ContributionTypeExists").Should().BeTrue();
        fixture.GetValidationContext<bool>("OperationIsContribution").Should().BeFalse();
        fixture.GetValidationContext<bool>("PortfolioFound").Should().BeTrue();
        fixture.GetValidationContext<bool>("DebitNoteTypeExists").Should().BeTrue();
        fixture.GetValidationContext<bool>("NoPendingAnnulment").Should().BeTrue();
        fixture.GetValidationContext<bool>("TrustAdjustmentTypeExists").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenContributionTypeNotFound_ReturnsFailure()
    {
        var validationError = new RuleValidationError(
            "OPS_ACC_CONTRIBUTION_TYPE_NOT_FOUND",
            "No es posible registrar la nota débito: el tipo de operación Aporte no está configurado.");

        var fixture = TestFixture.Create()
            .WithoutContributionType()
            .WithValidationFailure(validationError);

        var result = await fixture.HandleAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(CoreErrorType.Validation);
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        fixture.GetValidationContext<bool>("OperationExists").Should().BeTrue();
        fixture.GetValidationContext<bool>("OperationTypeExists").Should().BeTrue();
        fixture.GetValidationContext<bool>("ContributionTypeExists").Should().BeFalse();
        fixture.GetValidationContext<bool>("OperationIsContribution").Should().BeFalse();
        fixture.GetValidationContext<bool>("PortfolioFound").Should().BeTrue();
        fixture.GetValidationContext<bool>("DebitNoteTypeExists").Should().BeTrue();
        fixture.GetValidationContext<bool>("NoPendingAnnulment").Should().BeTrue();
        fixture.GetValidationContext<bool>("TrustAdjustmentTypeExists").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenTrustAdjustmentTypeNotFound_ReturnsFailure()
    {
        var validationError = new RuleValidationError(
            "OPS_ACC_TRUST_ADJUSTMENT_TYPE_NOT_FOUND",
            "No es posible registrar el ajuste de rendimientos: el tipo de operación 'Ajuste Rendimientos' no está configurado.");

        var fixture = TestFixture.Create()
            .WithoutTrustAdjustmentType()
            .WithValidationFailure(validationError);

        var result = await fixture.HandleAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(CoreErrorType.Validation);
        result.Error.Code.Should().Be(validationError.Code);
        result.Error.Description.Should().Be(validationError.Message);
        fixture.AccountingRecordsOperMock.Verify(
            service => service.ExecuteAsync(
                It.IsAny<AccountingRecordsOperRequest>(),
                It.IsAny<AccountingRecordsValidationResult>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        fixture.GetValidationContext<bool>("TrustAdjustmentTypeExists").Should().BeFalse();
    }

    private static OperationType CreateOperationType(
        long id,
        string name,
        int? categoryId,
        IncomeEgressNature nature = IncomeEgressNature.Income,
        Status status = Status.Active,
        bool visible = true)
    {
        var operationType = OperationType.Create(
            name,
            categoryId,
            nature,
            status,
            external: string.Empty,
            visible,
            additionalAttributes: JsonDocument.Parse("{}"),
            homologatedCode: name).Value;

        SetProperty(operationType, nameof(OperationType.OperationTypeId), id);

        return operationType;
    }

    private static T GetPropertyValue<T>(object target, string propertyName)
    {
        return (T)target.GetType().GetProperty(propertyName)!.GetValue(target)!;
    }

    private static void SetProperty<T>(T target, string propertyName, object value)
    {
        typeof(T)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(target, value);
    }

    private sealed class TestFixture
    {
        private const long DefaultClientOperationId = 100;
        private const decimal DefaultAmount = 1_000m;
        private const int DefaultCauseId = 10;
        private const int DefaultCauseConfigurationParameterId = 20;

        private ConfigurationParameter? _causeParameter;
        private OperationType? _operationType;
        private OperationType? _contributionType;
        private OperationType? _debitNoteType;
        private OperationType? _trustAdjustmentType;
        private Result<TrustInfoResult> _trustInfoResult;
        private bool _hasActiveLinkedOperation;
        private (bool Success, IReadOnlyCollection<RuleResultTree> Results, IReadOnlyCollection<RuleValidationError> Errors) _validationResult;

        private long _clientOperationTypeId;
        private int _affiliateId;
        private int _objectiveId;
        private int _portfolioId;
        private DateTime _processDate;
        private LifecycleStatus _status;
        private long? _trustId;

        private ClientOperation _clientOperation;

        private TestFixture()
        {
            RuleEvaluatorMock = new Mock<IInternalRuleEvaluator<OperationsModuleMarker>>();
            ClientOperationRepositoryMock = new Mock<IClientOperationRepository>();
            ConfigurationParameterRepositoryMock = new Mock<IConfigurationParameterRepository>();
            OperationTypeRepositoryMock = new Mock<IOperationTypeRepository>();
            PortfolioLocatorMock = new Mock<IPortfolioLocator>();
            ClosingValidatorMock = new Mock<IClosingValidator>();
            TrustInfoProviderMock = new Mock<ITrustInfoProvider>();
            AccountingRecordsOperMock = new Mock<IAccountingRecordsOper>();
            LoggerMock = new Mock<ILogger<AccountingRecordsValCommandHandler>>();

            PortfolioCurrentDate = DateTime.UtcNow.Date;
            _validationResult = SuccessfulRules;

            _causeParameter = CreateConfigurationParameter(
                DefaultCauseId,
                DefaultCauseConfigurationParameterId);

            _contributionType = CreateOperationType(200, "Aporte", null);
            _operationType = CreateOperationType(201, "Aporte Nómina", (int)_contributionType.OperationTypeId);
            _debitNoteType = CreateOperationType(202, "Nota Débito", null, IncomeEgressNature.Egress);
            _trustAdjustmentType = CreateOperationType(203, "Ajuste Rendimientos", null);

            _clientOperationTypeId = _operationType.OperationTypeId;
            _affiliateId = 300;
            _objectiveId = 400;
            _portfolioId = 500;
            _processDate = DateTime.UtcNow.AddDays(-2).Date;
            _status = LifecycleStatus.Active;
            _trustId = 700;

            _clientOperation = BuildClientOperation();
            _trustInfoResult = Result.Success(new TrustInfoResult(_trustId.Value));

            SetupMocks();
        }

        public static TestFixture Create() => new();

        public Mock<IInternalRuleEvaluator<OperationsModuleMarker>> RuleEvaluatorMock { get; }
        public Mock<IClientOperationRepository> ClientOperationRepositoryMock { get; }
        public Mock<IConfigurationParameterRepository> ConfigurationParameterRepositoryMock { get; }
        public Mock<IOperationTypeRepository> OperationTypeRepositoryMock { get; }
        public Mock<IPortfolioLocator> PortfolioLocatorMock { get; }
        public Mock<IClosingValidator> ClosingValidatorMock { get; }
        public Mock<ITrustInfoProvider> TrustInfoProviderMock { get; }
        public Mock<IAccountingRecordsOper> AccountingRecordsOperMock { get; }
        public Mock<ILogger<AccountingRecordsValCommandHandler>> LoggerMock { get; }

        public long ClientOperationId => DefaultClientOperationId;
        public decimal Amount => DefaultAmount;
        public int AffiliateId => _affiliateId;
        public int ObjectiveId => _objectiveId;
        public int CauseId => DefaultCauseId;
        public DateTime PortfolioCurrentDate { get; private set; }
        public object? ValidationContext { get; private set; }

        private void SetupMocks()
        {
            RuleEvaluatorMock
                .Setup(evaluator => evaluator.EvaluateAsync(
                    "Operations.DebitNote.RequiredFields",
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(SuccessfulRules);

            RuleEvaluatorMock
                .Setup(evaluator => evaluator.EvaluateAsync(
                    "Operations.DebitNote.Validation",
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
                .Callback((string _, object context, CancellationToken _) => ValidationContext = context)
                .ReturnsAsync(() => _validationResult);

            ConfigurationParameterRepositoryMock
                .Setup(repository => repository.GetByIdAsync(
                    It.Is<int>(id => id == DefaultCauseId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _causeParameter);

            ClientOperationRepositoryMock
                .Setup(repository => repository.GetAsync(DefaultClientOperationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _clientOperation);

            ClientOperationRepositoryMock
                .Setup(repository => repository.HasActiveLinkedOperationAsync(
                    DefaultClientOperationId,
                    It.IsAny<long>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _hasActiveLinkedOperation);

            OperationTypeRepositoryMock
                .Setup(repository => repository.GetByIdAsync(
                    It.Is<long>(id => id == _clientOperationTypeId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _operationType);

            OperationTypeRepositoryMock
                .Setup(repository => repository.GetByNameAsync("Aporte", It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => ToCollection(_contributionType));

            OperationTypeRepositoryMock
                .Setup(repository => repository.GetByNameAsync("Nota Débito", It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => ToCollection(_debitNoteType));

            OperationTypeRepositoryMock
                .Setup(repository => repository.GetByNameAsync("Ajuste Rendimientos", It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => ToCollection(_trustAdjustmentType));

            PortfolioLocatorMock
                .Setup(locator => locator.FindByPortfolioIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => Result.Success((PortfolioId: (long)_portfolioId, Name: "Portfolio", CurrentDate: PortfolioCurrentDate)));

            ClosingValidatorMock
                .Setup(validator => validator.IsClosingActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            TrustInfoProviderMock
                .Setup(provider => provider.GetAsync(
                    It.IsAny<long>(),
                    It.IsAny<decimal>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _trustInfoResult);
        }

        public AccountingRecordsValCommand CreateCommand(
            long? clientOperationId = null,
            decimal? amount = null,
            int? causeId = null,
            int? affiliateId = null,
            int? objectiveId = null)
        {
            return new AccountingRecordsValCommand(
                clientOperationId ?? DefaultClientOperationId,
                amount ?? DefaultAmount,
                causeId ?? DefaultCauseId,
                affiliateId ?? _affiliateId,
                objectiveId ?? _objectiveId);
        }

        public Task<Result<AccountingRecordsValResult>> HandleAsync(AccountingRecordsValCommand? command = null)
        {
            var handler = new AccountingRecordsValCommandHandler(
                RuleEvaluatorMock.Object,
                ClientOperationRepositoryMock.Object,
                ConfigurationParameterRepositoryMock.Object,
                OperationTypeRepositoryMock.Object,
                PortfolioLocatorMock.Object,
                ClosingValidatorMock.Object,
                TrustInfoProviderMock.Object,
                AccountingRecordsOperMock.Object,
                LoggerMock.Object);

            return handler.Handle(command ?? CreateCommand(), CancellationToken.None);
        }

        public TestFixture WithValidationFailure(RuleValidationError error)
        {
            _validationResult = (false, Array.Empty<RuleResultTree>(), new[] { error });
            return this;
        }

        public TestFixture WithClosingAvailable(bool closingAvailable)
        {
            ClosingValidatorMock
                .Setup(validator => validator.IsClosingActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(!closingAvailable);

            return this;
        }

        public TestFixture WithProcessDate(Func<TestFixture, DateTime> selector)
        {
            _processDate = selector(this);
            _clientOperation = BuildClientOperation();
            return this;
        }

        public TestFixture WithPendingAnnulment(bool hasPending)
        {
            _hasActiveLinkedOperation = hasPending;
            return this;
        }

        public TestFixture WithOperationStatus(LifecycleStatus status)
        {
            _status = status;
            _clientOperation = BuildClientOperation();
            return this;
        }

        public TestFixture WithoutCauseConfigurationParameter()
        {
            _causeParameter = null;
            return this;
        }

        public TestFixture WithOperationType(OperationType operationType)
        {
            _operationType = operationType;
            _clientOperationTypeId = operationType.OperationTypeId;
            _clientOperation = BuildClientOperation();
            return this;
        }

        public TestFixture WithoutOperationType()
        {
            _operationType = null;
            return this;
        }

        public TestFixture WithoutContributionType()
        {
            _contributionType = null;
            return this;
        }

        public TestFixture WithoutTrustAdjustmentType()
        {
            _trustAdjustmentType = null;
            _operationType = CreateOperationType(204, "Operación Alterna", null);
            _clientOperationTypeId = _operationType.OperationTypeId;
            _clientOperation = BuildClientOperation();
            return this;
        }

        public TestFixture WithTrustInfo(Result<TrustInfoResult> trustInfoResult)
        {
            _trustInfoResult = trustInfoResult;
            return this;
        }

        public TProperty GetValidationContext<TProperty>(string propertyName)
        {
            ValidationContext.Should().NotBeNull();
            return GetPropertyValue<TProperty>(ValidationContext!, propertyName);
        }

        private ClientOperation BuildClientOperation()
        {
            var operation = ClientOperation.Create(
                DateTime.UtcNow,
                _affiliateId,
                _objectiveId,
                _portfolioId,
                DefaultAmount,
                _processDate,
                _clientOperationTypeId,
                DateTime.UtcNow.AddDays(-1),
                _status,
                causeId: null,
                trustId: _trustId).Value;

            SetProperty(operation, nameof(ClientOperation.ClientOperationId), DefaultClientOperationId);

            return operation;
        }

        private static ConfigurationParameter CreateConfigurationParameter(
            int causeId,
            int parameterId)
        {
            var parameter = ConfigurationParameter.Create(
                "Causal Nota Débito",
                causeId.ToString(CultureInfo.InvariantCulture),
                "CauseScope");

            SetProperty(parameter, nameof(ConfigurationParameter.ConfigurationParameterId), parameterId);

            return parameter;
        }

        private static IReadOnlyCollection<OperationType> ToCollection(OperationType? operationType)
        {
            return operationType is null
                ? Array.Empty<OperationType>()
                : new[] { operationType };
        }
    }
}
