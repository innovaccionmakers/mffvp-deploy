using Common.SharedKernel.Application.Messaging;
using System;

namespace Activations.Integrations.Affiliates.GetAffiliate;
public sealed record GetAffiliateQuery(
    int AffiliateId
) : IQuery<AffiliateResponse>;