using System.Text.Json;
using Common.SharedKernel.Domain;
using Operations.Domain.Channels;
using Operations.Domain.Origins;
using Operations.Domain.TemporaryClientOperations;

namespace Operations.Domain.TemporaryAuxiliaryInformations;

public sealed class TemporaryAuxiliaryInformation : Entity
{
    public long TemporaryAuxiliaryInformationId { get; private set; }
    public long TemporaryClientOperationId { get; private set; }
    public int OriginId { get; private set; }
    public int CollectionMethodId { get; private set; }
    public int PaymentMethodId { get; private set; }
    public int CollectionAccount { get; private set; }
    public JsonDocument PaymentMethodDetail { get; private set; } = null!;
    public int CertificationStatusId { get; private set; }
    public int TaxConditionId { get; private set; }
    public decimal ContingentWithholding { get; private set; }
    public JsonDocument VerifiableMedium { get; private set; } = null!;
    public int CollectionBankId { get; private set; }
    public DateTime DepositDate { get; private set; }
    public string SalesUser { get; private set; } = null!;
    public int OriginModalityId { get; private set; }
    public int CityId { get; private set; }
    public int ChannelId { get; private set; }
    public string UserId { get; private set; }

    public TemporaryClientOperation TemporaryClientOperation { get; private set; } = null!;
    public Origin Origin { get; private set; } = null!;
    public Channel Channel { get; private set; } = null!;

    private TemporaryAuxiliaryInformation()
    {
    }

    public static Result<TemporaryAuxiliaryInformation> Create(
        long temporaryClientOperationId,
        int originId,
        int collectionMethodId,
        int paymentMethodId,
        int collectionAccount,
        JsonDocument paymentMethodDetail,
        int certificationStatusId,
        int taxConditionId,
        decimal contingentWithholding,
        JsonDocument verifiableMedium,
        int collectionBank,
        DateTime depositDate,
        string salesUser,
        int originModalityId,
        int cityId,
        int channelId,
        string userId
    )
    {
        var auxiliaryInformation = new TemporaryAuxiliaryInformation
        {
            TemporaryAuxiliaryInformationId = default,
            TemporaryClientOperationId = temporaryClientOperationId,
            OriginId = originId,
            CollectionMethodId = collectionMethodId,
            PaymentMethodId = paymentMethodId,
            CollectionAccount = collectionAccount,
            PaymentMethodDetail = paymentMethodDetail,
            CertificationStatusId = certificationStatusId,
            TaxConditionId = taxConditionId,
            ContingentWithholding = contingentWithholding,
            VerifiableMedium = verifiableMedium,
            CollectionBankId = collectionBank,
            DepositDate = depositDate,
            SalesUser = salesUser,
            OriginModalityId = originModalityId,
            CityId = cityId,
            ChannelId = channelId,
            UserId = userId
        };

        auxiliaryInformation.Raise(
            new TemporaryAuxiliaryInformationCreatedDomainEvent(auxiliaryInformation.TemporaryAuxiliaryInformationId));
        return Result.Success(auxiliaryInformation);
    }

    public void UpdateDetails(
        long newTemporaryClientOperationId,
        int newOriginId,
        int newCollectionMethodId,
        int newPaymentMethodId,
        int newCollectionAccount,
        JsonDocument newPaymentMethodDetail,
        int newCertificationStatusId,
        int newTaxConditionId,
        decimal newContingentWithholding,
        JsonDocument newVerifiableMedium,
        int newCollectionBank,
        DateTime newDepositDate,
        string newSalesUser,
        int newOriginModalityId,
        int newCityId,
        int newChannelId,
        string newUserId
    )
    {
        TemporaryClientOperationId = newTemporaryClientOperationId;
        OriginId = newOriginId;
        CollectionMethodId = newCollectionMethodId;
        PaymentMethodId = newPaymentMethodId;
        CollectionAccount = newCollectionAccount;
        PaymentMethodDetail = newPaymentMethodDetail;
        CertificationStatusId = newCertificationStatusId;
        TaxConditionId = newTaxConditionId;
        ContingentWithholding = newContingentWithholding;
        VerifiableMedium = newVerifiableMedium;
        CollectionBankId = newCollectionBank;
        DepositDate = newDepositDate;
        SalesUser = newSalesUser;
        OriginModalityId = newOriginModalityId;
        CityId = newCityId;
        ChannelId = newChannelId;
        UserId = newUserId;
    }
}
