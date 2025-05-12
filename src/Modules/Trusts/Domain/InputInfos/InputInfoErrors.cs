using Common.SharedKernel.Domain;

namespace Trusts.Domain.InputInfos;

public static class InputInfoErrors
{
    public static Error NotFound(Guid inputinfoId)
    {
        return Error.NotFound(
            "InputInfo.NotFound",
            $"The inputinfo with identifier {inputinfoId} was not found"
        );
    }
}