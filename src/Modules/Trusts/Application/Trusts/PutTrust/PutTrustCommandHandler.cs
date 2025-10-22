using System;
using System.Linq;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts.PutTrust;

namespace Trusts.Application.Trusts.PutTrust;

internal sealed class PutTrustCommandHandler(
    ITrustRepository trustRepository,
    IUnitOfWork unitOfWork,
    IInternalRuleEvaluator<TrustsModuleMarker> ruleEvaluator)
    : ICommandHandler<PutTrustCommand>
{
    private const string RequiredFieldsWorkflow = "Trusts.PutTrust.RequiredFields";
    private const string ValidationWorkflow = "Trusts.PutTrust.Validation";

    public async Task<Result> Handle(PutTrustCommand request, CancellationToken cancellationToken)
    {
        var requiredContext = new
        {
            request.ClientOperationId,
            request.UpdateDate
        };

        var (requiredOk, _, requiredErrors) = await ruleEvaluator
            .EvaluateAsync(RequiredFieldsWorkflow, requiredContext, cancellationToken);

        if (!requiredOk)
        {
            var first = requiredErrors.First();
            return Result.Failure(Error.Validation(first.Code, first.Message));
        }

        var trust = await trustRepository.GetByClientOperationIdAsync(
            request.ClientOperationId,
            cancellationToken);

        var validationContext = new
        {
            TrustExists = trust is not null
        };

        var (validationOk, _, validationErrors) = await ruleEvaluator
            .EvaluateAsync(ValidationWorkflow, validationContext, cancellationToken);

        if (!validationOk)
        {
            var first = validationErrors.First();
            return Result.Failure(Error.Validation(first.Code, first.Message));
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        trust!.UpdateState(
            request.TotalBalance,
            request.Principal,
            request.ContingentWithholding,
            request.Status,
            request.UpdateDate);
        trustRepository.Update(trust);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
