namespace Common.SharedKernel.Domain
{
    public static class ContributionErrors
    {
        public static Error IdTypeRequired => Error.Validation("6260", "Identification type is required");
        public static Error IdentificationRequired => Error.Validation("6261", "Identification is required");
        public static Error ObjectiveRequired => Error.Validation("6262", "ObjectiveId is required");
        public static Error OriginRequired => Error.Validation("6263", "Origin is required");
        public static Error CollectionRequired => Error.Validation("6264", "Collection method is required");
        public static Error ExecutionDateRequired => Error.Validation("6269", "Execution date is required");

        public static Error IdTypeNotHomologated => Error.Validation("6270", "Identification type not homologated");

        public static Error ClientNotFound => Error.NotFound("6000", "Client does not exist");
        public static Error ClientInactive => Error.Conflict("6001", "Client is inactive");
        public static Error ClientBlocked => Error.Conflict("0009", "Client is blocked");
        public static Error ClientNotInProduct => Error.Conflict("6003", "Client not activated for the product");

        public static Error PortfolioNotFound => Error.NotFound("6356", "Portfolio code does not exist");
        public static Error PortfolioMismatch => Error.Conflict("6272", "Portfolio does not belong to objective");

        public static Error OriginNotFound => Error.NotFound("6275", "Origin does not exist");
        public static Error OriginInactive => Error.Conflict("6276", "Origin is inactive");
        public static Error CollectionNotFound => Error.NotFound("6277", "Collection method does not exist");
        public static Error CollectionInactive => Error.Conflict("6278", "Collection method is inactive");
        public static Error PaymentNotFound => Error.NotFound("6279", "Payment method does not exist");
        public static Error PaymentInactive => Error.Conflict("6280", "Payment method is inactive");

        public static Error CertifiedRequired => Error.Validation("6281", "Certified flag is required for this origin");
        public static Error CertifiedInvalidValue => Error.Validation("6282", "Certified flag must be \"SI\" or \"NO\"");

        public static Error ExecutionDateMismatch => Error.Validation("6283", "Execution date must equal portfolio operation date");
    }
}
