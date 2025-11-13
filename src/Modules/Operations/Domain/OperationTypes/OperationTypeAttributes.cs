namespace Operations.Domain.OperationTypes;

public static class OperationTypeAttributes
{
    // Clave fija del JSONB
    public const string GroupListKey = "GrupoLista";
    public static class GroupList
    {
        public const string OperacionesInternas = "OperacionesInternas";
        public const string OperacionesClientes = "OperacionesClientes";
        public const string NotasContables = "NotasContables";

        public static readonly ISet<string> All = new HashSet<string>(StringComparer.Ordinal)
        {
            OperacionesInternas,
            OperacionesClientes,
            NotasContables
        };
    }
}