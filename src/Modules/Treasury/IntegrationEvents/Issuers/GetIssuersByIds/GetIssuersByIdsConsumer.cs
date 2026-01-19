using Common.SharedKernel.Application.Rpc;
using Treasury.Domain.Issuers;

namespace Treasury.IntegrationEvents.Issuers.GetIssuersByIds;

public sealed class GetIssuersByIdsConsumer(
    IIssuerRepository issuerRepository)
    : IRpcHandler<GetIssuersByIdsRequest, GetIssuersByIdsResponse>
{
    public async Task<GetIssuersByIdsResponse> HandleAsync(
        GetIssuersByIdsRequest message,
        CancellationToken cancellationToken)
    {
        try
        {
            var issuers = await issuerRepository.GetByIdsAsync(message.Ids, cancellationToken);

            var issuerResponses = issuers.Select(issuer => new IssuerResponse(
                issuer.Id,
                issuer.IssuerCode,
                issuer.Description,
                issuer.Nit,
                issuer.Digit,
                issuer.HomologatedCode,
                issuer.IsBank
            )).ToList();

            return new GetIssuersByIdsResponse(
                IsValid: true,
                Code: null,
                Message: null,
                Issuers: issuerResponses);
        }
        catch (Exception ex)
        {
            return new GetIssuersByIdsResponse(
                IsValid: false,
                Code: "Error",
                Message: ex.Message,
                Issuers: Array.Empty<IssuerResponse>());
        }
    }
}

