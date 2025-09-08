using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Reports.Domain.DailyClosing;
using Common.SharedKernel.Application.Helpers.Finance;
using Microsoft.Extensions.Options;
using Reports.Infrastructure.Options;
using Reports.Infrastructure.ConnectionFactory.Interfaces;

namespace Reports.Infrastructure.DailyClosing;

public class DailyClosingReportRepository(
    IReportsDbConnectionFactory dbConnectionFactory,
    IProfitabilityCalculator profitabilityCalculator,
    IOptions<ReportsOptions> options) : IDailyClosingReportRepository
{
    private readonly string ProductsSchema = string.IsNullOrWhiteSpace(options.Value.ProductsSchema) ? "productos" : options.Value.ProductsSchema!;
    private readonly string ClosingSchema = string.IsNullOrWhiteSpace(options.Value.ClosingSchema) ? "cierre" : options.Value.ClosingSchema!;
    private readonly string Rt1Keyword = string.IsNullOrWhiteSpace(options.Value.Rt1Keyword) ? "CLAVE123456" : options.Value.Rt1Keyword!;

    public async Task<Rt1Header> GetRt1HeaderAsync(int portfolioId, CancellationToken cancellationToken)
    {
        var sql = $@"
            -- Administrator data
            SELECT pr.metadata->>'tipo' AS ""EntityType"", a.codigo_entidad AS ""EntityCode""
            FROM {ProductsSchema}.portafolios po
            JOIN {ProductsSchema}.alternativas_portafolios apo ON apo.""PortfolioId"" = po.id
            JOIN {ProductsSchema}.alternativas alt ON alt.id = apo.""AlternativeId""
            JOIN {ProductsSchema}.planes_fondo pf ON pf.id = alt.planes_fondo_id
            JOIN {ProductsSchema}.fondos_voluntarios_pensiones pfu ON pfu.id = pf.fondo_id
            JOIN {ProductsSchema}.administradores a ON a.id = pfu.administrador_id
            JOIN {ProductsSchema}.parametros_configuracion pr ON pr.id = a.tipo_entidad
            WHERE po.id = @PortfolioId
            LIMIT 1;
        ";

        using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { PortfolioId = portfolioId }, cancellationToken: cancellationToken);
        var adminData = await connection.QueryFirstOrDefaultAsync<AdministratorDto>(command);
        if (adminData is null)
        {
            throw new InvalidOperationException($"No se encontraron datos de administrador para el portafolio {portfolioId}.");
        }

        return new Rt1Header(adminData.EntityType, adminData.EntityCode, Rt1Keyword);
    }

    public async Task<Rt2Header> GetRt2HeaderAsync(int portfolioId, CancellationToken cancellationToken)
    {
        var sql = $@"
            SELECT po.""cod_Negocio_SFC""::text AS ""PortfolioCode""
            FROM {ProductsSchema}.portafolios po
            WHERE po.id = @PortfolioId
            LIMIT 1;";

        using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { PortfolioId = portfolioId }, cancellationToken: cancellationToken);
        var header = await connection.QueryFirstOrDefaultAsync<Rt2Header>(command);
        if (header is null)
        {
            throw new InvalidOperationException($"No se encontró el código SFC (cod_Negocio_SFC) para el portafolio {portfolioId}.");
        }
        return header;
    }

    public async Task<decimal> GetUnitValueAsync(int portfolioId, DateTime date, CancellationToken cancellationToken)
    {
        var sql = $@"
            SELECT vp.valor_unidad AS ""UnitValue""
            FROM {ClosingSchema}.valoracion_portafolio vp
            WHERE vp.portafolio_id = @PortfolioId AND vp.fecha_cierre = @Date::date
            LIMIT 1;";

        using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { PortfolioId = portfolioId, Date = date }, cancellationToken: cancellationToken);
        var value = await connection.QueryFirstOrDefaultAsync<decimal?>(command);
        return value ?? 0m;
    }

    public async Task<Rt4ValuationMovements> GetValuationMovementsAsync(int portfolioId, DateTime date, CancellationToken cancellationToken)
    {
        var previousDate = date.AddDays(-1);
        const int contributionOperationTypeId = 1;

        var sql = $@"
            -- previous day valuation
            SELECT vp.unidades AS ""Units"", vp.valor AS ""Amount""
            FROM {ClosingSchema}.valoracion_portafolio vp
            WHERE vp.portafolio_id = @PortfolioId AND vp.fecha_cierre = @PreviousDate::date;

            -- yield credited
            SELECT COALESCE(SUM(r.rendimientos_abonados), 0)
            FROM {ClosingSchema}.rendimientos r
            WHERE r.portafolio_id = @PortfolioId AND r.fecha_cierre = @Date::date;

            -- contribution units
            SELECT COALESCE(SUM(tf.unidades), 0)
            FROM {ClosingSchema}.rendimientos_fideicomisos tf
            WHERE tf.portafolio_id = @PortfolioId AND tf.fecha_cierre = @Date::date
              AND (tf.participacion = 0 OR tf.participacion IS NULL);

            -- contribution amount
            SELECT COALESCE(SUM(oc.valor), 0)
            FROM {ClosingSchema}.operaciones_cliente oc
            WHERE oc.portafolio_id = @PortfolioId AND oc.fecha_proceso = @Date::date
              AND oc.tipo_operaciones_id = @ContributionTypeId;

            -- current day valuation
            SELECT vp.unidades AS ""Units"", vp.valor AS ""Amount""
            FROM {ClosingSchema}.valoracion_portafolio vp
            WHERE vp.portafolio_id = @PortfolioId AND vp.fecha_cierre = @Date::date;";

        using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, new
        {
            PortfolioId = portfolioId,
            Date = date,
            PreviousDate = previousDate,
            ContributionTypeId = contributionOperationTypeId
        }, cancellationToken: cancellationToken);

        using var reader = await connection.QueryMultipleAsync(command);
        var previous = await reader.ReadFirstOrDefaultAsync<ValuationDto>();
        var yieldAmount = await reader.ReadFirstAsync<decimal>();
        var contributionUnits = await reader.ReadFirstAsync<decimal>();
        var contributionAmount = await reader.ReadFirstAsync<decimal>();
        var current = await reader.ReadFirstOrDefaultAsync<ValuationDto>();

        return new Rt4ValuationMovements(
            previous?.Units ?? 0,
            previous?.Amount ?? 0,
            yieldAmount,
            contributionUnits,
            contributionAmount,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            current?.Units ?? 0,
            current?.Amount ?? 0);
    }

    public async Task<Rt4Profitabilities> GetProfitabilitiesAsync(int portfolioId, DateTime date, CancellationToken cancellationToken)
    {
        var date30 = date.AddDays(-29);
        var date180 = date.AddDays(-179);
        var date365 = date.AddDays(-364);

        var sql = $@"
            SELECT vp.rentabilidad_diaria
            FROM {ClosingSchema}.valoracion_portafolio vp
            WHERE vp.portafolio_id = @PortfolioId AND vp.fecha_cierre = @Date::date;

            SELECT vp.rentabilidad_diaria
            FROM {ClosingSchema}.valoracion_portafolio vp
            WHERE vp.portafolio_id = @PortfolioId AND vp.fecha_cierre = @Date30::date;

            SELECT vp.rentabilidad_diaria
            FROM {ClosingSchema}.valoracion_portafolio vp
            WHERE vp.portafolio_id = @PortfolioId AND vp.fecha_cierre = @Date180::date;

            SELECT vp.rentabilidad_diaria
            FROM {ClosingSchema}.valoracion_portafolio vp
            WHERE vp.portafolio_id = @PortfolioId AND vp.fecha_cierre = @Date365::date;";

        using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, new
        {
            PortfolioId = portfolioId,
            Date = date,
            Date30 = date30,
            Date180 = date180,
            Date365 = date365
        }, cancellationToken: cancellationToken);

        using var reader = await connection.QueryMultipleAsync(command);
        var current = await reader.ReadFirstOrDefaultAsync<decimal?>();
        var value30 = await reader.ReadFirstOrDefaultAsync<decimal?>();
        var value180 = await reader.ReadFirstOrDefaultAsync<decimal?>();
        var value365 = await reader.ReadFirstOrDefaultAsync<decimal?>();

        decimal Calc(decimal? f, decimal? i, int d)
            => (f is null || i is null) ? 0m : profitabilityCalculator.AnnualizedPercentage(f.Value, i.Value, d);

        var rent30 = Calc(current, value30, 30);
        var rent180 = Calc(current, value180, 180);
        var rent365 = Calc(current, value365, 365);

        return new Rt4Profitabilities(rent30, rent180, rent365);
    }

    private sealed record AdministratorDto(string EntityType, string EntityCode);
    private sealed record ValuationDto(decimal Units, decimal Amount);
}
