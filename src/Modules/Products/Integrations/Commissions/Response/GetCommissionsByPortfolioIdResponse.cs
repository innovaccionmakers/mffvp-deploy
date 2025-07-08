
namespace Products.Integrations.Commissions.Response;
public sealed record GetCommissionsByPortfolioIdResponse(
    int CommissionId,
    string Concept,
    string Modality,
    string CommissionType,
    string Period,
    string CalculationBase,
    string CalculationRule
    );