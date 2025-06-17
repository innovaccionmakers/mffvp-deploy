
using Common.SharedKernel.Application.Attributes;
using Customers.Domain.ConfigurationParameters;
using Customers.Domain.People;
using Customers.Integrations.People.GetPerson;
using Integrations.People.GetPerson;

namespace Application.People.GetPerson
{
    public class GetPersonQueryHandlerValidation(
        IConfigurationParameterRepository configurationParameterRepository,
        IPersonRepository personRepository)
    {
        public async Task<GetPersonValidationContext> ValidateAsync(
            GetPersonForIdentificationQuery request, 
            CancellationToken cancellationToken)
        {
            var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
                request.DocumentType,
                HomologScope.Of<GetPersonForIdentificationQuery>(c => c.DocumentType),
                cancellationToken);

            var docTypeUuid = configurationParameter?.Uuid ?? Guid.Empty;
            var existingPerson = await personRepository.GetForIdentificationAsync(
                docTypeUuid, request.Identification, cancellationToken);

            return new GetPersonValidationContext(existingPerson)
            {
                DocumentTypeExists = configurationParameter != null
            };
        }
    }
}