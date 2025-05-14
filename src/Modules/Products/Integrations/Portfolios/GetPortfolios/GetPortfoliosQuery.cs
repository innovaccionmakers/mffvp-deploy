using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Products.Integrations.Portfolios.GetPortfolios;
public sealed record GetPortfoliosQuery() : IQuery<IReadOnlyCollection<PortfolioResponse>>;