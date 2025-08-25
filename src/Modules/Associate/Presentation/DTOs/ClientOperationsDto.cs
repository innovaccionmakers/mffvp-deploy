using System.Text.Json;

namespace Associate.Presentation.DTOs
{
    public record ClientOperationsDto
    (
        //operaciones_clientes
        decimal Amount,

        //informacion_auxiliar
        int CollectionAccount,
        JsonDocument PaymentMethodDetail,

        //subtipo_transacciones
        string Name
    );
}
