using Common.SharedKernel.Domain;

namespace Activations.Domain.Affiliates;
public sealed class Affiliate : Entity
{
    public int AffiliateId { get; private set; }
    public string IdentificationType { get; private set; }
    public string Identification { get; private set; }
    public bool Pensioner { get; private set; }
    public bool MeetsRequirements { get; private set; }
    public DateTime ActivationDate { get; private set; }

    private Affiliate() { }

    public static Result<Affiliate> Create(
        string identificationtype, string identification, bool pensioner, bool meetsrequirements, DateTime activationdate
    )
    {
        var affiliate = new Affiliate
        {
                AffiliateId = new int(),
                IdentificationType = identificationtype,
                Identification = identification,
                Pensioner = pensioner,
                MeetsRequirements = meetsrequirements,
                ActivationDate = activationdate,
        };
        affiliate.Raise(new AffiliateCreatedDomainEvent(affiliate.AffiliateId));
        return Result.Success(affiliate);
    }

    public void UpdateDetails(
        string newIdentificationType, string newIdentification, bool newPensioner, bool newMeetsRequirements, DateTime newActivationDate
    )
    {
        IdentificationType = newIdentificationType;
        Identification = newIdentification;
        Pensioner = newPensioner;
        MeetsRequirements = newMeetsRequirements;
        ActivationDate = newActivationDate;
    }
}