using System.Text.Json;

namespace Operations.Integrations.AuxiliaryInformations;

public sealed record AuxiliaryInformationResponse(
    long AuxiliaryInformationId,
    long ClientOperationId,
    int OriginId,
    int CollectionMethodId,
    int PaymentMethodId,
    int CollectionAccount,
    JsonDocument PaymentMethodDetail,
    int CertificationStatusId,
    int TaxConditionId,
    decimal ContingentWithholding,
    JsonDocument VerifiableMedium,
    string CollectionBank,
    DateTime DepositDate,
    string SalesUser,
    string City
);