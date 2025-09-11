using Operations.Application.Abstractions.Services.Channel;
using Operations.Application.Abstractions.Services.ContributionService;
using Operations.Application.Abstractions.Services.Portfolio;
using Operations.Application.Abstractions.Services.SalesUser;

namespace Operations.Application.Contributions.Services;

public class BuildMissingFieldsContributionService : IBuildMissingFieldsContributionService
{
    private readonly IChannelService _channelService;
    private readonly IPortfolioService _portfolioService;
    private readonly ISalesUserService _salesUserService;

    public BuildMissingFieldsContributionService(
        IChannelService channelService,
        IPortfolioService portfolioService,
        ISalesUserService salesUserService)
    {
        _channelService = channelService;
        _portfolioService = portfolioService;
        _salesUserService = salesUserService;
    }

    public async Task<(DateTime ExecuteDate, string Channel, string SalesUser)> BuildAsync(
        string? portfolioId,
        int objetiveId,
        CancellationToken cancellationToken = default)
    {
        var executeDate = await _portfolioService.GetNextDateFromCurrentDateAsync(portfolioId, objetiveId, cancellationToken);
        var channel = await _channelService.GetChannelCodeAsync("Makers", cancellationToken);
        var salesUser = _salesUserService.GetUser();

        return (executeDate, channel, salesUser);
    }
}