using Application.People.CreatePerson;
using Common.SharedKernel.Application.Attributes;
using Customers.Domain.ConfigurationParameters;
using Customers.Domain.People;
using Customers.Integrations.Countries.GetCountry;
using Customers.Integrations.Departments.GetDepartment;
using Customers.Integrations.EconomicActivities.GetEconomicActivity;
using Customers.Integrations.Municipalities.GetMunicipality;
using Integrations.People.CreatePerson;
using MediatR;

namespace Application.People
{
    public sealed class PersonCommandHandlerValidation(
       IConfigurationParameterRepository configurationParameterRepository,
       IPersonRepository personRepository,
       ISender sender)
    {
        public async Task<CreatePersonValidationContext> ValidateRequestAsync(CreatePersonRequestCommand request, CancellationToken cancellationToken)
        {
            var docTypeParam = await configurationParameterRepository.GetByCodeAndScopeAsync(
                request.DocumentType,
                HomologScope.Of<CreatePersonRequestCommand>(c => c.DocumentType),
                cancellationToken);

            var docTypeUuid = docTypeParam?.Uuid ?? Guid.Empty;
            var existingPerson = await personRepository.GetForIdentificationAsync(
                docTypeUuid, request.Identification, cancellationToken);

            var existingHomologatedCode = await personRepository.GetExistingHomologatedCode(request.HomologatedCode!, cancellationToken);

            var context = new CreatePersonValidationContext(request, existingPerson, docTypeUuid)
            {
                DocumentTypeHomologated = docTypeParam != null,
                ExistingHomologatedCode = existingHomologatedCode
            };

            await ValidateCountries(context, cancellationToken);
            await ValidateDepartments(context, cancellationToken);
            await ValidateMunicipalities(context, cancellationToken);
            await ValidateEconomicActivity(context, cancellationToken);
            await ValidateConfigurationParameter(context, cancellationToken);

            return context;
        }

        private async Task ValidateCountries(CreatePersonValidationContext context, CancellationToken cancellationToken)
        {
            var response = await sender.Send(new GetCountryQuery(context.Request.CountryOfResidence), cancellationToken);
            if (response != null)
            {
                context.CountryId = response.IsSuccess ? response.Value.CountryId : null;
                context.CountryHomologated = true;
            }
        }

        internal async Task ValidateDepartments(CreatePersonValidationContext context, CancellationToken cancellationToken)
        {
            var response = await sender.Send(new GetDepartmentQuery(context.Request.Department), cancellationToken);
            if (response != null)
            {
                context.DepartmentId = response.IsSuccess ? response.Value.DepartmentId : null;
                context.DepartmentHomologated = true;
            }
        }

        internal async Task ValidateEconomicActivity(CreatePersonValidationContext context, CancellationToken cancellationToken)
        {
            var response = await sender.Send(new GetEconomicActivityQuery(context.Request.EconomicActivity), cancellationToken);
            if (response != null)
            {
                context.EconomicActivityId = response.IsSuccess ? response.Value.EconomicActivityId : null;
                context.EconomicActivityHomologated = true;
            }
        }

        internal async Task ValidateMunicipalities(CreatePersonValidationContext context, CancellationToken cancellationToken)
        {
            var response = await sender.Send(new GetMunicipalityQuery(context.Request.Municipality), cancellationToken);
            if (response != null)
            {
                context.MunicipalityId = response.IsSuccess ? response.Value.MunicipalityId : null;
                context.MunicipalityHomologated = true;
            }
        }

        internal async Task ValidateConfigurationParameter(CreatePersonValidationContext context, CancellationToken cancellationToken)
        {
            var scopesAndValues = HomologScope.GetScopesAndValues(context.Request);

            var queryParams = scopesAndValues
                .Where(x => x.Value.Value != null && !string.IsNullOrEmpty(x.Value.Value.ToString()))
                .Select(x => (code: x.Value.Value!.ToString()!, Scope: x.Value.Scope)).Distinct().ToList();

            var parameters = await configurationParameterRepository.GetByCodesAndScopesAsync(
                queryParams, cancellationToken);

            foreach (var item in scopesAndValues)
            {
                var (scope, value) = item.Value;

                if (value == null) continue;

                var code = value.ToString()!;
                var parameter = parameters.FirstOrDefault(p =>
                    p.HomologationCode == code &&
                    p.Type == scope);

                if (parameter != null)
                {
                    switch (item.Key)
                    {
                        case nameof(context.Request.DocumentType):
                            context.DocumentTypeHomologated = true;
                            context.DocumentTypeId = parameter.ConfigurationParameterId;
                            break;

                        case nameof(context.Request.Gender):
                            context.GenderHomologated = true;
                            context.GenderId = parameter.ConfigurationParameterId;
                            break;

                        case nameof(context.Request.InvestorType):
                            context.InvestorTypeHomologated = true;
                            context.InvestorTypeId = parameter.ConfigurationParameterId;
                            break;

                        case nameof(context.Request.RiskProfile):
                            context.RiskProfileHomologated = true;
                            context.RiskProfileId = parameter.ConfigurationParameterId;
                            break;
                    }
                }
            }
        }
    }
}