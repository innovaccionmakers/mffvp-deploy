using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.People.UpdatePerson;

namespace People.Presentation.People
{
    internal sealed class UpdatePerson : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("persons/{id:long}", async (long id, Request request, ISender sender) =>
            {
                var command = new UpdatePersonCommand(
                    id,
                    request.NewDocumentType, 
                    request.NewStandardCode, 
                    request.NewIdentification, 
                    request.NewFirstName, 
                    request.NewMiddleName, 
                    request.NewLastName, 
                    request.NewSecondLastName, 
                    request.NewIssueDate, 
                    request.NewIssueCityId, 
                    request.NewBirthDate, 
                    request.NewBirthCityId, 
                    request.NewMobile, 
                    request.NewFullName, 
                    request.NewMaritalStatusId, 
                    request.NewGenderId, 
                    request.NewCountryId, 
                    request.NewEmail, 
                    request.NewEconomicActivityId
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Persons);
        }

        internal sealed class Request
        {
            public string NewDocumentType { get; set; }
            public string NewStandardCode { get; set; }
            public string NewIdentification { get; set; }
            public string NewFirstName { get; set; }
            public string NewMiddleName { get; set; }
            public string NewLastName { get; set; }
            public string NewSecondLastName { get; set; }
            public DateTime NewIssueDate { get; set; }
            public int NewIssueCityId { get; set; }
            public DateTime NewBirthDate { get; set; }
            public int NewBirthCityId { get; set; }
            public string NewMobile { get; set; }
            public string NewFullName { get; set; }
            public int NewMaritalStatusId { get; set; }
            public int NewGenderId { get; set; }
            public int NewCountryId { get; set; }
            public string NewEmail { get; set; }
            public string NewEconomicActivityId { get; set; }
        }
    }
}