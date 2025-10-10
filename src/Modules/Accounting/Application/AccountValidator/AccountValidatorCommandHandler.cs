using Accounting.Application.Abstractions;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Integrations.AccountValidator;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountValidator;

internal sealed class AccountValidatorCommandHandler(
    IAccountingInconsistencyRepository inconsistencyRepository,
    ILogger<AccountValidatorCommandHandler> logger) : ICommandHandler<AccountValidatorCommand, Unit>
{
    public async Task<Result<Unit>> Handle(AccountValidatorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Iniciando validación de servicios contables");

            // Los tipos de servicios contables que necesitamos validar
            var serviceTypes = new[]
            {
                ProcessTypes.AccountingFees,
                ProcessTypes.AccountingReturns,
                ProcessTypes.AccountingOperations,
                ProcessTypes.AccountingConcepts
            };

            var allServicesSuccessful = true;
            var servicesWithInconsistencies = new List<string>();

            // Consultar el estado de los servicios contables
            foreach (var serviceType in serviceTypes)
            {
                logger.LogInformation("Validando servicio: {ServiceType}", serviceType);

                // Consultar en Redis si hay inconsistencias para este servicio
                var inconsistenciesResult = await inconsistencyRepository.GetInconsistenciesAsync(
                    request.ProcessDate,
                    serviceType,
                    cancellationToken);

                if (inconsistenciesResult.IsFailure)
                {
                    logger.LogError("Error al consultar inconsistencias para {ServiceType}: {Error}",
                        serviceType, inconsistenciesResult.Error);
                    allServicesSuccessful = false;
                    continue;
                }

                var inconsistencies = inconsistenciesResult.Value.ToList();

                if (inconsistencies.Any())
                {
                    logger.LogWarning("Se encontraron {Count} inconsistencias para el servicio {ServiceType}",
                        inconsistencies.Count, serviceType);

                    allServicesSuccessful = false;
                    servicesWithInconsistencies.Add(serviceType);
                }
                else
                {
                    logger.LogInformation("Servicio {ServiceType} procesado exitosamente sin inconsistencias", serviceType);
                }
            }

            // Determinar el resultado y la acción a tomar
            if (allServicesSuccessful)
            {
                logger.LogInformation("Todos los servicios contables funcionaron correctamente");

                // TODO: Publicar mensaje en el centro de notificaciones indicando que todos los servicios funcionaron correctamente
                // await notificationCenter.PublishSuccessMessageAsync(request.ProcessDate, cancellationToken);
            }
            else
            {
                logger.LogWarning("Algunos servicios contables fallaron o tienen inconsistencias");

                if (servicesWithInconsistencies.Any())
                {
                    logger.LogWarning("Servicios con inconsistencias: {Services}",
                        string.Join(", ", servicesWithInconsistencies));

                    // TODO: Publicar en el centro de notificaciones que hay inconsistencias con un Excel
                    // await notificationCenter.PublishInconsistenciesMessageAsync(request.ProcessDate, servicesWithInconsistencies, cancellationToken);
                }
                else
                {
                    logger.LogInformation("No hay inconsistencias registradas, pero algunos servicios fallaron");

                    // TODO: Publicar mensaje de fallo general en el centro de notificaciones
                    // await notificationCenter.PublishFailureMessageAsync(request.ProcessDate, cancellationToken);
                }
            }

            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado durante la validación de servicios contables");
            return Result.Failure<Unit>(Error.Failure("Exception", "Ocurrió un error inesperado durante la validación."));
        }
    }
}
