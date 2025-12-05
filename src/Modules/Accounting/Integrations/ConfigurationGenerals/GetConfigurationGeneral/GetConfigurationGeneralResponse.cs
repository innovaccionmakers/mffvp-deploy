namespace Accounting.Integrations.ConfigurationGenerals.GetConfigurationGeneral
{
    public sealed record class GetConfigurationGeneralResponse(
            long Id,
            string AccountingCode,
            string CostCenter
        );
}
