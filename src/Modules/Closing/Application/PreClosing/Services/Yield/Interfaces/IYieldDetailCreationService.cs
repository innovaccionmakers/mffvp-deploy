using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;

namespace Closing.Application.PreClosing.Services.Yield.Interfaces
{
    public interface IYieldDetailCreationService
    {
        public Task CreateYieldDetailsAsync(IEnumerable<YieldDetail> yieldDetails,
            CancellationToken cancellationToken = default);

    }
}
