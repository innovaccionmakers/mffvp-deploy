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

    protected override string GetFieldSeparator() => "";

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
                                                     IReadOnlyCollection<AccountingAssistant> yieldAssistants,
                                                     IReadOnlyCollection<AccountingAssistant> conceptsAssistants,
                                                     IEnumerable<Consecutive> consecutives,
                                                     IEnumerable<GeneralConfiguration> generalConfigurations,
                                                     CancellationToken cancellationToken)
    {
        var configurationByPortfolioId = generalConfigurations
            .ToDictionary(gc => gc.PortfolioId, gc => gc.AccountingCode);

        var centerCostByPortfolioId = generalConfigurations
            .ToDictionary(gc => gc.PortfolioId, gc => gc.CostCenter);

        var incomeConsecutive = consecutives.FirstOrDefault(c => c.Nature == NatureTypes.Income);
        var egressConsecutive = consecutives.FirstOrDefault(c => c.Nature == NatureTypes.Egress);
        var yieldConsecutive = consecutives.FirstOrDefault(c => c.Nature == NatureTypes.Yields);
        var conceptConsecutive = consecutives.FirstOrDefault(c => c.Nature == NatureTypes.Concept);

        var incomeGroups = incomeAssistants
            .GroupBy(a => a.Identifier)
            .OrderBy(g => g.Key)
            .ToList();

        var egressGroups = egressAssistants
            .GroupBy(a => a.Identifier)
            .OrderBy(g => g.Key)
            .ToList();

        var yieldGroups = yieldAssistants
            .GroupBy(a => a.Identifier)
            .OrderBy(g => g.Key)
            .ToList();

        var conceptGroups = conceptsAssistants
            .GroupBy(a => a.Identifier)
            .OrderBy(g => g.Key)
            .ToList();

        var uniqueIncomeCount = incomeGroups.Count;
        var uniqueEgressCount = egressGroups.Count;
        var uniqueYieldCount = yieldGroups.Count;
        var uniqueConceptCount = conceptGroups.Count;

        var lastIncomeConsecutive = incomeConsecutive != null && uniqueIncomeCount > 0
            ? incomeConsecutive.Number + uniqueIncomeCount
            : incomeConsecutive?.Number ?? 0;

        var lastEgressConsecutive = egressConsecutive != null && uniqueEgressCount > 0
            ? egressConsecutive.Number + uniqueEgressCount
            : egressConsecutive?.Number ?? 0;

        var lastYieldConsecutive = yieldConsecutive != null && uniqueYieldCount > 0
            ? yieldConsecutive.Number + uniqueYieldCount
            : yieldConsecutive?.Number ?? 0;

        var lastConceptConsecutive = conceptConsecutive != null && uniqueConceptCount > 0
            ? conceptConsecutive.Number + uniqueConceptCount
            : conceptConsecutive?.Number ?? 0;

        var rows = new List<object[]>();

        // Procesar registros de Ingreso agrupados por Identifier
        if (incomeConsecutive != null && incomeGroups.Count > 0)
        {
            var startConsecutive = incomeConsecutive.Number;
            var sourceDocument = incomeConsecutive.SourceDocument;

            for (int i = 0; i < incomeGroups.Count; i++)
            {
                var group = incomeGroups[i];
                var consecutiveNumber = startConsecutive + (i + 1);
                foreach (var accountingAssistant in group)
                {
                    var accountingCode = configurationByPortfolioId.GetValueOrDefault(accountingAssistant.PortfolioId, string.Empty);
                    var cenint = centerCostByPortfolioId.GetValueOrDefault(accountingAssistant.PortfolioId, string.Empty);
                    rows.Add(CreateRow(sourceDocument, consecutiveNumber, accountingAssistant, accountingCode, cenint));
                }
            }
        }

        // Procesar registros de Egreso agrupados por Identifier
        if (egressConsecutive != null && egressGroups.Count > 0)
        {
            var startConsecutive = egressConsecutive.Number;
            var sourceDocument = egressConsecutive.SourceDocument;

            for (int i = 0; i < egressGroups.Count; i++)
            {
                var group = egressGroups[i];
                var consecutiveNumber = startConsecutive + (i + 1);
                foreach (var accountingAssistant in group)
                {
                    var accountingCode = configurationByPortfolioId.GetValueOrDefault(accountingAssistant.PortfolioId, string.Empty);
                    var cenint = centerCostByPortfolioId.GetValueOrDefault(accountingAssistant.PortfolioId, string.Empty);
                    rows.Add(CreateRow(sourceDocument, consecutiveNumber, accountingAssistant, accountingCode, cenint));
                }
            }
        }

        // Procesar registros de Rendimientos agrupados por Identifier
        if (yieldConsecutive != null && yieldGroups.Count > 0)
        {
            var startConsecutive = yieldConsecutive.Number;
            var sourceDocument = yieldConsecutive.SourceDocument;

            for (int i = 0; i < yieldGroups.Count; i++)
            {
                var group = yieldGroups[i];
                var consecutiveNumber = startConsecutive + (i + 1);
                foreach (var accountingAssistant in group)
                {
                    var accountingCode = configurationByPortfolioId.GetValueOrDefault(accountingAssistant.PortfolioId, string.Empty);
                    var cenint = centerCostByPortfolioId.GetValueOrDefault(accountingAssistant.PortfolioId, string.Empty);
                    rows.Add(CreateRow(sourceDocument, consecutiveNumber, accountingAssistant, accountingCode, cenint));
                }
            }
        }

        // Procesar registros de Conceptos agrupados por Identifier
        if (conceptConsecutive != null && conceptGroups.Count > 0)
        {
            var startConsecutive = conceptConsecutive.Number;
            var sourceDocument = conceptConsecutive.SourceDocument;

            for (int i = 0; i < conceptGroups.Count; i++)
            {
                var group = conceptGroups[i];
                var consecutiveNumber = startConsecutive + (i + 1);
                foreach (var accountingAssistant in group)
                {
                    var accountingCode = configurationByPortfolioId.GetValueOrDefault(accountingAssistant.PortfolioId, string.Empty);
                    var cenint = centerCostByPortfolioId.GetValueOrDefault(accountingAssistant.PortfolioId, string.Empty);
                    rows.Add(CreateRow(sourceDocument, consecutiveNumber, accountingAssistant, accountingCode, cenint));
                }
            }
        }

        await UpdateConsecutivesInDatabaseAsync(lastIncomeConsecutive, lastEgressConsecutive, lastYieldConsecutive, lastConceptConsecutive, cancellationToken);

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
        return
        [
            new(4, ColumnAlignment.Left, ' '),
            new(1, ColumnAlignment.Center),
            new(7, ColumnAlignment.Left, ' '),
            new(2, ColumnAlignment.Center),
            new(1, ColumnAlignment.Center),
            new(8, ColumnAlignment.Center),
            new(12, ColumnAlignment.Left, ' '),
            new(12, ColumnAlignment.Center),
            new(1, ColumnAlignment.Center),
            new(13, ColumnAlignment.Left, ' '),
            new(1, ColumnAlignment.Center),
            new(1, ColumnAlignment.Center),
            new(50, ColumnAlignment.Left, ' '),
            new(4, ColumnAlignment.Left, ' '),
            new(10, ColumnAlignment.Center),
            new(1, ColumnAlignment.Center),
            new(2, ColumnAlignment.Left, ' '),
            new(1, ColumnAlignment.Center),
            new(2, ColumnAlignment.Left, ' '),
            new(1, ColumnAlignment.Center),
            new(8, ColumnAlignment.Left, ' '),
            new(1, ColumnAlignment.Center),
            new(8, ColumnAlignment.Left, ' '),
            new(1, ColumnAlignment.Center),
            new(18, ColumnAlignment.Right, '0', PaddingSide.Left),
            new(1, ColumnAlignment.Center),
            new(18, ColumnAlignment.Right, '0', PaddingSide.Left),
            new(1, ColumnAlignment.Center),
            new(18, ColumnAlignment.Center),
            new(4, ColumnAlignment.Center),
            new(1, ColumnAlignment.Center),
            new(18, ColumnAlignment.Center),
            new(4, ColumnAlignment.Center),
            new(10, ColumnAlignment.Right, '0', PaddingSide.Left),
            new(60, ColumnAlignment.Left, ' '),
            new(4, ColumnAlignment.Center),
            new(1, ColumnAlignment.Center),
            new(10, ColumnAlignment.Center),
            new(4, ColumnAlignment.Center),
            new(19, ColumnAlignment.Right, ' ', PaddingSide.Left),
        ];
    }

    private object[] CreateRow(string sourceDocument, int consecutiveNumber, AccountingAssistant accountingAssistant, string accountingCode, string cenint)
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
            AccountingReportConstants.BlankSpace,
            consecutiveNumber,
            AccountingReportConstants.FORINT,
            AccountingReportConstants.BlankSpace,
            accountingAssistant.Date.ToString("yyyyMMdd"),
            accountingAssistant.Account ?? "",
            cenint,
            AccountingReportConstants.BlankSpace,
            accountingAssistant.Identification,
            AccountingReportConstants.BlankSpace,
            accountingAssistant.VerificationDigit,
            accountingAssistant.Name,
            DBAINT,
            AccountingReportConstants.NBAINT,
            AccountingReportConstants.BlankSpace,
            AccountingReportConstants.VBAINT,
            AccountingReportConstants.BlankSpace,
            AccountingReportConstants.NVBINT,
            AccountingReportConstants.BlankSpace,
            AccountingReportConstants.FEMINT,
            AccountingReportConstants.BlankSpace,
            AccountingReportConstants.FVEINT,
            AccountingReportConstants.BlankSpace,
            creditValue,
            AccountingReportConstants.BlankSpace,
            debitValue,
            AccountingReportConstants.BlankSpace,
            AccountingReportConstants.ZeroValue,
            new string(AccountingReportConstants.BlankSpace, 4),
            AccountingReportConstants.BlankSpace,
            AccountingReportConstants.ZeroValue,
            new string(AccountingReportConstants.BlankSpace, 4),
            accountingCode,
            accountingAssistant.Detail ?? "",
            AccountingReportConstants.BlankSpace,
            AccountingReportConstants.ESTINT,
            new string(AccountingReportConstants.BlankSpace, 10),
            new string(AccountingReportConstants.BlankSpace, 4),
            AccountingReportConstants.NDOINT,
        ];
    }

    private async Task UpdateConsecutivesInDatabaseAsync(int lastIncomeConsecutive, int lastEgressConsecutive, int lastYieldConsecutive, int lastConceptConsecutive, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await consecutiveRepository.UpdateIncomeConsecutiveAsync(lastIncomeConsecutive, cancellationToken);

            await consecutiveRepository.UpdateEgressConsecutiveAsync(lastEgressConsecutive, cancellationToken);

            await consecutiveRepository.UpdateYieldsConsecutiveAsync(lastYieldConsecutive, cancellationToken);
            
            await consecutiveRepository.UpdateConceptConsecutiveAsync(lastConceptConsecutive, cancellationToken);

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

        string fileName = $"{ReportName}{processDate:yyMMdd}{consecutive:D3}.txt";
        return fileName;
    }
}
