namespace Trusts.IntegrationEvents.TrustYields;

public sealed record GetTrustParticipantRequest(IEnumerable<long> TrustIds);
