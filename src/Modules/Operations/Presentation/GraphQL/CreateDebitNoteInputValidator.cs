using FluentValidation;
using Operations.Presentation.GraphQL.Inputs;

namespace Operations.Presentation.GraphQL;

public sealed class CreateDebitNoteInputValidator : AbstractValidator<CreateDebitNoteInput>
{
    public CreateDebitNoteInputValidator()
    {
        RuleFor(input => input.ClientOperationId)
            .GreaterThan(0);

        RuleFor(input => input.Amount)
            .GreaterThan(0);

        RuleFor(input => input.CauseId)
            .GreaterThan(0);

        RuleFor(input => input.AffiliateId)
            .GreaterThan(0);

        RuleFor(input => input.ObjectiveId)
            .GreaterThan(0);
    }
}
