
namespace Trusts.IntegrationEvents.TrustYields;

public sealed record UpdateTrustFromYieldResponse(
       bool Succeeded,
       string? Code,
       string? Message
   );