namespace Products.Presentation.GraphQL;

using HotChocolate;
using MediatR;
using Products.Integrations.Alternatives;
using Products.Integrations.Commercials;
using Products.Integrations.ConfigurationParameters.DocumentTypes;
using Products.Integrations.ConfigurationParameters.GoalTypes;
using Products.Integrations.Objectives.GetObjectives;
using Products.Integrations.Objectives.GetObjectivesByAffiliate;
using Products.Integrations.Offices;
using Products.Integrations.PlanFunds.GetPlanFund;
using Products.Integrations.Portfolios.Queries;
using Products.Presentation.DTOs;
using Products.Presentation.DTOs.PlanFund;

public class ProductsExperienceQueries(IMediator mediator) : IProductsExperienceQueries
{
    public async Task<PortfolioDto> GetPortfolioAsync(string objetiveId,
        CancellationToken cancellationToken)
    {
            var result = await mediator.Send(new GetPortfolioInformationByObjetiveIdQuery(objetiveId), cancellationToken);

            if (!result.IsSuccess || result.Value == null)
            {
                throw new InvalidOperationException("Failed to retrieve portfolio information.");
            }

            var portfolioInformation = result.Value;


            return new PortfolioDto(
                portfolioInformation.Found,
                portfolioInformation.Plan,
                portfolioInformation.Alternative,
                portfolioInformation.Portfolio
            );
    }

    public async Task<IReadOnlyCollection<DocumentTypeDto>> GetDocumentTypesAsync(
      CancellationToken cancellationToken = default)
    {

        var result = await mediator.Send(new GetDocumentTypesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve transaction types.");
        }

        var documentTypes = result.Value;


        return documentTypes.Select(x => new DocumentTypeDto(
            x.Uuid,
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<GoalTypeDto>> GetGoalTypesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = await mediator.Send(new GetGoalTypesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve goal types.");
        }

        var goalTypes = result.Value;

        return goalTypes.Select(x => new GoalTypeDto(
            x.Uuid,
            x.Name,
            x.Status,
            x.HomologationCode
        )).ToList();
    }


    public async Task<IReadOnlyCollection<GoalDto>> GetGoalsAsync(string typeId, string identification, StatusType status, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetObjectivesQuery(typeId, identification, status), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            var error = new GoalError(
                result.Error.Code,
                result.Error.Type.ToString(),
                result.Error.Description
            );

            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Description)
                    .SetCode(error.Code)
                    .SetExtension("type", error.Type)
                    .SetExtension("description", error.Description)
                    .Build()
            );
        }

        var goals = result.Value;

        return goals.Select(x => new GoalDto(
            x.Objective.ObjectiveId,
             x.Objective.ObjectiveName,
             x.Objective.Status
        )).ToList();
    }

    public async Task<IReadOnlyCollection<OfficeDto>> GetOfficesAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetOfficesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Failed to retrieve Offices")
                    .Build()
            );
        }

        var offices = result.Value;

        return offices.Select(x => new OfficeDto(
            x.OfficeId,
            x.Name,
            x.Prefix,
            x.CityId.ToString(),
            x.Status.ToString(),
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<CommercialDto>> GetCommercialsAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCommercialsQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Failed to retrieve Commercials")
                    .Build()
            );
        }

        var commercials = result.Value;

        return commercials.Select(x => new CommercialDto(
            x.CommercialId,
            x.Name,
            x.Prefix,
            x.Status.ToString(),
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<AlternativeDto>> GetAlternativesAsync(
        CancellationToken cancellationToken = default)
    {

        var result = await mediator.Send(new GetAlternativesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve alternatives.");
        }

        var alternatives = result.Value;

        return alternatives.Select(x => new AlternativeDto(
            x.AlternativeId,
            x.AlternativeTypeId,
            x.Name,
            x.Description,
            x.Status.ToString(),
            x.HomologatedCode
        )).ToList();

    }

    public async Task<PlanFundDto> GetPlanFundAsync(string alternativeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPlanFundByAlternativeIdQuery(alternativeId), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve plan and fund information.");
        }

        var planFundInformation = result.Value;

        return new PlanFundDto(
            new PlanDto(
                planFundInformation.PlanId,
                planFundInformation.PlanName,
                planFundInformation.HomologatedCodePlan),
            new FundDto(
                planFundInformation.FundId,
                planFundInformation.FundName,
                planFundInformation.HomologatedCodeFund)
        );
    }

    public async Task<IReadOnlyCollection<AffiliateGoalDto>> GetAffiliateObjectivesAsync(
        int affiliateId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetObjectivesByAffiliateQuery(affiliateId),
            cancellationToken);

        if (!result.IsSuccess || result.Value == null)
            return Array.Empty<AffiliateGoalDto>();

        return result.Value
            .Select(o => new AffiliateGoalDto(
                Id: o.Id,
                Name: o.Name,
                IdType: o.IdType.ToString(),
                Type: o.Type,
                IdPlan: o.IdPlan,
                Plan: o.Plan,
                IdFund: o.IdFund,
                Fund: o.Fund,
                IdAlternative: o.IdAlternative,
                Alternative: o.Alternative,
                IdCommercial: o.IdCommercial,
                Commercial: o.Commercial,
                IdOpeningOffice: o.IdOpeningOffice,
                OpeningOffice: o.OpeningOffice,
                IdCurrentOffice: o.IdCurrentOffice,
                CurrentOffice: o.CurrentOffice,
                Status: o.Status
            )).ToList();
    }
}
