using Common.SharedKernel.Core.Primitives;

namespace Customers.Domain.People
{
    public record PersonInformation(
        long PersonId, 
        Guid DocumentType, 
        string DocumentTypeHomologatedCode, 
        string Identification,  
        string FullName, 
        Status Status
    );    
}
