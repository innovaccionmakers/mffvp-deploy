using Common.SharedKernel.Domain;

namespace Accounting.Domain.ConfigurationGenerals;

public class GeneralConfiguration : Entity
{
    public long Id { get; private set; }
    public int PortfolioId { get; private set; }
    public string AccountingCode { get; private set; }
    public string CostCenter { get; private set; }

    private GeneralConfiguration()
    {
    }

    public static Result<GeneralConfiguration> Create(
        int portfolioId,
        string accountingCode,
        string costCenter)
    {
        var configuration = new GeneralConfiguration
        {
            Id = default,
            PortfolioId = portfolioId,
            AccountingCode = accountingCode,
            CostCenter = costCenter
        };
        return Result.Success(configuration);
    }

    public void UpdateDetails(
        int portfolioId,
        string accountingCode,
        string costCenter)
    {
        PortfolioId = portfolioId;
        AccountingCode = accountingCode;
        CostCenter = costCenter;
    }
}

