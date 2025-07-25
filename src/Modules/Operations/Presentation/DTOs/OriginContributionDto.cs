﻿using HotChocolate;

namespace Operations.Presentation.DTOs;

[GraphQLName("ContribucionOrigen")]
public record OriginContributionDto(
    [property: GraphQLName("id")] int Id,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);
