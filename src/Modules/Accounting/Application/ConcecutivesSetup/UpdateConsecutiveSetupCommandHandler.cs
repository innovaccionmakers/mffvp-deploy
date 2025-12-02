using System.Linq;
using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.Data;
using Accounting.Domain.Consecutives;
using Accounting.Integrations.ConsecutivesSetup;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Accounting.Application.ConcecutivesSetup;

internal sealed class UpdateConsecutiveSetupCommandHandler(
    IConsecutiveRepository consecutiveRepository,
    IUnitOfWork unitOfWork,
    IInternalRuleEvaluator<AccountingModuleMarker> ruleEvaluator)
    : ICommandHandler<UpdateConsecutiveSetupCommand, ConsecutiveSetupResponse>
{
    private const string UpdateWorkflow = "Accounting.ConcecutivesSetup.UpdateValidation";

    public async Task<Result<ConsecutiveSetupResponse>> Handle(
        UpdateConsecutiveSetupCommand request,
        CancellationToken cancellationToken)
    {
        var consecutive = await consecutiveRepository.GetByIdAsync(request.Id, cancellationToken);

        var isSourceDocumentUsed = await consecutiveRepository
            .IsSourceDocumentInUseAsync(request.SourceDocument, request.Id, cancellationToken);

        var validationContext = new
        {
            ConsecutiveExists = consecutive is not null,
            IsSourceDocumentUnique = !isSourceDocumentUsed
        };

        var (isValid, _, errors) = await ruleEvaluator
            .EvaluateAsync(UpdateWorkflow, validationContext, cancellationToken);

        if (!isValid)
        {
            var error = errors.First();
            return Result.Failure<ConsecutiveSetupResponse>(Error.Validation(error.Code, error.Message));
        }

        consecutive!.UpdateDetails(
            consecutive.Nature,
            request.SourceDocument,
            request.Consecutive);

        await consecutiveRepository.UpdateAsync(consecutive, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ConsecutiveSetupResponse(
            consecutive.ConsecutiveId,
            consecutive.Nature,
            consecutive.SourceDocument,
            consecutive.Number);

        return Result.Success(response);
    }
}
