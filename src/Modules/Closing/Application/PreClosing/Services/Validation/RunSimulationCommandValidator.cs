using Closing.Integrations.PreClosing.RunSimulation;
using FluentValidation;


namespace Closing.Application.PreClosing.Services.Validation;

public class RunSimulationCommandValidator : AbstractValidator<RunSimulationCommand>
{
    public RunSimulationCommandValidator()
    {
        RuleFor(x => x.PortfolioId)
            .GreaterThan(0)
            .LessThanOrEqualTo(int.MaxValue);

        RuleFor(x => x.ClosingDate.Date)
            .Must(date => date >= new DateTime(1900, 1, 1) && date <= DateTime.MaxValue.Date)
            .WithMessage("La fecha de cierre debe ser válida y sin hora.");
    }
}
