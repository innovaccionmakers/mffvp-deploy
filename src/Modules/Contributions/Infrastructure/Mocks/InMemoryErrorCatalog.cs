using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.SharedKernel.Domain;
using Contributions.Application.Abstractions.Rules;
using Microsoft.AspNetCore.Hosting;

namespace Contributions.Infrastructure.Mocks
{
    internal sealed class InMemoryErrorCatalog : IErrorCatalog
    {
        private readonly ConcurrentDictionary<string, (int Code, string Msg)> _errors;

        public InMemoryErrorCatalog()
        {
            _errors = new ConcurrentDictionary<string, (int, string)>();

            foreach (PropertyInfo p in typeof(ContributionErrors)
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

            int lastDot = ruleKey.LastIndexOf('.');
            if (lastDot >= 0)
            {
                string tail = ruleKey[(lastDot + 1)..];
                if (_errors.TryGetValue(tail, out hit))
                    return Task.FromResult(hit);
            }

            return Task.FromResult((6000, "Validation error"));
        }
    }
}
