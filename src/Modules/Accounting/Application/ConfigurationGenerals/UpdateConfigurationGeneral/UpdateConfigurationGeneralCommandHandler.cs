using Accounting.Application.Abstractions.Data;
using Accounting.Application.ConfigurationGenerals.CreateConfigurationGeneral;
using Accounting.Domain.ConfigurationGenerals;
using Accounting.Integrations.ConfigurationGenerals.GetConfigurationGeneral;
using Accounting.Integrations.ConfigurationGenerals.UpdateConfigurationGeneral;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.ConfigurationGenerals.UpdateConfigurationGeneral
{
    internal class UpdateConfigurationGeneralCommandHandler(
        IGeneralConfigurationRepository generalConfigurationRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateConfigurationGeneralCommandHandler> logger
        ) : ICommandHandler<UpdateConfigurationGeneralCommand>
    {
        public async Task<Result> Handle(UpdateConfigurationGeneralCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var configurationGeneral = await generalConfigurationRepository.GetGeneralConfigurationByPortfolioIdAsync(request.PortfolioId, cancellationToken);

                if (configurationGeneral is null)
                    return Result.Failure<IReadOnlyCollection<GetConfigurationGeneralResponse>>(Error.NotFound("0", "No se pudo obtener la configuración general."));

                configurationGeneral.UpdateDetails(
                    request.PortfolioId,
                    request.AccountingCode,
                    request.CostCenter
                    );

                generalConfigurationRepository.Update(configurationGeneral);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al actualizar la configuración general. Error: {Message}", ex.Message);
                return Result.Failure(Error.NotFound("0", "No se pudo actualizar la configuración general."));
            }
        }
    }
}
