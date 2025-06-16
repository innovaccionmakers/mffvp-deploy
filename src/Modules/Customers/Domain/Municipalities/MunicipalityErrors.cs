using Common.SharedKernel.Domain;

namespace Customers.Domain.Municipalities;
public static class MunicipalityErrors
{
    public static Error NotFound(int municipalityId) =>
        Error.NotFound(
            "Municipality.NotFound",
            $"The municipality with identifier {municipalityId} was not found"
        );
}