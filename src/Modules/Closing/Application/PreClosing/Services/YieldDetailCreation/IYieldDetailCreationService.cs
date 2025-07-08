

using Closing.Domain.ProfitLosses;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosingSimulation.RunSimulation;

namespace Closing.Application.PreClosing.Services.YieldDetailCreation
{
    public interface IYieldDetailCreationService
    {
        public void CreateYieldDetailsAsync(
            IEnumerable<YieldDetail> yieldDetails,
            CancellationToken cancellationToken = default);

        public YieldDetail PandLConceptToYieldDetail(ProfitLossConceptSummary conceptSummary, RunSimulationCommand parameters);
        public IReadOnlyList<YieldDetail> PandLConceptSummaryToYieldDetails(IReadOnlyList<ProfitLossConceptSummary> conceptSummary, RunSimulationCommand parameters);

    }
}
