using HotChocolate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Closing.Presentation.GraphQL.Inputs
{
    public record RunSimulationInput(
        [property: GraphQLName("idPortafolio")] int PortfolioId,
        [property: GraphQLName("fechaCierre")] DateTime ClosingDate,
        [property: GraphQLName("estaCerrado")] bool IsClosing
        );
}
