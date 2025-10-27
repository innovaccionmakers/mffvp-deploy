using Common.SharedKernel.Application.Messaging;
using MediatR;

namespace Accounting.Integrations.AccountingGeneration;

public sealed record AccountingGenerationCommand : ICommand<Unit>;