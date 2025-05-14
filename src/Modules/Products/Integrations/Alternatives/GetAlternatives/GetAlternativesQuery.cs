using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Products.Integrations.Alternatives.GetAlternatives;
public sealed record GetAlternativesQuery() : IQuery<IReadOnlyCollection<AlternativeResponse>>;