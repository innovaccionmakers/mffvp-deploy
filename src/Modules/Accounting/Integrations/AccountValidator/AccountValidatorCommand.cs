using Common.SharedKernel.Application.Messaging;
using MediatR;

namespace Accounting.Integrations.AccountValidator;

public sealed record AccountValidatorCommand(DateTime ProcessDate, string ProcessType) : ICommand<Unit>;
