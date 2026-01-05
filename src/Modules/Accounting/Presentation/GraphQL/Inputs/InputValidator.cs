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
                var gqlName = GetGraphQLName(prop);

                if (isAnnotatedNullable)
                    continue;

                if (underlyingType == typeof(string))
                {
                    int? maxLength = null;

                    var maxAttr = prop.GetCustomAttributes(inherit: true)
                                      .OfType<MaxCharLengthAttribute>()
                                      .FirstOrDefault();

                    if (maxAttr is not null)
                        maxLength = maxAttr.MaxLength;

                    InputValidationHelpers.AddStringRules<T>(this, prop, gqlName, maxLength);
                    continue;
                }

                if (typeof(IEnumerable).IsAssignableFrom(underlyingType) && underlyingType != typeof(string))
                {
                    InputValidationHelpers.AddEnumerableRules<T>(this, prop, gqlName);
                    continue;
                }

                if (underlyingType.IsValueType)
                {
                    InputValidationHelpers.AddValueTypeRules<T>(this, prop, gqlName, underlyingType);
                    continue;
                }

                InputValidationHelpers.AddReferenceRules<T>(this, prop, gqlName);
            }
        }

        private static string GetGraphQLName(PropertyInfo prop)
        {
            var cad = prop.CustomAttributes
                .FirstOrDefault(a => a.AttributeType.Name == "GraphQLNameAttribute" || a.AttributeType.Name == "GraphQLName");

            if (cad != null && cad.ConstructorArguments.Count > 0)
            {
                var arg = cad.ConstructorArguments[0].Value;
                if (arg is string s && !string.IsNullOrWhiteSpace(s))
                    return s;
            }

            return prop.Name;
        }
    }
}