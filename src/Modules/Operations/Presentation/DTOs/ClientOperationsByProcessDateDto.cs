using System.Text.Json;

namespace Operations.Presentation.DTOs
{
    public record class ClientOperationsByProcessDateDto(
        //operaciones_clientes
        decimal Amount,

        //informacion_auxiliar
        string CollectionAccount,
        JsonDocument PaymentMethodDetail,

        //subtipo_transacciones
        string Name
    );
}
