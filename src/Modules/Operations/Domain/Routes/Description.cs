namespace Operations.Domain.Routes;

public struct Description
{
    public const string CreateContribution = """
                                             **Ejemplo de petición (application/json):**
                                             ```json
                                             {
                                             "TipoId": "C",
                                             "Identificacion": "123456789",
                                             "IdObjetivo": 456,
                                             "IdPortafolio": "ALT123",
                                             "Valor": 1500.75,
                                             "Origen": "Sucursal",
                                             "ModalidadOrigen": "Efectivo",
                                             "MetodoRecaudo": "POS",
                                             "FormaPago": "Tarjeta",
                                             "DetalleFormaPago": { "cardNumber": "**** **** **** 1234", "expiry": "12/25" },
                                             "BancoRecaudo": "Banco X",
                                             "CuentaRecaudo": "000123456",
                                             "AporteCertificado": "CERT123",
                                             "RetencionContingente": 50.25,
                                             "FechaConsignacion": "2025-06-01T00:00:00Z",
                                             "FechaEjecucion": "2025-06-02T00:00:00Z",
                                             "UsuarioComercial": "user123",
                                             "MedioVerificable": { "url": "http://example.com/recibo.pdf" },
                                             "Subtipo": "Extra",
                                             "Canal": "Online",
                                             "Usuario": "system"
                                             }
                                             ```
                                             """,
        GetAllOperationTypes = """
                                 **Ejemplo de llamada:**

                                 ```http
                                 GET /FVP/Operations/OperationTypes
                                 ```
                                 """;
}

public struct RequestBodyDescription
{
    public const string CreateContribution = "";
}
