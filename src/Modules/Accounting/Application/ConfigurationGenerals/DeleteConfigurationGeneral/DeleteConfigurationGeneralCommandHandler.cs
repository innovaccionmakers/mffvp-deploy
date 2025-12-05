using Accounting.Application.Abstractions.Data;
using Accounting.Application.ConfigurationGenerals.CreateConfigurationGeneral;
using Accounting.Domain.ConfigurationGenerals;
using Accounting.Integrations.ConfigurationGenerals.DeleteConfigurationGeneral;
using Accounting.Integrations.ConfigurationGenerals.GetConfigurationGeneral;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.ConfigurationGenerals.DeleteConfigurationGeneral
{
    internal class DeleteConfigurationGeneralCommandHandler(
        IGeneralConfigurationRepository generalConfigurationRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateConfigurationGeneralCommandHandler> logger
        ) : ICommandHandler<DeleteConfigurationGeneralCommand>
    {
        public async Task<Result> Handle(DeleteConfigurationGeneralCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var configurationGeneral = await generalConfigurationRepository.GetGeneralConfigurationByPortfolioIdAsync(request.PortfolioId, cancellationToken);

                if (configurationGeneral is null)
                    return Result.Failure<IReadOnlyCollection<GetConfigurationGeneralResponse>>(Error.NotFound("0", "No se pudo obtener la configuración general."));

                await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

                generalConfigurationRepository.Delete(configurationGeneral);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al eliminar la configuración general. Error: {Message}", ex.Message);
                return Result.Failure(Error.NotFound("0", "No se pudo eliminar la configuración general."));
            }
        }
    }
}
