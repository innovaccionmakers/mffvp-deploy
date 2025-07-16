
using Closing.Domain.YieldDetails;


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
        //var tasks = yieldDetails
        //    .Select(detail => _yieldDetailRepository.InsertAsync(detail, cancellationToken));

        //await Task.WhenAll(tasks);
        foreach (var item in yieldDetails)
        {
            await _yieldDetailRepository.InsertAsync(item, cancellationToken);
        }
    }

}