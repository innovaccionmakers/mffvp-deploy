using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.Trusts.PutTrust;

[AuditLog]
public sealed record PutTrustCommand(long ClientOperationId) : ICommand;
