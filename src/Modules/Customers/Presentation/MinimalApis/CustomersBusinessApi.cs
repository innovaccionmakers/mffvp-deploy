using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;

using Customers.Integrations.People;
using Customers.Integrations.People.GetPersons;

using Integrations.People.CreatePerson;
using Customers.Domain.Routes;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Customers.Presentation.MinimalApis;

public static class CustomersBusinessApi
{
    public static void MapCustomersBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(Routes.Customer)
            .WithTags(TagName.TagCustomer)
            .WithOpenApi();

        group.MapGet(
                NameEndpoints.GetCustomer,
                async (ISender sender) =>
                {
                    var result = await sender.Send(new GetPersonsQuery());
                    return result.Value;
                }
            )
        .WithName(NameEndpoints.GetCustomer)
        .WithSummary(Summary.GetCustomer)
        .WithDescription(Description.GetCustomer)
        .Produces<IReadOnlyCollection<PersonResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost(
                NameEndpoints.PostCustomer,
                async ([FromBody] CreatePersonRequestCommand request, ISender sender) =>
                {
                    var result = await sender.Send(request);
                    return result.ToApiResult(result.Description);
                }
            )
            .WithName(NameEndpoints.PostCustomer)
            .WithSummary(Summary.PostCustomer)
            .WithDescription(Description.PostCustomer)
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Description = RequestBodyDescription.PostCustomer;
                return operation;
            })
            .AddEndpointFilter<TechnicalValidationFilter<CreatePersonRequestCommand>>()
            .Produces<PersonResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
