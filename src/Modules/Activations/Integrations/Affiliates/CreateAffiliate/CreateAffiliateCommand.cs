using Common.SharedKernel.Application.Messaging;

namespace Activations.Integrations.Affiliates.CreateAffiliate;

public sealed record CreateAffiliateCommand(
    string IdentificationType,
    string Identification,
    bool Pensioner,
    bool MeetsRequirements,
    DateTime ActivationDate
) : ICommand<AffiliateResponse>;