using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Contributions.Integrations.Trusts.GetTrusts;
public sealed record GetTrustsQuery() : IQuery<IReadOnlyCollection<TrustResponse>>;