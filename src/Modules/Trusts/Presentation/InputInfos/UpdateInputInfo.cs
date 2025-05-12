using System.Text.Json;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.InputInfos.UpdateInputInfo;

namespace Trusts.Presentation.InputInfos;

internal sealed class UpdateInputInfo : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("inputinfos/{id:guid}", async (Guid id, Request request, ISender sender) =>
            {
                var command = new UpdateInputInfoCommand(
                    id,
                    request.NewCustomerDealId,
                    request.NewOriginId,
                    request.NewCollectionMethodId,
                    request.NewPaymentFormId,
                    request.NewCollectionAccount,
                    request.NewPaymentFormDetail,
                    request.NewCertificationStatusId,
                    request.NewTaxConditionId,
                    request.NewContingentWithholding,
                    request.NewVerifiableMedium,
                    request.NewCollectionBank,
                    request.NewDepositDate,
                    request.NewSalesUser,
                    request.NewCity
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.InputInfos);
    }

    internal sealed class Request
    {
        public Guid NewCustomerDealId { get; set; }
        public int NewOriginId { get; set; }
        public int NewCollectionMethodId { get; set; }
        public int NewPaymentFormId { get; set; }
        public int NewCollectionAccount { get; set; }
        public JsonDocument NewPaymentFormDetail { get; set; }
        public int NewCertificationStatusId { get; set; }
        public int NewTaxConditionId { get; set; }
        public int NewContingentWithholding { get; set; }
        public JsonDocument NewVerifiableMedium { get; set; }
        public string NewCollectionBank { get; set; }
        public DateTime NewDepositDate { get; set; }
        public string NewSalesUser { get; set; }
        public string NewCity { get; set; }
    }
}