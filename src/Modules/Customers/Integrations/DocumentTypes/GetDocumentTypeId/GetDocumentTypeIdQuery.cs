using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Customers.Integrations.DocumentTypes.GetDocumentTypeId;

public sealed record GetDocumentTypeIdQuery(
    [property: HomologScope("TipoDocumento")]
    string TypeIdHomologationCode)
    : IQuery<GetDocumentTypeIdResponse>;