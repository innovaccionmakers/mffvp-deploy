using System.ComponentModel;

namespace Reports.Domain.BalancesAndMovements
{
    public enum WorksheetName
    {
        [Description("Saldos")]
        Balances,

        [Description("Movimientos")]
        Movements,

        [Description("Ficha Técnica")]
        TechnicalSheet
    }

    public static class WorksheetNameExtensions
    {
        public static string GetDescription(this WorksheetName worksheetName)
        {
            var field = worksheetName.GetType().GetField(worksheetName.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute?.Description ?? worksheetName.ToString();
        }
    }
}
