using System.Collections.Concurrent;
using System.Reflection;
using Activations.Application.Abstractions.Rules;
using Common.SharedKernel.Domain;

namespace Activations.Infrastructure.Mocks;

internal sealed class InMemoryActivationErrorCatalog : IErrorCatalog
{
    private readonly ConcurrentDictionary<string, (int Code, string Msg)> _errors;

    public InMemoryActivationErrorCatalog()
    {
        _errors = new ConcurrentDictionary<string, (int, string)>();

        foreach (var p in typeof(ActivationErrors)
                     .GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
            if (p.GetValue(null) is not Error err)
                continue;

            _errors[p.Name] = (int.Parse(err.Code), err.Description);
        }
    }

    public Task<(int Code, string DefaultMessage)> GetAsync(
        string ruleKey,
        CancellationToken ct)
    {
        if (_errors.TryGetValue(ruleKey, out var hit))
            return Task.FromResult(hit);

        var lastDot = ruleKey.LastIndexOf('.');
        if (lastDot >= 0)
        {
            var tail = ruleKey[(lastDot + 1)..];
            if (_errors.TryGetValue(tail, out hit))
                return Task.FromResult(hit);
        }

        return Task.FromResult((6666, "Validation error"));
    }
}