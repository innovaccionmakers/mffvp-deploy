using Common.SharedKernel.Domain;

namespace Products.Domain.Offices;

public static class OfficeErrors
{
    public static Error NotFound(int officeId)
    {
        return Error.NotFound(
            "Office.NotFound",
            $"The office with identifier {officeId} was not found"
        );
    }
}