using System;
using System.Collections;
using System.Reflection;
using FluentValidation;

namespace Accounting.Presentation.GraphQL.Inputs
{
    internal static class InputValidationHelpers
    {
        public static void AddStringRules<T>(
            AbstractValidator<T> validator,
            PropertyInfo prop,
            string gqlName,
            int? maxLength)
            where T : class
        {
            // Base not-empty rule
            var rule = validator.RuleFor(x => prop.GetValue(x) as string)
                .Cascade(CascadeMode.Stop)
                .Must(v => v is string s && !string.IsNullOrWhiteSpace(s))
                .WithName(gqlName)
                .WithMessage($"El campo {gqlName} es obligatorio y no puede estar vacío.");

            if (maxLength.HasValue)
            {
                var max = maxLength.Value;
                rule = rule.Must(v => v is string s && s.Length <= max)
                    .WithMessage($"El campo {gqlName} no puede tener más de {max} caracteres.");
            }
        }

        public static void AddEnumerableRules<T>(
            AbstractValidator<T> validator,
            PropertyInfo prop,
            string gqlName)
            where T : class
        {
            validator.RuleFor(x => prop.GetValue(x))
                .Cascade(CascadeMode.Stop)
                .Must(v => v is IEnumerable en && en.GetEnumerator().MoveNext())
                .WithName(gqlName)
                .WithMessage($"El campo {gqlName} es obligatorio y debe contener al menos un elemento.");
        }

        public static void AddValueTypeRules<T>(
            AbstractValidator<T> validator,
            PropertyInfo prop,
            string gqlName,
            Type underlyingType)
            where T : class
        {
            validator.RuleFor(x => prop.GetValue(x))
                .Cascade(CascadeMode.Stop)
                .Must(v => v != null && !Equals(v, Activator.CreateInstance(underlyingType)))
                .WithName(gqlName)
                .WithMessage($"El campo {gqlName} es obligatorio y no puede tener el valor por defecto ({underlyingType.Name}).");
        }

        public static void AddReferenceRules<T>(
            AbstractValidator<T> validator,
            PropertyInfo prop,
            string gqlName)
            where T : class
        {
            validator.RuleFor(x => prop.GetValue(x))
                .Cascade(CascadeMode.Stop)
                .Must(v => v != null)
                .WithName(gqlName)
                .WithMessage($"El campo {gqlName} es obligatorio y no puede ser null.");
        }
    }
}