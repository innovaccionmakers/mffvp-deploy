using Accounting.Domain.ConfigurationGenerals;
using Accounting.Integrations.ConfigurationGenerals.GetConfigurationGenerals;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.ConfigurationGenerals.GetConfigurationsGeneral
{
    internal class GetConfigurationGeneralsQueryHandler(
        IGeneralConfigurationRepository generalConfigurationRepository,
        ILogger<GetConfigurationGeneralsQueryHandler> logger) : IQueryHandler<GetConfigurationGeneralsQuery, IReadOnlyCollection<GetConfigurationGeneralsResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetConfigurationGeneralsResponse>>> Handle(GetConfigurationGeneralsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var configurationGenerals = await generalConfigurationRepository.GetConfigurationGeneralsAsync(cancellationToken);

                if (configurationGenerals is null)
                    return Result.Failure<IReadOnlyCollection<GetConfigurationGeneralsResponse>>(Error.NotFound("0", "No se encontraron registros de la configuración general."));

                var result = configurationGenerals.Select(cg => new GetConfigurationGeneralsResponse(
                        cg.Id,
                        cg.AccountingCode,
                        cg.CostCenter)).ToList();

                return Result.Success<IReadOnlyCollection<GetConfigurationGeneralsResponse>>(result);

            }
            catch (Exception ex)
            {
                logger.LogError("Error al consultar la configuración general. Error: {Message}", ex.Message);
                return Result.Failure<IReadOnlyCollection<GetConfigurationGeneralsResponse>>(Error.NotFound("0", "No se pudo obtener la configuración general."));
            }
        }
    }
}
