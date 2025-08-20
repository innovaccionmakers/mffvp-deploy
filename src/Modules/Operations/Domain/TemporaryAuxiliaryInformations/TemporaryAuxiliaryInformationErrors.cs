using Common.SharedKernel.Core.Primitives;

namespace Operations.Domain.TemporaryAuxiliaryInformations;

public static class TemporaryAuxiliaryInformationErrors
{
    public static Error NotFound(long temporaryAuxiliaryInformationId)
    {
        return Error.NotFound(
            "TemporaryAuxiliaryInformation.NotFound",
            $"The temporary auxiliary information with identifier {temporaryAuxiliaryInformationId} was not found"
        );
    }
}
