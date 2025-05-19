using Common.SharedKernel.Application.Messaging;
using System;

namespace Operations.Integrations.AuxiliaryInformations.GetAuxiliaryInformation;

public sealed record GetAuxiliaryInformationQuery(
    long AuxiliaryInformationId
) : IQuery<AuxiliaryInformationResponse>;