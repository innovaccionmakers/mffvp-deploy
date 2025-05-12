using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Activations.Integrations.Affiliates.GetAffiliates;
public sealed record GetAffiliatesQuery() : IQuery<IReadOnlyCollection<AffiliateResponse>>;