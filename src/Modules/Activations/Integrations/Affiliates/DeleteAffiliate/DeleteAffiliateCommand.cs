using Common.SharedKernel.Application.Messaging;
using System;

namespace Activations.Integrations.Affiliates.DeleteAffiliate;
public sealed record DeleteAffiliateCommand(
    int AffiliateId
) : ICommand;