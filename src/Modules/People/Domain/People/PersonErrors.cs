using Common.SharedKernel.Domain;

namespace People.Domain.People;
public static class PersonErrors
{
    public static Error NotFound(long personId) =>
        Error.NotFound(
            "Person.NotFound",
            $"The person with identifier {personId} was not found"
        );
}