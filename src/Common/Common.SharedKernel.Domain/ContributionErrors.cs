namespace Common.SharedKernel.Domain;

public static class ContributionErrors
{
    public static Error IdTypeRequired => Error.Validation("6260", "El Tipo Id es obligatorio");
    public static Error IdentificationRequired => Error.Validation("6261", "La Identificación es obligatoria");
    public static Error ObjectiveRequired => Error.Validation("6262", "El IdObjetivo es obligatorio");
    public static Error AmountPositive => Error.Validation("6267", "El Valor es obligatorio y debe ser mayor a 0");
    public static Error OriginRequired => Error.Validation("6263", "El Origen es obligatorio");
    public static Error CollectionRequired => Error.Validation("6264", "El Método de Recaudo es obligatorio");
    public static Error PaymentRequired => Error.Validation("6268", "La FormaPago es obligatoria");
    public static Error ExecutionDateRequired => Error.Validation("6269", "La Fecha de Ejecución es obligatoria");
    public static Error IdTypeHomologated => Error.Validation("6270", "El Tipo Id no se encuentra homologado");
    public static Error OriginExists => Error.NotFound("6275", "El Origen no existe");
    public static Error OriginActive => Error.Conflict("6276", "El Origen se encuentra inactivo");
    public static Error CollectionExists => Error.NotFound("6277", "El Método de Recaudo no existe");
    public static Error CollectionActive => Error.Conflict("6278", "El Método de Recaudo se encuentra inactivo");
    public static Error PaymentExists => Error.NotFound("6279", "La forma de pago no existe");
    public static Error PaymentActive => Error.Conflict("6280", "La forma de pago se encuentra inactiva");

    public static Error CertifiedRequiredWhenOriginDemands
        => Error.Validation("6281",
            "El estado del aporte certificado es obligatorio cuando el OrigenAporte ExigeCertificacion es igual a 1");

    public static Error CertifiedValid =>
        Error.Validation("6282", "El campo Aporte Certificado solo recibe valor 'SI' o 'NO'");

    public static Error ExecutionDateMatch => Error.Validation("6283",
        "La fecha de ejecución debe ser igual a la fecha de operación del portafolio");

    public static Error ClientExists => Error.NotFound("6000", "El Cliente no existe");
    public static Error ClientActive => Error.Conflict("6001", "El Cliente se encuentra Inactivo");
    public static Error ClientNotBlocked => Error.Conflict("0009", "El Cliente se encuentra Bloqueado");
    public static Error ClientInProduct => Error.Conflict("6003", "El cliente no se encuentra activo en el producto");
    public static Error PortfolioExists => Error.NotFound("6356", "El código homologado del Portafolio no existe");

    public static Error PortfolioBelongsToObj =>
        Error.Conflict("6272", "El Portafolio no pertenece a la alternativa del objetivo");
}