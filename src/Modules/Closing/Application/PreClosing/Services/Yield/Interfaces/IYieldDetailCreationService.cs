using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Domain.YieldDetails;

namespace Closing.Application.PreClosing.Services.Yield.Interfaces
{
    public interface IYieldDetailCreationService
    {
        Task CreateYieldDetailsAsync(
         IEnumerable<YieldDetail> yieldDetails,
         PersistenceMode mode = PersistenceMode.Immediate,
         CancellationToken cancellationToken = default);

    }
}
