using HotChocolate;
using MediatR;
using System.Linq;
using Treasury.Integrations.BankAccounts.GetBankAccountsByPortfolio;
using Treasury.Integrations.BankAccounts.GetBankAccountsByPortfolioAndIssuer;
using Treasury.Integrations.Banks;
using Treasury.Integrations.Issuers.GetIssuers;
using Treasury.Integrations.TreasuryConcepts.GetTreasuryConcepts;
using Treasury.Integrations.TreasuryMovements.Queries;
using Treasury.Presentation.DTOs;

namespace Treasury.Presentation.GraphQL;

public class TreasuryExperienceQueries(IMediator mediator) : ITreasuryExperienceQueries
{
    public async Task<IReadOnlyCollection<IssuerDto>> GetIssuersAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetIssuersQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Failed to retrieve Portfolios")
                    .Build()
            );
        }

        var issuers = result.Value;

        return issuers.Select(x => new IssuerDto(
            x.Id,
            x.IssuerCode,
            x.Description,
            x.Nit,
            x.Digit,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<BankDto>> GetBanksAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetBanksQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Failed to retrieve banks")
                    .Build()
            );
        }

        var banks = result.Value;

        return banks.Select(x => new BankDto(
            x.Id.ToString(),
            x.Description,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<BankAccountDto>> GetBankAccountsByPortfolioAsync(
        long portfolioId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetBankAccountsByPortfolioQuery(portfolioId), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Failed to retrieve bank accounts")
                    .Build()
            );
        }

        var accounts = result.Value;

        return accounts.Select(x => new BankAccountDto(
            x.Id,
            x.PortfolioId,
            x.IssuerId,
            x.Issuer?.Description ?? string.Empty,
            x.Issuer?.IssuerCode ?? string.Empty,
            x.AccountNumber,
            x.AccountType == "C" ? "Corriente" : "Ahorros",
            x.Observations,
            x.ProcessDate
        )).ToList();
    }

    public async Task<IReadOnlyCollection<TreasuryConceptDto>> GetTreasuryConceptsAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetTreasuryConceptsQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Failed to retrieve Treasury Concepts")
                    .Build()
            );
        }

        var concepts = result.Value;

        string BoolToSiNo(bool value) => value ? "SI" : "NO";

        return concepts.Select(x => new TreasuryConceptDto(
            x.Id,
            x.Concept,
            x.Nature.ToString(),
            BoolToSiNo(x.AllowsNegative),
            BoolToSiNo(x.AllowsExpense),
            BoolToSiNo(x.RequiresBankAccount),
            BoolToSiNo(x.RequiresCounterparty),
            x.ProcessDate,
            x.Observations
        )).ToList();
    }

    public async Task<IReadOnlyCollection<BankAccountDto>> GetBankAccountsByPortfolioAndIssuerAsync(
        long portfolioId,
        long issuerId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetBankAccountsByPortfolioAndIssuerQuery(portfolioId, issuerId), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Failed to retrieve bank accounts")
                    .Build()
            );
        }

        var accounts = result.Value;

        return accounts.Select(x => new BankAccountDto(
            x.Id,
            x.PortfolioId,
            x.IssuerId,
            x.Issuer?.IssuerCode ?? string.Empty,
            x.Issuer?.Description ?? string.Empty,
            x.AccountNumber,
            x.AccountType,
            x.Observations,
            x.ProcessDate
        )).ToList();
    }

    public async Task<IReadOnlyCollection<TreasuryMovementDto>> GetTreasuryMovementsByPortfolioIdsAsync(IEnumerable<long> portfolioIds, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetTreasuryMovementsByPortfolioIdsQuery(portfolioIds), cancellationToken);

        if (!result.IsSuccess || result.Value == null)  return [];

        var movements = result.Value;

        return movements.Select(x => new TreasuryMovementDto(
             x.PortfolioId,
             DateOnly.FromDateTime(x.ClosingDate),
             x.TreasuryConcept.Concept,
             x.TreasuryConcept.AllowsExpense ? "Gasto" : "Rendimiento Bruto",
             x.TreasuryConcept.Nature.ToString(),
             x.BankAccount.AccountNumber,
             x.Counterparty.Description,
             x.Value
        )).ToList();
    }
}