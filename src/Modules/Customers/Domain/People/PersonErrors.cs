using Common.SharedKernel.Core.Primitives;

namespace Customers.Domain.People;

public static class PersonErrors
{
    public static Error NotFound(long personId)
    {
        return Error.NotFound(
            "Person.NotFound",
            $"The person with identifier {personId} was not found"
        );
    }
}