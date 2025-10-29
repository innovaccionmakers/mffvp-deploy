using Accounting.Application.Abstractions.Data;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.ConsecutiveFiles;
using Common.SharedKernel.Application.Reports.Strategies;
using Common.SharedKernel.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingGeneration.Reports;

public class AccountingGenerationReport(ILogger<AccountingGenerationReport> logger,
                                        IAccountingAssistantRepository accountingAssistantRepository,
                                        IConsecutiveFileRepository consecutiveFileRepository,
                                        IUnitOfWork unitOfWork) : ExcelReportStrategyBase(logger)
{
    public override string ReportName => "E";

    public override string[] ColumnHeaders => throw new NotImplementedException();

    public async override Task<IActionResult> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await GenerateExcelReportAsync(
                   ct => GetWorksheetDataAsync(cancellationToken),
                   await GenerateReportFileNameAsync(cancellationToken),
                   cancellationToken);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar el reporte de generación contable");
            throw;
        }
    }

    private async Task<List<WorksheetData>> GetWorksheetDataAsync(CancellationToken cancellationToken)
    {
        var worksheetDataList = new List<WorksheetData>();

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

        var result = new WorksheetData
        {
            WorksheetName = WorksheetNames.Accounting,
            ColumnHeaders = ColumnHeaders,
            Rows = rows ?? []
        };

        worksheetDataList.Add(result);

        return worksheetDataList;
    }

    private async Task<string> GenerateReportFileNameAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.Today;

        
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

       
        var consecutiveFile = await consecutiveFileRepository.GetByGenerationDateAsync(today, cancellationToken);
        int consecutive = 1;

        if (consecutiveFile is not null)
        {
            consecutiveFile.Increment();
            consecutive = consecutiveFile.Consecutive;
            await consecutiveFileRepository.UpdateAsync(consecutiveFile);
        }
        else
        {
            var newConsecutive = ConsecutiveFile.Create(today, 1, DateTime.Now);
            if (newConsecutive.IsFailure)
                throw new Exception($"Error al crear el consecutivo para la fecha {today:yyyy-MM-dd}");

            consecutive = newConsecutive.Value.Consecutive;
            await consecutiveFileRepository.AddAsync(newConsecutive.Value, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        
        string fileName = $"{ReportName}{today:ddMMyyyy}{consecutive:D3}.xlsx";
        return fileName;
    }
}
