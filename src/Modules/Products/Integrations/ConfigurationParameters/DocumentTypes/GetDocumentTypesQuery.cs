using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.ConfigurationParameters.DocumentTypes;

public sealed record class GetDocumentTypesQuery : IQuery<IReadOnlyCollection<ConfigurationParameterResponse>>;
