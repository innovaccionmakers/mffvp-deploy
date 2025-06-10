using Associate.Domain.ConfigurationParameters;
using Associate.Application.Abstractions.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ConfigurationParameters;

public class ErrorCatalog : IErrorCatalog
{
    private readonly IConfigurationParameterRepository _repo;
    private readonly ILogger<ErrorCatalog> _log;

    public ErrorCatalog(IConfigurationParameterRepository repo, ILogger<ErrorCatalog> log)
    {
        _repo = repo;
        _log = log;
    }

    public async Task<(string Code, string Message)> GetAsync(
        Guid ruleUuid,
        CancellationToken cancellationToken = default
    )
    {
        var param = await _repo.GetByUuidAsync(ruleUuid, cancellationToken);

        if (param is null)
        {
            _log.LogWarning(
                "No se encontró parámetro de error para Uuid '{Uuid}'",
                ruleUuid
            );
            return (ruleUuid.ToString(), $"Error desconocido ({ruleUuid})");
        }

        return (param.Uuid.ToString(), param.Name);
    }
}