using System.Reflection;
using FluentValidation;

namespace Common.SharedKernel.Infrastructure.Validation;

public sealed class TechnicalValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : class
{
    public TechnicalValidator()
    {
        var nullContext = new NullabilityInfoContext();

        foreach (var prop in typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var p = prop;
            var propertyType = p.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            var nullability = nullContext.Create(p);
            var isNullable = nullability.ReadState == NullabilityState.Nullable
                             || Nullable.GetUnderlyingType(propertyType) != null;

            if (!isNullable)
                RuleFor(x => p.GetValue(x))
                    .NotNull()
                    .WithName(p.Name)
                    .WithMessage($"{p.Name} cannot be null");

            if (propertyType == typeof(string))
                RuleFor(x => (string?)p.GetValue(x))
                    .Must(s => s is null || !string.IsNullOrWhiteSpace(s))
                    .WithName(p.Name)
                    .WithMessage($"{p.Name} cannot be empty or whitespace");

            RuleFor(x => p.GetValue(x))
                .Must(v => v == null || underlyingType.IsInstanceOfType(v))
                .WithName(p.Name)
                .WithMessage($"{p.Name} must be of type {underlyingType.Name}");
        }
    }
}