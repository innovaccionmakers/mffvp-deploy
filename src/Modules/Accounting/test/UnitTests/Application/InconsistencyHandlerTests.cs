using Accounting.Application;
using Accounting.Application.Abstractions;
using Accounting.Domain.AccountingInconsistencies;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accounting.test.UnitTests.Application;

/// <summary>
/// Tests unitarios para la clase InconsistencyHandler
/// </summary>
public class InconsistencyHandlerTests
{
    private readonly Mock<IAccountingInconsistencyRepository> _repositoryMock;
    private readonly Mock<ILogger<InconsistencyHandler>> _loggerMock;
    private readonly InconsistencyHandler _handler;

    public InconsistencyHandlerTests()
    {
        _repositoryMock = new Mock<IAccountingInconsistencyRepository>();
        _loggerMock = new Mock<ILogger<InconsistencyHandler>>();
        _handler = new InconsistencyHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleInconsistenciesAsync_ConInconsistenciasValidas_DeberiaProcesarExitosamente()
    {
        // Arrange
        var inconsistencies = new List<AccountingInconsistency>
        {
            AccountingInconsistency.Create(1, "TXN001", "Error de validación", "Actividad A"),
            AccountingInconsistency.Create(2, "TXN002", "Error de cálculo", "Actividad B")
        };
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;

        _repositoryMock.Setup(x => x.SaveInconsistenciesAsync(
                It.IsAny<IEnumerable<AccountingInconsistency>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.HandleInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Assert
        _repositoryMock.Verify(x => x.SaveInconsistenciesAsync(
            inconsistencies, processDate, processType, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task HandleInconsistenciesAsync_ConListaVacia_DeberiaProcesarSinErrores()
    {
        // Arrange
        var inconsistencies = new List<AccountingInconsistency>();
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;

        _repositoryMock.Setup(x => x.SaveInconsistenciesAsync(
                It.IsAny<IEnumerable<AccountingInconsistency>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.HandleInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Assert
        _repositoryMock.Verify(x => x.SaveInconsistenciesAsync(
            inconsistencies, processDate, processType, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task HandleInconsistenciesAsync_ConFalloEnRepositorio_DeberiaManejarElError()
    {
        // Arrange
        var inconsistencies = new List<AccountingInconsistency>
        {
            AccountingInconsistency.Create(1, "TXN001", "Error de validación", "Actividad A")
        };
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;
        var errorMessage = "Error de conexión a Redis";

        _repositoryMock.Setup(x => x.SaveInconsistenciesAsync(
                It.IsAny<IEnumerable<AccountingInconsistency>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.NotFound("Repository.Error", errorMessage)));

        // Act
        await _handler.HandleInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Assert
        _repositoryMock.Verify(x => x.SaveInconsistenciesAsync(
            inconsistencies, processDate, processType, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task HandleInconsistenciesAsync_ConExcepcionEnRepositorio_DeberiaManejarLaExcepcion()
    {
        // Arrange
        var inconsistencies = new List<AccountingInconsistency>
        {
            AccountingInconsistency.Create(1, "TXN001", "Error de validación", "Actividad A")
        };
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Error interno del repositorio");

        _repositoryMock.Setup(x => x.SaveInconsistenciesAsync(
                It.IsAny<IEnumerable<AccountingInconsistency>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await _handler.HandleInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Assert
        // La excepción se captura y loggea, pero no se propaga
        _repositoryMock.Verify(x => x.SaveInconsistenciesAsync(
            inconsistencies, processDate, processType, cancellationToken), Times.Once);

        // Verificar que se loggeó el error con la excepción
        VerifyLogErrorWithException(exception, "Error al manejar las inconsistencias para {ProcessType}", processType);
    }

    [Fact]
    public async Task HandleInconsistenciesAsync_ConCancellationTokenCancelado_DeberiaManejarLaCancelacion()
    {
        // Arrange
        var inconsistencies = new List<AccountingInconsistency>
        {
            AccountingInconsistency.Create(1, "TXN001", "Error de validación", "Actividad A")
        };
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var operationCanceledException = new OperationCanceledException();

        _repositoryMock.Setup(x => x.SaveInconsistenciesAsync(
                It.IsAny<IEnumerable<AccountingInconsistency>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(operationCanceledException);

        // Act
        await _handler.HandleInconsistenciesAsync(inconsistencies, processDate, processType, cancellationTokenSource.Token);

        // Assert
        // La excepción de cancelación se captura y loggea, pero no se propaga
        _repositoryMock.Verify(x => x.SaveInconsistenciesAsync(
            inconsistencies, processDate, processType, cancellationTokenSource.Token), Times.Once);

        // Verificar que se loggeó el error con la excepción de cancelación
        VerifyLogErrorWithException(operationCanceledException, "Error al manejar las inconsistencias para {ProcessType}", processType);
    }

    [Fact]
    public async Task HandleInconsistenciesAsync_DeberiaLoggearWarningAlIniciar()
    {
        // Arrange
        var inconsistencies = new List<AccountingInconsistency>
        {
            AccountingInconsistency.Create(1, "TXN001", "Error de validación", "Actividad A")
        };
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;

        _repositoryMock.Setup(x => x.SaveInconsistenciesAsync(
                It.IsAny<IEnumerable<AccountingInconsistency>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.HandleInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Assert
        VerifyLogWarning("Se detectaron inconsistencias en el proceso {ProcessType} para la fecha {ProcessDate}", processType, processDate);
    }

    [Fact]
    public async Task HandleInconsistenciesAsync_ConExito_DeberiaLoggearInformacion()
    {
        // Arrange
        var inconsistencies = new List<AccountingInconsistency>
        {
            AccountingInconsistency.Create(1, "TXN001", "Error de validación", "Actividad A")
        };
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;

        _repositoryMock.Setup(x => x.SaveInconsistenciesAsync(
                It.IsAny<IEnumerable<AccountingInconsistency>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.HandleInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Assert
        VerifyLogInformation("Inconsistencias procesadas exitosamente para el proceso {ProcessType} en la fecha {ProcessDate}", processType, processDate);
    }

    [Fact]
    public async Task HandleInconsistenciesAsync_ConFalloEnRepositorio_DeberiaLoggearError()
    {
        // Arrange
        var inconsistencies = new List<AccountingInconsistency>
        {
            AccountingInconsistency.Create(1, "TXN001", "Error de validación", "Actividad A")
        };
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;
        var errorMessage = "Error de conexión a Redis";

        _repositoryMock.Setup(x => x.SaveInconsistenciesAsync(
                It.IsAny<IEnumerable<AccountingInconsistency>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.NotFound("Repository.Error", errorMessage)));

        // Act
        await _handler.HandleInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Assert
        VerifyLogError("Error al procesar inconsistencias: {Error}", errorMessage);
    }

    [Fact]
    public async Task HandleInconsistenciesAsync_ConExcepcion_DeberiaLoggearErrorConExcepcion()
    {
        // Arrange
        var inconsistencies = new List<AccountingInconsistency>
        {
            AccountingInconsistency.Create(1, "TXN001", "Error de validación", "Actividad A")
        };
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Error interno del repositorio");

        _repositoryMock.Setup(x => x.SaveInconsistenciesAsync(
                It.IsAny<IEnumerable<AccountingInconsistency>>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await _handler.HandleInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Assert
        // La excepción se captura y loggea, pero no se propaga
        VerifyLogErrorWithException(exception, "Error al manejar las inconsistencias para {ProcessType}", processType);
    }

    private void VerifyLogWarning(string message, string processType, DateTime processDate)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Se detectaron inconsistencias")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void VerifyLogInformation(string message, string processType, DateTime processDate)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Inconsistencias procesadas exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void VerifyLogError(string message, string error)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al procesar inconsistencias")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void VerifyLogErrorWithException(Exception exception, string message, string processType)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al manejar las inconsistencias")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
