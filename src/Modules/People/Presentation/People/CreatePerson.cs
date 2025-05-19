using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.People.CreatePerson;

namespace People.Presentation.People;

internal sealed class CreatePerson : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("persons", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreatePersonCommand(
                    request.DocumentType,
                    request.StandardCode,
                    request.Identification,
                    request.FirstName,
                    request.MiddleName,
                    request.LastName,
                    request.SecondLastName,
                    request.IssueDate,
                    request.IssueCityId,
                    request.BirthDate,
                    request.BirthCityId,
                    request.Mobile,
                    request.FullName,
                    request.MaritalStatusId,
                    request.GenderId,
                    request.CountryId,
                    request.Email,
                    request.EconomicActivityId
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Persons);
    }

    internal sealed class Request
    {
        public string DocumentType { get; init; }
        public string StandardCode { get; init; }
        public string Identification { get; init; }
        public string FirstName { get; init; }
        public string MiddleName { get; init; }
        public string LastName { get; init; }
        public string SecondLastName { get; init; }
        public DateTime IssueDate { get; init; }
        public int IssueCityId { get; init; }
        public DateTime BirthDate { get; init; }
        public int BirthCityId { get; init; }
        public string Mobile { get; init; }
        public string FullName { get; init; }
        public int MaritalStatusId { get; init; }
        public int GenderId { get; init; }
        public int CountryId { get; init; }
        public string Email { get; init; }
        public string EconomicActivityId { get; init; }
    }
}