namespace Common.SharedKernel.Domain.OperationTypes;


public static class OperationTypeAttributes
{
    // Clave fija del JSONB
    public const string GroupListKey = "GrupoLista";
    public static class GroupList
    {
        public const string InternalOperations = "OperacionesInternas";
        public const string ClientOperations = "OperacionesClientes";
        public const string AccountingNotes = "NotasContables";

        public static readonly ISet<string> All = new HashSet<string>(StringComparer.Ordinal)
        {
            InternalOperations,
            ClientOperations,
            AccountingNotes
        };
    }

    public static class Names
    {
        public const string Contribution = "Aporte";
        public const string DebitNote = "Nota Débito";
        public const string Yields = "Rendimientos";
        public const string None = "Ninguno";

        public static readonly ISet<string> All = new HashSet<string>(StringComparer.Ordinal)
        {
            Contribution,
            DebitNote,
            Yields,
            None
        };
    }
}