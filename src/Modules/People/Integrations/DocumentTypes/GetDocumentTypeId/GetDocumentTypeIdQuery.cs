using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace People.Integrations.DocumentTypes.GetDocumentTypeId;

public sealed record GetDocumentTypeIdQuery(
    [property: HomologScope("Tipo documento")]
    string TypeIdHomologationCode)
    : IQuery<GetDocumentTypeIdResponse>;