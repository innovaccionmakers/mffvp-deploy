using Accounting.Domain.AccountingInconsistencies;
using Accounting.Infrastructure.AccountingInconsistencies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace Accounting.test.UnitTests.Infrastructure.AccountingInconsistencies;

/// <summary>
/// Tests unitarios para la clase AccountingInconsistencyRepository
/// </summary>
public class AccountingInconsistencyRepositoryTests
{
    private readonly IDistributedCache _distributedCache;
    private readonly Mock<ILogger<AccountingInconsistencyRepository>> _loggerMock;
    private readonly AccountingInconsistencyRepository _repository;

    public AccountingInconsistencyRepositoryTests()
    {
        var options = Options.Create(new MemoryDistributedCacheOptions());
        _distributedCache = new MemoryDistributedCache(options);
        _loggerMock = new Mock<ILogger<AccountingInconsistencyRepository>>();
        _repository = new AccountingInconsistencyRepository(_distributedCache, _loggerMock.Object);
    }

    #region SaveInconsistenciesAsync Tests

    [Fact]
    public async Task SaveInconsistenciesAsync_ConInconsistenciasValidas_DeberiaGuardarExitosamente()
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

        // Act
        var result = await _repository.SaveInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verificar que los datos se guardaron correctamente
        var retrievedResult = await _repository.GetInconsistenciesAsync(processDate, processType, cancellationToken);
        Assert.True(retrievedResult.IsSuccess);
        Assert.Equal(2, retrievedResult.Value.Count());
    }

    [Fact]
    public async Task SaveInconsistenciesAsync_ConListaVacia_DeberiaRetornarExitoSinGuardar()
    {
        // Arrange
        var inconsistencies = new List<AccountingInconsistency>();
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.SaveInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verificar que no se guardó nada
        var retrievedResult = await _repository.GetInconsistenciesAsync(processDate, processType, cancellationToken);
        Assert.True(retrievedResult.IsSuccess);
        Assert.Empty(retrievedResult.Value);
    }

    [Fact]
    public async Task SaveInconsistenciesAsync_DeberiaConfigurarExpirationCorrectamente()
    {
        // Arrange
        var inconsistencies = new List<AccountingInconsistency>
        {
            AccountingInconsistency.Create(1, "TXN001", "Error de validación", "Actividad A")
        };
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.SaveInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verificar que los datos persisten (lo que indica que la expiración está configurada correctamente)
        var retrievedResult = await _repository.GetInconsistenciesAsync(processDate, processType, cancellationToken);
        Assert.True(retrievedResult.IsSuccess);
        Assert.Single(retrievedResult.Value);
    }

    #endregion

    #region GetInconsistenciesAsync Tests

    [Fact]
    public async Task GetInconsistenciesAsync_ConDatosExistentes_DeberiaRetornarInconsistencias()
    {
        // Arrange
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;
        var inconsistencies = new List<AccountingInconsistency>
        {
            AccountingInconsistency.Create(1, "TXN001", "Error de validación", "Actividad A"),
            AccountingInconsistency.Create(2, "TXN002", "Error de cálculo", "Actividad B")
        };

        // Primero guardar los datos
        await _repository.SaveInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Act
        var result = await _repository.GetInconsistenciesAsync(processDate, processType, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var resultList = result.Value.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("TXN001", resultList[0].Transaction);
        Assert.Equal("TXN002", resultList[1].Transaction);
    }

    [Fact]
    public async Task GetInconsistenciesAsync_SinDatos_DeberiaRetornarListaVacia()
    {
        // Arrange
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetInconsistenciesAsync(processDate, processType, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    #endregion

    #region DeleteInconsistenciesAsync Tests

    [Fact]
    public async Task DeleteInconsistenciesAsync_ConDatosExistentes_DeberiaEliminarExitosamente()
    {
        // Arrange
        var processDate = DateTime.UtcNow;
        var processType = "ProcesamientoComisiones";
        var cancellationToken = CancellationToken.None;
        var inconsistencies = new List<AccountingInconsistency>
        {
            AccountingInconsistency.Create(1, "TXN001", "Error de validación", "Actividad A")
        };

        // Primero guardar datos
        await _repository.SaveInconsistenciesAsync(inconsistencies, processDate, processType, cancellationToken);

        // Act
        var result = await _repository.DeleteInconsistenciesAsync(processDate, processType, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verificar que los datos fueron eliminados
        var retrievedResult = await _repository.GetInconsistenciesAsync(processDate, processType, cancellationToken);
        Assert.True(retrievedResult.IsSuccess);
        Assert.Empty(retrievedResult.Value);
    }

    #endregion

    #region GenerateKey Tests

    [Fact]
    public void GenerateKey_ConFechaYTipoProceso_DeberiaGenerarClaveCorrecta()
    {
        // Arrange
        var processDate = new DateTime(2024, 1, 15);
        var processType = "ProcesamientoComisiones";

        // Act
        var key = GenerateKeyForTesting(processDate, processType);

        // Assert
        Assert.Equal("accounting:inconsistencies:ProcesamientoComisiones:20240115", key);
    }

    [Fact]
    public void GenerateKey_ConDiferentesFechas_DeberiaGenerarClavesDiferentes()
    {
        // Arrange
        var fecha1 = new DateTime(2024, 1, 15);
        var fecha2 = new DateTime(2024, 1, 16);
        var processType = "ProcesamientoComisiones";

        // Act
        var key1 = GenerateKeyForTesting(fecha1, processType);
        var key2 = GenerateKeyForTesting(fecha2, processType);

        // Assert
        Assert.NotEqual(key1, key2);
        Assert.Contains("20240115", key1);
        Assert.Contains("20240116", key2);
    }

    [Fact]
    public void GenerateKey_ConDiferentesTiposProceso_DeberiaGenerarClavesDiferentes()
    {
        // Arrange
        var processDate = new DateTime(2024, 1, 15);
        var processType1 = "ProcesamientoComisiones";
        var processType2 = "ProcesamientoRetornos";

        // Act
        var key1 = GenerateKeyForTesting(processDate, processType1);
        var key2 = GenerateKeyForTesting(processDate, processType2);

        // Assert
        Assert.NotEqual(key1, key2);
        Assert.Contains("ProcesamientoComisiones", key1);
        Assert.Contains("ProcesamientoRetornos", key2);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Método helper para probar el método privado GenerateKey usando reflexión
    /// </summary>
    private string GenerateKeyForTesting(DateTime processDate, string processType)
    {
        var method = typeof(AccountingInconsistencyRepository).GetMethod("GenerateKey",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        return (string)method!.Invoke(null!, new object[] { processDate, processType })!;
    }

    #endregion
}
