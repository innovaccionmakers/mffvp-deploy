using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.Extensions.Logging;

namespace Common.SharedKernel.Infrastructure.ConfigurationParameters;

public sealed class ErrorCatalog<TModule> : IErrorCatalog<TModule>
{
    private readonly IConfigurationParameterLookupRepository<TModule> _repo;
    private readonly ILogger<ErrorCatalog<TModule>> _log;

    public ErrorCatalog(IConfigurationParameterLookupRepository<TModule> repo, ILogger<ErrorCatalog<TModule>> log)
    {
        _repo = repo;
        _log = log;
    }

    public async Task<(string Code, string Message)> GetAsync(Guid ruleUuid, CancellationToken cancellationToken = default)
    {
        var param = await _repo.GetByUuidAsync(ruleUuid, cancellationToken);

        if (param is null)
        {
            _log.LogWarning("No se encontró parámetro de error para Uuid '{Uuid}'", ruleUuid);
            return (ruleUuid.ToString(), $"Error desconocido ({ruleUuid})");
        }

        return (param.HomologationCode, param.Name);
    }
}
