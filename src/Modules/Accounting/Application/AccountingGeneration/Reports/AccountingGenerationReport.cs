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
                                                     IEnumerable<AccountingAssistant> accountingAssistants,
                                                     IEnumerable<Consecutive> consecutives,
                                                     CancellationToken cancellationToken)
    {
        var consecutiveByNature = consecutives.ToDictionary(c => c.Nature, c => c);

        var currentConsecutiveByNature = consecutives.ToDictionary(c => c.Nature, c => c.Number);

        var lastShownConsecutiveByNature = consecutives.ToDictionary(c => c.Nature, c => c.Number);

        var groupedByNature = accountingAssistants.GroupBy(x => x.Nature).OrderBy(g => g.Key);

        var rows = new List<object[]>();

        foreach (var natureGroup in groupedByNature)
        {
            var nature = natureGroup.Key;
            var consecutive = consecutiveByNature.GetValueOrDefault(nature);

            if (consecutive == null)
                continue;

            var sourceDocument = consecutive.SourceDocument;

            foreach (var accountingAssistant in natureGroup)
            {
                var currentConsecutive = currentConsecutiveByNature[nature];

                rows.Add(new object[]
                {
                    sourceDocument,
                    currentConsecutive,
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
                });

                lastShownConsecutiveByNature[nature] = currentConsecutive;

                currentConsecutiveByNature[nature] = currentConsecutive + 1;
            }
        }

        await UpdateConsecutivesInDatabaseAsync(lastShownConsecutiveByNature, cancellationToken);

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

    private async Task UpdateConsecutivesInDatabaseAsync(Dictionary<string, int> consecutiveNumbersByNature, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await consecutiveRepository.UpdateConsecutivesByNatureAsync(consecutiveNumbersByNature, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
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
