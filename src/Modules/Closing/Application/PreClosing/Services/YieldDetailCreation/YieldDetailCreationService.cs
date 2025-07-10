using Closing.Domain.Commission;
using Closing.Domain.Constants;
using Closing.Domain.ProfitLosses;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Domain.Utils;

namespace Closing.Application.PreClosing.Services.YieldDetailCreation
{
    public class YieldDetailCreationService : IYieldDetailCreationService
    {

        private IYieldDetailRepository _yieldDetailRepository;
        public YieldDetailCreationService(
            IYieldDetailRepository yieldDetailRepository)
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
            //foreach (var item in yieldDetails)
            //{
            //    await _yieldDetailRepository.InsertAsync(item, cancellationToken);
            //}
        }


        public YieldDetail PandLConceptToYieldDetail(ProfitLossConceptSummary conceptSummary, RunSimulationCommand parameters)
        {
            var closingDateUtc = DateTimeConverter.ToUtcDateTime(parameters.ClosingDate);
            var detail =  YieldDetail.Create(
                parameters.PortfolioId,
                closingDateUtc,
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

        public YieldDetail CommissionConceptToYieldDetail(CommissionConceptSummary commisionConceptSummary, RunSimulationCommand parameters)
        {
            var closingDateUtc = DateTimeConverter.ToUtcDateTime(parameters.ClosingDate);
            var detail = YieldDetail.Create(
                parameters.PortfolioId,
                closingDateUtc,
                YieldsSources.Commission,
                CommissionConceptSummaryExtensions.ToJsonSummary(commisionConceptSummary),
                0,
                0,
                commisionConceptSummary.TotalAmount,
                DateTime.UtcNow,
                parameters.IsClosing
                );

            return detail.Value;
        }
        public IReadOnlyList<YieldDetail> CommissionConceptSummaryToYieldDetails(IReadOnlyList<CommissionConceptSummary> commissionsSummary, RunSimulationCommand parameters)
        {
            var details = new List<YieldDetail>();
            foreach (var yieldDetail in commissionsSummary)
            {
                details.Add(this.CommissionConceptToYieldDetail(yieldDetail, parameters));
            }
            return details;
        }
    }
}