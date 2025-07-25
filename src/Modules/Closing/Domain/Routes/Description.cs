
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
                                   "FechaCierre": "2025-07-01"
                                 }
                                 ```

                                 - `IdPortafolio`: Identificador del portafolio
                                 - `FechaCierre`: Fecha en que se va a ejecutar el cierre del portafolio
                                 """,
                  RunClosing = """
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
                                 - `FechaCierre`: Fecha en que se va a confirmar el cierre del portafolio
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
                                 - `FechaCierre`: Fecha en que se va a cancelar el cierre del portafolio
                                 """

            ,
            AbortClosing = ""
            ;
    }

    public struct RequestBodyDescription
    {
        public const string
            RunPreclosing = "", 
            RunClosing = "",
            ConfirmClosing = "",
            CancelClosing = "",
            AbortClosing = "";
    }
}
