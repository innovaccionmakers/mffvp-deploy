
namespace Products.Domain.Portfolios;

public class PortfolioInformation
{
    public string Found { get; set; }
    public int FoundId { get; set; }
    public string Plan { get; set; }
    public int PlanId { get; set; }
    public string Alternative { get; set; }
    public int AlternativeId { get; set; }
    public string Portfolio { get; set; }
    public int PortfolioId { get; set; }

    public PortfolioInformation()
    {
        
    }

    public static PortfolioInformation Create(
        string found,
        int foundId,
        string plan,
        int planId,
        string alternative,
        int alternativeId,
        string portfolio,
        int portfolioId)
    {
        return new PortfolioInformation
        {
            Found = found,
            FoundId = foundId,
            Plan = plan,
            PlanId = planId,
            Alternative = alternative,
            AlternativeId = alternativeId,
            Portfolio = portfolio,
            PortfolioId = portfolioId
        };
    }
}
