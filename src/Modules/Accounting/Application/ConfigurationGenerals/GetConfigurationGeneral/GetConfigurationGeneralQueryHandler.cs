using Accounting.Application.ConfigurationGenerals.CreateConfigurationGeneral;
using Accounting.Domain.ConfigurationGenerals;
using Accounting.Integrations.ConfigurationGenerals.GetConfigurationGeneral;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.ConfigurationGenerals.GetConfigurationGeneral
{
    internal class GetConfigurationGeneralQueryHandler(
        IGeneralConfigurationRepository generalConfigurationRepository,
        ILogger<CreateConfigurationGeneralCommandHandler> logger
        ) : IQueryHandler<GetConfigurationGeneralQuery, GetConfigurationGeneralResponse>
    {
        public async Task<Result<GetConfigurationGeneralResponse>> Handle(GetConfigurationGeneralQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var configurationGeneral = await generalConfigurationRepository.GetGeneralConfigurationByPortfolioIdAsync(request.PortfolioId, cancellationToken);

                if (configurationGeneral is null)
                    return Result.Failure<GetConfigurationGeneralResponse>(Error.NotFound("0", "No se encontró registro de la configuración general."));

                var result = new GetConfigurationGeneralResponse(
                        configurationGeneral.Id,
                        configurationGeneral.AccountingCode,
                        configurationGeneral.CostCenter);

                return Result.Success(result);
                
            }
            catch (Exception ex)
            {
                logger.LogError("Error al consultar la configuración general. Error: {Message}", ex.Message);
                return Result.Failure<GetConfigurationGeneralResponse>(Error.NotFound("0", "No se pudo obtener la configuración general."));
            }
        }
    }
}
