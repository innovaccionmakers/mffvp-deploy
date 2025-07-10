namespace Closing.Domain.Constants;

public struct CommissionRateBase
{
    
    public const int
        /// <summary>
        /// Base de 360 días, común en bancos y cálculo de intereses.
        /// </summary>
        Days360 = 360,
        /// <summary>
        ///  Base de 365 días, común en fondos o tasas anuales reales.
        /// </summary>
         Days365 = 365;   
}


public struct CommissionConcepts
{
    public const string
        Administrative = "Administración"
        ;
}