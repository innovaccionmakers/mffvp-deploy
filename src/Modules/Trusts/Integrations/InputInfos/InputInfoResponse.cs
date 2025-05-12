using System.Text.Json;

namespace Trusts.Integrations.InputInfos;

public sealed record InputInfoResponse(
    Guid InputInfoId,
    Guid CustomerDealId,
    int OriginId,
    int CollectionMethodId,
    int PaymentFormId,
    int CollectionAccount,
    JsonDocument PaymentFormDetail,
    int CertificationStatusId,
    int TaxConditionId,
    int ContingentWithholding,
    JsonDocument VerifiableMedium,
    string CollectionBank,
    DateTime DepositDate,
    string SalesUser,
    string City
);