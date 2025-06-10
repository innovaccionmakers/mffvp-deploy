using Trusts.Domain.ConfigurationParameters;
using Microsoft.Extensions.Logging;
using Trusts.Application.Abstractions.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;

namespace Trusts.Infrastructure.ConfigurationParameters;

internal sealed class ErrorCatalog : IErrorCatalog
{
    private readonly ILogger<ErrorCatalog> _log;
    private readonly IConfigurationParameterRepository _repo;

    public ErrorCatalog(
        IConfigurationParameterRepository repo,
        ILogger<ErrorCatalog> log
    )
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