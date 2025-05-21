using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using People.Application.Abstractions;
using People.Application.Abstractions.Rules;
using People.Domain.People;
using People.Integrations.People.GetPersonValidation;
using Common.SharedKernel.Application.Messaging;

namespace People.Infrastructure.People;

public class PersonValidationConsumer : ICapSubscribe
{
    private readonly IPersonRepository _repo;
    private readonly IRuleEvaluator<PeopleModuleMarker> _rules;
    private readonly ILogger<PersonValidationConsumer> _logger;
    private const string Workflow = "People.Person.Validation";

    public PersonValidationConsumer(IPersonRepository repo, IRuleEvaluator<PeopleModuleMarker> rules, ILogger<PersonValidationConsumer> logger)
    {
        _repo = repo;
        _rules = rules;
        _logger = logger;
    }

    [CapSubscribe("people.validation.request")]
    public async Task<GetPersonValidationResponse> ValidateAsync(GetPersonValidationRequest msg, [FromCap] CapHeader header, CancellationToken ct)
    {
        var corr = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, corr);

        _logger.LogDebug("Validate person {PersonId} cid={Cid}", msg.PersonId, corr);

        var person = await _repo.GetAsync(msg.PersonId, ct);
        var (ok, _, errs) = await _rules.EvaluateAsync(Workflow, person, ct);

        if (ok) return new GetPersonValidationResponse(true, null, null);

        var first = errs.First();
        return new GetPersonValidationResponse(false, first.Code, first.Message);
    }
}