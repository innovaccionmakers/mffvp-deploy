namespace Operations.Presentation.DTOs;

public record TransactionTypeDto(
    string Uuid,
    string Name,
    bool Status,
    string HomologationCode,
    List<TransactionSubtypesDto> Subtypes
);

public record TransactionSubtypesDto(
    string Id,
    string Name,
    string HomologationCode
);

