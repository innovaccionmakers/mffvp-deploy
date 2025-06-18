
namespace Products.Domain.Portfolios;

public class PortfolioInformation
{
    public string Found { get; set; }
    public string Plan { get; set; }
    public string Alternative { get; set; }
    public string Portfolio { get; set; }

    public PortfolioInformation()
    {
        
    }

    public static PortfolioInformation Create(
        string found,
        string plan,
        string alternative,
        string portfolio)
    {
        return new PortfolioInformation
        {
            Found = found,
            Plan = plan,
            Alternative = alternative,
            Portfolio = portfolio
        };
    }
}
