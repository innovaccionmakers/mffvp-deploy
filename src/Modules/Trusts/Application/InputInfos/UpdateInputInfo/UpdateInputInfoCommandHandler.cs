using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.InputInfos;
using Trusts.Integrations.InputInfos;
using Trusts.Integrations.InputInfos.UpdateInputInfo;

namespace Trusts.Application.InputInfos.UpdateInputInfo;

internal sealed class UpdateInputInfoCommandHandler(
    IInputInfoRepository inputinfoRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateInputInfoCommand, InputInfoResponse>
{
    public async Task<Result<InputInfoResponse>> Handle(UpdateInputInfoCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await inputinfoRepository.GetAsync(request.InputInfoId, cancellationToken);
        if (entity is null) return Result.Failure<InputInfoResponse>(InputInfoErrors.NotFound(request.InputInfoId));

        entity.UpdateDetails(
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

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new InputInfoResponse(entity.InputInfoId, entity.CustomerDealId, entity.OriginId,
            entity.CollectionMethodId, entity.PaymentFormId, entity.CollectionAccount, entity.PaymentFormDetail,
            entity.CertificationStatusId, entity.TaxConditionId, entity.ContingentWithholding, entity.VerifiableMedium,
            entity.CollectionBank, entity.DepositDate, entity.SalesUser, entity.City);
    }
}