using Common.SharedKernel.Application.Messaging;

namespace Treasury.Integrations.ConfigurationParameters.AccountTypes;

public sealed record GetAccountTypesQuery : IQuery<IReadOnlyCollection<AccountTypeResponse>>;

