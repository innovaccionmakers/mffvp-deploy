

namespace Closing.Application.Closing.Services.Validation.Dto;
public readonly record struct ConfigFlags(
       bool HasWithholdingPercentage,     // existe PorcentajeRetencionRendimientos
       bool WithholdingPercentageIsValid, // metadata.valor > 0
       bool HasYieldTolerance,            // existe ToleranciaRendimientos
       bool YieldToleranceIsValid,        // metadata.valor > 0
       bool HasAutomaticConceptIncome,    // existe Ajuste Rendimiento Ingreso
       bool HasAutomaticConceptExpense    // existe Ajuste Rendimiento Gasto
   );