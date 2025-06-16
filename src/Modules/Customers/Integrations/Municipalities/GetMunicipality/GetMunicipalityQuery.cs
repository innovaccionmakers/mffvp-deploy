using Common.SharedKernel.Application.Messaging;
using System;

namespace Customers.Integrations.Municipalities.GetMunicipality;
public sealed record GetMunicipalityQuery(
    string HomologatedCode
) : IQuery<MunicipalityResponse>;