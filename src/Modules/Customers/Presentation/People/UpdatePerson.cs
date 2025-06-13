using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Customers.Integrations.People.UpdatePerson;
using Common.SharedKernel.Domain;

namespace Customers.Presentation.People
{
    internal sealed class UpdatePerson : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("persons/{id:int}", async (int id, Request request, ISender sender) =>
            {
                var command = new UpdatePersonCommand(
                    id,
                    request.NewIdentificationType, 
                    request.NewHomologatedCode, 
                    request.NewIdentification, 
                    request.NewFirstName, 
                    request.NewMiddleName, 
                    request.NewLastName, 
                    request.NewSecondLastName, 
                    request.NewMobile, 
                    request.NewFullName, 
                    request.NewGenderId, 
                    request.NewCountryOfResidenceId, 
                    request.NewDepartmentId, 
                    request.NewMunicipalityId, 
                    request.NewEmail, 
                    request.NewEconomicActivityId, 
                    request.NewStatus, 
                    request.NewAddress, 
                    request.NewIsDeclarant, 
                    request.NewInvestorTypeId, 
                    request.NewRiskProfileId
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Persons);
        }

        internal sealed class Request
        {
            public Guid NewIdentificationType { get; set; }
            public string NewHomologatedCode { get; set; }
            public string NewIdentification { get; set; }
            public string NewFirstName { get; set; }
            public string NewMiddleName { get; set; }
            public string NewLastName { get; set; }
            public string NewSecondLastName { get; set; }
            public string NewMobile { get; set; }
            public string NewFullName { get; set; }
            public int NewGenderId { get; set; }
            public int NewCountryOfResidenceId { get; set; }
            public int NewDepartmentId { get; set; }
            public int NewMunicipalityId { get; set; }
            public string NewEmail { get; set; }
            public int NewEconomicActivityId { get; set; }
            public Status NewStatus { get; set; }
            public string NewAddress { get; set; }
            public bool NewIsDeclarant { get; set; }
            public int NewInvestorTypeId { get; set; }
            public int NewRiskProfileId { get; set; }
        }
    }
}