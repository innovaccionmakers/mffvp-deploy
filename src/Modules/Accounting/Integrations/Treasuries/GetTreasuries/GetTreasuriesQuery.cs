using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Treasuries.GetTreasuries
{
    public class GetTreasuriesQuery() : IQuery<IReadOnlyCollection<GetTreasuriesResponse>>;
}
