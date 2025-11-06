using Accounting.Application.Abstractions.Data;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Domain.ConsecutiveFiles;
using Accounting.Domain.Consecutives;
using Common.SharedKernel.Application.Reports.Strategies;
using Common.SharedKernel.Core.Formatting;
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
        var incomeAssistantsList = incomeAssistants.ToList();
        var egressAssistantsList = egressAssistants.ToList();

        var incomeConsecutive = consecutives.FirstOrDefault(c => c.Nature == NatureTypes.Income);
        var egressConsecutive = consecutives.FirstOrDefault(c => c.Nature == NatureTypes.Egress);

        var lastIncomeConsecutive = incomeConsecutive != null && incomeAssistantsList.Count > 0
            ? incomeConsecutive.Number + (incomeAssistantsList.Count - 1)
            : incomeConsecutive?.Number ?? 0;

        var lastEgressConsecutive = egressConsecutive != null && egressAssistantsList.Count > 0
            ? egressConsecutive.Number + (egressAssistantsList.Count - 1)
            : egressConsecutive?.Number ?? 0;

        var rows = new List<object[]>();

        // Procesar registros de Ingreso
        if (incomeConsecutive != null && incomeAssistantsList.Count > 0)
        {
            var currentConsecutive = incomeConsecutive.Number;
            var sourceDocument = incomeConsecutive.SourceDocument;

            for (int i = 0; i < incomeAssistantsList.Count; i++)
            {
                var accountingAssistant = incomeAssistantsList[i];
                var consecutiveNumber = currentConsecutive + i;

                rows.Add(CreateRow(sourceDocument, consecutiveNumber, accountingAssistant));
            }
        }

        // Procesar registros de Egreso
        if (egressConsecutive != null && egressAssistantsList.Count > 0)
        {
            var currentConsecutive = egressConsecutive.Number;
            var sourceDocument = egressConsecutive.SourceDocument;

            for (int i = 0; i < egressAssistantsList.Count; i++)
            {
                var accountingAssistant = egressAssistantsList[i];
                var consecutiveNumber = currentConsecutive + i;

                rows.Add(CreateRow(sourceDocument, consecutiveNumber, accountingAssistant));
            }
        }

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
        var DBAINT = string.Empty;
        if (accountingAssistant.Detail != OperationTypeNames.Yield)
        {
            DBAINT = accountingAssistant.Nature == NatureTypes.Income
                ? AccountingReportConstants.IncomeCode
                : AccountingReportConstants.EgressCode;
        }

        var creditValue = accountingAssistant.Type == AccountingTypes.Credit
            ? FixedWidthTextFormatter.FormatNumber(accountingAssistant.Value, 18, 2)
            : AccountingReportConstants.ZeroValue;

        var debitValue = accountingAssistant.Type == AccountingTypes.Debit
            ? FixedWidthTextFormatter.FormatNumber(accountingAssistant.Value, 18, 2)
            : AccountingReportConstants.ZeroValue;

        return new object[]
        {
            sourceDocument,
            consecutiveNumber,
            AccountingReportConstants.FORINT,
            accountingAssistant.Date.ToString("yyyymmdd"),
            accountingAssistant.Account ?? "",
            AccountingReportConstants.CENINT,
            accountingAssistant.Identification,
            accountingAssistant.VerificationDigit,
            accountingAssistant.Name,
            DBAINT,
            AccountingReportConstants.VBAINT,
            AccountingReportConstants.NVBINT,
            AccountingReportConstants.FEMINT,
            AccountingReportConstants.FVEINT,
            creditValue,
            debitValue,
            AccountingReportConstants.ZeroValue,
            AccountingReportConstants.ZeroValue,
            accountingAssistant.Detail ?? "",
            AccountingReportConstants.NDOINT,
        };
    }

    private async Task UpdateConsecutivesInDatabaseAsync(int lastIncomeConsecutive, int lastEgressConsecutive, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await consecutiveRepository.UpdateIncomeConsecutiveAsync(lastIncomeConsecutive, cancellationToken);

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
