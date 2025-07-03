namespace Associate.Presentation.DTOs;

public record AssociateDto(
    string Identification,
    string IdentificationType,
    int Id,
    bool Pensioner,
    DateTime ActivateDate
);
