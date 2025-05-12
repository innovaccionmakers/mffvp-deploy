using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.CustomerDeals;
using Trusts.Domain.InputInfos;
using Trusts.Integrations.InputInfos;
using Trusts.Integrations.InputInfos.CreateInputInfo;

namespace Trusts.Application.InputInfos.CreateInputInfo;

internal sealed class CreateInputInfoCommandHandler(
    ICustomerDealRepository customerdealRepository,
    IInputInfoRepository inputinfoRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateInputInfoCommand, InputInfoResponse>
{
    public async Task<Result<InputInfoResponse>> Handle(CreateInputInfoCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var customerdeal = await customerdealRepository.GetAsync(request.CustomerDealId, cancellationToken);

        if (customerdeal is null)
            return Result.Failure<InputInfoResponse>(CustomerDealErrors.NotFound(request.CustomerDealId));


        var result = InputInfo.Create(
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
            request.City,
            customerdeal
        );

        if (result.IsFailure) return Result.Failure<InputInfoResponse>(result.Error);

        var inputinfo = result.Value;

        inputinfoRepository.Insert(inputinfo);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new InputInfoResponse(
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
    }
}