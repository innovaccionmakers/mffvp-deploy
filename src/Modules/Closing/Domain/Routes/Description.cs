
namespace Closing.Domain.Routes
{
    public struct Description
    {
        public const string
            RunPreclosing = """
                                 **Ejemplo de llamada:**

                                 ```http
                                 POST /FVP/closing/RunSimulation
                                 Content-Type: application/json

                                 {
                                   "IdPortafolio": 1,
                                   "FechaCierre": "2025-07-01",
                                   "EsCierre": false
                                 }
                                 ```

                                 - `IdPortafolio`: Identificador del portafolio
                                 - `FechaCierre`: Fecha en que se va a ejecutar el cierre del portafolio
                                 - `EsCierre`: Indicador si la ejecución es en modo de cierre real o simulación
                                 """,
                  PrepareClosing = """
                                 **Ejemplo de llamada:**

                                 ```http
                                 POST /FVP/closing/RunClosing
                                 Content-Type: application/json

                                 {
                                   "IdPortafolio": 1,
                                   "FechaCierre": "2025-07-01"
                                 }
                                 ```

                                 - `IdPortafolio`: Identificador del portafolio
                                 - `FechaCierre`: Fecha en que se va a ejecutar el cierre del portafolio
                                 """,
            ConfirmClosing = """
                                 **Ejemplo de llamada:**

                                 ```http
                                 POST /FVP/closing/ConfirmClosing
                                 Content-Type: application/json

                                 {
                                   "IdPortafolio": 1,
                                   "FechaCierre": "2025-07-01"
                                 }
                                 ```

                                 - `IdPortafolio`: Identificador del portafolio
                                 - `FechaCierre`: Fecha de Cierre del portafolio
                                 """,
             CancelClosing = """
                                 **Ejemplo de llamada:**

                                 ```http
                                 POST /FVP/closing/CancelClosing
                                 Content-Type: application/json

                                 {
                                   "IdPortafolio": 1,
                                   "FechaCierre": "2025-07-01"
                                 }
                                 ```

                                 - `IdPortafolio`: Identificador del portafolio
                                 - `FechaCierre`: Fecha de Cierre del portafolio
                                 """
            ;
    }

    public struct RequestBodyDescription
    {
        public const string
            RunPreclosing = "", 
            PrepareClosing = "",
            ConfirmClosing = "",
            CancelClosing = "";
    }
}
