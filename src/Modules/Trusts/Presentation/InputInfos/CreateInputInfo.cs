using System.Text.Json;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.InputInfos.CreateInputInfo;

namespace Trusts.Presentation.InputInfos;

internal sealed class CreateInputInfo : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("inputinfos", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateInputInfoCommand(
                    request.CustomerDealId,
                    request.OriginId,
                    request.CollectionMethodId,
                    request.PaymentFormId,
                    request.CollectionAccount,
                    request.PaymentFormDetail,
                    request.CertificationStatusId,
                    request.TaxConditionId,
                    request.ContingentWithholding,
                    request.VerifiableMedium,
                    request.CollectionBank,
                    request.DepositDate,
                    request.SalesUser,
                    request.City
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.InputInfos);
    }

    internal sealed class Request
    {
        public Guid CustomerDealId { get; init; }
        public int OriginId { get; init; }
        public int CollectionMethodId { get; init; }
        public int PaymentFormId { get; init; }
        public int CollectionAccount { get; init; }
        public JsonDocument PaymentFormDetail { get; init; }
        public int CertificationStatusId { get; init; }
        public int TaxConditionId { get; init; }
        public int ContingentWithholding { get; init; }
        public JsonDocument VerifiableMedium { get; init; }
        public string CollectionBank { get; init; }
        public DateTime DepositDate { get; init; }
        public string SalesUser { get; init; }
        public string City { get; init; }
    }
}