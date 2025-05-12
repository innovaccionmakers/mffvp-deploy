namespace Common.SharedKernel.Domain
{
    public static class ActivationErrors
    {
        public static Error IdTypeRequired => Error.Validation("1", "El Tipo Id es obligatorio");
        public static Error IdentificationRequired => Error.Validation("2", "La Identificación es obligatoria");
        public static Error PensionerRequired => Error.Validation("3", "El campo Pensionado es obligatorio");
        public static Error PensionRequirementsRequiredWhenFalse => Error.Validation("4", "El campo CumpleRequisitosPension es obligatorio solo cuando el campo Pensionado tenga el valor de false");
        public static Error PensionRequirementsMissingWhenFalse => Error.Validation("5", "El campo CumpleRequisitosPension es obligatorio solo cuando el campo Pensionado tenga el valor de false");
        public static Error StartDateNotRequiredWhenPensionerTrue => Error.Validation("6", "El campo FechaInicioReqPen es obligatorio solo cuando el campo Pensionado tenga el valor de false y el campo CumpleRequisitosPension tenga el valor true");
        public static Error StartDateRequiredWhenConditionsMet => Error.Validation("7", "El campo FechaInicioReqPen es obligatorio solo cuando el campo Pensionado tenga el valor de false y el campo CumpleRequisitosPension tenga el valor true");
        public static Error StartDateRequired => Error.Validation("8", "El campo FechaInicioReqPen es obligatorio solo cuando el campo Pensionado tenga el valor de false y el campo CumpleRequisitosPension tenga el valor true");
        public static Error ClientBlocked => Error.Validation("9", "El Cliente se encuentra Bloqueado");
        public static Error EndDateNotRequiredWhenPensionerTrue => Error.Validation("10", "El campo FechaFinReqPen es obligatorio solo cuando el campo Pensionado tenga el valor de false y el campo CumpleRequisitosPension tenga el valor true");
        public static Error EndDateRequiredWhenConditionsMet => Error.Validation("11", "El campo FechaFinReqPen es obligatorio solo cuando el campo Pensionado tenga el valor de false y el campo CumpleRequisitosPension tenga el valor true");
        public static Error EndDateRequired => Error.Validation("12", "El campo FechaFinReqPen es obligatorio solo cuando el campo Pensionado tenga el valor de false y el campo CumpleRequisitosPension tenga el valor true");
        public static Error ClientInactive => Error.Validation("14", "El Cliente se encuentra Inactivo");
        public static Error ClientNotAffiliated => Error.Validation("15", "El cliente ya se encuentra afiliado en el producto");
        public static Error InvalidPensionerValue => Error.Validation("16", "El campo pensionado solo recibe valor 'true' o 'false'.");
        public static Error InvalidPensionRequirementsValue => Error.Validation("17", "El campo CumpleRequisitosPension solo recibe valor 'true' o 'false'.");
        public static Error ClientNotFound => Error.Validation("6000", "El Cliente no existe");
    }
}
