using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Infrastructure.Auditing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Common.SharedKernel.Infrastructure.ActiveProcessService
{
    public class ActiveProcess(
        IDistributedCache distributedCache,
        ILogger<AuditLogStore> logger) : IActiveProcess
    {
        private const string ProcessActive = "ProcessActive";
        private const int DefaultExpirationHours = 24;

        public async Task SaveProcessActiveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var serializedReference = JsonSerializer.Serialize(ProcessActive);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(DefaultExpirationHours)
                };

                await distributedCache.SetStringAsync(ProcessActive, serializedReference, options, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al guardar el proceso activo.");
                throw;
            }
        }

        public async Task<bool> GetProcessActiveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await distributedCache.GetStringAsync(ProcessActive, cancellationToken);
                return response != null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el proceso activo.");
                return false;
            }
        }

        public async Task RemoveProcessActiveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await distributedCache.RemoveAsync(ProcessActive, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar el proceso activo.");
                throw;
            }
        }
    }
}
