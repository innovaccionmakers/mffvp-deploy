using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.InputInfos;
using Trusts.Integrations.InputInfos;
using Trusts.Integrations.InputInfos.GetInputInfos;

namespace Trusts.Application.InputInfos.GetInputInfos;

internal sealed class GetInputInfosQueryHandler(
    IInputInfoRepository inputinfoRepository)
    : IQueryHandler<GetInputInfosQuery, IReadOnlyCollection<InputInfoResponse>>
{
    public async Task<Result<IReadOnlyCollection<InputInfoResponse>>> Handle(GetInputInfosQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await inputinfoRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new InputInfoResponse(
                e.InputInfoId,
                e.CustomerDealId,
                e.OriginId,
                e.CollectionMethodId,
                e.PaymentFormId,
                e.CollectionAccount,
                e.PaymentFormDetail,
                e.CertificationStatusId,
                e.TaxConditionId,
                e.ContingentWithholding,
                e.VerifiableMedium,
                e.CollectionBank,
                e.DepositDate,
                e.SalesUser,
                e.City))
            .ToList();

        return Result.Success<IReadOnlyCollection<InputInfoResponse>>(response);
    }
}