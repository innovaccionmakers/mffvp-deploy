using Common.SharedKernel.Domain;

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
