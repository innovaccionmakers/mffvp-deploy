using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Treasury.GetTreasuries
{
    [AuditLog]
    public sealed record class GetTreasuryQuery(
        int PortfolioId,
        string BankAccount
        ) : IQuery<GetTreasuryResponse>;
}
