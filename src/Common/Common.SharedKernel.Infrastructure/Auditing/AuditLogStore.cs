using Common.SharedKernel.Domain.Auditing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Security.Domain.Logs;
using System.Text.Json;

namespace Common.SharedKernel.Infrastructure.Auditing
{
    public sealed class AuditLogStore(
        IDistributedCache distributedCache,
        ILogRepository _logRepository,
        ILogger<AuditLogStore> logger) : IAuditLogStore
    {
        private const string LogReferenceKeyPrefix = "audit:log:reference";
        private const int DefaultExpirationHours = 24;

        public async Task SaveLogReferenceAsync(long id, string processId, CancellationToken cancellationToken = default)
        {
            try
            {
                var logReference = new LogReference(id, processId);
                var key = GenerateKey(processId);
                var serializedReference = JsonSerializer.Serialize(logReference);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(DefaultExpirationHours)
                };

                await distributedCache.SetStringAsync(key, serializedReference, options, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al guardar referencia de log para el proceso {ProcessId}", processId);
                throw;
            }
        }

        public async Task UpdateLogStatusAsync(string processId, CancellationToken cancellationToken)
        {
            try
            {
                var logReference = await GetLogReferenceAsync(processId);

                if (logReference is not null)
                {
                    var logResult = Log.UpdateSuccessStatus(logReference.Id, false);
                    await _logRepository.Update(logResult.Value);

                    await RemoveLogReferenceAsync(processId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar estado de log para CorrelationId: {CorrelationId}", processId);
                throw;
            }
        }

        public async Task<LogReference?> GetLogReferenceAsync(string processId, CancellationToken cancellationToken = default)
        {
            try
            {
                var key = GenerateKey(processId);
                var serializedReference = await distributedCache.GetStringAsync(key, cancellationToken);

                if (string.IsNullOrEmpty(serializedReference))
                {
                    logger.LogWarning("No se encontró referencia de log para el proceso {ProcessId}", processId);
                    return null;
                }

                var logReference = JsonSerializer.Deserialize<LogReference>(serializedReference);
                return logReference;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener referencia de log para el proceso {ProcessId}", processId);
                return null;
            }
        }

        public async Task RemoveLogReferenceAsync(string processId, CancellationToken cancellationToken = default)
        {
            try
            {
                var key = GenerateKey(processId);
                await distributedCache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar referencia de log para el proceso {ProcessId}", processId);
                throw;
            }
        }

        private static string GenerateKey(string processId)
        {
            return $"{LogReferenceKeyPrefix}:{processId}";
        }
    }
}