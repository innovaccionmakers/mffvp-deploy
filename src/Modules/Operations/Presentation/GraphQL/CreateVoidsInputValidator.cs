using FluentValidation;
using Operations.Presentation.GraphQL.Inputs;

namespace Operations.Presentation.GraphQL;

public sealed class CreateVoidsInputValidator : AbstractValidator<CreateVoidsInput>
{
    public CreateVoidsInputValidator()
    {
        RuleFor(input => input.CauseId)
            .GreaterThan(0);

        RuleFor(input => input.AffiliateId)
            .GreaterThan(0);

        RuleFor(input => input.ObjectiveId)
            .GreaterThan(0);

        RuleFor(input => input.Items)
            .NotEmpty();

        RuleForEach(input => input.Items)
            .ChildRules(item =>
            {
                item.RuleFor(value => value.ClientOperationId)
                    .GreaterThan(0);

                item.RuleFor(value => value.Amount)
                    .GreaterThan(0);
            });
    }
}
