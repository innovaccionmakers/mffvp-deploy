using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Application.AccountingFees;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingFees;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Operations.Integrations.OperationTypes;

namespace Accounting.test.UnitTests.Application.AccountingFees;

/// <summary>
/// Tests unitarios para la clase AccountingFeesCommandHandler
/// </summary>
public class AccountingFeesCommandHandlerTests
{
    private readonly Mock<ILogger<AccountingFeesCommandHandler>> _loggerMock;
    private readonly Mock<IPassiveTransactionRepository> _passiveTransactionRepositoryMock;
    private readonly Mock<IYieldLocator> _yieldLocatorMock;
    private readonly Mock<IPortfolioLocator> _portfolioLocatorMock;
    private readonly Mock<IOperationLocator> _operationLocatorMock;
    private readonly Mock<IInconsistencyHandler> _inconsistencyHandlerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AccountingFeesCommandHandler _handler;

    public AccountingFeesCommandHandlerTests()
    {
        _loggerMock = new Mock<ILogger<AccountingFeesCommandHandler>>();
        _passiveTransactionRepositoryMock = new Mock<IPassiveTransactionRepository>();
        _yieldLocatorMock = new Mock<IYieldLocator>();
        _portfolioLocatorMock = new Mock<IPortfolioLocator>();
        _operationLocatorMock = new Mock<IOperationLocator>();
        _inconsistencyHandlerMock = new Mock<IInconsistencyHandler>();
        _mediatorMock = new Mock<IMediator>();

        _handler = new AccountingFeesCommandHandler(
            _loggerMock.Object,
            _passiveTransactionRepositoryMock.Object,
            _yieldLocatorMock.Object,
            _portfolioLocatorMock.Object,
            _operationLocatorMock.Object,
            _inconsistencyHandlerMock.Object,
            _mediatorMock.Object);
    }

    #region Handle Tests

    [Fact]
    public async Task Handle_ConDatosValidos_DeberiaProcesarExitosamente()
    {
        // Arrange
        var command = new AccountingFeesCommand(new[] { 1, 2 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;

        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true),
            new(2, 2, 2000, 200, 100, 50, 1750, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };

        var operationType = new OperationTypeResponse(1, OperationTypeNames.Commission, null, IncomeEgressNature.Egress, Status.Active, "1", "CO");
        var passiveTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(1, 1, "1234", "5678", "123", "123");
        var portfolioInfo = new PortfolioResponse(
            "1232",
            1,
            "Name"
        );

        _yieldLocatorMock.Setup(x => x.GetAllComissionsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Commission, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Nature, string Name)>((operationType.OperationTypeId, operationType.Nature.ToString(), operationType.Name)));

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdAndOperationTypeAsync(It.IsAny<int>(), It.IsAny<long>(), cancellationToken))
            .ReturnsAsync(passiveTransaction.Value);

