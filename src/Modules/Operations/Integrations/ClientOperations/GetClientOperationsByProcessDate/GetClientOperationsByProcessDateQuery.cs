using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.ClientOperations.GetClientOperationsByProcessDate
{
    public sealed record class GetClientOperationsByProcessDateQuery(DateTime ProcessDate) : IQuery<IReadOnlyCollection<ClientOperationsByProcessDateResponse>>;
}
