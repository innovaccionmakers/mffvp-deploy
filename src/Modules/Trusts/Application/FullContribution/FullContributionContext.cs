using Trusts.Domain.Clients;
using Trusts.Domain.Portfolios;
using Trusts.Integrations.FullContribution;

namespace Trusts.Application.FullContribution;

public sealed class FullContributionContext
{
    public FullContributionContext(
        CreateFullContributionCommand cmd,
        Client? client,
        Portfolio? portfolio,
        bool idTypeHomologated,
        bool originExists,
        bool originActive,
        bool collectionExists,
        bool collectionActive,
        bool paymentExists,
        bool paymentActive,
        bool originRequiresCertification)
    {
        Cmd = cmd;
        Client = client;
        Portfolio = portfolio;
        IdTypeHomologated = idTypeHomologated;
        OriginExists = originExists;
        OriginActive = originActive;
        CollectionExists = collectionExists;
        CollectionActive = collectionActive;
        PaymentExists = paymentExists;
        PaymentActive = paymentActive;
        OriginRequiresCertification = originRequiresCertification;
    }

    public CreateFullContributionCommand Cmd { get; }

    public Client? Client { get; }
    public Portfolio? Portfolio { get; }

    public bool IdTypeHomologated { get; }
    public bool OriginExists { get; }
    public bool OriginActive { get; }
    public bool CollectionExists { get; }
    public bool CollectionActive { get; }
    public bool PaymentExists { get; }
    public bool PaymentActive { get; }
    public bool OriginRequiresCertification { get; }
}