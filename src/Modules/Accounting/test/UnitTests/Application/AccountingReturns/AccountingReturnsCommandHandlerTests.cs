using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Application.AccountingReturns;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingReturns;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Operations.Integrations.OperationTypes;
using System.Text.Json;

namespace Accounting.test.UnitTests.Application.AccountingReturns;

/// <summary>
/// Tests unitarios para la clase AccountingReturnsCommandHandler
/// </summary>
public class AccountingReturnsCommandHandlerTests
{
    private readonly Mock<ILogger<AccountingReturnsCommandHandler>> _loggerMock;
    private readonly Mock<ILogger> _yieldProcessorLoggerMock;
    private readonly Mock<ILogger> _yieldDetailProcessorLoggerMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<IPassiveTransactionRepository> _passiveTransactionRepositoryMock;
    private readonly Mock<IYieldLocator> _yieldLocatorMock;
    private readonly Mock<IYieldDetailsLocator> _yieldDetailsLocatorMock;
    private readonly Mock<IPortfolioLocator> _portfolioLocatorMock;
    private readonly Mock<IOperationLocator> _operationLocatorMock;
    private readonly Mock<IInconsistencyHandler> _inconsistencyHandlerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AccountingReturnsCommandHandler _handler;

    public AccountingReturnsCommandHandlerTests()
    {
        _loggerMock = new Mock<ILogger<AccountingReturnsCommandHandler>>();
        _yieldProcessorLoggerMock = new Mock<ILogger>();
        _yieldDetailProcessorLoggerMock = new Mock<ILogger>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _passiveTransactionRepositoryMock = new Mock<IPassiveTransactionRepository>();
        _yieldLocatorMock = new Mock<IYieldLocator>();
        _yieldDetailsLocatorMock = new Mock<IYieldDetailsLocator>();
        _portfolioLocatorMock = new Mock<IPortfolioLocator>();
        _operationLocatorMock = new Mock<IOperationLocator>();
        _inconsistencyHandlerMock = new Mock<IInconsistencyHandler>();
        _mediatorMock = new Mock<IMediator>();

        var yieldProcessorCategoryName = typeof(YieldProcessor).FullName!;
        var yieldDetailProcessorCategoryName = typeof(YieldDetailProcessor).FullName!;

        _loggerFactoryMock.Setup(x => x.CreateLogger(yieldProcessorCategoryName))
            .Returns(_yieldProcessorLoggerMock.Object);
        _loggerFactoryMock.Setup(x => x.CreateLogger(yieldDetailProcessorCategoryName))
            .Returns(_yieldDetailProcessorLoggerMock.Object);

        _handler = new AccountingReturnsCommandHandler(
            _loggerMock.Object,
            _loggerFactoryMock.Object,
            _passiveTransactionRepositoryMock.Object,
            _yieldLocatorMock.Object,
            _yieldDetailsLocatorMock.Object,
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
        var command = new AccountingReturnsCommand(new[] { 1, 2 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;

        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true),
            new(2, 2, 2000, 200, 100, 50, 1750, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };

        var yieldDetails = new List<YieldDetailResponse>
        {
            new(1, 1, 100, 10, 5, DateTime.UtcNow, DateTime.UtcNow, true),
            new(2, 2, 200, 20, 10, DateTime.UtcNow, DateTime.UtcNow, true)
        };

        var operationTypeYield = new OperationTypeResponse(1, OperationTypeNames.Yield, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));

        var operationTypeAdjustYields = new OperationTypeResponse(2, OperationTypeNames.AdjustYields, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));

        var passiveTransaction1 = Domain.PassiveTransactions.PassiveTransaction.Create(1, 1, "1234", "5678", "910", "112");
        var passiveTransaction2 = Domain.PassiveTransactions.PassiveTransaction.Create(2, 1, "1234", "5678", "910", "112");
        var passiveTransactionAdjust1 = Domain.PassiveTransactions.PassiveTransaction.Create(1, 2, "2234", "2678", "2910", "2112");
        var passiveTransactionAdjust2 = Domain.PassiveTransactions.PassiveTransaction.Create(2, 2, "2234", "2678", "2910", "2112");

        var portfolioInfo = new PortfolioResponse(
            "1232",
            1,
            "Name"
        );

        _yieldLocatorMock.Setup(x => x.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _yieldDetailsLocatorMock.Setup(x => x.GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(command.PortfolioIds, command.ProcessDate, SourceTypes.ExtraYield, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetails));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Yield, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Nature, string Name)>((operationTypeYield.OperationTypeId, operationTypeYield.Nature.ToString(), operationTypeYield.Name)));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.AdjustYields, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Nature, string Name)>((operationTypeAdjustYields.OperationTypeId, operationTypeAdjustYields.Nature.ToString(), operationTypeAdjustYields.Name)));

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdsAndOperationTypeAsync(
                It.Is<IEnumerable<int>>(ids => ids.Contains(1) || ids.Contains(2)),
                It.Is<long>(id => id == operationTypeYield.OperationTypeId),
                cancellationToken))
            .ReturnsAsync(new[] { passiveTransaction1.Value, passiveTransaction2.Value });

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdsAndOperationTypeAsync(
                It.Is<IEnumerable<int>>(ids => ids.Contains(1) || ids.Contains(2)),
                It.Is<long>(id => id == operationTypeAdjustYields.OperationTypeId),
                cancellationToken))
            .ReturnsAsync(new[] { passiveTransactionAdjust1.Value, passiveTransactionAdjust2.Value });

        _portfolioLocatorMock.Setup(x => x.GetPortfolioInformationAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(Result.Success(portfolioInfo));

        _mediatorMock.Setup(x => x.Send(It.IsAny<AddAccountingEntitiesCommand>(), cancellationToken))
            .ReturnsAsync(Result.Success(true));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        _mediatorMock.Verify(x => x.Send(It.IsAny<AddAccountingEntitiesCommand>(), cancellationToken), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ConFalloEnYieldLocator_DeberiaRetornarVerdadero()
    {
        // Arrange
        var command = new AccountingReturnsCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var error = Error.NotFound("YieldLocator.Error", "No se encontraron yields");

        _yieldLocatorMock.Setup(x => x.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Failure<IReadOnlyCollection<YieldResponse>>(error));

        var yieldDetails = new List<YieldDetailResponse>();
        _yieldDetailsLocatorMock.Setup(x => x.GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(command.PortfolioIds, command.ProcessDate, SourceTypes.ExtraYield, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetails));

        var operationTypeAdjustYields = new OperationTypeResponse(2, OperationTypeNames.AdjustYields, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));
        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.AdjustYields, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Nature, string Name)>((operationTypeAdjustYields.OperationTypeId, operationTypeAdjustYields.Nature.ToString(), operationTypeAdjustYields.Name)));

        _mediatorMock.Setup(x => x.Send(It.IsAny<AddAccountingEntitiesCommand>(), cancellationToken))
            .ReturnsAsync(Result.Success(true));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ConFalloEnOperationLocator_DeberiaRetornarFalse()
    {
        // Arrange
        var command = new AccountingReturnsCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };
        var error = Error.NotFound("OperationLocator.Error", "No se encontró el tipo de operación");

        _yieldLocatorMock.Setup(x => x.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Yield, cancellationToken))
            .ReturnsAsync(Result.Failure<(long OperationTypeId, string Name, string Nature)>(error));

        var yieldDetails = new List<YieldDetailResponse>();
        _yieldDetailsLocatorMock.Setup(x => x.GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(command.PortfolioIds, command.ProcessDate, SourceTypes.ExtraYield, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetails));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ConInconsistenciasEnCreateRange_DeberiaManejarInconsistencias()
    {
        // Arrange
        var command = new AccountingReturnsCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };
        var operationType = new OperationTypeResponse(1, OperationTypeNames.Yield, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));

        _yieldLocatorMock.Setup(x => x.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Yield, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Name, string Nature)>((operationType.OperationTypeId, operationType.Name, operationType.Nature.ToString())));

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdsAndOperationTypeAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<long>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<Domain.PassiveTransactions.PassiveTransaction>());

        var yieldDetails = new List<YieldDetailResponse>();
        _yieldDetailsLocatorMock.Setup(x => x.GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(command.PortfolioIds, command.ProcessDate, SourceTypes.ExtraYield, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetails));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        _inconsistencyHandlerMock.Verify(x => x.HandleInconsistenciesAsync(
            It.IsAny<IEnumerable<AccountingInconsistency>>(),
            command.ProcessDate,
            ProcessTypes.AccountingReturns,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcion_DeberiaManejarExcepcionYRetornarFalse()
    {
        // Arrange
        var command = new AccountingReturnsCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Error interno");

        _yieldLocatorMock.Setup(x => x.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
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
        var command = new AccountingReturnsCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };

        var operationType = new OperationTypeResponse(1, OperationTypeNames.Yield, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));
        var passiveTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(1, 1, "123", null, "123", "123");

        _yieldLocatorMock.Setup(x => x.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Yield, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Nature, string Name)>((operationType.OperationTypeId, operationType.Nature.ToString(), operationType.Name)));

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdsAndOperationTypeAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<long>(), cancellationToken))
            .ReturnsAsync(new[] { passiveTransaction.Value });

        var yieldDetails = new List<YieldDetailResponse>();
        _yieldDetailsLocatorMock.Setup(x => x.GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(command.PortfolioIds, command.ProcessDate, SourceTypes.ExtraYield, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetails));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        _inconsistencyHandlerMock.Verify(x => x.HandleInconsistenciesAsync(
            It.IsAny<IEnumerable<AccountingInconsistency>>(),
            command.ProcessDate,
            ProcessTypes.AccountingReturns,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ConTransaccionPasivaSinCuentaDebito_DeberiaGenerarInconsistencia()
    {
        // Arrange
        var command = new AccountingReturnsCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };
        var operationType = new OperationTypeResponse(1, OperationTypeNames.Yield, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));
        var passiveTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(1, 1, null, "123", "123", "123");

        _yieldLocatorMock.Setup(x => x.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Yield, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Name, string Nature)>((operationType.OperationTypeId, operationType.Name, operationType.Nature.ToString())));

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdsAndOperationTypeAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<long>(), cancellationToken))
            .ReturnsAsync(new[] { passiveTransaction.Value });

        var yieldDetails = new List<YieldDetailResponse>();
        _yieldDetailsLocatorMock.Setup(x => x.GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(command.PortfolioIds, command.ProcessDate, SourceTypes.ExtraYield, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetails));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        _inconsistencyHandlerMock.Verify(x => x.HandleInconsistenciesAsync(
            It.IsAny<IEnumerable<AccountingInconsistency>>(),
            command.ProcessDate,
            ProcessTypes.AccountingReturns,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ConFalloEnPortfolioLocator_DeberiaGenerarInconsistencia()
    {
        // Arrange
        var command = new AccountingReturnsCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };
        var operationType = new OperationTypeResponse(1, OperationTypeNames.Yield, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));
        var passiveTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(1, 1, "1234", "5678", "910", "112");
        var error = Error.NotFound("PortfolioLocator.Error", "No se encontró el portafolio");

        _yieldLocatorMock.Setup(x => x.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Yield, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Name, string Nature)>((operationType.OperationTypeId, operationType.Name, operationType.Nature.ToString())));

         _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdsAndOperationTypeAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<long>(), cancellationToken))
            .ReturnsAsync(new[] { passiveTransaction.Value });

        _portfolioLocatorMock.Setup(x => x.GetPortfolioInformationAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(Result.Failure<PortfolioResponse>(error));

        var yieldDetails = new List<YieldDetailResponse>();
        _yieldDetailsLocatorMock.Setup(x => x.GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(command.PortfolioIds, command.ProcessDate, SourceTypes.ExtraYield, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetails));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        _inconsistencyHandlerMock.Verify(x => x.HandleInconsistenciesAsync(
            It.IsAny<IEnumerable<AccountingInconsistency>>(),
            command.ProcessDate,
            ProcessTypes.AccountingReturns,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ConTransactionPasivaSinCuentasContra_DeberiaGenerarInconsistencia()
    {
        // Arrange
        var command = new AccountingReturnsCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 25, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };
        var operationType = new OperationTypeResponse(1, OperationTypeNames.Yield, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));
        var passiveTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(1, 1, "1234", "5678", null, null);

        _yieldLocatorMock.Setup(x => x.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Yield, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Name, string Nature)>((operationType.OperationTypeId, operationType.Name, operationType.Nature.ToString())));

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdsAndOperationTypeAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<long>(), cancellationToken))
            .ReturnsAsync(new[] { passiveTransaction.Value });

        var portfolioInfo = new PortfolioResponse(
            "1232",
            1,
            "Name"
        );

        _portfolioLocatorMock.Setup(x => x.GetPortfolioInformationAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(Result.Success(portfolioInfo));

        var yieldDetails = new List<YieldDetailResponse>();
        _yieldDetailsLocatorMock.Setup(x => x.GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(command.PortfolioIds, command.ProcessDate, SourceTypes.ExtraYield, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetails));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        _inconsistencyHandlerMock.Verify(x => x.HandleInconsistenciesAsync(
            It.IsAny<IEnumerable<AccountingInconsistency>>(),
            command.ProcessDate,
            ProcessTypes.AccountingReturns,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ConYieldPositivo_DeberiaUsarCuentasNormales()
    {
        // Arrange
        var command = new AccountingReturnsCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, 100, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };

        var operationType = new OperationTypeResponse(1, OperationTypeNames.Yield, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));
        var passiveTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(1, 1, "1234", "5678", "910", "112");
        var portfolioInfo = new PortfolioResponse(
            "1232",
            1,
            "Name"
        );

        _yieldLocatorMock.Setup(x => x.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        var operationTypeResult = Result.Success<(long OperationTypeId, string Name, string Nature)>((operationType.OperationTypeId, operationType.Name, operationType.Nature.ToString()));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Yield, cancellationToken))
            .ReturnsAsync(operationTypeResult);

        var operationTypeAdjustYields = new OperationTypeResponse(2, OperationTypeNames.AdjustYields, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));
        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.AdjustYields, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Nature, string Name)>((operationTypeAdjustYields.OperationTypeId, operationTypeAdjustYields.Nature.ToString(), operationTypeAdjustYields.Name)));

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdsAndOperationTypeAsync(
                It.Is<IEnumerable<int>>(ids => ids.Contains(1)),
                It.Is<long>(id => id == operationType.OperationTypeId),
                cancellationToken))
            .ReturnsAsync(new[] { passiveTransaction.Value });

        _portfolioLocatorMock.Setup(x => x.GetPortfolioInformationAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(Result.Success(portfolioInfo));

        _mediatorMock.Setup(x => x.Send(It.IsAny<AddAccountingEntitiesCommand>(), cancellationToken))
            .ReturnsAsync(Result.Success(true));

        var yieldDetails = new List<YieldDetailResponse>();
        _yieldDetailsLocatorMock.Setup(x => x.GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(command.PortfolioIds, command.ProcessDate, SourceTypes.ExtraYield, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetails));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        _mediatorMock.Verify(x => x.Send(It.IsAny<AddAccountingEntitiesCommand>(), cancellationToken), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ConYieldNegativo_DeberiaUsarCuentasContra()
    {
        // Arrange
        var command = new AccountingReturnsCommand(new[] { 1 }, DateTime.UtcNow);
        var cancellationToken = CancellationToken.None;
        var yields = new List<YieldResponse>
        {
            new(1, 1, 1000, 100, 50, -100, 875, 0, DateTime.UtcNow, DateTime.UtcNow, true)
        };

        var operationType = new OperationTypeResponse(1, OperationTypeNames.Yield, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));
        var passiveTransaction = Domain.PassiveTransactions.PassiveTransaction.Create(1, 1, "1234", "5678", "910", "112");
        var portfolioInfo = new PortfolioResponse(
            "1232",
            1,
            "Name"
        );

        _yieldLocatorMock.Setup(x => x.GetAllReturnsPortfolioIdsAndClosingDate(command.PortfolioIds, command.ProcessDate, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldResponse>>(yields));

        var operationTypeResult = Result.Success<(long OperationTypeId, string Name, string Nature)>((operationType.OperationTypeId, operationType.Name, operationType.Nature.ToString()));

        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.Yield, cancellationToken))
            .ReturnsAsync(operationTypeResult);

        var operationTypeAdjustYields = new OperationTypeResponse(2, OperationTypeNames.AdjustYields, null, IncomeEgressNature.Income, Status.Active, "", "RE",
            JsonSerializer.SerializeToDocument(new
            {
                GrupoLista = "OperacionesClientes"
            }));
        _operationLocatorMock.Setup(x => x.GetOperationTypeByNameAsync(OperationTypeNames.AdjustYields, cancellationToken))
            .ReturnsAsync(Result.Success<(long OperationTypeId, string Nature, string Name)>((operationTypeAdjustYields.OperationTypeId, operationTypeAdjustYields.Nature.ToString(), operationTypeAdjustYields.Name)));

        _passiveTransactionRepositoryMock.Setup(x => x.GetByPortfolioIdsAndOperationTypeAsync(
                It.Is<IEnumerable<int>>(ids => ids.Contains(1)),
                It.Is<long>(id => id == operationType.OperationTypeId),
                cancellationToken))
            .ReturnsAsync(new[] { passiveTransaction.Value });

        _portfolioLocatorMock.Setup(x => x.GetPortfolioInformationAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(Result.Success(portfolioInfo));

        _mediatorMock.Setup(x => x.Send(It.IsAny<AddAccountingEntitiesCommand>(), cancellationToken))
            .ReturnsAsync(Result.Success(true));

        var yieldDetails = new List<YieldDetailResponse>();
        _yieldDetailsLocatorMock.Setup(x => x.GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(command.PortfolioIds, command.ProcessDate, SourceTypes.ExtraYield, cancellationToken))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetails));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        _mediatorMock.Verify(x => x.Send(It.IsAny<AddAccountingEntitiesCommand>(), cancellationToken), Times.Exactly(2));
    }

    #endregion
}
