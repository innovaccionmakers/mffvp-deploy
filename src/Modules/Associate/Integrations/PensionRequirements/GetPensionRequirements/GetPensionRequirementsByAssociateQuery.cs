using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.PensionRequirements.GetPensionRequirements;

public sealed record class GetPensionRequirementsByAssociateQuery(int AssociateId) : IQuery<IReadOnlyCollection<PensionRequirementResponse>>;
