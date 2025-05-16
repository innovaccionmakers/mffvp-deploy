namespace Common.SharedKernel.Domain;

public static class ActivationErrors
{
    public static Error IdTypeRequired => Error.Validation("1", "El Tipo Id es obligatorio");
    public static Error IdentificationRequired => Error.Validation("2", "La Identificación es obligatoria");
    public static Error PensionerRequired => Error.Validation("3", "El campo Pensionado es obligatorio");
    public static Error PensionerBoolean => Error.Validation("4", "El campo pensionado solo recibe valor 'true' o 'false'");
    public static Error PensionRequirementsWhenFalse => Error.Validation("5", "El campo CumpleRequisitosPension es obligatorio solo cuando el campo Pensionado tenga el valor de false");
    public static Error PensionRequirementsMissing => Error.Validation("6", "El campo CumpleRequisitosPension es obligatorio solo cuando el campo Pensionado tenga el valor de false");
    public static Error MeetsPensionRequirementsBoolean => Error.Validation("7", "El campo CumpleRequisitosPension solo recibe valor 'true' o 'false'");
    public static Error StartDateNotRequiredWhenPensionerTrue => Error.Validation("8", "El campo FechaInicioReqPen es obligatorio solo cuando el campo Pensionado tenga el valor de false y el campo CumpleRequisitosPension tenga el valor true");
    public static Error EndDateRequiredWhenConditionsMet => Error.Validation("9", "El campo FechaFinReqPen es obligatorio solo cuando el campo Pensionado tenga el valor de false y el campo CumpleRequisitosPension tenga el valor true");
    public static Error DateValidation => Error.Validation("10", "La Fecha Final debe ser mayor a la Fecha Inicial");
    public static Error EndDateAfterCurrentDate => Error.Validation("11", "La Fecha Final debe ser mayor a la Fecha Actual");
    public static Error IdTypeHomologated => Error.Validation("12", "El tipo Id no se encuentra homologado");
    public static Error ClientExists => Error.Validation("13", "El Cliente no existe");
    public static Error ClientNotBlocked => Error.Validation("14", "El Cliente se encuentra Bloqueado");
    public static Error ClientActive => Error.Validation("15", "El Cliente se encuentra Inactivo");
    public static Error ClientNotActivates => Error.Validation("16", "El cliente ya se encuentra afiliado en el producto");
}