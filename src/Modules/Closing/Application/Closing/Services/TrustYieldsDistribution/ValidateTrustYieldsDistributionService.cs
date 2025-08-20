using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.TrustYields;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Closing.Application.PreClosing.Services.Yield.Dto;
using Common.SharedKernel.Core.Primitives;

namespace Closing.Application.Closing.Services.TrustYieldsDistribution;

public class ValidateTrustYieldsDistributionService(
    IYieldRepository yieldRepository,
    ITrustYieldRepository trustYieldRepository,
    IYieldDetailCreationService yieldDetailCreationService,
    ITimeControlService timeControlService,
    ILogger<ValidateTrustYieldsDistributionService> logger)
    : IValidateTrustYieldsDistributionService
{
    public async Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        using var _ = logger.BeginScope(new System.Collections.Generic.Dictionary<string, object>
        {
            ["Service"] = "ValidateTrustYieldsDistributionService",
            ["PortfolioId"] = portfolioId,
            ["ClosingDate"] = closingDate.Date
        });
        const string svc = "[ValidateTrustYieldsDistributionService]";

        logger.LogInformation("Validando distribución de rendimientos para portafolio {PortfolioId}", portfolioId);
        logger.LogInformation("{Svc} Inicio de validación de distribución de rendimientos.", svc);

        var now = DateTime.UtcNow;

        // Publicar evento de paso
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingAllocationCheck", now, ct);
        logger.LogDebug("{Svc} Paso de control de tiempo actualizado: NowUtc={NowUtc}", svc, now);

        var yield = await yieldRepository.GetByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (yield is null)
        {
            logger.LogWarning("{Svc} No se encontró información de rendimientos para {ClosingDate}", svc, closingDate);
            return Result.Failure(new Error("001", "No se encontró información de rendimientos para la fecha de cierre.", ErrorType.Failure));
        }
        logger.LogDebug("{Svc} Yield obtenido: Income={Income}, Expenses={Expenses}, Commissions={Commissions}, Costs={Costs}, YieldToCredit={YieldToCredit}",
            svc, yield.Income, yield.Expenses, yield.Commissions, yield.Costs, yield.YieldToCredit);

        var trustYields = await trustYieldRepository.GetByPortfolioAndDateAsync(portfolioId, closingDate, ct);
        if (!trustYields.Any())
        {
            logger.LogWarning("{Svc} No existen rendimientos distribuidos para fideicomisos en {ClosingDate}", svc, closingDate);
            return Result.Failure(new Error("002", "No existen registros de rendimientos distribuidos para fideicomisos.", ErrorType.Failure));
        }
        logger.LogInformation("{Svc} Registros de trust_yields encontrados: {Count}", svc, trustYields.Count);

        var distributedTotal = trustYields.Sum(x => x.YieldAmount);
        var expectedTotal = yield.YieldToCredit;
        var difference = distributedTotal - expectedTotal;

        logger.LogInformation("Distribuido: {Distribuido}, Esperado: {Esperado}, Diferencia: {Diferencia}",
            distributedTotal, expectedTotal, difference);
        logger.LogDebug("{Svc} Totales calculados => DistributedTotal={DistributedTotal}, ExpectedTotal={ExpectedTotal}, Difference={Difference}",
            svc, distributedTotal, expectedTotal, difference);

        logger.LogDebug("{Svc} Actualizando Yield con creditedYields={CreditedYields}", svc, distributedTotal);
        yield.UpdateDetails(
            portfolioId: yield.PortfolioId,
            income: yield.Income,
            expenses: yield.Expenses,
            commissions: yield.Commissions,
            costs: yield.Costs,
            yieldToCredit: yield.YieldToCredit,
            creditedYields: distributedTotal,
            closingDate: yield.ClosingDate,
            processDate: yield.ProcessDate,
            isClosed: yield.IsClosed
        );

        await yieldRepository.SaveChangesAsync(ct);
        logger.LogInformation("{Svc} Cambios persistidos en yields.", svc);

        if (difference != 0)
        {
            logger.LogInformation("{Svc} Se detectó diferencia distinta de 0. Se generará ajuste.", svc);

            var nextClosingDate = closingDate.AddDays(1);
            logger.LogDebug("{Svc} Próxima fecha de cierre para ajuste: {NextClosingDate}", svc, nextClosingDate);

            var conceptText = difference > 0
             ? "Ajuste Rendimiento Ingreso"
             : "Ajuste Rendimiento Gasto";

            var conceptId = difference > 0
            ? "1"
            : "2";

            logger.LogDebug("{Svc} Concepto seleccionado: Id={ConceptId}, Texto={ConceptText}", svc, conceptId, conceptText);

            var dto = new StringEntityDto(
                EntityId: conceptId,
                EntityValue: conceptText
            );
            using var conceptJson = JsonSerializer.SerializeToDocument(dto);
            logger.LogDebug("{Svc} Concepto serializado a JSON para YieldDetail.", svc);

            var yieldDetailResult = YieldDetail.Create(
                portfolioId: portfolioId,
                closingDate: nextClosingDate,
                source: "Concepto Automático",
                concept: conceptJson,
                income: difference,
                expenses: 0,
                commissions: 0,
                processDate: now,
                isClosed: true
            );

            if (yieldDetailResult.IsFailure)
            {
                logger.LogError("{Svc} Error creando YieldDetail: {Code} {Description}", svc, yieldDetailResult.Error.Code, yieldDetailResult.Error.Description);
                return Result.Failure(yieldDetailResult.Error);
            }

            logger.LogDebug("{Svc} YieldDetail creado correctamente. Ejecutando creación en servicio.", svc);
            await yieldDetailCreationService.CreateYieldDetailsAsync(new[] { yieldDetailResult.Value }, ct);

            logger.LogInformation("Se generó ajuste de rendimiento para el portafolio {PortfolioId} en el día siguiente", portfolioId);
            logger.LogInformation("{Svc} Ajuste generado y persistido: Difference={Difference}, NextClosingDate={NextClosingDate}", svc, difference, nextClosingDate);
        }
        else
        {
            logger.LogInformation("No se detectaron diferencias en rendimientos distribuidos para portafolio {PortfolioId}", portfolioId);
            logger.LogInformation("{Svc} Sin diferencias: no se genera ajuste.", svc);
        }

        logger.LogInformation("{Svc} Validación de distribución finalizada correctamente.", svc);
        return Result.Success();
    }
}
