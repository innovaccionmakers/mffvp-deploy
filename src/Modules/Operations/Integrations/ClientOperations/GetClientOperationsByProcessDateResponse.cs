using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Operations.Integrations.ClientOperations
{
    public sealed record GetClientOperationsByProcessDateResponse(
        //operaciones_clientes
        decimal Amount,

        //informacion_auxiliar
        int CollectionAccount,
        JsonDocument PaymentMethodDetail,

        //subtipo_transacciones
        string Name
    );
}
