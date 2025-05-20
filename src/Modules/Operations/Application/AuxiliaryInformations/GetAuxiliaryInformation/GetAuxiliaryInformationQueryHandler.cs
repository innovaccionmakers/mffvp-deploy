using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.AuxiliaryInformations;
using Operations.Integrations.AuxiliaryInformations.GetAuxiliaryInformation;
using Operations.Integrations.AuxiliaryInformations;

namespace Operations.Application.AuxiliaryInformations.GetAuxiliaryInformation;

internal sealed class GetAuxiliaryInformationQueryHandler(
    IAuxiliaryInformationRepository auxiliaryinformationRepository)
    : IQueryHandler<GetAuxiliaryInformationQuery, AuxiliaryInformationResponse>
{
    public async Task<Result<AuxiliaryInformationResponse>> Handle(GetAuxiliaryInformationQuery request,
        CancellationToken cancellationToken)
    {
        var auxiliaryinformation =
            await auxiliaryinformationRepository.GetAsync(request.AuxiliaryInformationId, cancellationToken);
        if (auxiliaryinformation is null)
            return Result.Failure<AuxiliaryInformationResponse>(
                AuxiliaryInformationErrors.NotFound(request.AuxiliaryInformationId));
        var response = new AuxiliaryInformationResponse(
            auxiliaryinformation.AuxiliaryInformationId,
            auxiliaryinformation.ClientOperationId,
            auxiliaryinformation.OriginId,
            auxiliaryinformation.CollectionMethodId,
            auxiliaryinformation.PaymentMethodId,
            auxiliaryinformation.CollectionAccount,
            auxiliaryinformation.PaymentMethodDetail,
            auxiliaryinformation.CertificationStatusId,
            auxiliaryinformation.TaxConditionId,
            auxiliaryinformation.ContingentWithholding,
            auxiliaryinformation.VerifiableMedium,
            auxiliaryinformation.CollectionBank,
            auxiliaryinformation.DepositDate,
            auxiliaryinformation.SalesUser,
            auxiliaryinformation.City
        );
        return response;
    }
}