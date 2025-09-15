using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.EntityValidation;

public sealed record ValidateEntityQuery(string Entity) : IQuery<bool>;
