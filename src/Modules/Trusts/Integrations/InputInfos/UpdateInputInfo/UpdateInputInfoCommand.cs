using System.Text.Json;
using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.InputInfos.UpdateInputInfo;

public sealed record UpdateInputInfoCommand(
    Guid InputInfoId,
    Guid NewCustomerDealId,
    int NewOriginId,
    int NewCollectionMethodId,
    int NewPaymentFormId,
    int NewCollectionAccount,
    JsonDocument NewPaymentFormDetail,
    int NewCertificationStatusId,
    int NewTaxConditionId,
    int NewContingentWithholding,
    JsonDocument NewVerifiableMedium,
    string NewCollectionBank,
    DateTime NewDepositDate,
    string NewSalesUser,
    string NewCity
) : ICommand<InputInfoResponse>;