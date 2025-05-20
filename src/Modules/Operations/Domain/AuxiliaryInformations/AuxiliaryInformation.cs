using System.Text.Json;
using Common.SharedKernel.Domain;

namespace Operations.Domain.AuxiliaryInformations;

public sealed class AuxiliaryInformation : Entity
{
    public long AuxiliaryInformationId { get; private set; }
    public long ClientOperationId { get; private set; }
    public int OriginId { get; private set; }
    public int CollectionMethodId { get; private set; }
    public int PaymentMethodId { get; private set; }
    public int CollectionAccount { get; private set; }
    public JsonDocument PaymentMethodDetail { get; private set; }
    public int CertificationStatusId { get; private set; }
    public int TaxConditionId { get; private set; }
    public int ContingentWithholding { get; private set; }
    public JsonDocument VerifiableMedium { get; private set; }
    public string CollectionBank { get; private set; }
    public DateTime DepositDate { get; private set; }
    public string SalesUser { get; private set; }
    public string City { get; private set; }

    private AuxiliaryInformation()
    {
    }

    public static Result<AuxiliaryInformation> Create(
        long clientOperationId, int originId, int collectionMethodId, int paymentMethodId, int collectionAccount,
        JsonDocument paymentMethodDetail, int certificationStatusId, int taxConditionId, int contingentWithholding,
        JsonDocument verifiableMedium, string collectionBank, DateTime depositDate, string salesUser, string city
    )
    {
        var auxiliaryinformation = new AuxiliaryInformation
        {
            AuxiliaryInformationId = default,
            ClientOperationId = clientOperationId,
            OriginId = originId,
            CollectionMethodId = collectionMethodId,
            PaymentMethodId = paymentMethodId,
            CollectionAccount = collectionAccount,
            PaymentMethodDetail = paymentMethodDetail,
            CertificationStatusId = certificationStatusId,
            TaxConditionId = taxConditionId,
            ContingentWithholding = contingentWithholding,
            VerifiableMedium = verifiableMedium,
            CollectionBank = collectionBank,
            DepositDate = depositDate,
            SalesUser = salesUser,
            City = city
        };

        auxiliaryinformation.Raise(
            new AuxiliaryInformationCreatedDomainEvent(auxiliaryinformation.AuxiliaryInformationId));
        return Result.Success(auxiliaryinformation);
    }

    public void UpdateDetails(
        long newClientOperationId, int newOriginId, int newCollectionMethodId, int newPaymentMethodId,
        int newCollectionAccount, JsonDocument newPaymentMethodDetail, int newCertificationStatusId,
        int newTaxConditionId, int newContingentWithholding, JsonDocument newVerifiableMedium, string newCollectionBank,
        DateTime newDepositDate, string newSalesUser, string newCity
    )
    {
        ClientOperationId = newClientOperationId;
        OriginId = newOriginId;
        CollectionMethodId = newCollectionMethodId;
        PaymentMethodId = newPaymentMethodId;
        CollectionAccount = newCollectionAccount;
        PaymentMethodDetail = newPaymentMethodDetail;
        CertificationStatusId = newCertificationStatusId;
        TaxConditionId = newTaxConditionId;
        ContingentWithholding = newContingentWithholding;
        VerifiableMedium = newVerifiableMedium;
        CollectionBank = newCollectionBank;
        DepositDate = newDepositDate;
        SalesUser = newSalesUser;
        City = newCity;
    }
}