using Closing.Domain.Commission;
using Closing.Domain.ProfitLosses;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;

namespace Closing.Application.PreClosing.Services.YieldDetailCreation
{
    public interface IYieldDetailCreationService
    {
        public Task CreateYieldDetailsAsync(
            IEnumerable<YieldDetail> yieldDetails,
            CancellationToken cancellationToken = default);

        public YieldDetail PandLConceptToYieldDetail(ProfitLossConceptSummary conceptSummary, RunSimulationCommand parameters);
        public IReadOnlyList<YieldDetail> PandLConceptSummaryToYieldDetails(IReadOnlyList<ProfitLossConceptSummary> conceptSummary, RunSimulationCommand parameters);
        public IReadOnlyList<YieldDetail> CommissionConceptSummaryToYieldDetails(IReadOnlyList<CommissionConceptSummary> commissionsSummary, RunSimulationCommand parameters);
    }
}
