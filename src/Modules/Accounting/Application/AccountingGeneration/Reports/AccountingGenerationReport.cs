using Accounting.Application.Abstractions.Data;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Domain.ConsecutiveFiles;
using Accounting.Domain.Consecutives;
using Common.SharedKernel.Application.Reports.Strategies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Accounting.Application.AccountingGeneration.Reports;

public class AccountingGenerationReport(ILogger<AccountingGenerationReport> logger,
                                        IAccountingAssistantRepository accountingAssistantRepository,
                                        IConsecutiveFileRepository consecutiveFileRepository,
                                        IConsecutiveRepository consecutiveRepository,
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
        throw new NotImplementedException();

    }

    public async Task<FileStreamResult> GenerateReportAsync(DateTime processDate,
                                                     IReadOnlyCollection<AccountingAssistant> incomeAssistants,
                                                     IReadOnlyCollection<AccountingAssistant> egressAssistants,
                                                     IEnumerable<Consecutive> consecutives,
                                                     CancellationToken cancellationToken)
    {
        // Obtener consecutivos de Ingreso y Egreso
        var incomeConsecutive = consecutives.FirstOrDefault(c => c.Nature == NatureTypes.Income);
        var egressConsecutive = consecutives.FirstOrDefault(c => c.Nature == NatureTypes.Egress);

        // Calcular últimos consecutivos: consecutivo actual + total de registros - 1
        var lastIncomeConsecutive = incomeConsecutive != null && incomeAssistants.Count > 0
            ? incomeConsecutive.Number + (incomeAssistants.Count - 1)
            : incomeConsecutive?.Number ?? 0;

        var lastEgressConsecutive = egressConsecutive != null && egressAssistants.Count > 0
            ? egressConsecutive.Number + (egressAssistants.Count - 1)
            : egressConsecutive?.Number ?? 0;

        var rows = new List<object[]>();

        // Procesar registros de Ingreso
        if (incomeConsecutive != null && incomeAssistants.Count > 0)
        {
            var currentConsecutive = incomeConsecutive.Number;
            var sourceDocument = incomeConsecutive.SourceDocument;

            for (int i = 0; i < incomeAssistants.Count; i++)
            {
                var accountingAssistant = incomeAssistants[i];
                var consecutiveNumber = currentConsecutive + i;

                rows.Add(CreateRow(sourceDocument, consecutiveNumber, accountingAssistant));
            }
        }

        // Procesar registros de Egreso
        if (egressConsecutive != null && egressAssistants.Count > 0)
        {
            var currentConsecutive = egressConsecutive.Number;
            var sourceDocument = egressConsecutive.SourceDocument;

            for (int i = 0; i < egressAssistants.Count; i++)
            {
                var accountingAssistant = egressAssistants[i];
                var consecutiveNumber = currentConsecutive + i;

                rows.Add(CreateRow(sourceDocument, consecutiveNumber, accountingAssistant));
            }
        }

        // Actualizar consecutivos en BD
        await UpdateConsecutivesInDatabaseAsync(lastIncomeConsecutive, lastEgressConsecutive, cancellationToken);

        var fileName = await GenerateReportFileNameAsync(processDate, cancellationToken);

        var textReportDataList = new List<TextReportData>
        {
            new()
            {
                SectionTitle = string.Empty,
                ColumnHeaders = ColumnHeaders,
                IncludeHeaders = false,
                Rows = rows
            }
        };

        return await GenerateTextReportAsync(textReportDataList, fileName, cancellationToken);
    }

    private object[] CreateRow(string sourceDocument, int consecutiveNumber, AccountingAssistant accountingAssistant)
    {
        return new object[]
        {
            sourceDocument,
            consecutiveNumber,
            AccountingReportConstants.FORINT,
            accountingAssistant.Date.ToString("yyyymmdd"),
            "TODO",
            AccountingReportConstants.CENINT,
            accountingAssistant.Identification,
            accountingAssistant.VerificationDigit,
            accountingAssistant.Name,
            "TODO",
            AccountingReportConstants.VBAINT,
            AccountingReportConstants.NVBINT,
            AccountingReportConstants.FEMINT,
            AccountingReportConstants.FVEINT,
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO VALOR",
            "TODO",
            accountingAssistant.Detail ?? "",
            " ",
            AccountingReportConstants.NDOINT,
        };
    }

    private async Task UpdateConsecutivesInDatabaseAsync(int lastIncomeConsecutive, int lastEgressConsecutive, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Actualizar consecutivo de Ingreso
            await consecutiveRepository.UpdateIncomeConsecutiveAsync(lastIncomeConsecutive, cancellationToken);

            // Actualizar consecutivo de Egreso
            await consecutiveRepository.UpdateEgressConsecutiveAsync(lastEgressConsecutive, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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
