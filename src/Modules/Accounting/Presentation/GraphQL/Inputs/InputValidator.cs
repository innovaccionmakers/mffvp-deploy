using Common.SharedKernel.Infrastructure.Validation;
using FluentValidation;
using System.Collections;
using System.Reflection;

namespace Accounting.Presentation.GraphQL.Inputs
{
    public sealed class InputValidator<T> : AbstractValidator<T>
        where T : class
    {
        public InputValidator()
        {
            Include(new TechnicalValidator<T>());

            var type = typeof(T);
            var nullContext = new NullabilityInfoContext();

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                var propertyType = prop.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                var nullabilityInfo = nullContext.Create(prop);
                var isAnnotatedNullable = nullabilityInfo.ReadState == NullabilityState.Nullable;

                // Skip required checks if the property is annotated nullable.
                if (isAnnotatedNullable)
                    continue;

                // Strings: required and not empty/whitespace
                if (underlyingType == typeof(string))
                {
                    RuleFor(x => prop.GetValue(x))
                        .Cascade(CascadeMode.Stop)
                        .Must(v => v is string s && !string.IsNullOrWhiteSpace(s))
                        .WithName(prop.Name)
                        .WithMessage($"El campo {prop.Name} es obligatorio y no puede estar vacío.");
                    continue;
                }

                // IEnumerable (except string): required and not empty
                if (typeof(IEnumerable).IsAssignableFrom(underlyingType) && underlyingType != typeof(string))
                {
                    RuleFor(x => prop.GetValue(x))
                        .Cascade(CascadeMode.Stop)
                        .Must(v => v is IEnumerable en && en.GetEnumerator().MoveNext())
                        .WithName(prop.Name)
                        .WithMessage($"El campo {prop.Name} es obligatorio y debe contener al menos un elemento.");
                    continue;
                }

                // Value types (int, DateOnly, enums, structs): must not be default
                if (underlyingType.IsValueType)
                {
                    RuleFor(x => prop.GetValue(x))
                        .Cascade(CascadeMode.Stop)
                        .Must(v => v != null && !Equals(v, Activator.CreateInstance(underlyingType)))
                        .WithName(prop.Name)
                        .WithMessage($"El campo {prop.Name} es obligatorio y no puede tener el valor por defecto ({underlyingType.Name}).");
                    continue;
                }

                // Reference types: must not be null
                RuleFor(x => prop.GetValue(x))
                    .Cascade(CascadeMode.Stop)
                    .Must(v => v != null)
                    .WithName(prop.Name)
                    .WithMessage($"El campo {prop.Name} es obligatorio y no puede ser null.");
            }
        }
    }
}