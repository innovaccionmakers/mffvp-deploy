namespace Operations.Presentation.DTOs;

public record DocumentTypeDto(
    string Uuid,
    string Name,
    bool Status,
    string HomologationCode
);
