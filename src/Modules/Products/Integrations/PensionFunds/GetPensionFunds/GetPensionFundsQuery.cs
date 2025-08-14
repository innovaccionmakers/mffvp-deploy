using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.PensionFunds.GetPensionFunds
{
    public sealed record GetPensionFundsQuery : IQuery<IReadOnlyCollection<PensionFundsResponse>>;
}
