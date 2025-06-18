using Associate.Application.Activates.CreateActivate;
using Associate.Application.Activates.UpdateActivate;
using Associate.Domain.Activates;
using Associate.Domain.ConfigurationParameters;
using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.Activates.UpdateActivate;
using Common.SharedKernel.Application.Attributes;

namespace Application.Activates
{
    public class ActivatesCommandHandlerValidation(
        IConfigurationParameterRepository configurationParameterRepository,
        IActivateRepository activateRepository)
    {
        public async Task<UpdateActivateValidationContext> UpdateActivateValidationContext(
            UpdateActivateCommand request,
            CancellationToken cancellationToken)
        {
            var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
                request.DocumentType,
                HomologScope.Of<UpdateActivateCommand>(c => c.DocumentType),
                cancellationToken);

            var docTypeUuid = configurationParameter?.Uuid ?? Guid.Empty;

            var existingActivate = await activateRepository.GetByIdTypeAndNumber(docTypeUuid, request.Identification, cancellationToken);

            return new UpdateActivateValidationContext(request, existingActivate!)
            {
                DocumentTypeExists = configurationParameter != null
            };
        }

        public async Task<CreateActivateValidationContext> CreateActivateValidateRequestAsync(
            CreateActivateCommand request,
            CancellationToken cancellationToken)
        {
            var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
                request.DocumentType,
                HomologScope.Of<CreateActivateCommand>(c => c.DocumentType),
                cancellationToken);

            var docTypeUuid = configurationParameter?.Uuid ?? Guid.Empty;

            var existingActivate = await activateRepository.GetByIdTypeAndNumber(docTypeUuid, request.Identification, cancellationToken);

            return new CreateActivateValidationContext(request, existingActivate!, docTypeUuid)
            {
                DocumentTypeExists = configurationParameter != null
            };
        }
    }
}