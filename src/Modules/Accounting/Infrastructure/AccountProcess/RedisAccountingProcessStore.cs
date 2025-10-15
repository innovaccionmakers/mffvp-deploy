using Accounting.Application.AccountProcess;
using Accounting.Domain.Constants;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Accounting.Infrastructure.AccountProcess;

internal sealed class RedisAccountingProcessStore(
    IDistributedCache distributedCache,
    ILogger<RedisAccountingProcessStore> logger) : IAccountingProcessStore
{
    private const string ProcessKeyPrefix = "accounting:process";
    private const int DefaultExpirationMinutes = 60;

    public async Task RegisterProcessResultAsync(Guid processId, string processType, bool isSuccess, string? errorMessage, CancellationToken cancellationToken)
    {
        try
        {
            var key = GenerateProcessKey(processId);
            var processResult = new ProcessResult(processType, isSuccess, errorMessage);
            
            var existingData = await distributedCache.GetStringAsync(key, cancellationToken);
            var processData = existingData != null
                ? JsonSerializer.Deserialize<ProcessData>(existingData)
                : new ProcessData(processId, []);

            if (processData == null)
            {
                logger.LogError("Error al deserializar datos del proceso {ProcessId}", processId);
                return;
            }

            var existingResult = processData.Results.FirstOrDefault(r => r.ProcessType == processType);
            if (existingResult != null)
            {
                processData.Results.Remove(existingResult);
            }
            processData.Results.Add(processResult);
           
            var serializedData = JsonSerializer.Serialize(processData);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(DefaultExpirationMinutes)
            };

            await distributedCache.SetStringAsync(key, serializedData, options, cancellationToken);

            logger.LogInformation("Registrado resultado para proceso {ProcessId}, tipo {ProcessType}, éxito: {IsSuccess}",
                processId, processType, isSuccess);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al registrar resultado del proceso {ProcessId} tipo {ProcessType}",
                processId, processType);
        }
    }

    public async Task<bool> AreAllProcessesCompletedAsync(Guid processId, CancellationToken cancellationToken)
    {
        try
        {
            var key = GenerateProcessKey(processId);
            var existingData = await distributedCache.GetStringAsync(key, cancellationToken);

            if (existingData == null)
            {
                logger.LogWarning("No se encontraron datos para el proceso {ProcessId}", processId);
                return false;
            }

            var processData = JsonSerializer.Deserialize<ProcessData>(existingData);
            if (processData == null)
            {
                logger.LogError("Error al deserializar datos del proceso {ProcessId}", processId);
                return false;
            }
            
            var requiredProcessTypes = new[]
            {
                ProcessTypes.AccountingFees,
                ProcessTypes.AccountingReturns,
                ProcessTypes.AccountingOperations,
                ProcessTypes.AccountingConcepts
            };

            var allCompleted = requiredProcessTypes.All(type =>
                processData.Results.Any(r => r.ProcessType == type));

            logger.LogInformation("Verificación de completitud para proceso {ProcessId}: {AllCompleted}",
                processId, allCompleted);

            return allCompleted;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al verificar completitud del proceso {ProcessId}", processId);
            return false;
        }
    }

    public async Task<List<ProcessResult>> GetAllProcessResultsAsync(Guid processId, CancellationToken cancellationToken)
    {
        try
        {
            var key = GenerateProcessKey(processId);
            var existingData = await distributedCache.GetStringAsync(key, cancellationToken);

            if (existingData == null)
            {
                logger.LogWarning("No se encontraron datos para el proceso {ProcessId}", processId);
                return [];
            }

            var processData = JsonSerializer.Deserialize<ProcessData>(existingData);
            if (processData == null)
            {
                logger.LogError("Error al deserializar datos del proceso {ProcessId}", processId);
                return [];
            }

            logger.LogInformation("Obtenidos {Count} resultados para el proceso {ProcessId}",
                processData.Results.Count, processId);

            return processData.Results.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener resultados del proceso {ProcessId}", processId);
            return [];
        }
    }

    public async Task CleanupAsync(Guid processId, CancellationToken cancellationToken)
    {
        try
        {
            var key = GenerateProcessKey(processId);
            await distributedCache.RemoveAsync(key, cancellationToken);

            logger.LogInformation("Limpieza completada para el proceso {ProcessId}", processId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al limpiar datos del proceso {ProcessId}", processId);
        }
    }

    private static string GenerateProcessKey(Guid processId)
    {
        return $"{ProcessKeyPrefix}:{processId:N}";
    }

    private record ProcessData(Guid ProcessId, List<ProcessResult> Results);
}
