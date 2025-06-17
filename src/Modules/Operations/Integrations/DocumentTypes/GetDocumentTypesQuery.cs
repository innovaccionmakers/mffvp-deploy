using Common.SharedKernel.Application.Messaging;
using Operations.Domain.DocumentTypes;

namespace Operations.Integrations.DocumentTypes;

public sealed record class GetDocumentTypesQuery : IQuery<IReadOnlyCollection<DocumentType>>;
