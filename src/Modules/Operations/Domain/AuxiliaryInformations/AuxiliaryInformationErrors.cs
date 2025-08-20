using Common.SharedKernel.Core.Primitives;

namespace Operations.Domain.AuxiliaryInformations;

public static class AuxiliaryInformationErrors
{
    public static Error NotFound(long auxiliaryinformationId)
    {
        return Error.NotFound(
            "AuxiliaryInformation.NotFound",
            $"The auxiliaryinformation with identifier {auxiliaryinformationId} was not found"
        );
    }
}