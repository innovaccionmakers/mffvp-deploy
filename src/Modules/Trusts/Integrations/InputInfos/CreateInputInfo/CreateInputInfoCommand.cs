using System.Text.Json;
using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.InputInfos.CreateInputInfo;

public sealed record CreateInputInfoCommand(
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
) : ICommand<InputInfoResponse>;