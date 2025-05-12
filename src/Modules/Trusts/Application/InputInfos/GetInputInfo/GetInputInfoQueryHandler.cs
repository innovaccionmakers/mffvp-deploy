using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.InputInfos;
using Trusts.Integrations.InputInfos;
using Trusts.Integrations.InputInfos.GetInputInfo;

namespace Trusts.Application.InputInfos.GetInputInfo;

internal sealed class GetInputInfoQueryHandler(
    IInputInfoRepository inputinfoRepository)
    : IQueryHandler<GetInputInfoQuery, InputInfoResponse>
{
    public async Task<Result<InputInfoResponse>> Handle(GetInputInfoQuery request, CancellationToken cancellationToken)
    {
        var inputinfo = await inputinfoRepository.GetAsync(request.InputInfoId, cancellationToken);
        if (inputinfo is null) return Result.Failure<InputInfoResponse>(InputInfoErrors.NotFound(request.InputInfoId));
        var response = new InputInfoResponse(
            inputinfo.InputInfoId,
            inputinfo.CustomerDealId,
            inputinfo.OriginId,
            inputinfo.CollectionMethodId,
            inputinfo.PaymentFormId,
            inputinfo.CollectionAccount,
            inputinfo.PaymentFormDetail,
            inputinfo.CertificationStatusId,
            inputinfo.TaxConditionId,
            inputinfo.ContingentWithholding,
            inputinfo.VerifiableMedium,
            inputinfo.CollectionBank,
            inputinfo.DepositDate,
            inputinfo.SalesUser,
            inputinfo.City
        );
        return response;
    }
}