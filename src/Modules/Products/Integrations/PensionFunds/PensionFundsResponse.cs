using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Products.Integrations.PensionFunds
{
    public sealed record PensionFundsResponse
    (
        int PensionFundId,
        int DocumentTypeId,
        int IdentificationNumber,
        string Name,
        string ShortName,
        Status Status,
        string HomologatedCode
    );
}
