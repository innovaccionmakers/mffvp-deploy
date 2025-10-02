using Accounting.Domain.AccountingInconsistencies;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Accounting.Infrastructure.AccountingInconsistencies;

/// <summary>
/// Implementación del repositorio de inconsistencias contables usando Redis
/// </summary>
public sealed class AccountingInconsistencyRepository(
    IDistributedCache distributedCache,
    ILogger<AccountingInconsistencyRepository> logger) : IAccountingInconsistencyRepository
{
    private const string InconsistencyKeyPrefix = "accounting:inconsistencies";
    private const int DefaultExpirationMinutes = 1440; // 24 horas

    public async Task<Result> SaveInconsistenciesAsync(
        IEnumerable<AccountingInconsistency> inconsistencies,
        DateTime processDate,
        string processType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var inconsistencyList = inconsistencies.ToList();

            if (!inconsistencyList.Any())
            {
                logger.LogWarning("No hay inconsistencias para guardar para el proceso {ProcessType} en la fecha {ProcessDate}",
                    processType, processDate);
                return Result.Success();
            }

            var key = GenerateKey(processDate, processType);
            var serializedInconsistencies = JsonSerializer.Serialize(inconsistencyList);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(DefaultExpirationMinutes)
            };

            await distributedCache.SetStringAsync(key, serializedInconsistencies, options, cancellationToken);

            logger.LogInformation("Se guardaron {Count} inconsistencias para el proceso {ProcessType} en la fecha {ProcessDate}",
                inconsistencyList.Count, processType, processDate);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al guardar inconsistencias para el proceso {ProcessType} en la fecha {ProcessDate}",
                processType, processDate);
            return Result.Failure(Error.Failure("InconsistencyRepository.SaveInconsistencies",
                $"Error al guardar inconsistencias: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<AccountingInconsistency>>> GetInconsistenciesAsync(
        DateTime processDate,
        string processType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GenerateKey(processDate, processType);
            var serializedInconsistencies = await distributedCache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(serializedInconsistencies))
            {
                logger.LogInformation("No se encontraron inconsistencias para el proceso {ProcessType} en la fecha {ProcessDate}",
                    processType, processDate);
                return Result.Success<IEnumerable<AccountingInconsistency>>(Enumerable.Empty<AccountingInconsistency>());
            }

            var inconsistencies = JsonSerializer.Deserialize<List<AccountingInconsistency>>(serializedInconsistencies);

            if (inconsistencies == null)
            {
                logger.LogWarning("Error al deserializar inconsistencias para el proceso {ProcessType} en la fecha {ProcessDate}",
                    processType, processDate);
                return Result.Failure<IEnumerable<AccountingInconsistency>>(Error.Failure("InconsistencyRepository.GetInconsistencies",
                    "Error al deserializar inconsistencias"));
            }

            logger.LogInformation("Se obtuvieron {Count} inconsistencias para el proceso {ProcessType} en la fecha {ProcessDate}",
                inconsistencies.Count, processType, processDate);

            return Result.Success<IEnumerable<AccountingInconsistency>>(inconsistencies);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener inconsistencias para el proceso {ProcessType} en la fecha {ProcessDate}",
                processType, processDate);
            return Result.Failure<IEnumerable<AccountingInconsistency>>(Error.Failure("InconsistencyRepository.GetInconsistencies",
                $"Error al obtener inconsistencias: {ex.Message}"));
        }
    }

    public async Task<Result> DeleteInconsistenciesAsync(
        DateTime processDate,
        string processType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GenerateKey(processDate, processType);
            await distributedCache.RemoveAsync(key, cancellationToken);

            logger.LogInformation("Se eliminaron las inconsistencias para el proceso {ProcessType} en la fecha {ProcessDate}",
                processType, processDate);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al eliminar inconsistencias para el proceso {ProcessType} en la fecha {ProcessDate}",
                processType, processDate);
            return Result.Failure(Error.Failure("InconsistencyRepository.DeleteInconsistencies",
                $"Error al eliminar inconsistencias: {ex.Message}"));
        }
    }

    private static string GenerateKey(DateTime processDate, string processType)
    {
        var dateString = processDate.ToString("yyyyMMdd");
        return $"{InconsistencyKeyPrefix}:{processType}:{dateString}";
    }
}
