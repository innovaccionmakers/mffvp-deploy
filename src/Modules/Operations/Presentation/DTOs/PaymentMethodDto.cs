namespace Operations.Presentation.DTOs;

public record PaymentMethodDto(
    string Uuid,
    string Name,
    bool Status,
    string HomologationCode
);

