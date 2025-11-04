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

        var rows = accountingAssistants.Select(x =>
        {
            var consecutiveNature = x.Nature == NatureTypes.Expense ? NatureTypes.Egress : NatureTypes.Income;

            var consecutive = consecutiveByNature.GetValueOrDefault(consecutiveNature);

            var sourceDocument = consecutive?.SourceDocument ?? "";
            var consecutiveNumber = consecutive?.Number ?? 0;

            return new object[]
            {
                sourceDocument,
                consecutiveNumber,
                AccountingReportConstants.FORINT,
                x.Date.ToString("yyyymmdd"),
                "TODO",
                AccountingReportConstants.CENINT,
                x.Identification,
                x.VerificationDigit,
                x.Name,
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
                "TODO",
                x.Detail ?? "",
                " ",
                AccountingReportConstants.NDOINT,
            };
        }).ToList();

        var fileName = await GenerateReportFileNameAsync(processDate, cancellationToken);

        var textReportDataList = new List<TextReportData>
        {
            new()
            {

                SectionTitle = string.Empty,
                ColumnHeaders = ColumnHeaders,
                IncludeHeaders = false,
                Rows = rows ?? []
            }
        };

        return await GenerateTextReportAsync(textReportDataList, fileName, cancellationToken);
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
