using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.ClientOperations.GetAccountingOperations;

public sealed record class GetAccountingDebitNoteOperationsQuery(IEnumerable<int> PortfolioId, DateTime ProcessDate) : IQuery<IReadOnlyCollection<GetAccountingOperationsResponse>>;
