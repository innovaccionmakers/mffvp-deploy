﻿using HotChocolate;

namespace Operations.Presentation.DTOs;

[GraphQLName("TipoTransaccion")]
public record TransactionTypeDto(
    [property: GraphQLName("uuid")] Guid Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);
