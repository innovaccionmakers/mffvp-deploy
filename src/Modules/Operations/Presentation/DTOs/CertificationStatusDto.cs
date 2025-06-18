namespace Operations.Presentation.DTOs;

public record CertificationStatusDto(
    string Uuid,
    string Name,
    bool Status,
    string HomologationCode
);
