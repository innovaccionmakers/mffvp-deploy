using Common.SharedKernel.Application.Rpc;
using Customers.Integrations.People.GetPerson;
using MediatR;

namespace Customers.IntegrationEvents.PersonInformation;

public sealed class PersonInformationConsumer : IRpcHandler<GetPersonInformationRequest, GetPersonInformationResponse>
{
    private readonly ISender _mediator;

    public PersonInformationConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<GetPersonInformationResponse> HandleAsync(
        GetPersonInformationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetPersonForIdentificationQuery(request.DocumentType, request.Identification),
            cancellationToken);

        if (result.IsFailure)
        {
            var error = result.Error;
            return new GetPersonInformationResponse(false, error.Code, error.Description, null);
        }

        var person = result.Value;
        var personInformation = new PersonInformation(
            person.PersonId,
            person.DocumentType,
            person.HomologatedCode,
            person.Identification,
            person.FirstName,
            person.MiddleName,
            person.LastName,
            person.SecondLastName,
            person.BirthDate,
            person.Mobile,
            person.FullName,
            person.GenderId,
            person.CountryOfResidenceId,
            person.DepartmentId,
            person.MunicipalityId,
            person.Email,
            person.EconomicActivityId,
            person.Status,
            person.Address,
            person.IsDeclarant,
            person.InvestorTypeId,
            person.RiskProfileId);

        return new GetPersonInformationResponse(true, null, null, personInformation);
    }
}