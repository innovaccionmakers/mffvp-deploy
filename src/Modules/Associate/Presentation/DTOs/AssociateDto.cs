namespace Associate.Presentation.DTOs;

public record AssociateDto(
    string Identification,
    string DocumentType,
    Guid DocumentTypeUuid,
    int Id,
    bool Pensioner,
    DateTime ActivateDate
);
