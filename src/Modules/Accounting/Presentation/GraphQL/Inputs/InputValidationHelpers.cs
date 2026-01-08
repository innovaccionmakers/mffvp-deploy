using System;
using System.Collections;
using System.Reflection;
using System.Text;
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
            var name = TonameFieldName(gqlName);

            // Base not-empty rule
            var rule = validator.RuleFor(x => prop.GetValue(x) as string)
                .Cascade(CascadeMode.Stop)
                .Must(v => v is string s && !string.IsNullOrWhiteSpace(s))
                .WithName(gqlName)
                .WithMessage($"El campo {name} es obligatorio y no puede estar vacío.");

            if (maxLength.HasValue)
            {
                var max = maxLength.Value;
                rule = rule.Must(v => v is string s && s.Length <= max)
                    .WithMessage($"El campo {name} no puede tener más de {max} caracteres.");
            }
        }

        public static void AddEnumerableRules<T>(
            AbstractValidator<T> validator,
            PropertyInfo prop,
            string gqlName)
            where T : class
        {
            var name = TonameFieldName(gqlName);

            validator.RuleFor(x => prop.GetValue(x))
                .Cascade(CascadeMode.Stop)
                .Must(v => v is IEnumerable en && en.GetEnumerator().MoveNext())
                .WithName(gqlName)
                .WithMessage($"El campo {name} es obligatorio y debe contener al menos un elemento.");
        }

        public static void AddValueTypeRules<T>(
            AbstractValidator<T> validator,
            PropertyInfo prop,
            string gqlName,
            Type underlyingType)
            where T : class
        {
            var name = TonameFieldName(gqlName);

            validator.RuleFor(x => prop.GetValue(x))
                .Cascade(CascadeMode.Stop)
                .Must(v => v != null && !Equals(v, Activator.CreateInstance(underlyingType)))
                .WithName(gqlName)
                .WithMessage($"El campo {name} es obligatorio y no puede tener el valor por defecto ({underlyingType.Name}).");
        }

        public static void AddReferenceRules<T>(
            AbstractValidator<T> validator,
            PropertyInfo prop,
            string gqlName)
            where T : class
        {
            var name = TonameFieldName(gqlName);

            validator.RuleFor(x => prop.GetValue(x))
                .Cascade(CascadeMode.Stop)
                .Must(v => v != null)
                .WithName(gqlName)
                .WithMessage($"El campo {name} es obligatorio y no puede ser null.");
        }

        private static string TonameFieldName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name ?? string.Empty;

            name = name.Replace('_', ' ').Replace('-', ' ').Trim();

            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                var c = name[i];

                if (char.IsUpper(c) && i > 0)
                {
                    var prev = name[i - 1];
                    if (char.IsLower(prev) || char.IsDigit(prev) || prev == ' ')
                    {
                        sb.Append(' ');
                    }
                    else if (i + 1 < name.Length && char.IsLower(name[i + 1]))
                    {
                        sb.Append(' ');
                    }
                }

                sb.Append(char.ToLowerInvariant(c));
            }

            var result = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
            return result;
        }
    }
}