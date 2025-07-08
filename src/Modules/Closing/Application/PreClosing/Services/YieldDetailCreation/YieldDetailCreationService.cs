using Closing.Domain.ProfitLosses;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;

namespace Closing.Application.PreClosing.Services.YieldDetailCreation
{
    public class YieldDetailCreationService : IYieldDetailCreationService
    {

        private Domain.YieldDetails.IYieldDetailRepository _yieldDetailRepository;
        public YieldDetailCreationService(
            Domain.YieldDetails.IYieldDetailRepository yieldDetailRepository)
        {
            _yieldDetailRepository = yieldDetailRepository;
        }
        public async Task CreateYieldDetailsAsync(
        IEnumerable<YieldDetail> yieldDetails,
        CancellationToken cancellationToken = default)
        {
            var tasks = yieldDetails
                .Select(detail => _yieldDetailRepository.InsertAsync(detail, cancellationToken));

            await Task.WhenAll(tasks);
        }


        public YieldDetail PandLConceptToYieldDetail(ProfitLossConceptSummary conceptSummary, RunSimulationCommand parameters)
        {
            var detail =  YieldDetail.Create(
                parameters.PortfolioId,
                parameters.ClosingDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(parameters.ClosingDate, DateTimeKind.Utc)
                : parameters.ClosingDate.ToUniversalTime(),
                conceptSummary.Source,
                ProfitLossConceptSummaryExtensions.ToJsonSummary(conceptSummary),
                conceptSummary.Nature == Domain.ProfitLossConcepts.ProfitLossNature.Income ? conceptSummary.TotalAmount : 0,
                conceptSummary.Nature == Domain.ProfitLossConcepts.ProfitLossNature.Expense ? conceptSummary.TotalAmount : 0,
                0,
                DateTime.UtcNow,
                parameters.IsClosing
                );

            return detail.Value;
        }

        public IReadOnlyList<YieldDetail> PandLConceptSummaryToYieldDetails(IReadOnlyList<ProfitLossConceptSummary> conceptSummary, RunSimulationCommand parameters)
        {
            var details= new List<YieldDetail>();
            foreach (var yieldDetail in conceptSummary)
            {
                details.Add(this.PandLConceptToYieldDetail(yieldDetail, parameters));
            }
            return details;
        }
    }
}