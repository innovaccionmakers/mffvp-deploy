using Accounting.Application.Abstractions.Data;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.ConsecutiveFiles;
using Common.SharedKernel.Application.Reports.Strategies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingGeneration.Reports;

public class AccountingGenerationReport(ILogger<AccountingGenerationReport> logger,
                                        IAccountingAssistantRepository accountingAssistantRepository,
                                        IConsecutiveFileRepository consecutiveFileRepository,
                                        IUnitOfWork unitOfWork) : TextReportStrategyBase(logger)
{
    public override string ReportName => "E";

    public override string[] ColumnHeaders => [];

    protected override string GetFieldSeparator() => " ";

    protected override string NormalizeText(string text)
    {
        return RemoveAccentsAndNormalize(text);
    }

    public async override Task<IActionResult> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        if (request is DateTime processDate)
        {
            try
            {
                return await GenerateTextReportAsync(
                       ct => GetTextReportDataAsync(ct),
                       await GenerateReportFileNameAsync(processDate, cancellationToken),
                       cancellationToken);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al generar el reporte de generación contable");
                throw;
            }
        }
        else
        {
            logger.LogError("Tipo de request no válido. Se esperaba DateTime, se recibió: {RequestType}",
                typeof(TRequest).Name);
            throw new ArgumentException("El tipo de request no es válido. Se esperaba DateTime.");
        }
    }

    private async Task<List<TextReportData>> GetTextReportDataAsync(CancellationToken cancellationToken)
    {
        var textReportDataList = new List<TextReportData>();

        var data = await accountingAssistantRepository.GetAllAsync(cancellationToken);

        var rows = data?.Select(x => new object[]
        {
            x.AccountingAssistantId,
            x.PortfolioId,
            x.Identification,
            x.VerificationDigit,
            x.Name,
            x.Period,
            x.Account ?? "",
            x.Date,
            x.Detail ?? "",
            x.Type,
            x.Value,
            x.Nature,
            x.Identifier,
        }).ToList();

        var result = new TextReportData
        {
            SectionTitle = string.Empty,
            ColumnHeaders = ColumnHeaders,
            IncludeHeaders = false,
            Rows = rows ?? []
        };

        textReportDataList.Add(result);

        return textReportDataList;
    }

    private async Task<string> GenerateReportFileNameAsync(DateTime processDate, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var (_, consecutive) = await consecutiveFileRepository.GetOrCreateNextConsecutiveForTodayAsync(
            processDate,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        string fileName = $"{ReportName}{processDate:ddMMyyyy}{consecutive:D3}.txt";
        return fileName;
    }
}
