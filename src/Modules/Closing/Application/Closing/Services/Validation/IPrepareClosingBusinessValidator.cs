using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.Validation.Interfaces
{
    public interface IPrepareClosingBusinessValidator
    {
        Task<Result> ValidateAsync(
            PrepareClosingCommand command,
            bool isFirstClosingDay,
            CancellationToken cancellationToken = default);
    }
}