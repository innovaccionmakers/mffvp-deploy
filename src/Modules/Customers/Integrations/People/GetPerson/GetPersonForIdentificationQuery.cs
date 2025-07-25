using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Customers.Integrations.People.GetPerson;

public sealed record GetPersonForIdentificationQuery(    
    [property: HomologScope("TipoDocumento")]
    string DocumentType,
    string Identification
) : IQuery<PersonResponse>;