        _portfolioLocatorMock.Setup(x => x.GetPortfolioInformationAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(Result.Success(portfolioInfo));

        _mediatorMock.Setup(x => x.Send(It.IsAny<AddAccountingEntitiesCommand>(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        _mediatorMock.Verify(x => x.Send(It.IsAny<AddAccountingEntitiesCommand>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ConFalloEnYieldLocator_DeberiaRetornarFalso()
    {
        // Arrange
        var command = new AccountingFeesCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var error = Error.NotFound("YieldLocator.Error", "No se encontraron yields");

        _yieldLocatorMock.Setup(x => x.GetAllComissionsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Failure<IReadOnlyCollection<YieldResponse>>(error));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ConFalloEnOperationLocator_DeberiaRetornarFalso()
    {
        // Arrange
        var command = new AccountingFeesCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };
        var error = Error.NotFound("OperationLocator.Error", "No se encontró el tipo de operación");

        _yieldLocatorMock.Setup(x => x.GetAllComissionsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Commission, cancellationToken))
            .ReturnsAsync(Result.Failure<(long OperationTypeId, string Nature, string Name)>(error));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ConInconsistenciasEnCreateRange_DeberiaManejarInconsistencias()
    {
        // Arrange
        var command = new AccountingFeesCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };
        var operationType = new OperationTypeResponse(1, OperationTypeNames.Commission, null, IncomeEgressNature.Egress, Status.Active, "", "CO");

        _yieldLocatorMock.Setup(x => x.GetAllComissionsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Commission, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Nature, string Name)>((operationType.OperationTypeId, operationType.Nature.ToString(), operationType.Name)));

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdAndOperationTypeAsync(It.IsAny<int>(), It.IsAny<long>(), cancellationToken))
            .ReturnsAsync((Domain.PassiveTransactions.PassiveTransaction?)null); // Simular que no existe transacción pasiva

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        _inconsistencyHandlerMock.Verify(x => x.HandleInconsistenciesAsync(
            It.IsAny<IEnumerable<AccountingInconsistency>>(),
            command.ProcessDate,
            ProcessTypes.AccountingFees,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcion_DeberiaManejarExcepcionYRetornarFalso()
    {
        // Arrange
        var command = new AccountingFeesCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Error interno");

        _yieldLocatorMock.Setup(x => x.GetAllComissionsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
    }

    #endregion

    #region CreateRange Tests (Testing through Handle method)

    [Fact]
    public async Task Handle_ConTransaccionPasivaSinCuentaCredito_DeberiaGenerarInconsistencia()
    {
        // Arrange
        var command = new AccountingFeesCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };

        var operationType = new OperationTypeResponse(1, OperationTypeNames.Commission, null, IncomeEgressNature.Egress, Status.Active, "", "CO");
        var passiveTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(1, 1, "123", null, "123", "123"); // Sin cuenta de crédito

        _yieldLocatorMock.Setup(x => x.GetAllComissionsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Commission, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Nature, string Name)>((operationType.OperationTypeId, operationType.Nature.ToString(), operationType.Name)));

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdAndOperationTypeAsync(It.IsAny<int>(), It.IsAny<long>(), cancellationToken))
            .ReturnsAsync(passiveTransaction.Value);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        _inconsistencyHandlerMock.Verify(x => x.HandleInconsistenciesAsync(
            It.IsAny<IEnumerable<AccountingInconsistency>>(),
            command.ProcessDate,
            ProcessTypes.AccountingFees,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ConTransaccionPasivaSinCuentaDebito_DeberiaGenerarInconsistencia()
    {
        // Arrange
        var command = new AccountingFeesCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };
        var operationType = new OperationTypeResponse(1, OperationTypeNames.Commission, null, IncomeEgressNature.Egress, Status.Active, "", "CO");
        var passiveTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(1, 1, null, "123", "123", "123"); // Sin cuenta de débito

        _yieldLocatorMock.Setup(x => x.GetAllComissionsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Commission, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Nature, string Name)>((operationType.OperationTypeId, operationType.Nature.ToString(), operationType.Name)));

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdAndOperationTypeAsync(It.IsAny<int>(), It.IsAny<long>(), cancellationToken))
            .ReturnsAsync(passiveTransaction.Value);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        _inconsistencyHandlerMock.Verify(x => x.HandleInconsistenciesAsync(
            It.IsAny<IEnumerable<AccountingInconsistency>>(),
            command.ProcessDate,
            ProcessTypes.AccountingFees,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ConFalloEnPortfolioLocator_DeberiaGenerarInconsistencia()
    {
        // Arrange
        var command = new AccountingFeesCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };
        var operationType = new OperationTypeResponse(1, OperationTypeNames.Commission, null, IncomeEgressNature.Egress, Status.Active, "", "CO");
        var passiveTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(1, 1, "1234", "5678", "123", "123");
        var error = Error.NotFound("PortfolioLocator.Error", "No se encontró el portafolio");

        _yieldLocatorMock.Setup(x => x.GetAllComissionsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Commission, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Nature, string Name)>((operationType.OperationTypeId, operationType.Nature.ToString(), operationType.Name)));

         _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdAndOperationTypeAsync(It.IsAny<int>(), It.IsAny<long>(), cancellationToken)).ReturnsAsync(passiveTransaction.Value);

        _portfolioLocatorMock.Setup(x => x.GetPortfolioInformationAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(Result.Failure<PortfolioResponse>(error));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        _inconsistencyHandlerMock.Verify(x => x.HandleInconsistenciesAsync(
            It.IsAny<IEnumerable<AccountingInconsistency>>(),
            command.ProcessDate,
            ProcessTypes.AccountingFees,
            cancellationToken), Times.Once);
    }

    #endregion
}
