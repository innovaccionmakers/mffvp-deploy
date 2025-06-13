using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Customers.Integrations.People.CreatePerson;
using Common.SharedKernel.Domain;

namespace Customers.Presentation.People
{
    internal sealed class CreatePerson : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("persons", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreatePersonCommand(
                    request.DocumentType, 
                    request.HomologatedCode, 
                    request.Identification, 
                    request.FirstName, 
                    request.MiddleName, 
                    request.LastName, 
                    request.SecondLastName, 
                    request.Mobile, 
                    request.FullName, 
                    request.GenderId, 
                    request.CountryOfResidenceId, 
                    request.DepartmentId, 
                    request.MunicipalityId, 
                    request.Email, 
                    request.EconomicActivityId, 
                    request.Status, 
                    request.Address, 
                    request.IsDeclarant, 
                    request.InvestorTypeId, 
                    request.RiskProfileId
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Persons);
        }

        internal sealed class Request
        {
            public Guid DocumentType { get; init; }
            public string HomologatedCode { get; init; }
            public string Identification { get; init; }
            public string FirstName { get; init; }
            public string MiddleName { get; init; }
            public string LastName { get; init; }
            public string SecondLastName { get; init; }
            public string Mobile { get; init; }
            public string FullName { get; init; }
            public int GenderId { get; init; }
            public int CountryOfResidenceId { get; init; }
            public int DepartmentId { get; init; }
            public int MunicipalityId { get; init; }
            public string Email { get; init; }
            public int EconomicActivityId { get; init; }
            public Status Status { get; init; }
            public string Address { get; init; }
            public bool IsDeclarant { get; init; }
            public int InvestorTypeId { get; init; }
            public int RiskProfileId { get; init; }
        }
    }
}