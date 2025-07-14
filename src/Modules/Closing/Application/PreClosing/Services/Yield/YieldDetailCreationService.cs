using Closing.Application.PreClosing.Services.Yield;
using Closing.Domain.Commission;
using Closing.Domain.Constants;
using Closing.Domain.ProfitLosses;
using Closing.Domain.TreasuryMovements;
using Closing.Domain.YieldDetails;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Domain.Utils;

namespace Closing.Application.PreClosing.Services.Yield;

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

}