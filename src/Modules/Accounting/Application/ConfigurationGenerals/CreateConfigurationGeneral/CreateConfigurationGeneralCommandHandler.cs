using Accounting.Application.Abstractions.Data;
using Accounting.Domain.ConfigurationGenerals;
using Accounting.Integrations.ConfigurationGenerals.CreateConfigurationGeneral;
using Accounting.Integrations.ConfigurationGenerals.GetConfigurationGeneral;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.ConfigurationGenerals.CreateConfigurationGeneral
{
    internal class CreateConfigurationGeneralCommandHandler(
        IGeneralConfigurationRepository generalConfigurationRepository,
		IUnitOfWork unitOfWork,
		ILogger<CreateConfigurationGeneralCommandHandler> logger
        ) : ICommandHandler<CreateConfigurationGeneralCommand>
    {
        public async Task<Result> Handle(CreateConfigurationGeneralCommand request, CancellationToken cancellationToken)
		{
			try
            {
                var configurationGeneral = await generalConfigurationRepository.GetGeneralConfigurationByPortfolioIdAsync(request.PortfolioId, cancellationToken);

                if (configurationGeneral != null)
                    return Result.Failure<IReadOnlyCollection<GetConfigurationGeneralResponse>>(Error.NotFound("0", "Ya se encuentra creada una configuración general con ese portafolio."));

                var result = GeneralConfiguration.Create(
					request.PortfolioId,
					request.AccountingCode,
					request.CostCenter
					);

				generalConfigurationRepository.Insert(result.Value);
				await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
			}
			catch (Exception ex)
			{
				logger.LogError("Error al crear la configuración general. Error: {Message}", ex.Message);
				return Result.Failure(Error.NotFound("0", "No se pudo crear la configuración general."));
			}
        }
    }
}
