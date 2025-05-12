using System.Text.Json;
using Common.SharedKernel.Domain;
using Trusts.Domain.CustomerDeals;

namespace Trusts.Domain.InputInfos;

public sealed class InputInfo : Entity
{
    private InputInfo()
    {
    }

    public Guid InputInfoId { get; private set; }
    public Guid CustomerDealId { get; private set; }
    public int OriginId { get; private set; }
    public int CollectionMethodId { get; private set; }
    public int PaymentFormId { get; private set; }
    public int CollectionAccount { get; private set; }
    public JsonDocument PaymentFormDetail { get; private set; }
    public int CertificationStatusId { get; private set; }
    public int TaxConditionId { get; private set; }
    public int ContingentWithholding { get; private set; }
    public JsonDocument VerifiableMedium { get; private set; }
    public string CollectionBank { get; private set; }
    public DateTime DepositDate { get; private set; }
    public string SalesUser { get; private set; }
    public string City { get; private set; }

    public static Result<InputInfo> Create(
        int originId, int collectionMethodId, int paymentFormId, int collectionAccount, JsonDocument paymentFormDetail,
        int certificationStatusId, int taxConditionId, int contingentWithholding, JsonDocument verifiableMedium,
        string collectionBank, DateTime depositDate, string salesUser, string city, CustomerDeal customerDeal
    )
    {
        var inputinfo = new InputInfo
        {
            InputInfoId = Guid.NewGuid(),
            CustomerDealId = customerDeal.CustomerDealId,
            OriginId = originId,
            CollectionMethodId = collectionMethodId,
            PaymentFormId = paymentFormId,
            CollectionAccount = collectionAccount,
            PaymentFormDetail = paymentFormDetail,
            CertificationStatusId = certificationStatusId,
            TaxConditionId = taxConditionId,
            ContingentWithholding = contingentWithholding,
            VerifiableMedium = verifiableMedium,
            CollectionBank = collectionBank,
            DepositDate = depositDate,
            SalesUser = salesUser,
            City = city
        };
        inputinfo.Raise(new InputInfoCreatedDomainEvent(inputinfo.InputInfoId));
        return Result.Success(inputinfo);
    }

    public void UpdateDetails(
        Guid newCustomerDealId, int newOriginId, int newCollectionMethodId, int newPaymentFormId,
        int newCollectionAccount, JsonDocument newPaymentFormDetail, int newCertificationStatusId,
        int newTaxConditionId, int newContingentWithholding, JsonDocument newVerifiableMedium, string newCollectionBank,
        DateTime newDepositDate, string newSalesUser, string newCity
    )
    {
        CustomerDealId = newCustomerDealId;
        OriginId = newOriginId;
        CollectionMethodId = newCollectionMethodId;
        PaymentFormId = newPaymentFormId;
        CollectionAccount = newCollectionAccount;
        PaymentFormDetail = newPaymentFormDetail;
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