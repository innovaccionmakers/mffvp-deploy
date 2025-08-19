using Common.SharedKernel.Application.Attributes;
using MediatR;

namespace Products.Integrations.Portfolios.Commands;

[AuditLog]
public sealed record UpdatePortfolioFromClosingCommand(
    int PortfolioId,
    DateTime CloseDate
) : IRequest;