using Common.SharedKernel.Application.Messaging;
using System;

namespace Activations.Integrations.Affiliates.UpdateAffiliate;
public sealed record UpdateAffiliateCommand(
    int AffiliateId,
    string NewIdentificationType,
    string NewIdentification,
    bool NewPensioner,
    bool NewMeetsRequirements,
    DateTime NewActivationDate
) : ICommand<AffiliateResponse>;