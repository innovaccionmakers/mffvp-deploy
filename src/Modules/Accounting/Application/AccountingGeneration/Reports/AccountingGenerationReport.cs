using Accounting.Application.Abstractions.Data;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.ConfigurationGenerals;
using Accounting.Domain.Constants;
using Accounting.Domain.ConsecutiveFiles;
using Accounting.Domain.Consecutives;
using Common.SharedKernel.Application.Reports.Strategies;
using Common.SharedKernel.Core.Formatting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingGeneration.Reports;

public class AccountingGenerationReport(ILogger<AccountingGenerationReport> logger,
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
                                                     IEnumerable<GeneralConfiguration> generalConfigurations,
                                                     CancellationToken cancellationToken)
    {
        var configurationByPortfolioId = generalConfigurations
            .ToDictionary(gc => gc.PortfolioId, gc => gc.AccountingCode);

        var incomeConsecutive = consecutives.FirstOrDefault(c => c.Nature == NatureTypes.Income);
        var egressConsecutive = consecutives.FirstOrDefault(c => c.Nature == NatureTypes.Egress);

        var incomeGroups = incomeAssistants
            .GroupBy(a => a.Identifier)
            .OrderBy(g => g.Key)
            .ToList();

        var egressGroups = egressAssistants
            .GroupBy(a => a.Identifier)
            .OrderBy(g => g.Key)
            .ToList();

        var uniqueIncomeCount = incomeGroups.Count;
        var uniqueEgressCount = egressGroups.Count;

        var lastIncomeConsecutive = incomeConsecutive != null && uniqueIncomeCount > 0
            ? incomeConsecutive.Number + (uniqueIncomeCount - 1)
            : incomeConsecutive?.Number ?? 0;

        var lastEgressConsecutive = egressConsecutive != null && uniqueEgressCount > 0
            ? egressConsecutive.Number + (uniqueEgressCount - 1)
            : egressConsecutive?.Number ?? 0;

        var rows = new List<object[]>();

        // Procesar registros de Ingreso agrupados por Identifier
        if (incomeConsecutive != null && incomeGroups.Count > 0)
        {
            var currentConsecutive = incomeConsecutive.Number;
            var sourceDocument = incomeConsecutive.SourceDocument;

            for (int i = 0; i < incomeGroups.Count; i++)
            {
                var group = incomeGroups[i];
                var consecutiveNumber = currentConsecutive + i;
                foreach (var accountingAssistant in group)
                {
                    var accountingCode = configurationByPortfolioId.GetValueOrDefault(accountingAssistant.PortfolioId, string.Empty);
                    rows.Add(CreateRow(sourceDocument, consecutiveNumber, accountingAssistant, accountingCode));
                }
            }
        }

        // Procesar registros de Egreso agrupados por Identifier
        if (egressConsecutive != null && egressGroups.Count > 0)
        {
            var currentConsecutive = egressConsecutive.Number;
            var sourceDocument = egressConsecutive.SourceDocument;

            for (int i = 0; i < egressGroups.Count; i++)
            {
                var group = egressGroups[i];
                var consecutiveNumber = currentConsecutive + i;
                foreach (var accountingAssistant in group)
                {
                    var accountingCode = configurationByPortfolioId.GetValueOrDefault(accountingAssistant.PortfolioId, string.Empty);
                    rows.Add(CreateRow(sourceDocument, consecutiveNumber, accountingAssistant, accountingCode));
                }
            }
        }

        await UpdateConsecutivesInDatabaseAsync(lastIncomeConsecutive, lastEgressConsecutive, cancellationToken);

        var fileName = await GenerateReportFileNameAsync(processDate, cancellationToken);

        var columnConfigurations = GetColumnConfigurations();

        var textReportDataList = new List<TextReportData>
        {
            new()
            {
                SectionTitle = string.Empty,
                ColumnHeaders = ColumnHeaders,
                IncludeHeaders = false,
                Rows = rows,
                ColumnConfigurations = columnConfigurations
            }
        };

        return await GenerateTextReportAsync(textReportDataList, fileName, cancellationToken);
    }

    private List<ColumnConfiguration> GetColumnConfigurations()
    {
        return new List<ColumnConfiguration>
        {
            new(4, ColumnAlignment.Center),
            new(7, ColumnAlignment.Left, ' '),
            new(2, ColumnAlignment.Center),
            new(8, ColumnAlignment.Center),
            new(12, ColumnAlignment.Center),
            new(12, ColumnAlignment.Center),
            new(13, ColumnAlignment.Left, ' '),
            new(1, ColumnAlignment.Center),
            new(50, ColumnAlignment.Left, ' '),
            new(4, ColumnAlignment.Left, ' '),
            new(10, ColumnAlignment.Center),
            new(2, ColumnAlignment.Left, ' '),
            new(2, ColumnAlignment.Left, ' '),
            new(8, ColumnAlignment.Left, ' '),
            new(8, ColumnAlignment.Left, ' '),
            new(18, ColumnAlignment.Right, '0'),
            new(18, ColumnAlignment.Right, '0'),
            new(10, ColumnAlignment.Left, '0'),
            new(60, ColumnAlignment.Right, '0'),
            new(1, ColumnAlignment.Center),
            new(19, ColumnAlignment.Left, ' '),
        };
    }

    private object[] CreateRow(string sourceDocument, int consecutiveNumber, AccountingAssistant accountingAssistant, string accountingCode)
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

        return
        [
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
            accountingCode,
            accountingAssistant.Detail ?? "",
            AccountingReportConstants.NDOINT,
        ];
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
